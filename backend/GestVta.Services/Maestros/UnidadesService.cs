using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class UnidadesService : MaestroServiceBase<Unidad>
{
    public UnidadesService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Unidad> Set => Db.Unidades;
    protected override IQueryable<Unidad> OrderedQuery(IQueryable<Unidad> query) => query.OrderBy(e => e.Codigo);
}
