using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Application.Empleados;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

public record ActualizarCredencialRequest(string Pin);

/// <summary>Empleados (contracts/empleados.md). Todos los endpoints requieren rol Administrador.</summary>
[ApiController]
[Route("api/empleados")]
[Authorize(Roles = Roles.Administrador)]
public class EmpleadosController(EmpleadosService empleados) : ControllerBase
{
    /// <summary>Lista todos los empleados.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<EmpleadoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await empleados.ListarAsync(cancellationToken));

    /// <summary>Obtiene un empleado por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken cancellationToken)
    {
        var empleado = await empleados.ObtenerAsync(id, cancellationToken);
        return empleado is null ? NotFound() : Ok(empleado);
    }

    /// <summary>Crea un empleado.</summary>
    [HttpPost]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(CrearEmpleadoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empleado = await empleados.CrearAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { id = empleado.Id }, empleado);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Ya existe un empleado con ese número de empleado.");
        }
    }

    /// <summary>Actualiza un empleado existente (incluye baja lógica vía Estado).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, ActualizarEmpleadoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empleado = await empleados.ActualizarAsync(id, request, cancellationToken);
            return empleado is null ? NotFound() : Ok(empleado);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Ya existe un empleado con ese número de empleado.");
        }
    }

    /// <summary>Indica si el empleado ya tiene un PIN de marcaje configurado.</summary>
    [HttpGet("{id:int}/credencial")]
    [ProducesResponseType<CredencialInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerCredencial(int id, CancellationToken cancellationToken)
    {
        var credencial = await empleados.ObtenerCredencialAsync(id, cancellationToken);
        return credencial is null ? NotFound() : Ok(credencial);
    }

    /// <summary>Crea o actualiza el PIN de marcaje del empleado (se guarda hasheado).</summary>
    [HttpPut("{id:int}/credencial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarCredencial(int id, ActualizarCredencialRequest request, CancellationToken cancellationToken)
    {
        var actualizado = await empleados.ActualizarCredencialAsync(id, request.Pin, User.Identity?.Name, cancellationToken);
        return actualizado ? NoContent() : NotFound();
    }
}
