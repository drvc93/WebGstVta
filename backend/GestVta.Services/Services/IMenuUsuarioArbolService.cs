using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IMenuUsuarioArbolService
{
    Task<IReadOnlyList<MenuOpcionUsuarioDto>> GetMiArbolAsync(int usuarioId, CancellationToken ct);
}
