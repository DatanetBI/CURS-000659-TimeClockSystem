using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Application.Auditoria;
using TimeClockSystem.Application.Auth;
using TimeClockSystem.Application.CentrosTrabajo;
using TimeClockSystem.Application.ConsultaAsistencias;
using TimeClockSystem.Application.DiasFestivos;
using TimeClockSystem.Application.Empleados;
using TimeClockSystem.Application.Marcaje;
using TimeClockSystem.Application.Turnos;
using TimeClockSystem.Domain;
using TimeClockSystem.Infrastructure.Auditoria;
using TimeClockSystem.Infrastructure.Auth;
using TimeClockSystem.Infrastructure.Biometria;
using TimeClockSystem.Infrastructure.Data;
using TimeClockSystem.Infrastructure.Identity;
using TimeClockSystem.Infrastructure.Repositories;
using TimeClockSystem.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Cultura del producto: español de México (Principio II de la constitución).
var culturaMx = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = culturaMx;
CultureInfo.DefaultThreadCurrentUICulture = culturaMx;

// Nombre de aplicación explícito para Data Protection: el Backend y el Frontend corren en la
// misma máquina y comparten el almacén de claves por usuario (research.md).
builder.Services.AddDataProtection().SetApplicationName("TimeClockSystem.Api");

// Cadena de conexión desde configuración (Principio V — sin secretos en el código). Se valida
// contra nulo/vacío/blanco, no solo nulo: una variable de entorno presente pero vacía (p. ej.
// ConnectionStrings__DefaultConnection= en el contenedor) MUST fallar igual que si faltara del
// todo, en vez de dejar pasar una cadena de conexión vacía (FR-009, edge case de la spec).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No se configuró ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddTimeClockIdentity();

// JWT: clave de firma vía configuración/variable de entorno (Principio V), token de expiración
// fija sin renovación silenciosa (FR-002, research.md #2). Misma validación estricta que la
// cadena de conexión: nulo/vacío/blanco MUST fallar explícitamente (FR-009, edge case de la spec).
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    throw new InvalidOperationException("No se configuró Jwt:SigningKey.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TimeClockSystem.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TimeClockSystem.Web";
var jwtExpirationMinutes = builder.Configuration.GetValue("Jwt:ExpirationMinutes", 60);

builder.Services.AddSingleton(new JwtOptions(jwtSigningKey, jwtIssuer, jwtAudience, jwtExpirationMinutes));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuditingAuthorizationMiddlewareResultHandler>();

// Repositorios (Infrastructure implementa las abstracciones de Application).
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<ICentroTrabajoRepository, CentroTrabajoRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IAsignacionTurnoRepository, AsignacionTurnoRepository>();
builder.Services.AddScoped<IDiaFestivoRepository, DiaFestivoRepository>();
builder.Services.AddScoped<IMarcaRepository, MarcaRepository>();
builder.Services.AddScoped<ICredencialRepository, CredencialRepository>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IPinHasher, PinHasher>();
builder.Services.AddScoped<IBiometricVerificationProvider, NullBiometricVerificationProvider>();

// Casos de uso (Application).
builder.Services.AddScoped<AutenticarUseCase>();
builder.Services.AddScoped<RegistrarMarcaUseCase>();
builder.Services.AddScoped<EmpleadosService>();
builder.Services.AddScoped<CentrosTrabajoService>();
builder.Services.AddScoped<TurnosService>();
builder.Services.AddScoped<AsignacionesTurnoService>();
builder.Services.AddScoped<DiasFestivosService>();
builder.Services.AddScoped<ConsultarAsistenciasUseCase>();
builder.Services.AddScoped<ConsultarAuditoriaUseCase>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TimeClockSystem.Api",
        Version = "v1",
        Description = "Backend de TimeClockSystem: empleados, centros de trabajo, turnos, marcaje, " +
            "consulta de asistencias y auditoría (specs/002-split-backend-frontend/contracts/).",
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TimeClockSystem.Api.xml"), includeControllerXmlComments: true);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token obtenido de POST /api/auth/login.",
    });
});

var app = builder.Build();

// Migrar la base de datos y sembrar roles/datos mock al iniciar (igual que el Web original).
// Espera acotada a que la base de datos esté lista para aceptar conexiones (FR-007): reintenta
// solo errores de conexión, dejando que un fallo real de migración (esquema incompatible) se
// propague de inmediato y detenga el contenedor con un error claro (FR-010, research.md #4 y #5).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        // Las pruebas de integración usan SQLite en un archivo temporal, aislado por corrida
        // (CustomWebApiFactory), no SQL Server. Las migraciones se generan y expresan en SQL
        // específico de SQL Server (p. ej. "nvarchar(max)"), que no es sintaxis válida en SQLite,
        // así que aplicarlas ahí fallaría. EnsureCreated construye el esquema directamente desde
        // el modelo actual de EF Core (ya adaptado por el proveedor Sqlite en tiempo de ejecución),
        // que es todo lo que las pruebas necesitan (research.md #9).
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        const int maxIntentosConexion = 12;
        var esperaEntreIntentos = TimeSpan.FromSeconds(10);

        for (var intento = 1; ; intento++)
        {
            try
            {
                await db.Database.CanConnectAsync();
                break;
            }
            catch (SqlException ex) when (intento < maxIntentosConexion)
            {
                startupLogger.LogWarning(ex,
                    "No se pudo conectar a la base de datos (intento {Intento}/{MaxIntentos}). Reintentando en {Espera}s...",
                    intento, maxIntentosConexion, esperaEntreIntentos.TotalSeconds);
                await Task.Delay(esperaEntreIntentos);
            }
        }

        await db.Database.MigrateAsync();
    }

    await IdentityConfiguration.EnsureRolesCreatedAsync(scope.ServiceProvider);
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
