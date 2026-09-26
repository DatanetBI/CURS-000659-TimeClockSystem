using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Api.Tests.Integration;

/// <summary>
/// Verifica SC-007: con un volumen alto de empleados, las acciones típicas (aquí, la consulta de
/// asistencias de Administrador) se completan en menos de 5 segundos. Usa su propia fixture (no
/// comparte datos con otras pruebas) para poder sembrar 500 empleados de forma controlada.
/// </summary>
public class EscalaTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory factory;

    public EscalaTests(CustomWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task ConQuinientosEmpleados_LaConsultaDeAsistenciasRespondeEnMenosDe5Segundos()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            var centro = await db.CentrosTrabajo.FirstAsync();

            for (var i = 100; i < 600; i++)
            {
                db.Empleados.Add(new Empleado
                {
                    NumeroEmpleado = $"CARGA{i}",
                    Nombre = $"Empleado de Carga {i}",
                    CentroTrabajoId = centro.Id,
                    Estado = EstadoEmpleado.Activo,
                    ConsentimientoGeolocalizacion = true,
                });
            }
            await db.SaveChangesAsync();

            Assert.True(await db.Empleados.CountAsync() >= 500, "La siembra de carga no alcanzó los 500 empleados.");
        }

        var client = await AuthTestHelper.LoginAsAdminAsync(factory);

        var cronometro = Stopwatch.StartNew();
        var response = await client.GetAsync("api/consulta-asistencias/admin");
        cronometro.Stop();

        response.EnsureSuccessStatusCode();
        Assert.True(
            cronometro.Elapsed < TimeSpan.FromSeconds(5),
            $"La consulta de asistencias tardó {cronometro.Elapsed.TotalSeconds:0.0}s con 500+ empleados (se esperaba < 5s, SC-007).");
    }
}
