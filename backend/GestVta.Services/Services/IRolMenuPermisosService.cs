using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IRolMenuPermisosService
{
    Task<(IReadOnlyList<RolMenuPermisoFilaDto>? filas, bool rolNotFound)> GetPorRolAsync(int rolId, CancellationToken ct);
    Task<string?> GuardarAsync(int rolId, IReadOnlyList<RolMenuPermisoGuardarDto> filas, CancellationToken ct);
}
