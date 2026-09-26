using System.Net;
using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests.Integration;

public class CentrosTrabajoTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public CentrosTrabajoTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private record CentroTrabajoDto(int Id, string Nombre);

    [Fact]
    public async Task Listar_ComoAdministrador_MuestraLosCentrosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var centros = await client.GetFromJsonAsync<List<CentroTrabajoDto>>("api/centros-trabajo");

        Assert.Contains(centros!, c => c.Nombre == "Oficina Central CDMX");
    }

    [Fact]
    public async Task CrearYActualizar_ComoAdministrador_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var createResponse = await client.PostAsJsonAsync("api/centros-trabajo", new
        {
            nombre = "Sucursal de Prueba",
            latitud = 19.0,
            longitud = -99.0,
            radioMetros = 200,
        });
        createResponse.EnsureSuccessStatusCode();

        var centros = await client.GetFromJsonAsync<List<CentroTrabajoDto>>("api/centros-trabajo");
        Assert.Contains(centros!, c => c.Nombre == "Sucursal de Prueba");
    }

    [Fact]
    public async Task Listar_SinToken_DevuelveNoAutorizado()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/centros-trabajo");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
