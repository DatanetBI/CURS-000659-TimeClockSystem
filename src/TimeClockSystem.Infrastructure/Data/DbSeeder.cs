using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Identity;

namespace TimeClockSystem.Infrastructure.Data;

/// <summary>
/// Siembra datos mock para la prueba de concepto (plan.md - "Datos mock para la prueba de concepto").
/// Se ejecuta una sola vez al iniciar la aplicacion, si la base de datos esta vacia. Cada modulo
/// extiende este sembrador con su propio conjunto de datos de ejemplo, en el mismo orden de
/// construccion (T013 Centros de trabajo, T022 Empleados, T032 Turnos, T041 Dias festivos,
/// T055 Marcas).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.Users.AnyAsync())
        {
            // Ya existen datos (arranque posterior al primero); no volver a sembrar.
            return;
        }

        await SeedAdministradoresAsync(userManager);
        var centros = await SeedCentrosTrabajoAsync(db);
        var empleados = await SeedEmpleadosAsync(db, userManager, centros);
        await SeedTurnosAsync(db, empleados);
        await SeedDiasFestivosAsync(db);
        await SeedMarcasAsync(db, empleados);
    }

    private static async Task<List<CentroTrabajo>> SeedCentrosTrabajoAsync(ApplicationDbContext db)
    {
        var centros = new List<CentroTrabajo>
        {
            new() { Nombre = "Oficina Central CDMX", Latitud = 19.4326, Longitud = -99.1332, RadioMetros = 150 },
            new() { Nombre = "Planta Querétaro", Latitud = 20.5888, Longitud = -100.3899, RadioMetros = 300 },
            new() { Nombre = "Campo Norte", Latitud = 25.6866, Longitud = -100.3161, RadioMetros = 500 },
        };
        db.CentrosTrabajo.AddRange(centros);

        await db.SaveChangesAsync();
        return centros;
    }

    /// <summary>
    /// PIN y contraseña de portal de ejemplo para todos los empleados mock (solo para la prueba
    /// de concepto — en un entorno real cada empleado tendría su propio PIN asignado por RRHH/Admin).
    /// </summary>
    private const string PinDeEjemplo = "1234";
    private const string PasswordDePortalDeEjemplo = "Empleado123!";

    private static async Task<List<Empleado>> SeedEmpleadosAsync(
        ApplicationDbContext db, UserManager<ApplicationUser> userManager, IReadOnlyList<CentroTrabajo> centros)
    {
        (string numero, string nombre, int centroIndex)[] datos =
        [
            ("E001", "Juan Pérez", 0),
            ("E002", "María García", 0),
            ("E003", "Carlos López", 0),
            ("E004", "Ana Martínez", 0),
            ("E005", "Luis Hernández", 1),
            ("E006", "Sofía Ramírez", 1),
            ("E007", "Diego Torres", 1),
            ("E008", "Valentina Flores", 1),
            ("E009", "Miguel Sánchez", 2),
            ("E010", "Camila Rivera", 2),
            ("E011", "Jorge Díaz", 2),
            ("E012", "Fernanda Cruz", 2),
        ];

        var pinHasher = new PasswordHasher<Empleado>();
        var empleadosCreados = new List<Empleado>();

        foreach (var (numero, nombre, centroIndex) in datos)
        {
            var empleado = new Empleado
            {
                NumeroEmpleado = numero,
                Nombre = nombre,
                CentroTrabajoId = centros[centroIndex].Id,
                Estado = EstadoEmpleado.Activo,
                ConsentimientoGeolocalizacion = true,
            };
            db.Empleados.Add(empleado);
            await db.SaveChangesAsync(); // para obtener empleado.Id
            empleadosCreados.Add(empleado);

            db.CredencialesDeMarcaje.Add(new CredencialDeMarcaje
            {
                EmpleadoId = empleado.Id,
                PinHash = pinHasher.HashPassword(empleado, PinDeEjemplo),
                FechaActualizacion = DateTime.UtcNow,
            });

            var usuario = new ApplicationUser { UserName = numero, EmpleadoId = empleado.Id };
            var resultado = await userManager.CreateAsync(usuario, PasswordDePortalDeEjemplo);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(usuario, Roles.Empleado);
            }
        }

        await db.SaveChangesAsync();
        return empleadosCreados;
    }

    private static async Task SeedTurnosAsync(ApplicationDbContext db, IReadOnlyList<Empleado> empleados)
    {
        var turnos = new List<Turno>
        {
            new()
            {
                Nombre = "Fijo diurno", Tipo = TipoTurno.Fijo,
                HoraEntrada = new TimeSpan(8, 0, 0), HoraSalida = new TimeSpan(17, 0, 0),
                DuracionRecesoMinutos = 60, ToleranciaMinutos = 10,
            },
            new()
            {
                Nombre = "Nocturno", Tipo = TipoTurno.Nocturno,
                HoraEntrada = new TimeSpan(22, 0, 0), HoraSalida = new TimeSpan(6, 0, 0),
                DuracionRecesoMinutos = 30, ToleranciaMinutos = 15,
            },
            new()
            {
                Nombre = "Flexible", Tipo = TipoTurno.Flexible,
                HoraEntrada = new TimeSpan(9, 0, 0), HoraSalida = new TimeSpan(18, 0, 0),
                DuracionRecesoMinutos = 60, ToleranciaMinutos = 20,
            },
        };
        db.Turnos.AddRange(turnos);
        await db.SaveChangesAsync();

        // Asigna el turno fijo diurno a todos los empleados de ejemplo, para hoy, de modo que el
        // Módulo 5 (Registro de asistencias) tenga contra qué validar la puntualidad de inmediato.
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        foreach (var empleado in empleados)
        {
            db.AsignacionesTurno.Add(new AsignacionTurno
            {
                EmpleadoId = empleado.Id,
                TurnoId = turnos[0].Id,
                Fecha = hoy,
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedDiasFestivosAsync(ApplicationDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        db.DiasFestivos.AddRange(
            new DiaFestivo { Fecha = new DateOnly(hoy.Year, 11, 20), Descripcion = "Aniversario de la Revolución Mexicana" },
            new DiaFestivo { Fecha = new DateOnly(hoy.Year, 12, 25), Descripcion = "Navidad" },
            new DiaFestivo { Fecha = hoy.AddDays(7), Descripcion = "Día festivo de ejemplo (próximo)" });

        await db.SaveChangesAsync();
    }

    private static async Task SeedMarcasAsync(ApplicationDbContext db, IReadOnlyList<Empleado> empleados)
    {
        // Marcas de ejemplo de los últimos 2 días, para los primeros 3 empleados, con horarios
        // dentro de tolerancia — para que el Portal de consulta (Módulo 6) no se vea vacío.
        foreach (var empleado in empleados.Take(3))
        {
            foreach (var diasAtras in new[] { 1, 2 })
            {
                var fecha = DateTime.UtcNow.Date.AddDays(-diasAtras);
                db.Marcas.Add(new Marca
                {
                    EmpleadoId = empleado.Id,
                    Tipo = TipoMarca.Entrada,
                    Canal = CanalMarca.PortalWeb,
                    Timestamp = fecha.AddHours(8).AddMinutes(5),
                    Estado = EstadoMarca.Valida,
                });
                db.Marcas.Add(new Marca
                {
                    EmpleadoId = empleado.Id,
                    Tipo = TipoMarca.Salida,
                    Canal = CanalMarca.PortalWeb,
                    Timestamp = fecha.AddHours(17).AddMinutes(2),
                    Estado = EstadoMarca.Valida,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdministradoresAsync(UserManager<ApplicationUser> userManager)
    {
        (string userName, string password)[] administradores =
        [
            ("admin1", "Admin123!"),
            ("admin2", "Admin123!"),
        ];

        foreach (var (userName, password) in administradores)
        {
            var usuario = new ApplicationUser { UserName = userName };
            var resultado = await userManager.CreateAsync(usuario, password);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(usuario, Roles.Administrador);
            }
        }
    }
}
