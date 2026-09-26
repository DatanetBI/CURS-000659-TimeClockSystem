using System.Net.Http.Json;

namespace TimeClockSystem.Api.Tests.Integration;

public class TurnosTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public TurnosTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    private record TurnoDto(int Id, string Nombre);
    private record AsignacionTurnoDto(int Id, int EmpleadoId, int TurnoId);

    [Fact]
    public async Task Listar_ComoAdministrador_MuestraLosTurnosDeEjemplo()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var turnos = await client.GetFromJsonAsync<List<TurnoDto>>("api/turnos");

        Assert.Contains(turnos!, t => t.Nombre == "Fijo diurno");
        Assert.Contains(turnos!, t => t.Nombre == "Nocturno");
    }

    [Fact]
    public async Task CrearAsignacionTurno_ParaEmpleadoYTurnoExistentes_Persiste()
    {
        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var response = await client.PostAsJsonAsync("api/asignaciones-turno", new
        {
            empleadoId = 2,
            turnoId = 1,
            fecha = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
        });
        response.EnsureSuccessStatusCode();

        var asignaciones = await client.GetFromJsonAsync<List<AsignacionTurnoDto>>("api/asignaciones-turno?empleadoId=2");
        Assert.Contains(asignaciones!, a => a.EmpleadoId == 2 && a.TurnoId == 1);
    }
}
