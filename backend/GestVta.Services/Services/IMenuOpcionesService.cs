using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IMenuOpcionesService
{
    Task<IReadOnlyList<MenuOpcionDto>> GetAllAsync(CancellationToken ct);
    Task<MenuOpcionDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(MenuOpcionDto? created, string? error)> CreateAsync(MenuOpcionSaveDto dto, CancellationToken ct);
    Task<string?> UpdateAsync(int id, MenuOpcionSaveDto dto, CancellationToken ct);
    Task<string?> DeleteAsync(int id, CancellationToken ct);
}
