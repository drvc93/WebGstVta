using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ProveedoresService : MaestroServiceBase<Proveedor>
{
    public ProveedoresService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Proveedor> Set => Db.Proveedores;
    protected override IQueryable<Proveedor> OrderedQuery(IQueryable<Proveedor> query) => query.OrderBy(e => e.Codigo);
}
