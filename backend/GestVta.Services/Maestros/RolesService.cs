using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class RolesService : MaestroServiceBase<Rol>
{
    public RolesService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Rol> Set => Db.Roles;
    protected override IQueryable<Rol> OrderedQuery(IQueryable<Rol> query) => query.OrderBy(e => e.Codigo);
}
