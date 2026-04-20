using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class MonedasService : MaestroServiceBase<Moneda>
{
    public MonedasService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<Moneda> Set => Db.Monedas;
    protected override IQueryable<Moneda> OrderedQuery(IQueryable<Moneda> query) => query.OrderBy(e => e.Codigo);

    public override async Task<CrudCreateResult<Moneda>> CreateAsync(Moneda entity, CancellationToken ct)
    {
        var codigo = (entity.Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
            return new CrudCreateResult<Moneda>(null, "El código de moneda es obligatorio.");

        var exists = await Db.Monedas.AsNoTracking().AnyAsync(m => m.Codigo.ToUpper() == codigo, ct);
        if (exists)
            return new CrudCreateResult<Moneda>(null, $"Ya existe una moneda con el código '{codigo}'.");

        entity.Codigo = codigo;
        return await base.CreateAsync(entity, ct);
    }

    public override async Task<CrudUpdateResult> UpdateAsync(int id, Moneda dto, CancellationToken ct)
    {
        var codigo = (dto.Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
            return new CrudUpdateResult(NotFound: false, BadRequest: "El código de moneda es obligatorio.");

        var dup = await Db.Monedas.AsNoTracking().AnyAsync(m => m.Id != id && m.Codigo.ToUpper() == codigo, ct);
        if (dup)
            return new CrudUpdateResult(NotFound: false, BadRequest: $"Ya existe una moneda con el código '{codigo}'.");

        dto.Codigo = codigo;
        return await base.UpdateAsync(id, dto, ct);
    }
}
