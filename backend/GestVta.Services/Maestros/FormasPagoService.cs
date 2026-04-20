using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class FormasPagoService : MaestroServiceBase<FormaPago>
{
    public FormasPagoService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<FormaPago> Set => Db.FormasPago;
    protected override IQueryable<FormaPago> OrderedQuery(IQueryable<FormaPago> query) => query.OrderBy(e => e.Codigo);
}
