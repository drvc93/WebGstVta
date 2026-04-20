using GestVta.Services;
using GestVta.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (response, error, unauthorized) = await _authService.LoginAsync(req, ct);
        if (unauthorized) return Unauthorized();
        if (error is not null) return BadRequest(error);
        return Ok(response);
    }

    /// <summary>Compañías asociadas al usuario autenticado (mismo orden que en el login).</summary>
    [HttpGet("mis-companias")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CompaniaLoginOption>>> MisCompanias(CancellationToken ct)
    {
        var (companias, unauthorized) = await _authService.MisCompaniasAsync(User, ct);
        if (unauthorized) return Unauthorized();
        return Ok(companias);
    }

    /// <summary>Emite un nuevo JWT con otra <c>compania_id</c> sin cerrar sesión.</summary>
    [HttpPost("cambiar-compania")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> CambiarCompania([FromBody] CambiarCompaniaRequest req, CancellationToken ct)
    {
        var (response, error, unauthorized) = await _authService.CambiarCompaniaAsync(User, req, ct);
        if (unauthorized) return Unauthorized();
        if (error is not null) return BadRequest(error);
        return Ok(response);
    }
}
