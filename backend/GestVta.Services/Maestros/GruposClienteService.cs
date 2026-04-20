using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class GruposClienteService : MaestroServiceBase<GrupoCliente>
{
    public GruposClienteService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<GrupoCliente> Set => Db.GruposCliente;
    protected override IQueryable<GrupoCliente> OrderedQuery(IQueryable<GrupoCliente> query) => query.OrderBy(e => e.Codigo);
}
