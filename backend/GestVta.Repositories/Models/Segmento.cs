namespace GestVta.Api.Models;

public class Segmento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string? UltUsuario { get; set; }
    public DateTime? UltMod { get; set; }
}
