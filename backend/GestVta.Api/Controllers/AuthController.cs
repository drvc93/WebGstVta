using GestVta.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext db) : ControllerBase
{
    public record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Usuario y contraseña requeridos.");

        var name = req.Username.Trim().ToUpperInvariant();
        var user = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Compania)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Username.ToUpper() == name, ct);

        if (user is null || !user.Activo)
            return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var roles = user.UsuarioRoles.Where(ur => ur.Rol != null && ur.Rol.Activo).Select(ur => ur.Rol!.Codigo).ToList();

        var color = user.Compania?.ColorPrimario?.Trim();
        if (string.IsNullOrEmpty(color) || !IsHexColor(color))
            color = "#1a3a5c";

        return Ok(new
        {
            username = user.Username,
            nombreMostrar = user.NombreMostrar,
            roles,
            companiaId = user.CompaniaId,
            companiaNombre = user.Compania?.Nombre,
            colorPrimario = color
        });
    }

    private static bool IsHexColor(string s) =>
        s.Length == 7 && s[0] == '#' && s[1..].All(c => char.IsAsciiHexDigit(c));
}
