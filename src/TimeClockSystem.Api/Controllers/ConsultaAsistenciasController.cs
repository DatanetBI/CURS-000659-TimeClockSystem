using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.ConsultaAsistencias;

namespace TimeClockSystem.Api.Controllers;

/// <summary>Consulta de Asistencias (contracts/consulta-asistencias.md).</summary>
[ApiController]
[Route("api/consulta-asistencias")]
[Authorize]
public class ConsultaAsistenciasController(ConsultarAsistenciasUseCase consultarAsistencias) : ControllerBase
{
    /// <summary>Consulta el historial de marcas de un empleado (con indicador de puntualidad).</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MarcaConsultaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Consultar(
        [FromQuery] int empleadoId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta,
        CancellationToken cancellationToken)
    {
        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var empleadoIdSesionClaim = User.FindFirst("empleadoId")?.Value;
        int? empleadoIdSesion = int.TryParse(empleadoIdSesionClaim, out var valor) ? valor : null;

        var (resultado, marcas) = await consultarAsistencias.ConsultarAsync(
            empleadoId, empleadoIdSesion, rol, desde, hasta, cancellationToken);

        return resultado == ConsultaAsistenciasResultado.Prohibido ? Forbid() : Ok(marcas);
    }

    /// <summary>Vista de Administrador: navega las marcas de todos los empleados con filtros opcionales.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = TimeClockSystem.Domain.Roles.Administrador)]
    [ProducesResponseType<IEnumerable<FilaAsistenciaAdminDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarParaAdministrador(
        [FromQuery] int? empleadoId, [FromQuery] int? centroTrabajoId, [FromQuery] DateOnly? fecha,
        CancellationToken cancellationToken) =>
        Ok(await consultarAsistencias.ConsultarParaAdministradorAsync(empleadoId, centroTrabajoId, fecha, cancellationToken));
}
