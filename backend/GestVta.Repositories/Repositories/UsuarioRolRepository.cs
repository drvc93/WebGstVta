using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class UsuarioRolRepository(ApplicationDbContext db) : IUsuarioRolRepository
{
    public async Task<IReadOnlyList<int>> GetRolIdsByUsuarioIdAsync(int usuarioId, CancellationToken ct) =>
        await db.UsuarioRoles.AsNoTracking().Where(ur => ur.UsuarioId == usuarioId).Select(ur => ur.RolId).ToListAsync(ct);
}
