using GestVta.Api.Infrastructure;
using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CompaniasController : ControllerBase
{
    private static readonly HashSet<string> LogoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg",
    };

    private readonly ICompaniasService _companias;
    private readonly FileStoragePaths _filePaths;

    public CompaniasController(ICompaniasService companias, FileStoragePaths filePaths)
    {
        _companias = companias;
        _filePaths = filePaths;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Compania>>> GetAll(CancellationToken ct) =>
        Ok(await _companias.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Compania>> GetById(int id, CancellationToken ct)
    {
        var e = await _companias.GetByIdAsync(id, ct);
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

        Directory.CreateDirectory(_filePaths.CompaniasPhysical);
        var safeName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var physical = Path.Combine(_filePaths.CompaniasPhysical, safeName);
        await using (var stream = System.IO.File.Create(physical))
            await file.CopyToAsync(stream, ct);

        var webPath = _filePaths.WebPathForCompaniaLogo(safeName);
        return Ok(new LogoUploadResponse(webPath));
    }

    /// <summary>Elimina un logo físico a partir de su ruta web (/files/companias/xxx.png).</summary>
    [HttpDelete("logo")]
    public IActionResult DeleteLogo([FromQuery] string path)
    {
        var raw = (path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return BadRequest(new { message = "Parámetro path requerido." });

        if (!raw.StartsWith(_filePaths.StaticRequestPath + "/companias/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Ruta de logo inválida." });

        var fileName = Path.GetFileName(raw.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest(new { message = "Ruta de logo inválida." });

        var physical = Path.Combine(_filePaths.CompaniasPhysical, fileName);
        if (System.IO.File.Exists(physical))
            System.IO.File.Delete(physical);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Compania>> Create([FromBody] Compania entity, CancellationToken ct)
    {
        var created = await _companias.CreateAsync(entity, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Compania dto, CancellationToken ct)
    {
        var r = await _companias.UpdateAsync(id, dto, ct);
        if (r.NotFound) return NotFound();
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _companias.DeleteAsync(id, ct);
        return r.NotFound ? NotFound() : NoContent();
    }

    public sealed record LogoUploadResponse(string Path);
}
