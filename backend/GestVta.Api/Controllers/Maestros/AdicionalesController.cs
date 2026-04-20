using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/adicionales")]
public sealed class AdicionalesController : ControllerBase
{
    private readonly IMaestroService<Adicional> _crud;

    public AdicionalesController(IMaestroService<Adicional> crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<List<Adicional>>> GetAll(CancellationToken ct) => Ok(await _crud.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Adicional>> GetById(int id, CancellationToken ct)
    {
        var e = await _crud.GetByIdAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<Adicional>> Create([FromBody] Adicional entity, CancellationToken ct)
    {
        var r = await _crud.CreateAsync(entity, ct);
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return CreatedAtAction(nameof(GetById), new { id = r.Entity!.Id }, r.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Adicional dto, CancellationToken ct)
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
