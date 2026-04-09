using GestVta.Api.Data;
using GestVta.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniasController(ApplicationDbContext db) : ControllerBase
{
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
}
