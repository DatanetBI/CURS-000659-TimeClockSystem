using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.Turnos;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

/// <summary>Turnos (contracts/turnos-y-asignaciones.md). Requiere rol Administrador.</summary>
[ApiController]
[Route("api/turnos")]
[Authorize(Roles = Roles.Administrador)]
public class TurnosController(TurnosService turnos) : ControllerBase
{
    /// <summary>Lista todos los turnos.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<TurnoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await turnos.ListarAsync(cancellationToken));

    /// <summary>Obtiene un turno por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TurnoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken cancellationToken)
    {
        var turno = await turnos.ObtenerAsync(id, cancellationToken);
        return turno is null ? NotFound() : Ok(turno);
    }

    /// <summary>Crea un turno.</summary>
    [HttpPost]
    [ProducesResponseType<TurnoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(GuardarTurnoRequest request, CancellationToken cancellationToken)
    {
        var turno = await turnos.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = turno.Id }, turno);
    }

    /// <summary>Actualiza un turno existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<TurnoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, GuardarTurnoRequest request, CancellationToken cancellationToken)
    {
        var turno = await turnos.ActualizarAsync(id, request, cancellationToken);
        return turno is null ? NotFound() : Ok(turno);
    }

    /// <summary>Elimina un turno, si no tiene asignaciones vigentes.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await turnos.EliminarAsync(id, cancellationToken);
        return resultado switch
        {
            EliminarTurnoResultado.Eliminado => NoContent(),
            EliminarTurnoResultado.NoEncontrado => NotFound(),
            EliminarTurnoResultado.TieneAsignacionesVigentes => Conflict("El turno tiene asignaciones vigentes."),
            _ => Problem(),
        };
    }
}
