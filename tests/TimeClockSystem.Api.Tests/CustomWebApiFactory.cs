using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeClockSystem.Infrastructure.Data;

namespace TimeClockSystem.Api.Tests;

/// <summary>
/// Fixture compartida por las pruebas de integración de la API: cada instancia usa su propio
/// archivo SQLite temporal (creado y sembrado por el DbSeeder real al arrancar), borrado al
/// terminar. Reemplaza el registro de <see cref="ApplicationDbContext"/> después de que Program.cs
/// construye el host (patrón recomendado por Microsoft para pruebas de integración con EF Core).
/// </summary>
public class CustomWebApiFactory : WebApplicationFactory<Program>
{
    private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"timeclock-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
            catch (IOException)
            {
                // El archivo puede seguir en uso brevemente tras el shutdown; no es crítico.
            }
        }
    }
}
