# Implementation Plan: Migración a SQL Server y Contenerización con Docker

**Branch**: `003-sqlserver-docker-migration` | **Date**: 2026-09-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-sqlserver-docker-migration/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Migrar el proveedor de EF Core del Backend de `Microsoft.EntityFrameworkCore.Sqlite` a
`Microsoft.EntityFrameworkCore.SqlServer`, regenerando el conjunto de migraciones (las actuales
están atadas a tipos/convenciones de SQLite y no aplican contra SQL Server). Agregar un
`Dockerfile` para TimeClockSystem.Api y otro para TimeClockSystem.Web, y un `docker-compose.yml`
en la raíz del repositorio que levante los tres servicios (`sqlserver`, `api`, `web`) en una red
compartida, con el volumen `mssql-data` para persistencia, un healthcheck en `sqlserver` que
gatea el arranque de `api` (`depends_on: condition: service_healthy`), una ventana acotada de
reintentos en el propio Backend antes de aplicar migraciones, y secretos (contraseña SA, clave de
firma JWT) inyectados solo vía variables de entorno desde un archivo `.env` no versionado. Se
conserva la ruta de desarrollo sin Docker (`dotnet run` contra una instancia SQL Server accesible
localmente, incluida la misma expuesta por `docker compose` en el puerto del host).

## Technical Context

**Language/Version**: C# 13 / .NET 10 (ya en uso por los 5 proyectos existentes: Api, Application, Domain, Infrastructure, Web)

**Primary Dependencies**: ASP.NET Core 10, EF Core 10 (cambia el proveedor de `Microsoft.EntityFrameworkCore.Sqlite` a `Microsoft.EntityFrameworkCore.SqlServer`, misma versión `10.0.12` que el resto del stack EF Core), ASP.NET Core Identity, JWT Bearer, Swashbuckle/Swagger (sin cambios); Docker Engine + Docker Compose v2 para la contenerización

**Storage**: Microsoft SQL Server (imagen oficial `mcr.microsoft.com/mssql/server:2022-latest`, edición Developer vía `MSSQL_PID=Developer`, adecuada para desarrollo/demo local) en lugar de SQLite; datos persistidos en el volumen nombrado `mssql-data`

**Testing**: xUnit existente (`TimeClockSystem.Api.Tests`, `TimeClockSystem.Web.Tests`); las pruebas de integración del Api siguen usando SQLite en archivo temporal vía `CustomWebApiFactory` (aislamiento de pruebas), sin cambios — no es el motor de producción y migrar la infraestructura de pruebas a SQL Server no aporta valor a esta funcionalidad (Principio I, Simplicidad)

**Target Platform**: Contenedores Linux (`linux/amd64`) para los tres servicios vía Docker Compose en un equipo de desarrollo/demo local; alternativamente, host de desarrollo (Windows) ejecutando `dotnet run` contra una instancia de SQL Server accesible localmente

**Project Type**: Aplicación web ya separada en Backend (TimeClockSystem.Api) y Frontend (TimeClockSystem.Web) desde la funcionalidad 002; esta funcionalidad no cambia esa separación, solo su motor de datos y su forma de arranque

**Performance Goals**: 95% de las acciones típicas del usuario completadas en menos de 5 segundos (SC-007, heredado de la funcionalidad 002), pese a la latencia adicional de la red entre contenedores y del nuevo motor de base de datos

**Constraints**: Arranque completo del sistema con un solo comando en <10 minutos desde un ambiente limpio (SC-001); el Backend MUST limitar su espera a la base de datos a una ventana acotada de reintentos antes de fallar con error claro (FR-007); fallo explícito y detención del contenedor si una migración no puede aplicarse (FR-010); 0 secretos en texto plano en imágenes o archivos versionados (FR-009, SC-005)

**Scale/Scope**: 3 servicios contenedorizados (sqlserver, api, web) para un solo desarrollador/demo local por instancia; sin alta disponibilidad, sin clustering, sin orquestación en la nube (fuera de alcance explícito)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Simplicidad Ante Todo**: PASS. Se usa `docker compose` estándar (una sola herramienta, ya
  disponible con Docker Desktop/Engine), sin capas de orquestación adicionales (sin Kubernetes,
  sin scripts propios de espera cuando el healthcheck nativo de `depends_on` basta). Las pruebas
  de integración conservan SQLite en archivo temporal para no introducir infraestructura de
  pruebas adicional (contenedor de base de datos para tests) que esta funcionalidad no requiere.
- **II. Idioma y Mercado (Español de México)**: PASS / N/A. No se agregan pantallas, mensajes de
  usuario ni reportes nuevos; los únicos textos nuevos son mensajes de error técnicos en logs de
  contenedor (arranque fallido, migración fallida), que no son superficie de producto visible al
  usuario final y por tanto no están sujetos a este principio.
- **III. Cero Alcance Fantasma**: PASS. El plan se limita estrictamente a lo declarado en la spec:
  cambio de proveedor EF Core + migraciones, Dockerfiles de Api/Web, contenedor y volumen de SQL
  Server, orquestación con un solo comando, manejo de secretos vía variables de entorno, y
  siembra de datos mock sin cambios. No se tocan reglas de negocio, pantallas ni módulos.
- **IV. Verificable por una Persona No Técnica**: PASS. Los criterios de aceptación (SC-001 a
  SC-007) se validan usando la aplicación (Swagger, pantallas del Frontend, reinicio de
  contenedores) o observando logs de arranque, sin necesidad de leer código ni consultar la base
  de datos directamente.
- **V. Datos del Usuario: Mínimos y Sin Secretos**: PASS. Es el principio rector de FR-009/SC-005:
  contraseña de SQL Server (`MSSQL_SA_PASSWORD`) y clave de firma JWT (`Jwt__SigningKey`) se
  inyectan vía variables de entorno desde un archivo `.env` no versionado (con `.env.example`
  como plantilla sin valores reales); ninguna imagen ni archivo versionado contiene el valor real.

**Re-check post Phase 1 (tras research.md, data-model.md, contracts/, quickstart.md)**: Sin
cambios respecto al check inicial. El diseño detallado (migración inicial única, imagen Developer
de SQL Server, healthcheck + reintento acotado, propagación de excepción sin captura ante fallo de
migración, `.env`/`.env.example`, Dockerfiles multi-stage estándar) no introduce ninguna
funcionalidad, pantalla, secreto embebido o complejidad no cubierta por el check inicial. Los 5
principios permanecen en PASS.

## Project Structure

### Documentation (this feature)

```text
specs/003-sqlserver-docker-migration/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   └── environment-variables.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
# Aplicación web ya separada en Backend + Frontend (funcionalidad 002); esta funcionalidad solo
# agrega los artefactos de contenerización y cambia el proveedor de datos del Backend.

src/
├── TimeClockSystem.Api/
│   ├── Dockerfile                     # NUEVO — imagen del Backend
│   ├── appsettings.json               # Sin cambios de forma (mismas claves; el valor real de
│   │                                     ConnectionStrings:DefaultConnection y Jwt:SigningKey
│   │                                     sigue viniendo de variables de entorno)
│   └── Program.cs                     # Se agrega espera acotada + reintentos antes de MigrateAsync
├── TimeClockSystem.Application/       # Sin cambios (casos de uso agnósticos del proveedor de datos)
├── TimeClockSystem.Domain/            # Sin cambios
├── TimeClockSystem.Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs    # Sin cambios de forma (el proveedor se configura en Program.cs)
│       ├── DbSeeder.cs                # Sin cambios (ya es idempotente — no resiembra si ya hay datos)
│       └── Migrations/                # Se reemplaza el contenido: migraciones SQLite existentes
│                                         se eliminan y se regenera una migración inicial para el
│                                         proveedor SQL Server (FR-001)
└── TimeClockSystem.Web/
    ├── Dockerfile                     # NUEVO — imagen del Frontend
    └── appsettings.json               # Sin cambios de forma (Api:BaseUrl se sobrescribe con
                                          Api__BaseUrl=http://api:8080/ dentro de docker-compose)

tests/
├── TimeClockSystem.Api.Tests/         # Sin cambios (CustomWebApiFactory sigue usando SQLite en
│                                         archivo temporal para aislamiento de pruebas)
└── TimeClockSystem.Web.Tests/         # Sin cambios

docker-compose.yml                     # NUEVO — orquesta sqlserver + api + web con un solo comando
.env.example                           # NUEVO — plantilla de variables de entorno/secretos (sin valores reales)
.dockerignore                          # NUEVO
```

**Structure Decision**: Se mantiene la estructura de Option 2 (Web application: backend +
frontend) ya establecida por la funcionalidad 002, sin mover ni renombrar proyectos. Esta
funcionalidad solo añade archivos de contenerización junto a cada proyecto (`Dockerfile` dentro
de `src/TimeClockSystem.Api/` y `src/TimeClockSystem.Web/`) y en la raíz del repositorio
(`docker-compose.yml`, `.env.example`, `.dockerignore`), y reemplaza el contenido de
`src/TimeClockSystem.Infrastructure/Data/Migrations/` para el nuevo proveedor SQL Server.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No hay violaciones de la constitución en este plan; esta sección no aplica.
