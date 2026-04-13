namespace GestVta.Services.Dtos;

public sealed record MenuOpcionDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Ruta,
    string? Icono,
    int? ParentId,
    int Orden,
    bool Activo);

public sealed record MenuOpcionSaveDto(
    string Codigo,
    string Nombre,
    string? Ruta,
    string? Icono,
    int? ParentId,
    int Orden,
    bool Activo);

public sealed record RolMenuPermisoFilaDto(
    int MenuOpcionId,
    string Codigo,
    string Nombre,
    string? Ruta,
    int? ParentId,
    int Orden,
    bool PuedeLeer,
    bool PuedeEscribir,
    bool PuedeModificar,
    bool PuedeEliminar);

public sealed record RolMenuPermisoGuardarDto(
    int MenuOpcionId,
    bool PuedeLeer,
    bool PuedeEscribir,
    bool PuedeModificar,
    bool PuedeEliminar);

public sealed record MenuOpcionUsuarioDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Ruta,
    string? Icono,
    int? ParentId,
    int Orden,
    bool PuedeLeer,
    bool PuedeEscribir,
    bool PuedeModificar,
    bool PuedeEliminar);
