using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class AgenciasTransporteService : MaestroServiceBase<AgenciaTransporte>
{
    public AgenciasTransporteService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<AgenciaTransporte> Set => Db.AgenciasTransporte;
    protected override IQueryable<AgenciaTransporte> OrderedQuery(IQueryable<AgenciaTransporte> query) => query.OrderBy(e => e.Codigo);
}
