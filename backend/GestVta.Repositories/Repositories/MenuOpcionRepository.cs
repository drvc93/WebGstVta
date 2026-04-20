using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Repositories;

public sealed class MenuOpcionRepository : IMenuOpcionRepository
{
    private readonly ApplicationDbContext _db;

    public MenuOpcionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MenuOpcion>> ListOrderedNoTrackingAsync(CancellationToken ct) =>
        await _db.MenuOpciones.AsNoTracking().OrderBy(m => m.Orden).ThenBy(m => m.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<MenuOpcion>> ListActivosOrderedNoTrackingAsync(CancellationToken ct) =>
        await _db.MenuOpciones.AsNoTracking().Where(m => m.Activo).OrderBy(m => m.Orden).ThenBy(m => m.Id).ToListAsync(ct);

    public Task<MenuOpcion?> GetByIdNoTrackingAsync(int id, CancellationToken ct) =>
        _db.MenuOpciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<MenuOpcion?> GetByIdTrackingAsync(int id, CancellationToken ct) =>
        _db.MenuOpciones.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<MenuOpcion?> GetByIdWithHijosTrackingAsync(int id, CancellationToken ct) =>
        _db.MenuOpciones.Include(m => m.Hijos).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsByIdAsync(int id, CancellationToken ct) =>
        _db.MenuOpciones.AsNoTracking().AnyAsync(m => m.Id == id, ct);

    public Task<bool> CodigoExistsExcludingIdAsync(string codigo, int? excludeId, CancellationToken ct) =>
        _db.MenuOpciones.AsNoTracking().AnyAsync(m => m.Codigo == codigo && (!excludeId.HasValue || m.Id != excludeId.Value), ct);

    public async Task<IReadOnlyList<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return Array.Empty<int>();
        return await _db.MenuOpciones.AsNoTracking().Where(m => ids.Contains(m.Id)).Select(m => m.Id).ToListAsync(ct);
    }

    public void Add(MenuOpcion entity) => _db.MenuOpciones.Add(entity);

    public void Remove(MenuOpcion entity) => _db.MenuOpciones.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
