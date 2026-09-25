using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.Auditoria;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

/// <summary>Auditoría (contracts/auditoria.md). Solo Administrador — FR-011a.</summary>
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = Roles.Administrador)]
public class AuditoriaController(ConsultarAuditoriaUseCase consultarAuditoria) : ControllerBase
{
    /// <summary>Consulta paginada del log de auditoría, filtrando opcionalmente por fecha y usuario.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Consultar(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] string? usuario,
        [FromQuery] int pagina, [FromQuery] int? tamanoPagina, CancellationToken cancellationToken)
    {
        var resultado = await consultarAuditoria.ConsultarAsync(desde, hasta, usuario, pagina, tamanoPagina, cancellationToken);
        return Ok(new { total = resultado.Total, elementos = resultado.Elementos });
    }
}
