using GestVta.Api.Models;
using GestVta.Services.Maestros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tipos-cambio")]
public sealed class TiposCambioController : ControllerBase
{
    private readonly ITiposCambioService _tiposCambio;

    public TiposCambioController(ITiposCambioService tiposCambio)
    {
        _tiposCambio = tiposCambio;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoCambio>>> GetAll(CancellationToken ct) => Ok(await _tiposCambio.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoCambio>> GetById(int id, CancellationToken ct)
    {
        var e = await _tiposCambio.GetByIdAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<TipoCambio>> Create([FromBody] TipoCambio entity, CancellationToken ct)
    {
        var r = await _tiposCambio.CreateAsync(entity, ct);
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return CreatedAtAction(nameof(GetById), new { id = r.Entity!.Id }, r.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TipoCambio dto, CancellationToken ct)
    {
        var r = await _tiposCambio.UpdateAsync(id, dto, ct);
        if (r.NotFound) return NotFound();
        if (r.BadRequest is not null) return BadRequest(r.BadRequest);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await _tiposCambio.DeleteAsync(id, ct);
        return r.NotFound ? NotFound() : NoContent();
    }

    [HttpPost("ensure-hoy")]
    public async Task<ActionResult<object>> EnsureHoy(CancellationToken ct)
    {
        var r = await _tiposCambio.EnsureUsdHoyAsync(ct);
        if (!r.ok) return BadRequest(r.badRequest);
        return Ok(r.body);
    }
}
