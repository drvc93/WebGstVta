using System.Security.Claims;
using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IAuthService
{
    Task<(LoginResponse? response, string? error, bool unauthorized)> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<(IReadOnlyList<CompaniaLoginOption>? companias, bool unauthorized)> MisCompaniasAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<(LoginResponse? response, string? error, bool unauthorized)> CambiarCompaniaAsync(ClaimsPrincipal user, CambiarCompaniaRequest req, CancellationToken ct);
}
