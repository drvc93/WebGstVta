using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class ProcesosService : MaestroServiceBase<Proceso>
{
    public ProcesosService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Proceso> Set => Db.Procesos;
    protected override IQueryable<Proceso> OrderedQuery(IQueryable<Proceso> query) => query.OrderBy(e => e.Codigo);
}
