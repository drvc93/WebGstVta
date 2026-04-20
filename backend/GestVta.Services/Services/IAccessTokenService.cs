using GestVta.Api.Models;
using GestVta.Services.Dtos;

namespace GestVta.Services;

public interface IAccessTokenService
{
    AccessTokenResult CreateAccessToken(
        Usuario user,
        IReadOnlyList<string> roles,
        string colorPrimario,
        string? companiaNombre,
        int? companiaIdEfectiva = null);
}
