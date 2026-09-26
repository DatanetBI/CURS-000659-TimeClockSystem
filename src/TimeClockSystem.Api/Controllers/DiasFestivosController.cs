using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.DiasFestivos;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

/// <summary>
/// Días Festivos (contracts/dias-festivos.md). Lectura para Administrador o Empleado; escritura
/// solo para Administrador.
/// </summary>
[ApiController]
[Route("api/dias-festivos")]
[Authorize]
public class DiasFestivosController(DiasFestivosService diasFestivos) : ControllerBase
{
    /// <summary>Lista los días festivos registrados (Administrador o Empleado).</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<DiaFestivoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await diasFestivos.ListarAsync(cancellationToken));

    /// <summary>Agrega un día festivo (Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<DiaFestivoDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(GuardarDiaFestivoRequest request, CancellationToken cancellationToken) =>
        Created(string.Empty, await diasFestivos.CrearAsync(request, cancellationToken));

    /// <summary>Actualiza un día festivo (Administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<DiaFestivoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, GuardarDiaFestivoRequest request, CancellationToken cancellationToken)
    {
        var diaFestivo = await diasFestivos.ActualizarAsync(id, request, cancellationToken);
        return diaFestivo is null ? NotFound() : Ok(diaFestivo);
    }

    /// <summary>Elimina un día festivo (Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        await diasFestivos.EliminarAsync(id, cancellationToken) ? NoContent() : NotFound();
}
