using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class UsuarioRolRepository : IUsuarioRolRepository
{
    private readonly ApplicationDbContext _db;

    public UsuarioRolRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<int>> GetRolIdsByUsuarioIdAsync(int usuarioId, CancellationToken ct) =>
        await _db.UsuarioRoles.AsNoTracking().Where(ur => ur.UsuarioId == usuarioId).Select(ur => ur.RolId).ToListAsync(ct);
}
