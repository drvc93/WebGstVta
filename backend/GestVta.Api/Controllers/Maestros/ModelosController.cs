using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/modelos")]
public sealed class ModelosController : ControllerBase
{
    private readonly IMaestroService<Modelo> _crud;

    public ModelosController(IMaestroService<Modelo> crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<List<Modelo>>> GetAll(CancellationToken ct) => Ok(await _crud.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Modelo>> GetById(int id, CancellationToken ct)
    {
        var e = await _crud.GetByIdAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<Modelo>> Create([FromBody] Modelo entity, CancellationToken ct)
    {
        var r = await _crud.CreateAsync(entity, ct);
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return CreatedAtAction(nameof(GetById), new { id = r.Entity!.Id }, r.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Modelo dto, CancellationToken ct)
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
