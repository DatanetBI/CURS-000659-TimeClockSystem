using System.Text.RegularExpressions;

namespace TimeClockSystem.Tests;

public static class AuthTestHelper
{
    public const string AdminUserName = "admin1";
    public const string AdminPassword = "Admin123!";

    public static async Task<HttpClient> LoginAsync(CustomWebApplicationFactory factory, string userName, string password)
    {
        var client = factory.CreateClient();

        var loginPage = await client.GetAsync("/Cuenta/IniciarSesion");
        loginPage.EnsureSuccessStatusCode();
        var token = ExtractAntiForgeryToken(await loginPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token,
        };

        var response = await client.PostAsync("/Cuenta/IniciarSesion", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();
        return client;
    }

    public static Task<HttpClient> LoginAsAdminAsync(CustomWebApplicationFactory factory) =>
        LoginAsync(factory, AdminUserName, AdminPassword);

    public static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if (!match.Success)
        {
            throw new InvalidOperationException("No se encontró el token antifalsificación en la página.");
        }
        return match.Groups[1].Value;
    }
}
