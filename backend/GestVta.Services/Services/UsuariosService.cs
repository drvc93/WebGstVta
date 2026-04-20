using GestVta.Api.Data;
using GestVta.Api.Models;
using GestVta.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Services;

public sealed class UsuariosService : IUsuariosService
{
    private readonly ApplicationDbContext _db;

    public UsuariosService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UsuarioListDto>> GetAllAsync(CancellationToken ct)
    {
        var rows = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .Include(u => u.UsuarioCompanias)
            .OrderBy(u => u.Username)
            .ToListAsync(ct);

        return rows.Select(MapList).ToList();
    }

    public async Task<UsuarioDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
            .Include(x => x.UsuarioCompanias)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return u is null ? null : MapDetail(u);
    }

    public async Task<(UsuarioDetailDto? created, string? error)> CreateAsync(UsuarioSaveDto dto, CancellationToken ct)
    {
        var err = await ValidateSaveAsync(dto, excludeUserId: null, ct);
        if (err is not null) return (null, err);

        if (string.IsNullOrWhiteSpace(dto.Password))
            return (null, "La contraseña es obligatoria al crear un usuario.");

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

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync(ct);

        foreach (var cid in companiaIds)
            _db.UsuarioCompanias.Add(new UsuarioCompania { UsuarioId = user.Id, CompaniaId = cid });
        foreach (var rid in dto.RolIds.Distinct())
            _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
        await _db.SaveChangesAsync(ct);

        var created = await LoadDetailAsync(user.Id, ct);
        return (created is null ? null : MapDetail(created), null);
    }

    public async Task<string?> UpdateAsync(int id, UsuarioSaveDto dto, CancellationToken ct)
    {
        var user = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
            .Include(u => u.UsuarioCompanias)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return "NOT_FOUND";

        var err = await ValidateSaveAsync(dto, excludeUserId: id, ct);
        if (err is not null) return err;

        var companiaIds = dto.CompaniaIds.Distinct().ToList();
        user.Username = dto.Username.Trim().ToUpperInvariant();
        user.NombreMostrar = dto.NombreMostrar.Trim();
        user.Activo = dto.Activo;
        user.CompaniaId = companiaIds.Count > 0 ? companiaIds[0] : null;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        _db.UsuarioRoles.RemoveRange(user.UsuarioRoles);
        _db.UsuarioCompanias.RemoveRange(user.UsuarioCompanias);
        await _db.SaveChangesAsync(ct);

        foreach (var cid in companiaIds)
            _db.UsuarioCompanias.Add(new UsuarioCompania { UsuarioId = user.Id, CompaniaId = cid });
        foreach (var rid in dto.RolIds.Distinct())
            _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = user.Id, RolId = rid });
        await _db.SaveChangesAsync(ct);

        return null;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var user = await _db.Usuarios.FindAsync([id], ct);
        if (user is null) return false;

        _db.Usuarios.Remove(user);
        await _db.SaveChangesAsync(ct);
        return true;
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

    private async Task<Usuario?> LoadDetailAsync(int id, CancellationToken ct) =>
        await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .Include(u => u.UsuarioCompanias)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    private async Task<string?> ValidateSaveAsync(UsuarioSaveDto dto, int? excludeUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Username)) return "El usuario es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.NombreMostrar)) return "El nombre a mostrar es obligatorio.";

        var uname = dto.Username.Trim().ToUpperInvariant();
        var dupQuery = _db.Usuarios.Where(u => u.Username == uname);
        if (excludeUserId.HasValue)
            dupQuery = dupQuery.Where(u => u.Id != excludeUserId.Value);
        if (await dupQuery.AnyAsync(ct))
            return "Ya existe otro usuario con ese nombre de usuario.";

        var rolIds = dto.RolIds.Distinct().ToList();
        if (rolIds.Count == 0) return "Debe asignar al menos un rol.";

        var validRoles = await _db.Roles.Where(r => rolIds.Contains(r.Id)).CountAsync(ct);
        if (validRoles != rolIds.Count) return "Hay roles inválidos.";

        var cIds = dto.CompaniaIds.Distinct().ToList();
        if (cIds.Count > 0)
        {
            var validC = await _db.Companias.Where(c => cIds.Contains(c.Id)).CountAsync(ct);
            if (validC != cIds.Count) return "Hay compañías inválidas.";
        }

        return null;
    }
}
