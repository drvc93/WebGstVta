using GestVta.Services.Dtos;
using GestVta.Api.Models;
using GestVta.Repositories;

namespace GestVta.Services;

public sealed class MenuOpcionesService : IMenuOpcionesService
{
    private readonly IMenuOpcionRepository _menuRepo;
    private readonly IRolMenuPermisoRepository _permRepo;

    public MenuOpcionesService(IMenuOpcionRepository menuRepo, IRolMenuPermisoRepository permRepo)
    {
        _menuRepo = menuRepo;
        _permRepo = permRepo;
    }

    public async Task<IReadOnlyList<MenuOpcionDto>> GetAllAsync(CancellationToken ct)
    {
        var rows = await _menuRepo.ListOrderedNoTrackingAsync(ct);
        return rows.Select(MapDto).ToList();
    }

    public async Task<MenuOpcionDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var m = await _menuRepo.GetByIdNoTrackingAsync(id, ct);
        return m is null ? null : MapDto(m);
    }

    public async Task<(MenuOpcionDto? created, string? error)> CreateAsync(MenuOpcionSaveDto dto, CancellationToken ct)
    {
        var err = await ValidateSaveAsync(dto, excludeId: null, ct);
        if (err is not null) return (null, err);
        var e = new MenuOpcion
        {
            Codigo = dto.Codigo.Trim().ToUpperInvariant(),
            Nombre = dto.Nombre.Trim(),
            Ruta = string.IsNullOrWhiteSpace(dto.Ruta) ? null : dto.Ruta.Trim(),
            Icono = string.IsNullOrWhiteSpace(dto.Icono) ? null : dto.Icono.Trim(),
            ParentId = dto.ParentId,
            Orden = dto.Orden,
            Activo = dto.Activo,
        };
        _menuRepo.Add(e);
        await _menuRepo.SaveChangesAsync(ct);
        return (MapDto(e), null);
    }

    public async Task<string?> UpdateAsync(int id, MenuOpcionSaveDto dto, CancellationToken ct)
    {
        var e = await _menuRepo.GetByIdTrackingAsync(id, ct);
        if (e is null) return "NOT_FOUND";
        var err = await ValidateSaveAsync(dto, excludeId: id, ct);
        if (err is not null) return err;
        if (dto.ParentId == id) return "SELF_PARENT";
        e.Codigo = dto.Codigo.Trim().ToUpperInvariant();
        e.Nombre = dto.Nombre.Trim();
        e.Ruta = string.IsNullOrWhiteSpace(dto.Ruta) ? null : dto.Ruta.Trim();
        e.Icono = string.IsNullOrWhiteSpace(dto.Icono) ? null : dto.Icono.Trim();
        e.ParentId = dto.ParentId;
        e.Orden = dto.Orden;
        e.Activo = dto.Activo;
        await _menuRepo.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> DeleteAsync(int id, CancellationToken ct)
    {
        var e = await _menuRepo.GetByIdWithHijosTrackingAsync(id, ct);
        if (e is null) return "NOT_FOUND";
        if (e.Hijos.Count > 0) return "HAS_CHILDREN";
        var permisos = await _permRepo.ListTrackedByMenuOpcionIdAsync(id, ct);
        if (permisos.Count > 0)
            _permRepo.RemoveRange(permisos);
        _menuRepo.Remove(e);
        await _menuRepo.SaveChangesAsync(ct);
        return null;
    }

    private async Task<string?> ValidateSaveAsync(MenuOpcionSaveDto dto, int? excludeId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo)) return "El código es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return "El nombre es obligatorio.";
        var cod = dto.Codigo.Trim().ToUpperInvariant();
        if (await _menuRepo.CodigoExistsExcludingIdAsync(cod, excludeId, ct))
            return "Ya existe otra opción con ese código.";
        if (dto.ParentId is { } pid && !await _menuRepo.ExistsByIdAsync(pid, ct))
            return "El padre indicado no existe.";
        return null;
    }

    private static MenuOpcionDto MapDto(MenuOpcion m) =>
        new(m.Id, m.Codigo, m.Nombre, m.Ruta, m.Icono, m.ParentId, m.Orden, m.Activo);
}
