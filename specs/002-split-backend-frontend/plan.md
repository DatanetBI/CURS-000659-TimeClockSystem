# Implementation Plan: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

**Branch**: `002-split-backend-frontend` | **Date**: 2026-09-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-split-backend-frontend/spec.md`

## Summary

Separar el proyecto único `TimeClockSystem.Web` en dos aplicaciones desplegables por separado: un
Backend `TimeClockSystem.Api` (Web API con capas Domain/Application/Infrastructure de Clean
Architecture) que concentra todas las reglas de negocio, el acceso a datos (EF Core + SQLite) y la
autenticación por token; y un Frontend `TimeClockSystem.Web` que conserva exactamente las mismas
pantallas, Areas y flujos de hoy, pero que deja de usar EF Core directamente y en su lugar consume
al Backend por HTTP, puenteando su cookie de sesión existente con el token emitido por el Backend.
Se añade una pantalla de auditoría de solo lectura para Administrador (FR-011a) para cumplir con el
principio constitucional de verificabilidad no técnica.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (sin cambios respecto al proyecto actual)

**Primary Dependencies**:
- Backend: ASP.NET Core Web API, EF Core 10 + `Microsoft.EntityFrameworkCore.Sqlite`, ASP.NET Core
  Identity (`AddIdentityCore` + `AddEntityFrameworkStores`), `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `Swashbuckle.AspNetCore` (documentación Swagger/OpenAPI).
- Frontend: ASP.NET Core MVC (sin cambios de framework), autenticación por cookie
  (`AddAuthentication().AddCookie(...)`, sin `AddEntityFrameworkStores` — ya no hay Identity local),
  `IHttpClientFactory` con clientes tipados por recurso (uno por Area) para llamar al Backend.

**Storage**: SQLite (sin cambios de motor); la base de datos y su `ApplicationDbContext` pasan a
vivir exclusivamente en `TimeClockSystem.Infrastructure`, propiedad única del Backend. El Frontend
no MUST tener ninguna cadena de conexión a base de datos.

**Testing**: xUnit (ya en uso). Se conserva `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`)
para pruebas de integración de ambos hosts (Backend y Frontend).

**Target Platform**: Servidor ASP.NET Core (mismo modelo de hosting que hoy; sin cambios de SO/nube).

**Project Type**: Web (frontend + backend) — dos aplicaciones ASP.NET Core independientes dentro de
la misma solución.

**Performance Goals**: 95% de las acciones típicas del usuario (registrar marca, guardar un cambio
de empleado/turno) se completan en menos de 5 segundos de extremo a extremo (SC-007).

**Constraints**:
- El token del Backend tiene expiración fija, sin renovación silenciosa (FR-002); al vencer, el
  usuario debe reautenticarse.
- El token nunca se expone al navegador/JavaScript; el Frontend lo retiene del lado servidor
  (FR-002a).
- El registro de una marca de asistencia se intenta una única vez, sin reintento automático
  (FR-012), para no duplicar marcas ante una operación no idempotente.
- Cero pérdida de datos históricos durante la migración (FR-010).
- Ningún secreto (clave de firma del token, cadena de conexión) MUST estar en texto plano en el
  repositorio (Principio V de la constitución).

**Scale/Scope**: Mismo volumen de datos y usuarios que el sistema actual (proyecto de
formación/demo, una sola organización). 7 Areas funcionales existentes (CentrosTrabajo,
ConsultaAsistencias, DiasFestivos, Empleados, Marcaje, Turnos, Cuenta) más 1 pantalla nueva
(Auditoría, FR-011a).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Evaluación | Notas |
|---|---|---|
| I. Simplicidad Ante Todo | ⚠️ Justificado | La separación en 4 proyectos de Backend (Domain/Application/Infrastructure/Api) es una petición explícita del usuario y una necesidad directa de esta funcionalidad (Historia de Usuario 3: verificación independiente del Backend), no complejidad especulativa. Ver Complexity Tracking. |
| II. Idioma y Mercado (Español de México) | ✅ Cumple | La nueva pantalla de Auditoría y todos los mensajes de error del Frontend/Backend se redactan en español de México; no hay montos monetarios involucrados en esta funcionalidad. |
| III. Cero Alcance Fantasma | ✅ Cumple | Todo lo que se construye (incluida la pantalla de Auditoría y el registro de auditoría) está explícitamente en `spec.md` (FR-011, FR-011a, SC-006) tras la sesión de clarificación; no se agrega nada fuera de ese conjunto. |
| IV. Verificable por una Persona No Técnica | ✅ Cumple | Cada criterio de aceptación (incluida la auditoría) se valida desde la interfaz de usuario, sin leer código ni consultar la base de datos directamente. |
| V. Datos del Usuario: Mínimos y Sin Secretos | ✅ Cumple (con diseño) | La clave de firma del token y la cadena de conexión a SQLite se gestionan vía configuración externa (`appsettings.Development.json` fuera de control de versiones / variables de entorno en producción), nunca en código fuente. |

**Resultado**: Gate superado. La única desviación (múltiples proyectos) queda justificada en
Complexity Tracking.

**Re-chequeo post-diseño (tras Fase 1)**: ✅ Se mantiene el resultado. `data-model.md` y
`contracts/` no introducen entidades, pantallas ni campos fuera de lo ya registrado en `spec.md`;
`research.md` resuelve la autenticación y el almacenamiento de secretos sin agregar dependencias
nuevas más allá de las ya justificadas (Swashbuckle.AspNetCore, JwtBearer).

## Project Structure

### Documentation (this feature)

```text
specs/002-split-backend-frontend/
├── plan.md              # Este archivo
├── research.md          # Fase 0
├── data-model.md         # Fase 1
├── quickstart.md         # Fase 1
├── contracts/            # Fase 1 (contratos REST por recurso)
└── tasks.md              # Fase 2 (/speckit-tasks, no se crea aquí)
```

### Source Code (repository root)

```text
src/
├── TimeClockSystem.Domain/              # Entidades + reglas de negocio puras, sin dependencias
│   ├── Empleado.cs, CentroTrabajo.cs, Turno.cs, AsignacionTurno.cs, DiaFestivo.cs, Marca.cs,
│   │   CredencialDeMarcaje.cs, RegistroAuditoria.cs (nuevo)
│   ├── PuntualidadCalculator.cs, GeofenceValidator.cs, ConsentimientoValidator.cs
│   └── IBiometricVerificationProvider.cs (+ NullBiometricVerificationProvider)
│
├── TimeClockSystem.Application/         # Casos de uso, DTOs de entrada/salida, interfaces de repos
│   ├── Empleados/, CentrosTrabajo/, Turnos/, AsignacionesTurno/, DiasFestivos/, Marcaje/,
│   │   Auditoria/ (un caso de uso por operación existente en cada Area actual)
│   ├── Auth/ (contratos de login/emisión de token)
│   └── Abstractions/ (IEmpleadoRepository, IMarcaRepository, ITokenService, IAuditLogService, ...)
│
├── TimeClockSystem.Infrastructure/      # EF Core, Identity, adaptadores externos
│   ├── Data/ (ApplicationDbContext, Migrations, DbSeeder — migrados tal cual desde Web)
│   ├── Identity/ (ApplicationUser, Roles — Identity ahora vive solo aquí)
│   ├── Auth/ (JwtTokenService: emite y valida el token de expiración fija)
│   ├── Auditoria/ (AuditLogService: persiste RegistroAuditoria vía EF Core)
│   └── Biometria/ (implementación de IBiometricVerificationProvider, relocalizada tal cual)
│
├── TimeClockSystem.Api/                 # Host ASP.NET Core Web API
│   ├── Controllers/ ([ApiController] por recurso: EmpleadosController, CentrosTrabajoController,
│   │   TurnosController, AsignacionesTurnoController, DiasFestivosController, MarcajeController,
│   │   ConsultaAsistenciasController, AuditoriaController, AuthController)
│   ├── Program.cs (DI, JWT Bearer, Swagger/OpenAPI, políticas de autorización por rol)
│   └── appsettings*.json (clave de firma del token y cadena de conexión vía configuración)
│
└── TimeClockSystem.Web/                 # Frontend MVC existente (sin cambios de UI)
    ├── Areas/{CentrosTrabajo, ConsultaAsistencias, DiasFestivos, Empleados, Marcaje, Turnos,
    │   Auditoria (nueva, FR-011a)}/  → Controllers y Views se conservan; los Controllers dejan de
    │   usar `ApplicationDbContext` y pasan a usar un cliente HTTP tipado del Area.
    ├── Infrastructure/ApiClients/ (clientes HTTP tipados + modelos de solicitud/respuesta propios
    │   del Frontend, uno por Area — sin proyecto de contratos compartido, ver research.md)
    ├── Infrastructure/Auth/ (bridging: al iniciar sesión llama a `POST /api/auth/login`, guarda el
    │   token recibido dentro de los claims de la cookie de autenticación existente)
    └── Program.cs (autenticación por cookie, sin Identity ni EF Core)

tests/
├── TimeClockSystem.Api.Tests/           # Unit tests de Domain/Application + integración de la API
│   (WebApplicationFactory) — reemplaza y extiende a TimeClockSystem.Tests
└── TimeClockSystem.Web.Tests/           # Integración del Frontend (WebApplicationFactory) contra
    un Backend de prueba/fake — renombrado desde TimeClockSystem.Tests
```

**Structure Decision**: Arquitectura Web (Opción 2: frontend + backend), con el Backend dividido en
4 proyectos siguiendo Clean Architecture (Domain → Application → Infrastructure → Api, dependencias
solo hacia adentro) según lo pedido explícitamente por el usuario. El Frontend conserva su
estructura por Areas actual; no se introduce un proyecto de contratos compartido entre Backend y
Frontend (cada uno define sus propios modelos de solicitud/respuesta), para que ambos permanezcan
genuinamente desacoplados en tiempo de compilación y solo se relacionen por HTTP (FR-007).

## Complexity Tracking

> Fill ONLY if Constitution Check has violations that must be justified

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|
| 4 proyectos de Backend (Domain, Application, Infrastructure, Api) en vez de 1 solo proyecto de API | Petición explícita del usuario ("Asegúrate de que el WebAPI tenga las capas Domain, Application e Infrastructure de Clean Architecture"); además, separar Domain/Application del acceso a datos permite cumplir la Historia de Usuario 3 (probar reglas de negocio sin infraestructura) y aísla las reglas de negocio existentes (puntualidad, geocerca, consentimiento) para que se preserven sin regresiones (FR-005). | Un único proyecto `TimeClockSystem.Api` con Controllers, EF Core y entidades mezclados reproduciría el mismo problema de acoplamiento que hoy tiene `TimeClockSystem.Web` (motivo original de este split) y dificultaría probar las reglas de negocio de forma aislada. |
| Nueva pantalla de Auditoría (Area `Auditoria` en el Frontend) | Requerida por el Principio IV de la constitución (verificable por una persona no técnica): sin una pantalla, el log de auditoría (FR-011) solo sería consultable técnicamente, violando ese principio. Confirmado explícitamente con el usuario durante esta planificación. | Dejar la auditoría solo en base de datos/log técnico incumpliría el Principio IV y dejaría SC-006 sin forma de verificarse desde la aplicación. |
