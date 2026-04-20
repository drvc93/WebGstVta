using GestVta.Services.Dtos;
using GestVta.Repositories;

namespace GestVta.Services;

public sealed class MenuUsuarioArbolService : IMenuUsuarioArbolService
{
    private readonly IUsuarioRolRepository _usuarioRolRepo;
    private readonly IMenuOpcionRepository _menuRepo;
    private readonly IRolMenuPermisoRepository _permRepo;

    public MenuUsuarioArbolService(
        IUsuarioRolRepository usuarioRolRepo,
        IMenuOpcionRepository menuRepo,
        IRolMenuPermisoRepository permRepo)
    {
        _usuarioRolRepo = usuarioRolRepo;
        _menuRepo = menuRepo;
        _permRepo = permRepo;
    }

    public async Task<IReadOnlyList<MenuOpcionUsuarioDto>> GetMiArbolAsync(int usuarioId, CancellationToken ct)
    {
        var roleIds = await _usuarioRolRepo.GetRolIdsByUsuarioIdAsync(usuarioId, ct);
        if (roleIds.Count == 0) return Array.Empty<MenuOpcionUsuarioDto>();
        var menus = (await _menuRepo.ListActivosOrderedNoTrackingAsync(ct)).ToList();
        var perms = await _permRepo.ListByRolIdsNoTrackingAsync(roleIds, ct);
        var merged = new Dictionary<int, (bool L, bool E, bool M, bool D)>();
        foreach (var m in menus)
            merged[m.Id] = (false, false, false, false);
        foreach (var p in perms)
        {
            if (!merged.ContainsKey(p.MenuOpcionId)) continue;
            var x = merged[p.MenuOpcionId];
            if (p.PuedeLeer) x.L = true;
            if (p.PuedeEscribir) x.E = true;
            if (p.PuedeModificar) x.M = true;
            if (p.PuedeEliminar) x.D = true;
            merged[p.MenuOpcionId] = x;
        }

        var byId = menus.ToDictionary(m => m.Id);
        var visible = new HashSet<int>();
        foreach (var m in menus)
        {
            if (string.IsNullOrEmpty(m.Ruta)) continue;
            var x = merged[m.Id];
            if (!x.L) continue;
            visible.Add(m.Id);
            var cur = m;
            while (cur.ParentId is { } pid && byId.TryGetValue(pid, out var parent))
            {
                visible.Add(pid);
                cur = parent;
            }
        }

        return menus
            .Where(m => visible.Contains(m.Id))
            .Select(m =>
            {
                var x = merged[m.Id];
                return new MenuOpcionUsuarioDto(m.Id, m.Codigo, m.Nombre, m.Ruta, m.Icono, m.ParentId, m.Orden, x.L, x.E, x.M, x.D);
            })
            .ToList();
    }
}
