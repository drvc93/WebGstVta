using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class RolMenuPermisoRepository : IRolMenuPermisoRepository
{
    private readonly ApplicationDbContext _db;

    public RolMenuPermisoRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RolMenuPermiso>> ListByRolIdNoTrackingAsync(int rolId, CancellationToken ct) =>
        await _db.RolMenuPermisos.AsNoTracking().Where(p => p.RolId == rolId).ToListAsync(ct);

    public async Task<IReadOnlyList<RolMenuPermiso>> ListByRolIdsNoTrackingAsync(IReadOnlyList<int> rolIds, CancellationToken ct)
    {
        if (rolIds.Count == 0) return Array.Empty<RolMenuPermiso>();
        return await _db.RolMenuPermisos.AsNoTracking().Where(p => rolIds.Contains(p.RolId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RolMenuPermiso>> ListTrackedByRolIdAsync(int rolId, CancellationToken ct) =>
        await _db.RolMenuPermisos.Where(p => p.RolId == rolId).ToListAsync(ct);

    public async Task<IReadOnlyList<RolMenuPermiso>> ListTrackedByMenuOpcionIdAsync(int menuOpcionId, CancellationToken ct) =>
        await _db.RolMenuPermisos.Where(p => p.MenuOpcionId == menuOpcionId).ToListAsync(ct);

    public void RemoveRange(IEnumerable<RolMenuPermiso> entities) => _db.RolMenuPermisos.RemoveRange(entities);

    public void Add(RolMenuPermiso entity) => _db.RolMenuPermisos.Add(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
