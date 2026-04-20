using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class UbigeosService : MaestroServiceBase<Ubigeo>
{
    public UbigeosService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Ubigeo> Set => Db.Ubigeos;
    protected override IQueryable<Ubigeo> OrderedQuery(IQueryable<Ubigeo> query) => query.OrderBy(e => e.Codigo);
}
