# Implementation Plan: TimeClockSystem v1.0 — Gestión de Asistencias

**Branch**: `001-timeclock-spec-v1` | **Date**: 2026-09-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-timeclock-spec-v1/spec.md`

## Resumen ejecutivo (lenguaje de negocio)

TimeClockSystem v1 se construye como **una sola aplicación web**, no como una plataforma de varios
servicios independientes. Esta decisión es la más importante del plan y se explica en detalle en
"Decisión arquitectónica principal" más abajo. En términos de negocio: se puede publicar en línea en
días, no en meses; hay una sola cosa que mantener, respaldar y monitorear; y el costo de hosting es el
de una aplicación pequeña, no el de una plataforma de 9 servicios con balanceo, colas de mensajes y una
base de datos de alta disponibilidad. A cambio, se acepta que escalar un módulo específico de forma
independiente (por ejemplo, si algún día "Marcaje" necesita 100× más capacidad que el resto) requeriría
una migración posterior — un costo que no se paga hoy si no hay evidencia de que se necesite.

La aplicación se entrega como una interfaz web responsiva (funciona igual de bien en el navegador de un
celular que en una computadora de escritorio); no se construye una app móvil nativa en v1. La base de
datos se inicia con datos de ejemplo (empleados, turnos, solicitudes) para que cualquier persona del
negocio pueda entrar y probar los criterios de aceptación de la spec sin necesidad de cargar datos
manualmente primero.

## Decisión arquitectónica principal: una aplicación, no nueve microservicios

Los documentos de arquitectura existentes en el repositorio (`docs/architecture/c4-containers.md` v3.0
y `docs/architecture/decisions/ADR-001-*.md`) describen una plataforma de **9 microservicios en .NET**,
con API Gateway, Redis (cache + bus de eventos + backplane de tiempo real), SignalR, réplicas de lectura
de PostgreSQL, Kubernetes, trazabilidad distribuida (OpenTelemetry) y *circuit breakers* (Polly).

**Este plan NO adopta esa arquitectura para v1.** Se documenta la razón explícitamente porque es una
decisión que se aparta de un documento de arquitectura previo:

- La constitución del proyecto (`.specify/memory/constitution.md`, Principio I — *Simplicidad Ante
  Todo*) exige elegir siempre la opción más simple y prohíbe construir complejidad anticipada en una
  versión inicial.
- El dueño del producto pidió explícitamente, al solicitar este plan: *"Prioriza la simplicidad por
  encima de todo... No agregues nada de infraestructura que la spec no requiera"* y que la v1 se pueda
  *"publicar online enseguida"*.
- Ninguno de los 47 requisitos funcionales de `spec.md` exige escalado independiente por módulo,
  múltiples equipos desplegando por separado, ni tiempo real distribuido entre varias instancias — los
  09 servicios, Kubernetes, Redis y el API Gateway de los documentos de arquitectura existen para
  problemas (escala multi-equipo, picos de carga dispares por módulo) que v1 no tiene todavía (v1 está
  dimensionada para hasta 500 empleados — ver Aclaración de escala en `spec.md`).

**En palabras de negocio**: construir 9 servicios con Kubernetes y Redis para una primera versión que
debe salir "enseguida" y que sirve a una sola empresa de hasta 500 empleados sería pagar por adelantado
una complejidad operativa (más equipos de DevOps, más piezas que pueden fallar, más tiempo de puesta en
marcha) para un problema de escala que todavía no existe. Si el negocio crece y un módulo concreto (por
ejemplo, el marcaje biométrico) necesita escalar de forma independiente, esa migración se hace cuando
haya evidencia real de esa necesidad — no antes.

## Technical Context

**Language/Version**: C# / .NET 10 (LTS) — consistente con la versión de plataforma ya elegida en
`docs/architecture/c4-containers.md`; lo que cambia es que hay **un** proyecto ASP.NET Core, no nueve.

**Primary Dependencies**: ASP.NET Core 10 (MVC + Razor Views), Entity Framework Core 10 (SQLite
provider), ASP.NET Core Identity (autenticación y roles del portal), Bootstrap 5 vía CDN (responsivo,
sin paso de compilación de frontend). Sin Redis, sin API Gateway, sin SignalR, sin Kubernetes, sin
Hangfire/Quartz (el `BackgroundService` incluido en ASP.NET Core basta para la exportación programada).

**Storage**: SQLite (archivo único `timeclock.db`). No requiere un servidor de base de datos separado
ni credenciales de infraestructura adicionales — se puede desplegar junto con la aplicación en un único
contenedor/instancia. EF Core aísla el acceso a datos, por lo que migrar a PostgreSQL en una fase
posterior (si el volumen de datos lo justifica) es un cambio de proveedor, no un rediseño.

**Testing**: xUnit + `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) para pruebas de
integración de extremo a extremo contra una base SQLite temporal; pruebas unitarias de las reglas de
negocio (cálculo de horas extra, tolerancia, saldo de vacaciones) aisladas del framework web.

**Target Platform**: Aplicación web ASP.NET Core multiplataforma, desplegable como un único contenedor
en cualquier PaaS (Azure App Service, Railway, Fly.io, etc.). Interfaz responsiva (Bootstrap) para
navegador de escritorio y móvil — no hay app móvil nativa en v1.

**Project Type**: Web — proyecto único (backend + frontend server-rendered en el mismo proceso), Opción
1 de la plantilla ("Single project"), no la opción de frontend/backend separados ni la de móvil+API.

**Performance Goals**: Los de `spec.md` (SC-001 a SC-010): marcaje en <15s, presencia visible en <10s,
recálculo de periodo en <5min, hasta 500 empleados activos sin degradar esos tiempos.

**Constraints**: Un solo proceso desplegable (sin orquestación de múltiples servicios); sin conexión en
vivo a un ERP/HRIS o proveedor biométrico real (contrato de archivo/interfaz solamente, según `spec.md`
§Aclaraciones); todo el texto de la interfaz en español de México; ningún secreto o cadena de conexión en
el código fuente (variables de entorno / `dotnet user-secrets` en desarrollo).

**Scale/Scope**: Hasta 500 empleados activos (SC-010); 7 historias de usuario; 47 requisitos
funcionales; una sola entidad legal (México).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Evaluación |
|---|---|
| I. Simplicidad Ante Todo | ✅ PASA. Un solo proyecto ASP.NET Core, una sola base de datos SQLite, sin colas/cache/gateway. Ver "Decisión arquitectónica principal". |
| II. Idioma y Mercado | ✅ PASA. Toda la interfaz, mensajes y reportes en español de México; montos en MXN (heredado de `spec.md`). |
| III. Cero Alcance Fantasma | ✅ PASA. El plan solo cubre los 47 FR de `spec.md`; el reconocimiento facial real y la conexión en vivo a ERP/HRIS quedan explícitamente fuera (mismo alcance que la spec), no se anticipa infraestructura para ellos más allá de un punto de extensión (FR-007). |
| IV. Verificable por una Persona No Técnica | ✅ PASA. Toda regla de negocio se valida desde la interfaz (formularios, paneles, reportes descargables); las pruebas automatizadas no sustituyen la validación manual descrita en `quickstart.md`. |
| V. Datos del Usuario: Mínimos y Sin Secretos | ✅ PASA. PIN de marcaje y contraseña de portal se guardan con hash (ASP.NET Core Identity `PasswordHasher`), nunca en texto plano ni en código; cadenas de conexión y claves vía variables de entorno. |

**Resultado**: Sin violaciones. No se requiere la tabla de Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-timeclock-spec-v1/
├── plan.md              # Este archivo
├── research.md          # Fase 0 — decisiones técnicas y su razón de negocio
├── data-model.md         # Fase 1 — entidades, campos, relaciones, reglas
├── quickstart.md         # Fase 1 — cómo levantar y validar la app manualmente
├── contracts/            # Fase 1 — contratos de exportación/importación y endpoints
└── tasks.md              # Fase 2 (/speckit-tasks) — no se crea en este comando
```

### Source Code (repository root)

```text
src/
└── TimeClockSystem.Web/              # Único proyecto desplegable (ASP.NET Core 10)
    ├── Areas/
    │   ├── Marcaje/                   # US1 — registrar marca, geofence, PIN, offline
    │   ├── Turnos/                    # US2 — catálogo de turnos, asignación masiva, pre-nómina
    │   ├── Solicitudes/               # US3 — permisos, vacaciones, incapacidades, aprobación
    │   ├── Supervisor/                # US4 — panel de presencia, reasignación de cobertura
    │   ├── Empleado/                  # US5 — autoservicio (historial, saldo, notificaciones)
    │   ├── Integraciones/             # US6 — exportación a nómina, importación de altas/bajas
    │   └── Reportes/                  # US7 — reportes, KPIs, bitácora de auditoría
    ├── Domain/                        # Entidades y reglas de negocio (sin dependencias de EF Core)
    ├── Infrastructure/
    │   ├── Data/                      # DbContext, migraciones, DbSeeder (datos mock)
    │   └── Identity/                  # ASP.NET Core Identity, roles, hashing de PIN
    ├── wwwroot/                       # CSS (Bootstrap vía CDN + overrides), JS de marcaje offline
    └── Program.cs

tests/
└── TimeClockSystem.Tests/
    ├── Unit/                          # Reglas de negocio: tolerancia, horas extra, saldo vacaciones
    └── Integration/                   # Flujos de extremo a extremo vía WebApplicationFactory
```

**Structure Decision**: Un único proyecto web (`src/TimeClockSystem.Web`) organizado en carpetas por
área funcional (una por historia de usuario), no en microservicios separados. Esto es intencional: cada
área puede evolucionar a un proyecto/servicio independiente en el futuro si la evidencia de escala lo
justifica (la separación por carpetas ya refleja los mismos límites de dominio que los bounded contexts
de `docs/architecture/domain-model.md`), pero en v1 comparten proceso, despliegue y base de datos.

## Complexity Tracking

*No aplica — el Constitution Check no encontró violaciones.*
