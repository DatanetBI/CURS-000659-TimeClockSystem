using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.Turnos;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

/// <summary>Asignaciones de Turno (contracts/turnos-y-asignaciones.md). Requiere rol Administrador.</summary>
[ApiController]
[Route("api/asignaciones-turno")]
[Authorize(Roles = Roles.Administrador)]
public class AsignacionesTurnoController(AsignacionesTurnoService asignaciones) : ControllerBase
{
    /// <summary>Lista asignaciones de turno, filtrando opcionalmente por empleado y/o fecha.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<AsignacionTurnoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? empleadoId, [FromQuery] DateOnly? fecha, CancellationToken cancellationToken) =>
        Ok(await asignaciones.ListarAsync(empleadoId, fecha, cancellationToken));

    /// <summary>Asigna un turno a un empleado en una fecha.</summary>
    [HttpPost]
    [ProducesResponseType<AsignacionTurnoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(CrearAsignacionTurnoRequest request, CancellationToken cancellationToken)
    {
        var asignacion = await asignaciones.CrearAsync(request, cancellationToken);
        return asignacion is null ? BadRequest("El empleado o el turno no existen.") : Created(string.Empty, asignacion);
    }

    /// <summary>Elimina una asignación de turno.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        await asignaciones.EliminarAsync(id, cancellationToken) ? NoContent() : NotFound();
}
