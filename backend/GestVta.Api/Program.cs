using GestVta.Api.Data;
using GestVta.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GestVta API", Version = "v1" });
});

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Swagger: visible siempre salvo en Production (evita depender solo de ASPNETCORE_ENVIRONMENT=Development).
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GestVta API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();

// En Development, no forzar HTTPS: si entras por http://localhost:5288, no redirige a https (Swagger deja de "no abrir").
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapMaestroCrudEndpoints();
app.MapControllers();

app.Run();
