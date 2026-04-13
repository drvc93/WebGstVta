using GestVta.Api.Models;

namespace GestVta.Repositories;

public interface IMenuOpcionRepository
{
    Task<IReadOnlyList<MenuOpcion>> ListOrderedNoTrackingAsync(CancellationToken ct);
    Task<IReadOnlyList<MenuOpcion>> ListActivosOrderedNoTrackingAsync(CancellationToken ct);
    Task<MenuOpcion?> GetByIdNoTrackingAsync(int id, CancellationToken ct);
    Task<MenuOpcion?> GetByIdTrackingAsync(int id, CancellationToken ct);
    Task<MenuOpcion?> GetByIdWithHijosTrackingAsync(int id, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int id, CancellationToken ct);
    Task<bool> CodigoExistsExcludingIdAsync(string codigo, int? excludeId, CancellationToken ct);
    Task<IReadOnlyList<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
    void Add(MenuOpcion entity);
    void Remove(MenuOpcion entity);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
