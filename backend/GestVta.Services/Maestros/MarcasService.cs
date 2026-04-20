using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class MarcasService : MaestroServiceBase<Marca>
{
    public MarcasService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Marca> Set => Db.Marcas;
    protected override IQueryable<Marca> OrderedQuery(IQueryable<Marca> query) => query.OrderBy(e => e.Codigo);
}
