using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class AdicionalesService : MaestroServiceBase<Adicional>
{
    public AdicionalesService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Adicional> Set => Db.Adicionales;
    protected override IQueryable<Adicional> OrderedQuery(IQueryable<Adicional> query) => query.OrderBy(e => e.Codigo);
}
