using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Application.Marcaje;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Controllers;

public record RegistrarMarcaRequest(TipoMarca Tipo, double? Latitud, double? Longitud);
public record RegistrarMarcaPinRequest(string NumeroEmpleado, string Pin, TipoMarca Tipo, double? Latitud, double? Longitud);
public record MarcajeResponse(bool Aceptada, MotivoRechazoMarca? Motivo, int? MarcaId, DateTime? Timestamp, string? EmpleadoNombre);

/// <summary>Marcaje (contracts/marcaje.md).</summary>
[ApiController]
[Route("api/marcaje")]
public class MarcajeController(RegistrarMarcaUseCase registrarMarca) : ControllerBase
{
    /// <summary>Registra una marca para el empleado autenticado (canal PortalWeb).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Empleado)]
    [ProducesResponseType<MarcajeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar(RegistrarMarcaRequest request, CancellationToken cancellationToken)
    {
        var empleadoIdClaim = User.FindFirst("empleadoId")?.Value;
        if (empleadoIdClaim is null || !int.TryParse(empleadoIdClaim, out var empleadoId))
        {
            return NotFound("Tu cuenta no está vinculada a ningún empleado.");
        }

        var resultado = await registrarMarca.RegistrarPortalAsync(empleadoId, request.Tipo, request.Latitud, request.Longitud, cancellationToken);
        return Ok(AParaRespuesta(resultado));
    }

    /// <summary>Registra una marca por número de empleado + PIN (canal Pin, kiosco público).</summary>
    [HttpPost("pin")]
    [AllowAnonymous]
    [ProducesResponseType<MarcajeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarPorPin(RegistrarMarcaPinRequest request, CancellationToken cancellationToken)
    {
        var resultado = await registrarMarca.RegistrarPorPinAsync(
            request.NumeroEmpleado, request.Pin, request.Tipo, request.Latitud, request.Longitud, cancellationToken);
        return Ok(AParaRespuesta(resultado));
    }

    private static MarcajeResponse AParaRespuesta(ResultadoMarcaje resultado) =>
        new(resultado.Aceptada, resultado.Aceptada ? null : resultado.Motivo, resultado.MarcaId, resultado.Timestamp, resultado.EmpleadoNombre);
}
