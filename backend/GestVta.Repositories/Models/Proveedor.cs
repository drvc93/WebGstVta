using System.Text.Json.Serialization;

namespace GestVta.Api.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public int TipoDocumentoId { get; set; }

    [JsonIgnore]
    public TipoDocumento? TipoDocumento { get; set; }

    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
