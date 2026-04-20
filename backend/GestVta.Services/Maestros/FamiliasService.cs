using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class FamiliasService : MaestroServiceBase<Familia>
{
    public FamiliasService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Familia> Set => Db.Familias;
    protected override IQueryable<Familia> OrderedQuery(IQueryable<Familia> query) => query.OrderBy(e => e.Codigo);
}
