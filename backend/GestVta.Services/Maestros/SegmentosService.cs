using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class SegmentosService : MaestroServiceBase<Segmento>
{
    public SegmentosService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Segmento> Set => Db.Segmentos;
    protected override IQueryable<Segmento> OrderedQuery(IQueryable<Segmento> query) => query.OrderBy(e => e.Codigo);
}
