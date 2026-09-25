using System.Net.Http.Json;
using System.Web;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

public record RegistroAuditoriaApiDto(int Id, string Evento, string UsuarioOEmpleadoId, string Detalle, DateTime TimestampUtc);
public record ConsultaAuditoriaApiResponse(int Total, List<RegistroAuditoriaApiDto> Elementos);

/// <summary>Cliente HTTP de Auditoría (contracts/auditoria.md, FR-011a). Solo Administrador.</summary>
public class AuditoriaApiClient(HttpClient http)
{
    public async Task<ConsultaAuditoriaApiResponse> ConsultarAsync(
        DateOnly? desde, DateOnly? hasta, string? usuario, int pagina, CancellationToken cancellationToken = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        if (desde is not null) query["desde"] = desde.Value.ToString("yyyy-MM-dd");
        if (hasta is not null) query["hasta"] = hasta.Value.ToString("yyyy-MM-dd");
        if (!string.IsNullOrWhiteSpace(usuario)) query["usuario"] = usuario;
        query["pagina"] = pagina.ToString();

        var resultado = await http.GetFromJsonAsync<ConsultaAuditoriaApiResponse>(
            $"api/auditoria?{query}", ApiJsonOptions.Default, cancellationToken);
        return resultado ?? new ConsultaAuditoriaApiResponse(0, []);
    }
}
