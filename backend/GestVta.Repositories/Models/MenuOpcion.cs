namespace GestVta.Api.Models;

/// <summary>
/// Opción de menú jerárquica (sección sin ruta o ítem con ruta Angular).
/// </summary>
public class MenuOpcion
{
    public int Id { get; set; }
    /// <summary>Código estable único (ej. MAESTROS, OP_COMPANIA).</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Ruta del router (ej. /maestros/compania). Null si es solo agrupador.</summary>
    public string? Ruta { get; set; }
    /// <summary>Clase Bootstrap Icons sin prefijo bi- (ej. bi-building).</summary>
    public string? Icono { get; set; }
    public int? ParentId { get; set; }
    public MenuOpcion? Parent { get; set; }
    public ICollection<MenuOpcion> Hijos { get; set; } = new List<MenuOpcion>();
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<RolMenuPermiso> RolMenuPermisos { get; set; } = new List<RolMenuPermiso>();
}
