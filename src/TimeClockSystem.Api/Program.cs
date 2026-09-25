using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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

// Cadena de conexión desde configuración (Principio V — sin secretos en el código).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se configuró ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddTimeClockIdentity();

// JWT: clave de firma vía configuración/variable de entorno (Principio V), token de expiración
// fija sin renovación silenciosa (FR-002, research.md #2).
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("No se configuró Jwt:SigningKey.");
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

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
