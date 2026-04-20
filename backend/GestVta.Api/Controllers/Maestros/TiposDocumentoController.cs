using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tipos-documento")]
public sealed class TiposDocumentoController : ControllerBase
{
    private readonly IMaestroService<TipoDocumento> _crud;

    public TiposDocumentoController(IMaestroService<TipoDocumento> crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoDocumento>>> GetAll(CancellationToken ct) => Ok(await _crud.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoDocumento>> GetById(int id, CancellationToken ct)
    {
        var e = await _crud.GetByIdAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<TipoDocumento>> Create([FromBody] TipoDocumento entity, CancellationToken ct)
    {
        var r = await _crud.CreateAsync(entity, ct);
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return CreatedAtAction(nameof(GetById), new { id = r.Entity!.Id }, r.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TipoDocumento dto, CancellationToken ct)
    {
        var r = await _crud.UpdateAsync(id, dto, ct);
        if (r.NotFound) return NotFound();
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _crud.DeleteAsync(id, ct);
        return r.NotFound ? NotFound() : NoContent();
    }
}
