using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests.Integration;

public class DiasFestivosTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public DiasFestivosTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private record DiaFestivoDto(int Id, DateOnly Fecha, string Descripcion);

    [Fact]
    public async Task Listar_ComoAdministrador_MuestraLosFestivosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var festivos = await client.GetFromJsonAsync<List<DiaFestivoDto>>("api/dias-festivos");

        Assert.Contains(festivos!, f => f.Descripcion.Contains("Revolución Mexicana"));
    }

    [Fact]
    public async Task Crear_ComoAdministrador_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var response = await client.PostAsJsonAsync("api/dias-festivos", new { fecha = new DateOnly(2027, 1, 1), descripcion = "Año Nuevo de Prueba" });
        response.EnsureSuccessStatusCode();

        var festivos = await client.GetFromJsonAsync<List<DiaFestivoDto>>("api/dias-festivos");
        Assert.Contains(festivos!, f => f.Descripcion == "Año Nuevo de Prueba");
    }
}
