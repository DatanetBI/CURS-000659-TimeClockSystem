using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Web.Areas.Marcaje;

public class ResultadoMarcaje
{
    public bool Aceptada { get; init; }
    public MotivoRechazoMarca Motivo { get; init; } = MotivoRechazoMarca.Ninguno;
    public Marca? Marca { get; init; }
}

/// <summary>
/// Regla de negocio compartida por el marcaje desde el portal (sesión) y por PIN (kiosco) — ambos
/// canales aplican las mismas validaciones de geofence, consentimiento y entrada duplicada
/// (spec.md - User Story 1, escenario 6).
/// </summary>
public class MarcajeService(ApplicationDbContext db)
{
    public async Task<ResultadoMarcaje> RegistrarAsync(
        int empleadoId, TipoMarca tipo, CanalMarca canal, double? latitud, double? longitud,
        CancellationToken cancellationToken = default)
    {
        var empleado = await db.Empleados
            .Include(e => e.CentroTrabajo)
            .FirstOrDefaultAsync(e => e.Id == empleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Empleado {empleadoId} no encontrado.");

        if (tipo == TipoMarca.Entrada && await TieneEntradaAbiertaAsync(empleadoId, cancellationToken))
        {
            return await RechazarAsync(empleadoId, tipo, canal, latitud, longitud, MotivoRechazoMarca.EntradaDuplicada, cancellationToken);
        }

        if (latitud is not null && longitud is not null)
        {
            if (!ConsentimientoValidator.PuedeCapturarGeolocalizacion(empleado))
            {
                return await RechazarAsync(empleadoId, tipo, canal, latitud, longitud, MotivoRechazoMarca.SinConsentimientoGeolocalizacion, cancellationToken);
            }

            if (empleado.CentroTrabajo is null ||
                !GeofenceValidator.EstaDentroDelGeofence(empleado.CentroTrabajo, latitud.Value, longitud.Value))
            {
                return await RechazarAsync(empleadoId, tipo, canal, latitud, longitud, MotivoRechazoMarca.Geofence, cancellationToken);
            }
        }

        var marca = new Marca
        {
            EmpleadoId = empleadoId,
            Tipo = tipo,
            Canal = canal,
            Timestamp = DateTime.UtcNow,
            Latitud = latitud,
            Longitud = longitud,
            Estado = EstadoMarca.Valida,
        };
        db.Marcas.Add(marca);
        await db.SaveChangesAsync(cancellationToken);

        return new ResultadoMarcaje { Aceptada = true, Marca = marca };
    }

    private async Task<bool> TieneEntradaAbiertaAsync(int empleadoId, CancellationToken cancellationToken)
    {
        var ultima = await db.Marcas
            .Where(m => m.EmpleadoId == empleadoId && m.Estado == EstadoMarca.Valida &&
                        (m.Tipo == TipoMarca.Entrada || m.Tipo == TipoMarca.Salida))
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return ultima is { Tipo: TipoMarca.Entrada };
    }

    private async Task<ResultadoMarcaje> RechazarAsync(
        int empleadoId, TipoMarca tipo, CanalMarca canal, double? latitud, double? longitud,
        MotivoRechazoMarca motivo, CancellationToken cancellationToken)
    {
        var marca = new Marca
        {
            EmpleadoId = empleadoId,
            Tipo = tipo,
            Canal = canal,
            Timestamp = DateTime.UtcNow,
            Latitud = latitud,
            Longitud = longitud,
            Estado = EstadoMarca.Rechazada,
            MotivoRechazo = motivo,
        };
        db.Marcas.Add(marca);

        if (motivo == MotivoRechazoMarca.Geofence)
        {
            // FR-010: registrar como evento de seguridad todo intento rechazado por geofencing.
            // v1.0 no tiene un módulo de auditoría separado (diferido a v1.1); se deja constancia
            // en el propio registro de la marca rechazada con su MotivoRechazo.
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ResultadoMarcaje { Aceptada = false, Motivo = motivo, Marca = marca };
    }
}
