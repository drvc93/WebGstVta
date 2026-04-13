using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class RolRepository(ApplicationDbContext db) : IRolRepository
{
    public Task<bool> ExistsByIdAsync(int rolId, CancellationToken ct) =>
        db.Roles.AsNoTracking().AnyAsync(r => r.Id == rolId, ct);
}
