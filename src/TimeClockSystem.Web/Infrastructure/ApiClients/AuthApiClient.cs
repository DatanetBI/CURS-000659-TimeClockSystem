using System.Net.Http.Json;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

public record LoginApiResponse(string Token, DateTime ExpiraEnUtc, string Rol, string Nombre, int? EmpleadoId);

/// <summary>Cliente HTTP para POST /api/auth/login (contracts/auth.md).</summary>
public class AuthApiClient(HttpClient http)
{
    public async Task<LoginApiResponse?> LoginAsync(string usuario, string contrasena, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { usuario, contrasena }, ApiJsonOptions.Default, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginApiResponse>(ApiJsonOptions.Default, cancellationToken);
    }
}
