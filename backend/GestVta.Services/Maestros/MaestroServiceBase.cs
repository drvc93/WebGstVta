using GestVta.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services.Maestros;

public abstract class MaestroServiceBase<TEntity> : IMaestroService<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext Db;

    protected MaestroServiceBase(ApplicationDbContext db) => Db = db;

    protected abstract DbSet<TEntity> Set { get; }

    protected abstract IQueryable<TEntity> OrderedQuery(IQueryable<TEntity> query);

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken ct) =>
        await OrderedQuery(Set.AsNoTracking()).ToListAsync(ct);

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct) =>
        await Set.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id, ct);

    public virtual async Task<CrudCreateResult<TEntity>> CreateAsync(TEntity entity, CancellationToken ct)
    {
        var idProp = typeof(TEntity).GetProperty("Id");
        idProp?.SetValue(entity, 0);
        Set.Add(entity);
        await Db.SaveChangesAsync(ct);
        return new CrudCreateResult<TEntity>(entity, null);
    }

    public virtual async Task<CrudUpdateResult> UpdateAsync(int id, TEntity dto, CancellationToken ct)
    {
        var tracked = await Set.FindAsync([id], ct);
        if (tracked is null) return new CrudUpdateResult(NotFound: true, BadRequest: null);
        Db.Entry(tracked).CurrentValues.SetValues(dto);
        Db.Entry(tracked).Property("Id").CurrentValue = id;
        Db.Entry(tracked).Property("Id").IsModified = false;
        await Db.SaveChangesAsync(ct);
        return new CrudUpdateResult(NotFound: false, BadRequest: null);
    }

    public virtual async Task<CrudDeleteResult> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await Set.FindAsync([id], ct);
        if (entity is null) return new CrudDeleteResult(NotFound: true);
        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
        return new CrudDeleteResult(NotFound: false);
    }
}
