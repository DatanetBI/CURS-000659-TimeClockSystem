namespace TimeClockSystem.Tests.Integration;

public class ConsultaAsistenciasTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ConsultaAsistenciasTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_SinFiltros_MuestraLasMarcasDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var html = await (await client.GetAsync("/ConsultaAsistencias/ConsultaAsistencias")).Content.ReadAsStringAsync();

        Assert.Contains("E001", html);
        Assert.Contains("Oficina Central CDMX", html);
    }

    [Fact]
    public async Task Index_FiltradoPorEmpleado_SoloMuestraSusMarcas()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        // E001 tiene Id=1 en una base recién sembrada.
        var html = await (await client.GetAsync("/ConsultaAsistencias/ConsultaAsistencias?empleadoId=1"))
            .Content.ReadAsStringAsync();

        Assert.Contains("E001", html);
        Assert.DoesNotContain("E002 —", html);
    }

    [Fact]
    public async Task Index_SinSesion_RedirigeAIniciarSesion()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/ConsultaAsistencias/ConsultaAsistencias");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
    }
}
