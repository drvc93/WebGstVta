namespace GestVta.Api.Models;

public class Conductor
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Licencia { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
