using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ModelosService : MaestroServiceBase<Modelo>
{
    public ModelosService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Modelo> Set => Db.Modelos;
    protected override IQueryable<Modelo> OrderedQuery(IQueryable<Modelo> query) => query.OrderBy(e => e.Codigo);
}
