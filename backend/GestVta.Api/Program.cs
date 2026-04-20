using System.Text;
using System.Text.Json;
using System.IO;
using GestVta.Api.Data;
using GestVta.Api.Infrastructure;
using GestVta.Repositories;
using GestVta.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfig) =>
{
    // Sin ReadFrom.Configuration aquí: evita fallos de arranque si falta sección Serilog o hay conflicto con IIS.
    loggerConfig
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    // IIS: a menudo el pool no puede escribir en wwwroot. Se prueba: Logging:LogDirectory → ./Logs → ./App_Data/Logs → %TEMP%.
    var customDir = context.Configuration["Logging:LogDirectory"]?.Trim();
    var root = context.HostingEnvironment.ContentRootPath;
    var candidateDirs = new List<string>();
    if (!string.IsNullOrEmpty(customDir))
        candidateDirs.Add(customDir);
    candidateDirs.Add(Path.Combine(root, "Logs"));
    candidateDirs.Add(Path.Combine(root, "App_Data", "Logs"));
    candidateDirs.Add(Path.Combine(Path.GetTempPath(), "GestVtaApi", "Logs"));

    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    string? lastError = null;
    foreach (var logsDir in candidateDirs)
    {
        if (!seen.Add(logsDir))
            continue;

        try
        {
            Directory.CreateDirectory(logsDir);
            var probe = Path.Combine(logsDir, ".serilog-write-test");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            loggerConfig.WriteTo.File(
                path: Path.Combine(logsDir, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true);
            return;
        }
        catch (Exception ex)
        {
            lastError = ex.Message;
        }
    }

    try
    {
        var diag = Path.Combine(Path.GetTempPath(), "GestVtaApi-serilog-diagnostic.txt");
        File.AppendAllText(
            diag,
            $"[{DateTime.UtcNow:u}] No se pudo crear el sink de archivo Serilog. " +
            $"ContentRoot: {root}. Último error: {lastError}{Environment.NewLine}");
    }
    catch
    {
        // ignorar
    }
});

builder.Services.AddGestVtaPersistence(builder.Configuration);
builder.Services.AddGestVtaRepositories();
builder.Services.AddGestVtaApplicationServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        failureStatus: null,
        tags: null,
        customTestQuery: async (db, ct) =>
        {
            // El chequeo por defecto devuelve Unhealthy sin mensaje si CanConnectAsync es false.
            var ok = await db.Database.CanConnectAsync(ct);
            if (ok)
                return true;

            throw new InvalidOperationException(
                "CanConnectAsync devolvió false: no se pudo abrir conexión a SQL Server. " +
                "Revise ConnectionStrings:DefaultConnection (servidor, instancia, base GestVta), que el servicio SQL esté en ejecución, TCP habilitado y firewall del puerto.");
        });

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => Encoding.UTF8.GetBytes(o.Key).Length >= 32 && !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
        "Jwt:Issuer, Jwt:Audience y Jwt:Key (>=32 bytes UTF-8) son obligatorios.");

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IAccessTokenService>(sp => sp.GetRequiredService<JwtTokenService>());

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];
if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Falta configuración Jwt (Jwt:Key, Jwt:Issuer, Jwt:Audience). En IIS use appsettings del sitio o variables Jwt__Key, Jwt__Issuer, Jwt__Audience (clave UTF-8 >= 32 bytes).");
}

var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes en UTF-8.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GestVta API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT: encabezado Authorization con valor `Bearer {token}` (obtenido en POST /api/auth/login).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                  ?? Array.Empty<string>();
corsOrigins = corsOrigins.Where(static o => !string.IsNullOrWhiteSpace(o)).Select(static o => o.TrimEnd('/')).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
if (corsOrigins.Length == 0)
{
    corsOrigins = new[] { "http://localhost:4200" };
}

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 12 * 1024 * 1024);
builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var o = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;
    return FileStoragePaths.Create(env, o);
});

var app = builder.Build();

// Swagger: Development siempre; en Production solo si Swagger:Enabled = true (p. ej. temporal en IIS).
var swaggerEnabled =
    !app.Environment.IsProduction() || app.Configuration.GetValue("Swagger:Enabled", false);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GestVta API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();

var filePaths = app.Services.GetRequiredService<FileStoragePaths>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(filePaths.RootPhysical),
    RequestPath = filePaths.StaticRequestPath,
});

// En Development, no forzar HTTPS: si entras por http://localhost:5288, no redirige a https (Swagger deja de "no abrir").
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Solo proceso/IIS (útil si la BD aún no está accesible desde el servidor).
app.MapGet("/health/live", () => Results.Json(new { status = "Healthy", note = "No valida SQL Server." }));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        static string FormatExceptionChain(Exception? ex)
        {
            if (ex is null)
                return string.Empty;
            var parts = new List<string>();
            for (var x = ex; x is not null; x = x.InnerException)
                parts.Add($"{x.GetType().Name}: {x.Message}");
            return string.Join(" ← ", parts);
        }

        context.Response.ContentType = "application/json; charset=utf-8";
        var checks = report.Entries.Select(e =>
        {
            var entry = e.Value;
            var chain = FormatExceptionChain(entry.Exception);
            var errorOut = entry.Status == HealthStatus.Healthy
                ? null
                : (string.IsNullOrEmpty(chain)
                    ? (string.IsNullOrEmpty(entry.Description) ? "Fallo sin excepción ni descripción." : entry.Description)
                    : chain);
            return new
            {
                name = e.Key,
                status = entry.Status.ToString(),
                description = entry.Description,
                error = errorOut,
                data = entry.Data.Count > 0
                    ? entry.Data.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString())
                    : (Dictionary<string, string?>?)null,
            };
        });
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks,
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    },
});

app.Run();
