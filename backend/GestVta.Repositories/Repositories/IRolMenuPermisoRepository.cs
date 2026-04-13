using GestVta.Api.Models;

namespace GestVta.Repositories;

public interface IRolMenuPermisoRepository
{
    Task<IReadOnlyList<RolMenuPermiso>> ListByRolIdNoTrackingAsync(int rolId, CancellationToken ct);
    Task<IReadOnlyList<RolMenuPermiso>> ListByRolIdsNoTrackingAsync(IReadOnlyList<int> rolIds, CancellationToken ct);
    Task<IReadOnlyList<RolMenuPermiso>> ListTrackedByRolIdAsync(int rolId, CancellationToken ct);
    Task<IReadOnlyList<RolMenuPermiso>> ListTrackedByMenuOpcionIdAsync(int menuOpcionId, CancellationToken ct);
    void RemoveRange(IEnumerable<RolMenuPermiso> entities);
    void Add(RolMenuPermiso entity);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
