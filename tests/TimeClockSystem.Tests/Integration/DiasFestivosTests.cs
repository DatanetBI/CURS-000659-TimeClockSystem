namespace TimeClockSystem.Tests.Integration;

public class DiasFestivosTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DiasFestivosTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ComoAdministrador_MuestraLosFestivosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var html = await (await client.GetAsync("/DiasFestivos/DiasFestivos")).Content.ReadAsStringAsync();

        Assert.Contains("Revolución Mexicana", html);
    }

    [Fact]
    public async Task CrearDiaFestivo_ComoAdministrador_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var createPage = await client.GetAsync("/DiasFestivos/DiasFestivos/Create");
        var token = AuthTestHelper.ExtractAntiForgeryToken(await createPage.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["Fecha"] = "2027-01-01",
            ["Descripcion"] = "Año Nuevo de Prueba",
            ["__RequestVerificationToken"] = token,
        };

        var response = await client.PostAsync("/DiasFestivos/DiasFestivos/Create", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Año Nuevo de Prueba", html);
    }
}
