namespace GestVta.Services.Dtos;

public sealed record UsuarioListDto(
    int Id,
    string Username,
    string NombreMostrar,
    bool Activo,
    DateTime FechaCreacion,
    int? CompaniaId,
    IReadOnlyList<int> CompaniaIds,
    IReadOnlyList<string> RolCodigos);

public sealed record UsuarioDetailDto(
    int Id,
    string Username,
    string NombreMostrar,
    bool Activo,
    DateTime FechaCreacion,
    int? CompaniaId,
    IReadOnlyList<int> CompaniaIds,
    IReadOnlyList<int> RolIds);

public sealed record UsuarioSaveDto(
    string Username,
    string? Password,
    string NombreMostrar,
    bool Activo,
    IReadOnlyList<int> CompaniaIds,
    IReadOnlyList<int> RolIds);
