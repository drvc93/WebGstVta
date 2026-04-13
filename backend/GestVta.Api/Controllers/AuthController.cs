using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GestVta.Api.Data;
using GestVta.Api.Infrastructure;
using GestVta.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext db, JwtTokenService jwt) : ControllerBase
{
    public record LoginRequest(string Username, string Password, int? CompaniaId);

    public record CompaniaLoginOption(int Id, string Codigo, string Nombre, string? ColorPrimario);

    /// <summary>
    /// Si <see cref="RequiresCompaniaSelection"/> es true, no hay token: el cliente debe llamar de nuevo con <see cref="LoginRequest.CompaniaId"/>.
    /// </summary>
    public record LoginResponse(
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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Usuario y contraseña requeridos.");

        var name = req.Username.Trim().ToUpperInvariant();
        var user = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Username.ToUpper() == name, ct);

        if (user is null || !user.Activo)
            return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var roles = user.UsuarioRoles.Where(ur => ur.Rol != null && ur.Rol.Activo).Select(ur => ur.Rol!.Codigo).ToList();
        var opciones = BuildCompaniaOptions(user);

        if (opciones.Count > 1)
        {
            if (req.CompaniaId is not { } elegida || opciones.All(o => o.Id != elegida))
            {
                return Ok(new LoginResponse(
                    RequiresCompaniaSelection: true,
                    CompaniasDisponibles: opciones,
                    AccessToken: null,
                    ExpiresAt: null,
                    TokenType: null,
                    Username: user.Username,
                    NombreMostrar: user.NombreMostrar,
                    Roles: null,
                    CompaniaId: null,
                    CompaniaNombre: null,
                    ColorPrimario: null));
            }

            return Ok(BuildSuccessResponse(user, roles, opciones.First(o => o.Id == elegida)));
        }

        var unica = opciones.Count == 1 ? opciones[0] : null;
        return Ok(BuildSuccessResponse(user, roles, unica));
    }

    public record CambiarCompaniaRequest(int CompaniaId);

    /// <summary>Compañías asociadas al usuario autenticado (mismo orden que en el login).</summary>
    [HttpGet("mis-companias")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CompaniaLoginOption>>> MisCompanias(CancellationToken ct)
    {
        var userId = ParseUserId(User);
        if (userId is null) return Unauthorized();

        var user = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null) return Unauthorized();
        return Ok(BuildCompaniaOptions(user));
    }

    /// <summary>Emite un nuevo JWT con otra <c>compania_id</c> sin cerrar sesión.</summary>
    [HttpPost("cambiar-compania")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> CambiarCompania([FromBody] CambiarCompaniaRequest req, CancellationToken ct)
    {
        var userId = ParseUserId(User);
        if (userId is null) return Unauthorized();

        var user = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null) return Unauthorized();

        var opciones = BuildCompaniaOptions(user);
        if (opciones.Count == 0)
            return BadRequest("No tiene compañías asignadas.");
        if (opciones.All(o => o.Id != req.CompaniaId))
            return BadRequest("La compañía indicada no está asociada a su usuario.");

        var elegida = opciones.First(o => o.Id == req.CompaniaId);
        var roles = user.UsuarioRoles.Where(ur => ur.Rol != null && ur.Rol.Activo).Select(ur => ur.Rol!.Codigo).ToList();
        return Ok(BuildSuccessResponse(user, roles, elegida));
    }

    private static int? ParseUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    private LoginResponse BuildSuccessResponse(Usuario user, IReadOnlyList<string> roles, CompaniaLoginOption? elegida)
    {
        var color = elegida?.ColorPrimario;
        if (string.IsNullOrEmpty(color) || !IsHexColor(color))
            color = "#1a3a5c";

        var companiaNombre = elegida?.Nombre;
        var companiaId = elegida?.Id;
        var issued = jwt.CreateAccessToken(user, roles, color!, companiaNombre, companiaId);

        return new LoginResponse(
            RequiresCompaniaSelection: false,
            CompaniasDisponibles: null,
            AccessToken: issued.Token,
            ExpiresAt: issued.ExpiresAtUtc,
            TokenType: "Bearer",
            Username: user.Username,
            NombreMostrar: user.NombreMostrar,
            Roles: roles,
            CompaniaId: companiaId,
            CompaniaNombre: companiaNombre,
            ColorPrimario: color);
    }

    private static IReadOnlyList<CompaniaLoginOption> BuildCompaniaOptions(Usuario user)
    {
        var ids = OrderCompaniaIds(user);
        if (ids.Count == 0)
            return Array.Empty<CompaniaLoginOption>();

        var byId = new Dictionary<int, Compania>();
        if (user.Compania != null)
            byId[user.Compania.Id] = user.Compania;
        foreach (var uc in user.UsuarioCompanias)
        {
            if (uc.Compania != null)
                byId[uc.Compania.Id] = uc.Compania;
        }

        return ids
            .Where(id => byId.ContainsKey(id))
            .Select(id =>
            {
                var c = byId[id];
                return new CompaniaLoginOption(c.Id, c.Codigo, c.Nombre, c.ColorPrimario?.Trim());
            })
            .ToList();
    }

    /// <summary>Primero la compañía por defecto (<see cref="Usuario.CompaniaId"/>), luego el resto sin duplicar.</summary>
    private static List<int> OrderCompaniaIds(Usuario u)
    {
        var list = new List<int>();
        if (u.CompaniaId is { } def)
            list.Add(def);
        foreach (var cid in u.UsuarioCompanias.OrderBy(x => x.CompaniaId).Select(x => x.CompaniaId))
        {
            if (!list.Contains(cid))
                list.Add(cid);
        }

        return list;
    }

    private static bool IsHexColor(string s) =>
        s.Length == 7 && s[0] == '#' && s[1..].All(c => char.IsAsciiHexDigit(c));
}
