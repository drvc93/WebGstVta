namespace GestVta.Services.Maestros;

/// <summary>Configuración para consultar tipo de cambio USD (API Migo).</summary>
public sealed class MigoExchangeOptions
{
    public const string SectionName = "MigoExchange";

    /// <summary>URL del endpoint POST (ej. https://api.migo.pe/api/v1/exchange/date).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Token de autenticación de la API Migo.</summary>
    public string Token { get; set; } = string.Empty;
}
