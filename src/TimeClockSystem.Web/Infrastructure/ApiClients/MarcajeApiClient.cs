using System.Net.Http.Json;
using TimeClockSystem.Web.ViewModels;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

public record MarcajeApiResponse(bool Aceptada, MotivoRechazoMarca? Motivo, int? MarcaId, DateTime? Timestamp, string? EmpleadoNombre);

/// <summary>
/// Cliente HTTP de Marcaje (contracts/marcaje.md). Un único intento por llamada, sin reintento
/// automático (FR-012): si la solicitud falla por timeout o red, se deja que la excepción suba al
/// Controller para mostrar el error de inmediato.
/// </summary>
public class MarcajeApiClient(HttpClient http)
{
    public async Task<MarcajeApiResponse?> RegistrarAsync(TipoMarca tipo, double? latitud, double? longitud, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/marcaje", new { tipo, latitud, longitud }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MarcajeApiResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<MarcajeApiResponse?> RegistrarPorPinAsync(
        string numeroEmpleado, string pin, TipoMarca tipo, double? latitud, double? longitud,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync(
            "api/marcaje/pin", new { numeroEmpleado, pin, tipo, latitud, longitud }, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MarcajeApiResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}
