namespace GestVta.Services.Dtos;

public sealed record LoginRequest(string Username, string Password, int? CompaniaId);

public sealed record CompaniaLoginOption(int Id, string Codigo, string Nombre, string? ColorPrimario);

public sealed record LoginResponse(
    bool RequiresCompaniaSelection,
    IReadOnlyList<CompaniaLoginOption>? CompaniasDisponibles,
    string? AccessToken,
    DateTime? ExpiresAt,
    string? TokenType,
    string? Username,
    string? NombreMostrar,
    IReadOnlyList<string>? Roles,
    int? CompaniaId,
    string? CompaniaNombre,
    string? ColorPrimario);

public sealed record CambiarCompaniaRequest(int CompaniaId);
