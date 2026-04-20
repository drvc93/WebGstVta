using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class RolRepository : IRolRepository
{
    private readonly ApplicationDbContext _db;

    public RolRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsByIdAsync(int rolId, CancellationToken ct) =>
        _db.Roles.AsNoTracking().AnyAsync(r => r.Id == rolId, ct);
}
