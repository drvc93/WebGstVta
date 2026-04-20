using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class RptasSeguimientoService : MaestroServiceBase<RptaSeguimiento>
{
    public RptasSeguimientoService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<RptaSeguimiento> Set => Db.RptasSeguimiento;
    protected override IQueryable<RptaSeguimiento> OrderedQuery(IQueryable<RptaSeguimiento> query) => query.OrderBy(e => e.Codigo);
}
