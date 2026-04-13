namespace GestVta.Api.Models;

public class AgenciaTransporte
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Ruc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
