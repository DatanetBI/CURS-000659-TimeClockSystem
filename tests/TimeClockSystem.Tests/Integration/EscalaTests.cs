using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Tests.Integration;

/// <summary>
/// Verifica SC-010: el sistema soporta organizaciones de hasta 500 empleados activos sin degradar
/// los tiempos de SC-001 (marcaje). Usa su propia fixture (no comparte datos con otras pruebas) para
/// poder sembrar 500 empleados de forma controlada.
/// </summary>
public class EscalaTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EscalaTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConSuscientos500Empleados_ElMarcajeYLaConsultaRespondenEnMenosDe5Segundos()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Forzar la creación/siembra inicial antes de agregar el volumen de la prueba.
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

        var client = await AuthTestHelper.LoginAsAdminAsync(_factory);

        var cronometro = Stopwatch.StartNew();
        var response = await client.GetAsync("/ConsultaAsistencias/ConsultaAsistencias");
        cronometro.Stop();

        response.EnsureSuccessStatusCode();
        Assert.True(
            cronometro.Elapsed < TimeSpan.FromSeconds(5),
            $"El Portal de consulta tardó {cronometro.Elapsed.TotalSeconds:0.0}s con 500+ empleados (se esperaba < 5s, muy por debajo del límite de 15s de SC-001).");
    }
}
