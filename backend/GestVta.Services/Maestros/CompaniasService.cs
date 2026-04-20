using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public interface ICompaniasService
{
    Task<List<Compania>> GetAllAsync(CancellationToken ct);
    Task<Compania?> GetByIdAsync(int id, CancellationToken ct);
    Task<Compania> CreateAsync(Compania entity, CancellationToken ct);
    Task<CrudUpdateResult> UpdateAsync(int id, Compania dto, CancellationToken ct);
    Task<CrudDeleteResult> DeleteAsync(int id, CancellationToken ct);
}

public sealed class CompaniasService : ICompaniasService
{
    private readonly ApplicationDbContext _db;

    public CompaniasService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Compania>> GetAllAsync(CancellationToken ct) =>
        await _db.Companias.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync(ct);

    public async Task<Compania?> GetByIdAsync(int id, CancellationToken ct) =>
        await _db.Companias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Compania> CreateAsync(Compania entity, CancellationToken ct)
    {
        entity.Id = 0;
        entity.UltMod = DateTime.UtcNow;
        _db.Companias.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<CrudUpdateResult> UpdateAsync(int id, Compania dto, CancellationToken ct)
    {
        var entity = await _db.Companias.FindAsync([id], ct);
        if (entity is null) return new CrudUpdateResult(NotFound: true, BadRequest: null);

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.TipoDocumentoId = dto.TipoDocumentoId;
        entity.NumeroDocumento = dto.NumeroDocumento;
        entity.Direccion = dto.Direccion;
        entity.PaisId = dto.PaisId;
        entity.UbigeoId = dto.UbigeoId;
        entity.Correo = dto.Correo;
        entity.Activo = dto.Activo;
        entity.LogoPath = dto.LogoPath;
        entity.ColorPrimario = dto.ColorPrimario;
        entity.Telefono1 = dto.Telefono1;
        entity.Telefono2 = dto.Telefono2;
        entity.UltUsuario = dto.UltUsuario;
        entity.UltMod = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return new CrudUpdateResult(NotFound: false, BadRequest: null);
    }

    public async Task<CrudDeleteResult> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Companias.FindAsync([id], ct);
        if (entity is null) return new CrudDeleteResult(NotFound: true);
        _db.Companias.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return new CrudDeleteResult(NotFound: false);
    }
}
