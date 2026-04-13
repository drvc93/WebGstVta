namespace GestVta.Repositories;

public interface IUsuarioRolRepository
{
    Task<IReadOnlyList<int>> GetRolIdsByUsuarioIdAsync(int usuarioId, CancellationToken ct);
}
