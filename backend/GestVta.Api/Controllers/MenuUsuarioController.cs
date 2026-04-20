using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GestVta.Services;
using GestVta.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/menu-usuario")]
[Authorize]
public sealed class MenuUsuarioController : ControllerBase
{
    private readonly IMenuUsuarioArbolService _arbolService;

    public MenuUsuarioController(IMenuUsuarioArbolService arbolService)
    {
        _arbolService = arbolService;
    }

    [HttpGet("mi-arbol")]
    public async Task<ActionResult<IReadOnlyList<MenuOpcionUsuarioDto>>> MiArbol(CancellationToken ct)
    {
        var userId = ParseUserId(User);
        if (userId is null) return Unauthorized();
        var result = await _arbolService.GetMiArbolAsync(userId.Value, ct);
        return Ok(result);
    }

    private static int? ParseUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}
