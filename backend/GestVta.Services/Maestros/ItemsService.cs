using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ItemsService : MaestroServiceBase<Item>
{
    public ItemsService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Item> Set => Db.Items;
    protected override IQueryable<Item> OrderedQuery(IQueryable<Item> query) => query.OrderBy(e => e.Codigo);
}
