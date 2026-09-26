using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Marcaje;

public record ResultadoMarcaje(bool Aceptada, MotivoRechazoMarca Motivo, int? MarcaId, DateTime? Timestamp, string? EmpleadoNombre = null);

/// <summary>
/// Regla de negocio compartida por el marcaje desde el portal (sesión, canal PortalWeb) y por PIN
/// (kiosco, canal Pin) — ambos aplican las mismas validaciones de geofence, consentimiento y
/// entrada duplicada (contracts/marcaje.md). Adaptado del <c>MarcajeService</c> original,
/// preservando su comportamiento (FR-005) y registrando cada rechazo en auditoría (FR-011).
/// </summary>
public class RegistrarMarcaUseCase(
    IEmpleadoRepository empleados,
    IMarcaRepository marcas,
    ICredencialRepository credenciales,
    IPinHasher pinHasher,
    IAuditLogService auditoria)
{
    public async Task<ResultadoMarcaje> RegistrarPortalAsync(
        int empleadoId, TipoMarca tipo, double? latitud, double? longitud,
        CancellationToken cancellationToken = default)
    {
        var empleado = await empleados.ObtenerPorIdAsync(empleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Empleado {empleadoId} no encontrado.");

        return await RegistrarAsync(empleado, tipo, CanalMarca.PortalWeb, latitud, longitud, cancellationToken);
    }

    public async Task<ResultadoMarcaje> RegistrarPorPinAsync(
        string numeroEmpleado, string pin, TipoMarca tipo, double? latitud, double? longitud,
        CancellationToken cancellationToken = default)
    {
        var empleado = await empleados.ObtenerPorNumeroEmpleadoAsync(numeroEmpleado, cancellationToken);
        var credencial = empleado is null ? null : await credenciales.ObtenerPorEmpleadoIdAsync(empleado.Id, cancellationToken);

        if (empleado is null || credencial is null || !pinHasher.Verificar(pin, credencial.PinHash))
        {
            await auditoria.RegistrarAsync(
                EventoAuditoria.MarcajeRechazado, numeroEmpleado,
                $"Marcaje por PIN rechazado: {MotivoRechazoMarca.CredencialesInvalidas}.", cancellationToken);
            return new ResultadoMarcaje(false, MotivoRechazoMarca.CredencialesInvalidas, null, null);
        }

        return await RegistrarAsync(empleado, tipo, CanalMarca.Pin, latitud, longitud, cancellationToken);
    }

    private async Task<ResultadoMarcaje> RegistrarAsync(
        Empleado empleado, TipoMarca tipo, CanalMarca canal, double? latitud, double? longitud,
        CancellationToken cancellationToken)
    {
        if (tipo == TipoMarca.Entrada && await marcas.TieneEntradaAbiertaAsync(empleado.Id, cancellationToken))
        {
            return await RechazarAsync(empleado, tipo, canal, latitud, longitud, MotivoRechazoMarca.EntradaDuplicada, cancellationToken);
        }

        if (latitud is not null && longitud is not null)
        {
            if (!ConsentimientoValidator.PuedeCapturarGeolocalizacion(empleado))
            {
                return await RechazarAsync(empleado, tipo, canal, latitud, longitud, MotivoRechazoMarca.SinConsentimientoGeolocalizacion, cancellationToken);
            }

            if (empleado.CentroTrabajo is null ||
                !GeofenceValidator.EstaDentroDelGeofence(empleado.CentroTrabajo, latitud.Value, longitud.Value))
            {
                return await RechazarAsync(empleado, tipo, canal, latitud, longitud, MotivoRechazoMarca.Geofence, cancellationToken);
            }
        }

        var marca = new Marca
        {
            EmpleadoId = empleado.Id,
            Tipo = tipo,
            Canal = canal,
            Timestamp = DateTime.UtcNow,
            Latitud = latitud,
            Longitud = longitud,
            Estado = EstadoMarca.Valida,
        };
        await marcas.RegistrarAsync(marca, cancellationToken);

        return new ResultadoMarcaje(true, MotivoRechazoMarca.Ninguno, marca.Id, marca.Timestamp, empleado.Nombre);
    }

    private async Task<ResultadoMarcaje> RechazarAsync(
        Empleado empleado, TipoMarca tipo, CanalMarca canal, double? latitud, double? longitud,
        MotivoRechazoMarca motivo, CancellationToken cancellationToken)
    {
        var marca = new Marca
        {
            EmpleadoId = empleado.Id,
            Tipo = tipo,
            Canal = canal,
            Timestamp = DateTime.UtcNow,
            Latitud = latitud,
            Longitud = longitud,
            Estado = EstadoMarca.Rechazada,
            MotivoRechazo = motivo,
        };
        await marcas.RegistrarAsync(marca, cancellationToken);

        await auditoria.RegistrarAsync(
            EventoAuditoria.MarcajeRechazado, empleado.Id.ToString(),
            $"Marcaje rechazado: {motivo}.", cancellationToken);

        return new ResultadoMarcaje(false, motivo, marca.Id, marca.Timestamp, empleado.Nombre);
    }
}
