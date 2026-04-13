using System.Security.Claims;
using GestVta.Services;
using GestVta.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestVta.Api.Controllers;

[ApiController]
[Route("api/roles/{rolId:int}/permisos-menu")]
[Authorize]
public sealed class RolMenuPermisosController(IRolMenuPermisosService permisosService) : ControllerBase
{
    private static bool EsAdmin(ClaimsPrincipal u) => u.IsInRole("ADMIN");

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RolMenuPermisoFilaDto>>> GetPorRol(int rolId, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var (filas, rolNotFound) = await permisosService.GetPorRolAsync(rolId, ct);
        if (rolNotFound) return NotFound();
        return Ok(filas);
    }

    [HttpPut]
    public async Task<IActionResult> Guardar(int rolId, [FromBody] IReadOnlyList<RolMenuPermisoGuardarDto> filas, CancellationToken ct)
    {
        if (!EsAdmin(User)) return Forbid();
        var err = await permisosService.GuardarAsync(rolId, filas, ct);
        if (err == "ROL_NOT_FOUND") return NotFound();
        if (err == "INVALID_MENU_IDS") return BadRequest("Hay opciones de menú inválidas.");
        if (err is not null) return BadRequest(err);
        return NoContent();
    }
}
