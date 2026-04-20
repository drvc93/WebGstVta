using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class PaisesService : MaestroServiceBase<Pais>
{
    public PaisesService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Pais> Set => Db.Paises;
    protected override IQueryable<Pais> OrderedQuery(IQueryable<Pais> query) => query.OrderBy(e => e.Nombre).ThenBy(e => e.Codigo);
}
