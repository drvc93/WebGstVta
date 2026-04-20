using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ClientesService : MaestroServiceBase<Cliente>
{
    public ClientesService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Cliente> Set => Db.Clientes;
    protected override IQueryable<Cliente> OrderedQuery(IQueryable<Cliente> query) => query.OrderBy(e => e.Codigo);
}
