namespace GestVta.Api.Models;

/// <summary>
/// Permisos CRUD por rol y opción de menú.
/// </summary>
public class RolMenuPermiso
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public int MenuOpcionId { get; set; }
    public MenuOpcion MenuOpcion { get; set; } = null!;

    public bool PuedeLeer { get; set; }
    public bool PuedeEscribir { get; set; }
    public bool PuedeModificar { get; set; }
    public bool PuedeEliminar { get; set; }
}
