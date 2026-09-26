# TimeClockSystem

Sistema de gestión de asistencias, separado en dos aplicaciones independientes desplegables por
separado (ver `specs/002-split-backend-frontend/`):

- **`src/TimeClockSystem.Api`** — Backend Web API (Clean Architecture: `Domain` → `Application` →
  `Infrastructure` → `Api`). Concentra las reglas de negocio, el acceso a datos (EF Core + SQL
  Server) y la autenticación por token (JWT de expiración fija).
- **`src/TimeClockSystem.Web`** — Frontend ASP.NET Core MVC. Conserva las mismas pantallas y flujos
  de siempre, pero consume toda la información exclusivamente vía HTTP contra el Backend (sin EF
  Core ni acceso directo a la base de datos).

## Stack

| Componente | Tecnología |
|---|---|
| Lenguaje / Framework | C# 13 / .NET 10 (ASP.NET Core 10) |
| Backend | `TimeClockSystem.Api` — Clean Architecture (Domain → Application → Infrastructure → Api) |
| Frontend | `TimeClockSystem.Web` — ASP.NET Core MVC, consume el Backend solo por HTTP |
| Persistencia | Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Autenticación | ASP.NET Core Identity + JWT Bearer (token de expiración fija) |
| Documentación de API | Swagger / OpenAPI (`Swashbuckle.AspNetCore`) |
| Contenerización | Docker + Docker Compose (SQL Server + Api + Web) |
| Pruebas | xUnit (`TimeClockSystem.Api.Tests`, `TimeClockSystem.Web.Tests`) |

Detalle exhaustivo (entidades, casos de uso, repositorios, endpoints, validaciones ↔ requisitos):
ver [`ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md`](ENTREGABLES/ARQUITECTURA/Arquitectura_e_Implementacion.md) §5 ("Datos Técnicos").

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
usuarios de ejemplo (ver credenciales mock más abajo).

Swagger/OpenAPI del Backend (para probarlo sin el Frontend): `http://localhost:5073/swagger`.

## Credenciales de prueba (mock)

Datos ficticios sembrados automáticamente por `DbSeeder` la primera vez que se levanta el Backend
(con o sin Docker), únicamente para poder probar el sistema de inmediato. **No son datos reales y
no deben usarse en un entorno de producción.** Detalle completo de cada rol y pantalla en
[`specs/001-timeclock-spec-v1/MANUAL-USUARIO.md`](specs/001-timeclock-spec-v1/MANUAL-USUARIO.md).

**Administradores** (acceso al portal web):

| Usuario | Contraseña |
| --- | --- |
| admin1 | Admin123! |
| admin2 | Admin123! |

**Empleados** (acceso al portal web y marcaje por PIN de kiosco) — los 12 comparten la misma
contraseña de portal (`Empleado123!`) y el mismo PIN de marcaje (`1234`):

| Número de empleado | Nombre | Centro de trabajo |
| --- | --- | --- |
| E001 | Juan Pérez | Oficina Central CDMX |
| E002 | María García | Oficina Central CDMX |
| E003 | Carlos López | Oficina Central CDMX |
| E004 | Ana Martínez | Oficina Central CDMX |
| E005 | Luis Hernández | Planta Querétaro |
| E006 | Sofía Ramírez | Planta Querétaro |
| E007 | Diego Torres | Planta Querétaro |
| E008 | Valentina Flores | Planta Querétaro |
| E009 | Miguel Sánchez | Campo Norte |
| E010 | Camila Rivera | Campo Norte |
| E011 | Jorge Díaz | Campo Norte |
| E012 | Fernanda Cruz | Campo Norte |

Todos estos empleados ya tienen el turno "Fijo diurno" asignado para hoy y ya otorgaron su
consentimiento de geolocalización, para poder probar el marcaje de inmediato.

## Configuración y secretos

Ningún secreto (clave de firma del token, cadenas de conexión) debe vivir en `appsettings.json`.
Para desarrollo local sin Docker, se configuran en `src/TimeClockSystem.Api/appsettings.Development.json`
(`Jwt:SigningKey`, `ConnectionStrings:DefaultConnection`); con Docker, vía el archivo `.env` (no
versionado — ver `.env.example`); en producción, vía variables de entorno
(`Jwt__SigningKey`, `ConnectionStrings__DefaultConnection`) o un gestor de secretos.

## Publicar en MonsterASP.NET

A diferencia de las secciones anteriores (pensadas para Docker en desarrollo local), esto describe
cómo publicar el sistema en un hosting IIS + SQL Server tradicional. Guía completa y verificada,
con enlaces a la documentación oficial consultada:
[`ENTREGABLES/EVIDENCIAS/MosterASPNET-Publish.md`](ENTREGABLES/EVIDENCIAS/MosterASPNET-Publish.md).

Resumen de los pasos:

1. **Verifica tu plan** — la solución necesita 2 sitios (Api + Web); confirma que tu plan incluya
   al menos 2 sitios/subdominios y que **.NET Core 10** esté disponible para tu cuenta.
2. **Crea la base de datos**: panel → *Databases* → *Add database* → tipo **MSSQL**. El panel te
   entrega la cadena de conexión real.
3. **Crea dos sitios**: panel → *Websites* → *Add website*, uno para `TimeClockSystem.Api` y otro
   para `TimeClockSystem.Web`, cada uno con runtime **.NET Core 10**.
4. **Configura los secretos sin tocar código** — panel → *Websites → Manage website → Scripting →
   Environment Variables* (MonsterASP.NET usa la misma convención `Section__Clave` que ya lee este
   proyecto):
   - En el sitio del **Backend**: `ConnectionStrings__DefaultConnection` (la cadena del paso 2) y
     `Jwt__SigningKey` (una clave propia — nunca el placeholder de `appsettings.Development.json`).
   - En el sitio del **Frontend**: `Api__BaseUrl` con la URL pública del sitio del Backend.
5. **Publica desde Visual Studio**: activa *WebDeploy* en cada sitio, descarga su
   `.publishSettings`, y en Visual Studio usa *Publish → Import Profile* sobre cada proyecto
   (`TimeClockSystem.Api` y `TimeClockSystem.Web` por separado).
6. **Activa HTTPS** con un clic (Let's Encrypt gratuito) en ambos sitios.
7. **Verifica**: `https://<sitio-api>/swagger` debe cargar directamente (Swagger ya está visible en
   todo ambiente, no solo en Development); prueba el login con las credenciales mock y confirma que
   el Frontend le habla al Backend por su URL pública, no por `localhost`.

La migración de base de datos y la siembra de datos mock (`DbSeeder`) ocurren automáticamente en el
primer arranque, igual que con Docker — no se ejecuta ningún comando manual en el servidor.

## Evidencia de IA

Este proyecto se construyó con Claude Code (Spec Kit) como asistente de desarrollo. Toda la
evidencia del proceso —prompts reales, decisiones aceptadas/rechazadas, defectos encontrados y
corregidos, y el historial de commits— está documentada en detalle en
[`ENTREGABLES/EVIDENCIAS/`](ENTREGABLES/EVIDENCIAS/):

- [`Evidencias.md`](ENTREGABLES/EVIDENCIAS/Evidencias.md) — Log cronológico completo del proceso (prompts, respuestas, diffs, decisiones), desde la documentación funcional inicial hasta la migración a SQL Server/Docker.
- [`UsosEsperados.md`](ENTREGABLES/EVIDENCIAS/UsosEsperados.md) — Usos esperados de la IA (generación de código, refactorización, debugging, documentación, pruebas, diseño arquitectónico), cada uno con evidencia real del proyecto.
- [`CriterioHumano.md`](ENTREGABLES/EVIDENCIAS/CriterioHumano.md) — Qué se aceptó, qué se corrigió y por qué la propuesta final es adecuada.
- [`Auditoria-Secretos.md`](ENTREGABLES/EVIDENCIAS/Auditoria-Secretos.md) — Verificación, antes de cada `git push`, de que no se exponen claves ni datos sensibles (árbol de trabajo + historial completo de Git).
- [`MosterASPNET-Publish.md`](ENTREGABLES/EVIDENCIAS/MosterASPNET-Publish.md) — Guía verificada de publicación en MonsterASP.NET (ver sección anterior).
- [`Presentacion.pptx`](ENTREGABLES/EVIDENCIAS/Presentacion.pptx) — Presentación ejecutiva: problema → especificaciones → solución, arquitectura, uso de IA y demo.
- [`ENTREGABLES/COMMITS/CommitsRepo.md`](ENTREGABLES/COMMITS/CommitsRepo.md) — Historial completo de los commits del repositorio (hash, fecha, mensaje, archivos/líneas modificadas).

## Documentación de la especificación

- Migración a SQL Server y contenerización con Docker:
  `specs/003-sqlserver-docker-migration/`
- Guía de validación manual end-to-end (Docker): `specs/003-sqlserver-docker-migration/quickstart.md`
- Especificación, plan, tareas y contratos de la separación Backend/Frontend:
  `specs/002-split-backend-frontend/`
- Manual de usuario (v1.0, válido para ambos roles tras la separación):
  `specs/001-timeclock-spec-v1/MANUAL-USUARIO.md`
