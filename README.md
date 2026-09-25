# TimeClockSystem

Sistema de gestión de asistencias, separado en dos aplicaciones independientes desplegables por
separado (ver `specs/002-split-backend-frontend/`):

- **`src/TimeClockSystem.Api`** — Backend Web API (Clean Architecture: `Domain` → `Application` →
  `Infrastructure` → `Api`). Concentra las reglas de negocio, el acceso a datos (EF Core + SQLite) y
  la autenticación por token (JWT de expiración fija).
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

## Ejecutar en desarrollo

Ambos proyectos se ejecutan por separado (dos terminales), y el Frontend llama al Backend por HTTP:

```powershell
# Terminal 1 — Backend (por defecto en http://localhost:5073)
dotnet run --project src/TimeClockSystem.Api

# Terminal 2 — Frontend (por defecto en http://localhost:5095)
dotnet run --project src/TimeClockSystem.Web
```

La URL del Backend que usa el Frontend se configura en
`src/TimeClockSystem.Web/appsettings.json` (`Api:BaseUrl`), sobrescribible con la variable de
entorno `Api__BaseUrl`.

La primera vez que se levanta el Backend, migra la base de datos SQLite y siembra datos y usuarios
de ejemplo — ver `specs/001-timeclock-spec-v1/MANUAL-USUARIO.md` para las credenciales mock.

Swagger/OpenAPI del Backend (para probarlo sin el Frontend): `http://localhost:5073/swagger`.

## Configuración y secretos

Ningún secreto (clave de firma del token, cadenas de conexión) debe vivir en `appsettings.json`.
Para desarrollo local, se configuran en `src/TimeClockSystem.Api/appsettings.Development.json`
(`Jwt:SigningKey`); en producción, vía variables de entorno (`Jwt__SigningKey`,
`ConnectionStrings__DefaultConnection`) o un gestor de secretos.

## Documentación de la especificación

- Especificación, plan, tareas y contratos de la separación Backend/Frontend:
  `specs/002-split-backend-frontend/`
- Guía de validación manual end-to-end: `specs/002-split-backend-frontend/quickstart.md`
- Manual de usuario (v1.0, válido para ambos roles tras la separación):
  `specs/001-timeclock-spec-v1/MANUAL-USUARIO.md`
