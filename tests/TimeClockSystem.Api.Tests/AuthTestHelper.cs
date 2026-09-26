using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests;

public static class AuthTestHelper
{
    public const string AdminUserName = "admin1";
    public const string AdminPassword = "Admin123!";

    private record LoginResponse(string Token, DateTime ExpiraEnUtc, string Rol, string Nombre, int? EmpleadoId);

    public static async Task<HttpClient> LoginAsync(CustomWebApiFactory factory, string userName, string password)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/auth/login", new { usuario = userName, contrasena = password });
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Login de prueba no devolvió un token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        return client;
    }

    public static Task<HttpClient> LoginAsAdminAsync(CustomWebApiFactory factory) =>
        LoginAsync(factory, AdminUserName, AdminPassword);
}
