using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ConductoresService : MaestroServiceBase<Conductor>
{
    public ConductoresService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Conductor> Set => Db.Conductores;
    protected override IQueryable<Conductor> OrderedQuery(IQueryable<Conductor> query) => query.OrderBy(e => e.Codigo);
}
