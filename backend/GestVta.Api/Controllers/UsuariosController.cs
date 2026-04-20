using GestVta.Services;
using GestVta.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _usuariosService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetailDto>> GetById(int id, CancellationToken ct)
    {
        var u = await _usuariosService.GetByIdAsync(id, ct);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDetailDto>> Create([FromBody] UsuarioSaveDto dto, CancellationToken ct)
    {
        var (created, error) = await _usuariosService.CreateAsync(dto, ct);
        if (error is not null) return BadRequest(error);
        return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioSaveDto dto, CancellationToken ct)
    {
        var err = await _usuariosService.UpdateAsync(id, dto, ct);
        if (err == "NOT_FOUND") return NotFound();
        if (err is not null) return BadRequest(err);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _usuariosService.DeleteAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
