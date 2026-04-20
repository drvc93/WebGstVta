using GestVta.Api.Models;

namespace GestVta.Services.Maestros;

/// <summary>Operaciones estándar para entidades de maestros.</summary>
public interface IMaestroService<TEntity>
    where TEntity : class
{
    Task<List<TEntity>> GetAllAsync(CancellationToken ct);

    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct);

    Task<CrudCreateResult<TEntity>> CreateAsync(TEntity entity, CancellationToken ct);

    Task<CrudUpdateResult> UpdateAsync(int id, TEntity dto, CancellationToken ct);

    Task<CrudDeleteResult> DeleteAsync(int id, CancellationToken ct);
}

/// <summary>Tipo de cambio: maestro + sincronización USD del día vía API externa.</summary>
public interface ITiposCambioService : IMaestroService<TipoCambio>
{
    Task<(bool ok, object? body, string? badRequest)> EnsureUsdHoyAsync(CancellationToken ct);
}
