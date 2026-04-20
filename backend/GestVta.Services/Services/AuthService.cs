using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GestVta.Api.Data;
using GestVta.Api.Models;
using GestVta.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IAccessTokenService _accessTokenService;

    public AuthService(ApplicationDbContext db, IAccessTokenService accessTokenService)
    {
        _db = db;
        _accessTokenService = accessTokenService;
    }

    public async Task<(LoginResponse? response, string? error, bool unauthorized)> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return (null, "Usuario y contraseña requeridos.", false);

        var name = req.Username.Trim().ToUpperInvariant();
        var user = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Username.ToUpper() == name, ct);

        if (user is null || !user.Activo)
            return (null, null, true);

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return (null, null, true);

        var roles = user.UsuarioRoles.Where(ur => ur.Rol != null && ur.Rol.Activo).Select(ur => ur.Rol!.Codigo).ToList();
        var opciones = BuildCompaniaOptions(user);

        if (opciones.Count > 1)
        {
            if (req.CompaniaId is not { } elegida || opciones.All(o => o.Id != elegida))
            {
                return (new LoginResponse(
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
                    ColorPrimario: null), null, false);
            }

            return (BuildSuccessResponse(user, roles, opciones.First(o => o.Id == elegida)), null, false);
        }

        var unica = opciones.Count == 1 ? opciones[0] : null;
        return (BuildSuccessResponse(user, roles, unica), null, false);
    }

    public async Task<(IReadOnlyList<CompaniaLoginOption>? companias, bool unauthorized)> MisCompaniasAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = ParseUserId(principal);
        if (userId is null) return (null, true);

        var user = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null) return (null, true);
        return (BuildCompaniaOptions(user), false);
    }

    public async Task<(LoginResponse? response, string? error, bool unauthorized)> CambiarCompaniaAsync(ClaimsPrincipal principal, CambiarCompaniaRequest req, CancellationToken ct)
    {
        var userId = ParseUserId(principal);
        if (userId is null) return (null, null, true);

        var user = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioCompanias).ThenInclude(uc => uc.Compania)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null) return (null, null, true);

        var opciones = BuildCompaniaOptions(user);
        if (opciones.Count == 0)
            return (null, "No tiene compañías asignadas.", false);
        if (opciones.All(o => o.Id != req.CompaniaId))
            return (null, "La compañía indicada no está asociada a su usuario.", false);

        var elegida = opciones.First(o => o.Id == req.CompaniaId);
        var roles = user.UsuarioRoles.Where(ur => ur.Rol != null && ur.Rol.Activo).Select(ur => ur.Rol!.Codigo).ToList();
        return (BuildSuccessResponse(user, roles, elegida), null, false);
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
        var issued = _accessTokenService.CreateAccessToken(user, roles, color!, companiaNombre, companiaId);

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
        s.Length == 7 && s[0] == '#' && s[1..].All(char.IsAsciiHexDigit);
}
