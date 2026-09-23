using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeClockSystem.Web.Infrastructure.Data;

namespace TimeClockSystem.Tests;

/// <summary>
/// Fixture compartida por las pruebas de integración: cada instancia usa su propio archivo SQLite
/// temporal (creado y sembrado por el DbSeeder real de la aplicación al arrancar), y se borra al
/// terminar la prueba. Reemplaza el registro de <see cref="ApplicationDbContext"/> después de que
/// Program.cs construye el host, en vez de intentar sobreescribir la configuración antes — es el
/// patrón recomendado por Microsoft para pruebas de integración con EF Core.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"timeclock-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            SqliteConnectionHelper.TryDeleteFile(_dbPath);
        }
    }
}

internal static class SqliteConnectionHelper
{
    public static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // El archivo puede seguir en uso brevemente tras el shutdown; no es crítico para la prueba.
        }
    }
}
