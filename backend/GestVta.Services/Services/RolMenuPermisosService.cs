using GestVta.Services.Dtos;
using GestVta.Api.Models;
using GestVta.Repositories;

namespace GestVta.Services;

public sealed class RolMenuPermisosService : IRolMenuPermisosService
{
    private readonly IRolRepository _rolRepo;
    private readonly IMenuOpcionRepository _menuRepo;
    private readonly IRolMenuPermisoRepository _permRepo;

    public RolMenuPermisosService(
        IRolRepository rolRepo,
        IMenuOpcionRepository menuRepo,
        IRolMenuPermisoRepository permRepo)
    {
        _rolRepo = rolRepo;
        _menuRepo = menuRepo;
        _permRepo = permRepo;
    }

    public async Task<(IReadOnlyList<RolMenuPermisoFilaDto>? filas, bool rolNotFound)> GetPorRolAsync(int rolId, CancellationToken ct)
    {
        if (!await _rolRepo.ExistsByIdAsync(rolId, ct))
            return (null, rolNotFound: true);
        var menus = await _menuRepo.ListOrderedNoTrackingAsync(ct);
        var perms = await _permRepo.ListByRolIdNoTrackingAsync(rolId, ct);
        var map = perms.ToDictionary(p => p.MenuOpcionId);
        var filas = menus.Select(m =>
        {
            map.TryGetValue(m.Id, out var p);
            return new RolMenuPermisoFilaDto(
                m.Id, m.Codigo, m.Nombre, m.Ruta, m.ParentId, m.Orden,
                p?.PuedeLeer ?? false, p?.PuedeEscribir ?? false, p?.PuedeModificar ?? false, p?.PuedeEliminar ?? false);
        }).ToList();
        return (filas, rolNotFound: false);
    }

    public async Task<string?> GuardarAsync(int rolId, IReadOnlyList<RolMenuPermisoGuardarDto> filas, CancellationToken ct)
    {
        if (!await _rolRepo.ExistsByIdAsync(rolId, ct))
            return "ROL_NOT_FOUND";
        var ids = filas.Select(f => f.MenuOpcionId).Distinct().ToList();
        var valid = await _menuRepo.GetExistingIdsAsync(ids, ct);
        if (valid.Count != ids.Count) return "INVALID_MENU_IDS";
        var existentes = await _permRepo.ListTrackedByRolIdAsync(rolId, ct);
        _permRepo.RemoveRange(existentes);
        foreach (var f in filas.DistinctBy(f => f.MenuOpcionId))
        {
            _permRepo.Add(new RolMenuPermiso
            {
                RolId = rolId,
                MenuOpcionId = f.MenuOpcionId,
                PuedeLeer = f.PuedeLeer,
                PuedeEscribir = f.PuedeEscribir,
                PuedeModificar = f.PuedeModificar,
                PuedeEliminar = f.PuedeEliminar,
            });
        }

        await _permRepo.SaveChangesAsync(ct);
        return null;
    }
}
