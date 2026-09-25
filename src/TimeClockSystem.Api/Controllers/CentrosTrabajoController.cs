using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.CentrosTrabajo;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

/// <summary>Centros de Trabajo (contracts/centros-trabajo.md). Requiere rol Administrador.</summary>
[ApiController]
[Route("api/centros-trabajo")]
[Authorize(Roles = Roles.Administrador)]
public class CentrosTrabajoController(CentrosTrabajoService centros) : ControllerBase
{
    /// <summary>Lista todos los centros de trabajo.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CentroTrabajoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await centros.ListarAsync(cancellationToken));

    /// <summary>Obtiene un centro de trabajo por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CentroTrabajoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken cancellationToken)
    {
        var centro = await centros.ObtenerAsync(id, cancellationToken);
        return centro is null ? NotFound() : Ok(centro);
    }

    /// <summary>Crea un centro de trabajo.</summary>
    [HttpPost]
    [ProducesResponseType<CentroTrabajoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(GuardarCentroTrabajoRequest request, CancellationToken cancellationToken)
    {
        var centro = await centros.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = centro.Id }, centro);
    }

    /// <summary>Actualiza un centro de trabajo existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<CentroTrabajoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, GuardarCentroTrabajoRequest request, CancellationToken cancellationToken)
    {
        var centro = await centros.ActualizarAsync(id, request, cancellationToken);
        return centro is null ? NotFound() : Ok(centro);
    }

    /// <summary>Elimina un centro de trabajo, si no tiene empleados asignados.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await centros.EliminarAsync(id, cancellationToken);
        return resultado switch
        {
            EliminarCentroTrabajoResultado.Eliminado => NoContent(),
            EliminarCentroTrabajoResultado.NoEncontrado => NotFound(),
            EliminarCentroTrabajoResultado.TieneEmpleadosAsignados => Conflict("El centro de trabajo tiene empleados asignados."),
            _ => Problem(),
        };
    }
}
