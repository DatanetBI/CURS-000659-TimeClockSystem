using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.ConsultaAsistencias;

public enum ConsultaAsistenciasResultado { Ok, Prohibido }

public record MarcaConsultaDto(int MarcaId, TipoMarca Tipo, CanalMarca Canal, DateTime Timestamp, EstadoMarca Estado, MotivoRechazoMarca MotivoRechazo, IndicadorPuntualidad IndicadorPuntualidad);

public record FilaAsistenciaAdminDto(
    string NumeroEmpleado, string NombreEmpleado, string CentroTrabajo, DateTime Timestamp,
    TipoMarca Tipo, EstadoMarca Estado, MotivoRechazoMarca MotivoRechazo,
    IndicadorPuntualidad? Puntualidad, bool EsDiaFestivo);

/// <summary>
/// Consulta las marcas de un empleado con su indicador de puntualidad (contracts/consulta-asistencias.md).
/// Un Empleado solo puede consultar su propio historial; el Backend lo valida por sí mismo
/// (FR-003) y registra en auditoría cualquier intento de consultar a otro empleado (FR-011).
/// </summary>
public class ConsultarAsistenciasUseCase(
    IMarcaRepository marcas, IAsignacionTurnoRepository asignaciones, IDiaFestivoRepository diasFestivos, IAuditLogService auditoria)
{
    public async Task<(ConsultaAsistenciasResultado Resultado, IReadOnlyList<MarcaConsultaDto> Marcas)> ConsultarAsync(
        int empleadoIdSolicitado, int? empleadoIdSesion, string rolSesion, DateOnly desde, DateOnly hasta,
        CancellationToken cancellationToken = default)
    {
        if (rolSesion == Roles.Empleado && empleadoIdSolicitado != empleadoIdSesion)
        {
            await auditoria.RegistrarAsync(
                EventoAuditoria.AccesoDenegadoPorRol, empleadoIdSesion?.ToString() ?? "desconocido",
                $"Intento de consultar asistencias del empleado {empleadoIdSolicitado}.", cancellationToken);
            return (ConsultaAsistenciasResultado.Prohibido, []);
        }

        var marcasDelRango = await marcas.ConsultarAsync(empleadoIdSolicitado, desde, hasta, cancellationToken);
        var resultado = new List<MarcaConsultaDto>(marcasDelRango.Count);

        foreach (var marca in marcasDelRango)
        {
            var fecha = DateOnly.FromDateTime(marca.Timestamp);
            var asignacion = await asignaciones.ObtenerParaEmpleadoEnFechaAsync(empleadoIdSolicitado, fecha, cancellationToken);
            var indicador = PuntualidadCalculator.Calcular(marca, asignacion?.Turno);

            resultado.Add(new MarcaConsultaDto(marca.Id, marca.Tipo, marca.Canal, marca.Timestamp, marca.Estado, marca.MotivoRechazo, indicador));
        }

        return (ConsultaAsistenciasResultado.Ok, resultado);
    }

    /// <summary>Vista de Administrador: navega las marcas de todos los empleados (sin restricción de auto-consulta).</summary>
    public async Task<IReadOnlyList<FilaAsistenciaAdminDto>> ConsultarParaAdministradorAsync(
        int? empleadoId, int? centroTrabajoId, DateOnly? fecha, CancellationToken cancellationToken = default)
    {
        var marcasEncontradas = await marcas.ConsultarTodasAsync(empleadoId, centroTrabajoId, fecha, limite: 200, cancellationToken);
        var festivos = (await diasFestivos.ListarAsync(cancellationToken)).Select(f => f.Fecha).ToHashSet();

        var resultado = new List<FilaAsistenciaAdminDto>(marcasEncontradas.Count);
        foreach (var marca in marcasEncontradas)
        {
            IndicadorPuntualidad? puntualidad = null;
            if (marca.Tipo == TipoMarca.Entrada && marca.Estado == EstadoMarca.Valida)
            {
                var fechaMarca = DateOnly.FromDateTime(marca.Timestamp);
                var asignacion = await asignaciones.ObtenerParaEmpleadoEnFechaAsync(marca.EmpleadoId, fechaMarca, cancellationToken);
                puntualidad = PuntualidadCalculator.Calcular(marca, asignacion?.Turno);
            }

            resultado.Add(new FilaAsistenciaAdminDto(
                marca.Empleado?.NumeroEmpleado ?? string.Empty,
                marca.Empleado?.Nombre ?? string.Empty,
                marca.Empleado?.CentroTrabajo?.Nombre ?? string.Empty,
                marca.Timestamp, marca.Tipo, marca.Estado, marca.MotivoRechazo, puntualidad,
                festivos.Contains(DateOnly.FromDateTime(marca.Timestamp))));
        }

        return resultado;
    }
}
