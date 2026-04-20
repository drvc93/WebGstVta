using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IUsuariosService
{
    Task<IReadOnlyList<UsuarioListDto>> GetAllAsync(CancellationToken ct);
    Task<UsuarioDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(UsuarioDetailDto? created, string? error)> CreateAsync(UsuarioSaveDto dto, CancellationToken ct);
    Task<string?> UpdateAsync(int id, UsuarioSaveDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
