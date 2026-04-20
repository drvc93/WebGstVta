using System.Security.Claims;
using GestVta.Services;
using GestVta.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/menu-opciones")]
[Authorize]
public sealed class MenuOpcionesController : ControllerBase
{
    private readonly IMenuOpcionesService _menuService;

    public MenuOpcionesController(IMenuOpcionesService menuService)
    {
        _menuService = menuService;
    }

    private static bool EsAdmin(ClaimsPrincipal u) => u.IsInRole("ADMIN");

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MenuOpcionDto>>> GetAll(CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        return Ok(await _menuService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuOpcionDto>> GetById(int id, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var m = await _menuService.GetByIdAsync(id, ct);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpPost]
    public async Task<ActionResult<MenuOpcionDto>> Create([FromBody] MenuOpcionSaveDto dto, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var (created, err) = await _menuService.CreateAsync(dto, ct);
        if (err is not null) return BadRequest(err);
        return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuOpcionSaveDto dto, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var err = await _menuService.UpdateAsync(id, dto, ct);
        if (err == "NOT_FOUND") return NotFound();
        if (err == "SELF_PARENT") return BadRequest("La opción no puede ser padre de sí misma.");
        if (err is not null) return BadRequest(err);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var err = await _menuService.DeleteAsync(id, ct);
        if (err == "NOT_FOUND") return NotFound();
        if (err == "HAS_CHILDREN") return BadRequest("No se puede eliminar: tiene sub-opciones.");
        if (err is not null) return BadRequest(err);
        return NoContent();
    }
}
