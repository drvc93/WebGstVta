using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public sealed class TiposDocumentoService : MaestroServiceBase<TipoDocumento>
{
    public TiposDocumentoService(ApplicationDbContext db) : base(db)
    {
    }

    protected override DbSet<TipoDocumento> Set => Db.TiposDocumento;
    protected override IQueryable<TipoDocumento> OrderedQuery(IQueryable<TipoDocumento> query) => query.OrderBy(e => e.Codigo);

    public override async Task<CrudCreateResult<TipoDocumento>> CreateAsync(TipoDocumento entity, CancellationToken ct)
    {
        var codigo = (entity.Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
            return new CrudCreateResult<TipoDocumento>(null, "El código de tipo documento es obligatorio.");

        var exists = await Db.TiposDocumento.AsNoTracking().AnyAsync(m => m.Codigo.ToUpper() == codigo, ct);
        if (exists)
            return new CrudCreateResult<TipoDocumento>(null, $"Ya existe un tipo documento con el código '{codigo}'.");

        entity.Codigo = codigo;
        entity.Nombre = (entity.Nombre ?? string.Empty).Trim();
        return await base.CreateAsync(entity, ct);
    }

    public override async Task<CrudUpdateResult> UpdateAsync(int id, TipoDocumento dto, CancellationToken ct)
    {
        var codigo = (dto.Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
            return new CrudUpdateResult(NotFound: false, BadRequest: "El código de tipo documento es obligatorio.");

        var dup = await Db.TiposDocumento.AsNoTracking().AnyAsync(m => m.Id != id && m.Codigo.ToUpper() == codigo, ct);
        if (dup)
            return new CrudUpdateResult(NotFound: false, BadRequest: $"Ya existe un tipo documento con el código '{codigo}'.");

        dto.Codigo = codigo;
        dto.Nombre = (dto.Nombre ?? string.Empty).Trim();
        return await base.UpdateAsync(id, dto, ct);
    }
}
