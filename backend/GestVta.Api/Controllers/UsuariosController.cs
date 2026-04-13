using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsuariosController(ApplicationDbContext db) : ControllerBase
{
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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListDto>>> GetAll(CancellationToken ct)
    {
        var rows = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .Include(u => u.UsuarioCompanias)
            .OrderBy(u => u.Username)
            .ToListAsync(ct);

        return rows.Select(MapList).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetailDto>> GetById(int id, CancellationToken ct)
    {
        var u = await db.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
            .Include(x => x.UsuarioCompanias)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return u is null ? NotFound() : MapDetail(u);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDetailDto>> Create([FromBody] UsuarioSaveDto dto, CancellationToken ct)
    {
        var err = await ValidateSave(dto, excludeUserId: null, ct);
        if (err is not null) return BadRequest(err);

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("La contraseña es obligatoria al crear un usuario.");

        var companiaIds = dto.CompaniaIds.Distinct().ToList();
        var user = new Usuario
        {
            Username = dto.Username.Trim().ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            NombreMostrar = dto.NombreMostrar.Trim(),
            Activo = dto.Activo,
            FechaCreacion = DateTime.UtcNow,
            CompaniaId = companiaIds.Count > 0 ? companiaIds[0] : null,
        };

        db.Usuarios.Add(user);
        await db.SaveChangesAsync(ct);

        foreach (var cid in companiaIds)
            db.UsuarioCompanias.Add(new UsuarioCompania { UsuarioId = user.Id, CompaniaId = cid });
        foreach (var rid in dto.RolIds.Distinct())
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
        await db.SaveChangesAsync(ct);

        var created = await LoadDetail(user.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapDetail(created!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioSaveDto dto, CancellationToken ct)
    {
        var user = await db.Usuarios
            .Include(u => u.UsuarioRoles)
            .Include(u => u.UsuarioCompanias)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();

        var err = await ValidateSave(dto, excludeUserId: id, ct);
        if (err is not null) return BadRequest(err);

        var companiaIds = dto.CompaniaIds.Distinct().ToList();
        user.Username = dto.Username.Trim().ToUpperInvariant();
        user.NombreMostrar = dto.NombreMostrar.Trim();
        user.Activo = dto.Activo;
        user.CompaniaId = companiaIds.Count > 0 ? companiaIds[0] : null;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        db.UsuarioRoles.RemoveRange(user.UsuarioRoles);
        db.UsuarioCompanias.RemoveRange(user.UsuarioCompanias);
        await db.SaveChangesAsync(ct);

        foreach (var cid in companiaIds)
            db.UsuarioCompanias.Add(new UsuarioCompania { UsuarioId = user.Id, CompaniaId = cid });
        foreach (var rid in dto.RolIds.Distinct())
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await db.Usuarios.FindAsync([id], ct);
        if (user is null) return NotFound();
        db.Usuarios.Remove(user);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static UsuarioListDto MapList(Usuario u)
    {
        var cids = OrderCompaniaIds(u);
        var roles = u.UsuarioRoles.Where(ur => ur.Rol != null).Select(ur => ur.Rol!.Codigo).OrderBy(c => c).ToList();
        return new UsuarioListDto(
            u.Id,
            u.Username,
            u.NombreMostrar,
            u.Activo,
            u.FechaCreacion,
            u.CompaniaId,
            cids,
            roles);
    }

    private static UsuarioDetailDto MapDetail(Usuario u)
    {
        var cids = OrderCompaniaIds(u);
        var rolIds = u.UsuarioRoles.Select(ur => ur.RolId).OrderBy(x => x).ToList();
        return new UsuarioDetailDto(
            u.Id,
            u.Username,
            u.NombreMostrar,
            u.Activo,
            u.FechaCreacion,
            u.CompaniaId,
            cids,
            rolIds);
    }

    /// <summary>Primero la compañía por defecto (CompaniaId), luego el resto sin duplicar.</summary>
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

    private async Task<Usuario?> LoadDetail(int id, CancellationToken ct) =>
        await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .Include(u => u.UsuarioCompanias)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    private async Task<string?> ValidateSave(UsuarioSaveDto dto, int? excludeUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username)) return "El usuario es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.NombreMostrar)) return "El nombre a mostrar es obligatorio.";

        var uname = dto.Username.Trim().ToUpperInvariant();
        var dupQuery = db.Usuarios.Where(u => u.Username == uname);
        if (excludeUserId.HasValue)
            dupQuery = dupQuery.Where(u => u.Id != excludeUserId.Value);
        if (await dupQuery.AnyAsync(ct))
            return "Ya existe otro usuario con ese nombre de usuario.";

        var rolIds = dto.RolIds.Distinct().ToList();
        if (rolIds.Count == 0) return "Debe asignar al menos un rol.";

        var validRoles = await db.Roles.Where(r => rolIds.Contains(r.Id)).CountAsync(ct);
        if (validRoles != rolIds.Count) return "Hay roles inválidos.";

        var cIds = dto.CompaniaIds.Distinct().ToList();
        if (cIds.Count > 0)
        {
            var validC = await db.Companias.Where(c => cIds.Contains(c.Id)).CountAsync(ct);
            if (validC != cIds.Count) return "Hay compañías inválidas.";
        }

        return null;
    }
}
