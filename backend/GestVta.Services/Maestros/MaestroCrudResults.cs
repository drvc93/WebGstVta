namespace GestVta.Services.Maestros;

public readonly record struct CrudDeleteResult(bool NotFound);

public readonly record struct CrudUpdateResult(bool NotFound, string? BadRequest);

public readonly record struct CrudCreateResult<T>(T? Entity, string? BadRequest)
    where T : class;
