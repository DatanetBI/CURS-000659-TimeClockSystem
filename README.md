# TimeClockSystem

Sistema de gestión de asistencias, separado en dos aplicaciones independientes desplegables por
separado (ver `specs/002-split-backend-frontend/`):

- **`src/TimeClockSystem.Api`** — Backend Web API (Clean Architecture: `Domain` → `Application` →
  `Infrastructure` → `Api`). Concentra las reglas de negocio, el acceso a datos (EF Core + SQL
  Server) y la autenticación por token (JWT de expiración fija).
- **`src/TimeClockSystem.Web`** — Frontend ASP.NET Core MVC. Conserva las mismas pantallas y flujos
  de siempre, pero consume toda la información exclusivamente vía HTTP contra el Backend (sin EF
  Core ni acceso directo a la base de datos).

## Estructura

```text
src/
├── TimeClockSystem.Domain/          # Entidades y reglas de negocio puras, sin dependencias
├── TimeClockSystem.Application/     # Casos de uso, DTOs, interfaces de repositorio
├── TimeClockSystem.Infrastructure/  # EF Core, Identity, JWT, adaptadores
├── TimeClockSystem.Api/             # Host Web API (Controllers, Program.cs, Swagger)
└── TimeClockSystem.Web/             # Frontend MVC (Areas, clientes HTTP hacia la Api)

tests/
├── TimeClockSystem.Api.Tests/       # Unit tests de Domain + integración de la Api (WebApplicationFactory)
└── TimeClockSystem.Web.Tests/       # Pruebas de humo del Frontend (navegación, autenticación)
```

## Ejecutar con Docker (un solo comando)

Requiere Docker Engine + Docker Compose v2. Levanta los tres componentes (SQL Server, Backend y
Frontend) juntos, con los datos de SQL Server persistidos en un volumen:

```bash
cp .env.example .env
# Editar .env y completar MSSQL_SA_PASSWORD y JWT_SIGNING_KEY con valores propios
docker compose up --build
```

- Backend: `http://localhost:8080` (Swagger en `http://localhost:8080/swagger`)
- Frontend: `http://localhost:8081`

El Backend espera a que SQL Server esté lista antes de aplicar migraciones y sembrar datos mock,
y el Frontend le habla por el nombre de servicio `api` dentro de la red de contenedores (no
`localhost`). Ver `specs/003-sqlserver-docker-migration/quickstart.md` para la guía completa de
validación paso a paso.

## Ejecutar en desarrollo sin Docker

Ambos proyectos también pueden ejecutarse directamente en el host (dos terminales), apuntando a
una instancia de SQL Server accesible localmente (por ejemplo, la misma expuesta por
`docker compose up sqlserver` en el puerto 1433 del host):

```powershell
# Terminal 1 — Backend (por defecto en http://localhost:5073)
dotnet run --project src/TimeClockSystem.Api

# Terminal 2 — Frontend (por defecto en http://localhost:5095)
dotnet run --project src/TimeClockSystem.Web
```

La URL del Backend que usa el Frontend se configura en
`src/TimeClockSystem.Web/appsettings.json` (`Api:BaseUrl`), sobrescribible con la variable de
entorno `Api__BaseUrl`.

La primera vez que se levanta el Backend, migra la base de datos SQL Server y siembra datos y
usuarios de ejemplo — ver `specs/001-timeclock-spec-v1/MANUAL-USUARIO.md` para las credenciales
mock.

Swagger/OpenAPI del Backend (para probarlo sin el Frontend): `http://localhost:5073/swagger`.

## Configuración y secretos

Ningún secreto (clave de firma del token, cadenas de conexión) debe vivir en `appsettings.json`.
Para desarrollo local sin Docker, se configuran en `src/TimeClockSystem.Api/appsettings.Development.json`
(`Jwt:SigningKey`, `ConnectionStrings:DefaultConnection`); con Docker, vía el archivo `.env` (no
versionado — ver `.env.example`); en producción, vía variables de entorno
(`Jwt__SigningKey`, `ConnectionStrings__DefaultConnection`) o un gestor de secretos.

## Documentación de la especificación

- Migración a SQL Server y contenerización con Docker:
  `specs/003-sqlserver-docker-migration/`
- Guía de validación manual end-to-end (Docker): `specs/003-sqlserver-docker-migration/quickstart.md`
- Especificación, plan, tareas y contratos de la separación Backend/Frontend:
  `specs/002-split-backend-frontend/`
- Manual de usuario (v1.0, válido para ambos roles tras la separación):
  `specs/001-timeclock-spec-v1/MANUAL-USUARIO.md`
