using System.Globalization;
using System.Text;
using System.Text.Json;
using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GestVta.Services.Maestros;

public sealed class TiposCambioService : MaestroServiceBase<TipoCambio>, ITiposCambioService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MigoExchangeOptions _migo;

    public TiposCambioService(
        ApplicationDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<MigoExchangeOptions> migoOptions) : base(db)
    {
        _httpClientFactory = httpClientFactory;
        _migo = migoOptions.Value;
    }

    protected override DbSet<TipoCambio> Set => Db.TiposCambio;

    protected override IQueryable<TipoCambio> OrderedQuery(IQueryable<TipoCambio> query) => query.OrderByDescending(e => e.Fecha);

    /// <summary>
    /// Garantiza que exista tipo de cambio USD para la fecha de hoy.
    /// Si no existe, consulta API externa y lo registra antes de listar.
    /// </summary>
    public async Task<(bool ok, object? body, string? badRequest)> EnsureUsdHoyAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_migo.Endpoint))
            return (false, null, "Configure MigoExchange:Endpoint en appsettings (o variables de entorno).");
        if (string.IsNullOrWhiteSpace(_migo.Token))
            return (false, null, "Configure MigoExchange:Token en appsettings o secretos (no debe quedar vacío en producción).");

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var usd = await Db.Monedas.AsNoTracking().FirstOrDefaultAsync(m => m.Codigo.ToUpper() == "USD", ct);
        if (usd is null)
            return (false, null, "No existe la moneda USD en el maestro de monedas.");

        var exists = await Db.TiposCambio.AsNoTracking().AnyAsync(t => t.MonedaId == usd.Id && t.Fecha == hoy, ct);
        if (exists)
            return (true, new { success = true, created = false, fecha = hoy.ToString("yyyy-MM-dd"), moneda = "USD" }, null);

        var client = _httpClientFactory.CreateClient();
        var payload = JsonSerializer.Serialize(new { token = _migo.Token, fecha = hoy.ToString("yyyy-MM-dd") });
        using var req = new HttpRequestMessage(HttpMethod.Post, _migo.Endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };
        req.Headers.Accept.ParseAdd("application/json");

        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
            return (false, null, $"No se pudo consultar API de tipo de cambio (HTTP {(int)res.StatusCode}).");

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;
        if (!root.TryGetProperty("success", out var okNode) || okNode.ValueKind != JsonValueKind.True)
            return (false, null, "La API externa no devolvió una respuesta exitosa.");

        var monedaApi = root.TryGetProperty("moneda", out var monNode) ? monNode.GetString() : "USD";
        if (!string.Equals(monedaApi, "USD", StringComparison.OrdinalIgnoreCase))
            return (false, null, $"La API devolvió moneda inesperada: {monedaApi}.");

        var compraRaw = root.TryGetProperty("precio_compra", out var cNode) ? cNode.GetString() : null;
        var ventaRaw = root.TryGetProperty("precio_venta", out var vNode) ? vNode.GetString() : null;

        if (!decimal.TryParse(compraRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var compra) ||
            !decimal.TryParse(ventaRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var venta))
            return (false, null, "No se pudieron interpretar precio_compra / precio_venta desde la API externa.");

        exists = await Db.TiposCambio.AsNoTracking().AnyAsync(t => t.MonedaId == usd.Id && t.Fecha == hoy, ct);
        if (!exists)
        {
            Db.TiposCambio.Add(new TipoCambio
            {
                MonedaId = usd.Id,
                Fecha = hoy,
                ValorCompra = compra,
                ValorVenta = venta,
                Activo = true,
                UltUsuario = "API_MIGO",
                UltMod = DateTime.UtcNow,
            });
            await Db.SaveChangesAsync(ct);
            return (true, new { success = true, created = true, fecha = hoy.ToString("yyyy-MM-dd"), moneda = "USD" }, null);
        }

        return (true, new { success = true, created = false, fecha = hoy.ToString("yyyy-MM-dd"), moneda = "USD" }, null);
    }
}
