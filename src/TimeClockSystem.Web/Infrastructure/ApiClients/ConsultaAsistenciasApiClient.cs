using System.Net.Http.Json;
using System.Web;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

public record MarcaConsultaApiDto(int MarcaId, TipoMarca Tipo, CanalMarca Canal, DateTime Timestamp, EstadoMarca Estado, MotivoRechazoMarca MotivoRechazo, IndicadorPuntualidad IndicadorPuntualidad);

public record FilaAsistenciaAdminApiDto(
    string NumeroEmpleado, string NombreEmpleado, string CentroTrabajo, DateTime Timestamp,
    TipoMarca Tipo, EstadoMarca Estado, MotivoRechazoMarca MotivoRechazo,
    IndicadorPuntualidad? Puntualidad, bool EsDiaFestivo);

/// <summary>Cliente HTTP de Consulta de Asistencias (contracts/consulta-asistencias.md).</summary>
public class ConsultaAsistenciasApiClient(HttpClient http)
{
    public async Task<List<MarcaConsultaApiDto>> ConsultarAsync(int empleadoId, DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<MarcaConsultaApiDto>>(
            $"api/consulta-asistencias?empleadoId={empleadoId}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}",
            ApiJsonOptions.Default, cancellationToken) ?? [];

    public async Task<List<FilaAsistenciaAdminApiDto>> ConsultarParaAdministradorAsync(
        int? empleadoId, int? centroTrabajoId, DateOnly? fecha, CancellationToken cancellationToken = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        if (empleadoId is not null) query["empleadoId"] = empleadoId.ToString();
        if (centroTrabajoId is not null) query["centroTrabajoId"] = centroTrabajoId.ToString();
        if (fecha is not null) query["fecha"] = fecha.Value.ToString("yyyy-MM-dd");

        return await http.GetFromJsonAsync<List<FilaAsistenciaAdminApiDto>>(
            $"api/consulta-asistencias/admin?{query}", ApiJsonOptions.Default, cancellationToken) ?? [];
    }
}
