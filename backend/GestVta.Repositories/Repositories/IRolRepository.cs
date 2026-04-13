namespace GestVta.Repositories;

public interface IRolRepository
{
    Task<bool> ExistsByIdAsync(int rolId, CancellationToken ct);
}
