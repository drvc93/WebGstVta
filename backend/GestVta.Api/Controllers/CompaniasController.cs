using GestVta.Api.Data;
using GestVta.Api.Infrastructure;
using GestVta.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CompaniasController(ApplicationDbContext db, FileStoragePaths filePaths) : ControllerBase
{
    private static readonly HashSet<string> LogoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg",
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Compania>>> GetAll(CancellationToken ct)
    {
        return await db.Companias.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Compania>> GetById(int id, CancellationToken ct)
    {
        var e = await db.Companias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return e is null ? NotFound() : e;
    }

    /// <summary>Sube el logo de compañía a FileStorage:RootPath/companias y devuelve la ruta web para LogoPath.</summary>
    [HttpPost("logo")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<LogoUploadResponse>> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No se envió ningún archivo." });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !LogoExtensions.Contains(ext))
            return BadRequest(new { message = "Solo se permiten imágenes: png, jpg, jpeg, gif, webp, svg." });

        if (!string.IsNullOrEmpty(file.ContentType) && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "El contenido debe ser una imagen." });

        Directory.CreateDirectory(filePaths.CompaniasPhysical);
        var safeName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var physical = Path.Combine(filePaths.CompaniasPhysical, safeName);
        await using (var stream = System.IO.File.Create(physical))
            await file.CopyToAsync(stream, ct);

        var webPath = filePaths.WebPathForCompaniaLogo(safeName);
        return Ok(new LogoUploadResponse(webPath));
    }

    /// <summary>Elimina un logo físico a partir de su ruta web (/files/companias/xxx.png).</summary>
    [HttpDelete("logo")]
    public IActionResult DeleteLogo([FromQuery] string path)
    {
        var raw = (path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return BadRequest(new { message = "Parámetro path requerido." });

        if (!raw.StartsWith(filePaths.StaticRequestPath + "/companias/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Ruta de logo inválida." });

        var fileName = Path.GetFileName(raw.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest(new { message = "Ruta de logo inválida." });

        var physical = Path.Combine(filePaths.CompaniasPhysical, fileName);
        if (System.IO.File.Exists(physical))
            System.IO.File.Delete(physical);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Compania>> Create([FromBody] Compania entity, CancellationToken ct)
    {
        entity.Id = 0;
        entity.UltMod = DateTime.UtcNow;
        db.Companias.Add(entity);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Compania dto, CancellationToken ct)
    {
        var entity = await db.Companias.FindAsync([id], ct);
        if (entity is null) return NotFound();

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.TipoDocumentoId = dto.TipoDocumentoId;
        entity.NumeroDocumento = dto.NumeroDocumento;
        entity.Direccion = dto.Direccion;
        entity.PaisId = dto.PaisId;
        entity.UbigeoId = dto.UbigeoId;
        entity.Correo = dto.Correo;
        entity.Activo = dto.Activo;
        entity.LogoPath = dto.LogoPath;
        entity.ColorPrimario = dto.ColorPrimario;
        entity.Telefono1 = dto.Telefono1;
        entity.Telefono2 = dto.Telefono2;
        entity.UltUsuario = dto.UltUsuario;
        entity.UltMod = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await db.Companias.FindAsync([id], ct);
        if (entity is null) return NotFound();
        db.Companias.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    public sealed record LogoUploadResponse(string Path);
}
