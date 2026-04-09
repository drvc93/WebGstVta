namespace GestVta.Api.Models;

public class Pais
{
    public int Id { get; set; }
    /// <summary>ISO 3166-1 alpha-2 (ej. PE).</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? NombreEn { get; set; }
    public string? Iso3 { get; set; }
    public string? TelefonoCodigo { get; set; }
    public string? Continente { get; set; }
    public bool Activo { get; set; } = true;
}
