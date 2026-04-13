using System.Text.Json.Serialization;

namespace GestVta.Api.Models;

public class Compania
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int TipoDocumentoId { get; set; }

    [JsonIgnore]
    public TipoDocumento? TipoDocumento { get; set; }

    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public int PaisId { get; set; }

    [JsonIgnore]
    public Pais? Pais { get; set; }

    public int? UbigeoId { get; set; }

    [JsonIgnore]
    public Ubigeo? Ubigeo { get; set; }

    public string? Correo { get; set; }
    public bool Activo { get; set; } = true;
    public string? LogoPath { get; set; }
    /// <summary>Color de marca en hex (#RRGGBB), p. ej. #1a3a5c.</summary>
    public string? ColorPrimario { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
