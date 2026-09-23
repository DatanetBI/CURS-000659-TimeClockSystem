# Implementation Plan: TimeClockSystem v1.0 — Gestión de Asistencias

**Branch**: `001-timeclock-spec-v1` | **Date**: 2026-09-22 (revisado) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-timeclock-spec-v1/spec.md`

> **Revisión de este plan**: esta versión reemplaza la anterior. El cambio principal es de **alcance**,
> no de arquitectura: se reduce v1.0 a los 6 módulos esenciales para que el producto pueda publicarse en
> línea de inmediato, probarse paso a paso, y funcionar en modo conectado (sin soporte offline). El resto
> de `spec.md` (que sigue vigente como visión completa del producto) se entrega en una v1.1 posterior.

## Resumen ejecutivo (lenguaje de negocio)

Esta revisión del plan reduce deliberadamente lo que se construye en la primera versión, para poder
publicarla en línea lo antes posible y dejar que el negocio la pruebe con datos reales de inmediato. En
lugar de construir las 7 historias de usuario completas de `spec.md` de una sola vez, v1.0 entrega **6
módulos**, en un orden pensado para que cada uno se pueda probar en la aplicación web tan pronto como
está listo, sin esperar a que todo el sistema esté terminado. Vacaciones/permisos/incapacidades y la
exportación a nómina — que son las partes que más dependen de reglas legales y de un flujo de aprobación
— se dejan para una v1.1, junto con el registro de asistencia sin conexión a internet (offline). v1.0 es
una aplicación 100% en línea: el empleado marca su asistencia con conexión a internet; el caso de un
empleado sin señal se resuelve en la siguiente versión.

Sigue siendo **una sola aplicación web** (no 9 microservicios) — ver "Decisión arquitectónica principal"
en la sección siguiente, que no cambia con esta revisión. Lo que cambia es cuánto de la funcionalidad
total de `spec.md` entra en esta primera entrega.

## Decisión arquitectónica principal: una aplicación, no nueve microservicios

*(Sin cambios respecto a la versión anterior de este plan — se conserva aquí por referencia.)*

Los documentos `docs/architecture/c4-containers.md` (v3.0) y `ADR-001` describen una plataforma de 9
microservicios con API Gateway, Redis, SignalR y Kubernetes. Este plan no adopta esa arquitectura: la
constitución del proyecto (Principio I — *Simplicidad Ante Todo*) y la instrucción explícita de negocio
("prioriza la simplicidad... no agregues infraestructura que la spec no requiera") tienen precedencia.
Con el alcance de v1.0 reducido a 6 módulos (ver abajo), esta decisión queda todavía más justificada: hay
aún menos motivo para 9 servicios independientes cuando la primera entrega ni siquiera cubre las 7
historias de usuario completas.

## Alcance de v1.0 (este plan) vs. v1.1 (diferido)

### Incluido en v1.0 — 6 módulos, en este orden de construcción

| # | Módulo | Historias/Requisitos de `spec.md` que cubre | Requisitos que quedan fuera de v1.0 dentro de esa misma historia |
|---|---|---|---|
| 1 | **Centros de trabajo** | Entidad Geofence/Centro de Trabajo (soporta FR-006) | — |
| 2 | **Empleados y credenciales de marcaje** | FR-004 (US1); entidad Empleado | Roles diferenciados de FR-043 se simplifican (ver "Roles" abajo). FR-002/FR-003 (validar el PIN al marcar) se cubren en el Módulo 5, no aquí — este módulo solo asigna/restablece el PIN. |
| 3 | **Turnos y asignación de turnos** | FR-011, FR-013, FR-015 (US2) | FR-012 (reparto de horas de turno nocturno entre dos fechas) y FR-016 a FR-019 (motor de horas extra/pre-nómina) se difieren — dependen del mismo cálculo que Incidencias, diferido. FR-014 (excluir empleados con restricción individual aprobada) no tiene efecto real en v1.0 porque depende de Incidencias, diferido — el resumen de asignados/excluidos (T036) existe, pero nunca excluirá a nadie hasta v1.1. |
| 4 | **Días festivos** | Entidad Calendario de Festivos (soporta clasificación simple de "día festivo" en el portal de consulta) | Cálculo de "horas festivas" con factores de pago (FR-017/018) diferido junto con pre-nómina |
| 5 | **Registro de asistencias (empleado)** | FR-001, FR-002, FR-003, FR-005, FR-006, FR-007 (punto de integración, no implementación), FR-010, FR-046 (consentimiento de geolocalización) (US1) | FR-008/FR-009 (captura y sincronización offline) y CL2/CL10/CL14 se difieren — v1.0 requiere conexión a internet para marcar |
| 6 | **Portal de consulta de asistencias (administrador)** | Consulta/filtro de marcas por empleado, fecha y centro de trabajo (versión acotada de US5/US7) | Panel de presencia en vivo con actualización automática (US4, FR-031/032), reportes con KPIs y exportación PDF/Excel/CSV (FR-037/038/039... revisar numeración vigente), y bitácora de auditoría (FR-044 a FR-047) se difieren |

### Diferido explícitamente a v1.1 (no se construye en v1.0)

- **Incidencias y solicitudes** (User Story 3 completa): vacaciones, permisos, incapacidades, flujo de
  aprobación y escalamiento.
- **Exportación/Importación** (User Story 6 completa): exportación a nómina/ERP, importación de
  altas/bajas, y todo lo relacionado con `contracts/export-import.md`.
- **Registro de asistencia sin conexión (offline)**: parte de FR-008/FR-009 y los casos límite CL2, CL10,
  CL14 de User Story 1. v1.0 asume que el empleado tiene conexión a internet al momento de marcar.
- **Marcaje por RFID y terminal físico**: FR-001 se acota en v1.0 a los canales portal web y PIN de
  kiosco; RFID y terminal físico (hardware de terceros) quedan para v1.1, igual que la app nativa.
- **Motor de pre-nómina** (parte de User Story 2): clasificación de horas extra por tipo con factores de
  pago legales, reparto de turno nocturno entre fechas, recálculo automático de periodos.
- **Panel de presencia en tiempo real y reasignación de cobertura** (User Story 4 completa).
- **Reportes/KPIs exportables y bitácora de auditoría con inmutabilidad garantizada** (partes de User
  Story 7). El "Portal de consulta" de v1.0 es una versión mínima de solo lectura, no el módulo completo
  de reportería y auditoría.
- **Diferenciación completa de roles** (FR-043: supervisor, RRHH, nómina, auditor, TI, ejecutivo): v1.0
  solo implementa **Empleado** y **Administrador** (ver justificación abajo). Los demás roles se
  introducen junto con las funciones que realmente los necesitan (aprobaciones → supervisor/RRHH;
  exportación → TI/nómina; auditoría → auditor).

**Por qué se difiere de esta forma (lenguaje de negocio)**: cada pieza diferida depende de otra pieza
también diferida — no tiene sentido construir un flujo de aprobación (Incidencias) sin antes tener
empleados y turnos, ni construir la exportación a nómina antes de tener un cálculo de horas extra
confiable (motor de pre-nómina), ni ese cálculo antes de tener marcas de asistencia reales. Diferir todo
ese bloque junto, de una vez, evita construir la mitad de un flujo que el usuario no podría probar de
extremo a extremo todavía. Los 6 módulos de v1.0, en cambio, forman una cadena donde cada eslabón es
usable por sí mismo apenas se termina (ver "Orden de construcción incremental").

## Orden de construcción incremental

Cada módulo se apoya únicamente en los módulos anteriores, para que el usuario administrador pueda
probar la aplicación web paso a paso sin esperar a que todo esté terminado.

1. **Centros de trabajo** → *Se puede probar*: el administrador crea/edita/lista centros de trabajo
   (nombre, coordenadas, radio del geofence) desde el portal web.
2. **Empleados y credenciales de marcaje** (depende de 1: un empleado se asigna a un centro de trabajo) →
   *Se puede probar*: el administrador da de alta un empleado, lo asocia a un centro de trabajo y le
   asigna un PIN de marcaje.
3. **Turnos y asignación de turnos** (depende de 2: se asignan turnos a empleados ya existentes) → *Se
   puede probar*: el administrador crea un turno (horario, tolerancia) y lo asigna a uno o varios
   empleados, individual o masivamente.
4. **Días festivos** (independiente, se ubica aquí para estar listo antes del marcaje) → *Se puede
   probar*: el administrador da de alta fechas festivas en un calendario.
5. **Registro de asistencias** (depende de 1, 2 y 3: necesita centro de trabajo, empleado+PIN y,
   idealmente, un turno asignado) → *Se puede probar*: un empleado marca entrada/salida/receso desde el
   portal (con geolocalización validada contra su centro de trabajo, o con número de empleado + PIN), y
   ve la hora registrada de inmediato.
6. **Portal de consulta de asistencias** (depende de 5: necesita marcas ya registradas para mostrar algo)
   → *Se puede probar*: el administrador filtra y consulta las marcas registradas por empleado, fecha o
   centro de trabajo, viendo si cada una fue puntual/tardía (según el turno) y si cayó en día festivo.

Este orden coincide con la secuencia de tareas que generará `/speckit-tasks`: cada módulo es un conjunto
de tareas cerrado y demostrable antes de empezar el siguiente.

## Simplificación de roles para v1.0

**Decisión**: v1.0 implementa únicamente dos roles — **Empleado** (marca su propia asistencia) y
**Administrador** (gestiona centros de trabajo, empleados, turnos, festivos, y consulta las asistencias
de todos). Los seis roles restantes de FR-043 (supervisor, RRHH, nómina, auditor, TI, ejecutivo) no se
implementan todavía.

**En palabras de negocio**: en v1.0 nadie aprueba nada (Incidencias está diferido) ni exporta nada a
nómina (Exportación está diferida) ni consulta una bitácora de auditoría (diferida) — por lo tanto, los
roles que existen únicamente para esas tareas no tienen todavía una función que cumplir. Construirlos
ahora sería anticipar permisos para pantallas que aún no existen, lo cual contradice el Principio I de
la constitución (nada de complejidad anticipada). Cada rol se agrega en la versión donde su
funcionalidad correspondiente se construye.

## Datos mock para la prueba de concepto

Al iniciar la aplicación por primera vez, si la base de datos está vacía, se siembran automáticamente:

- **3 centros de trabajo** de ejemplo (p. ej. "Oficina Central CDMX", "Planta Querétaro", "Campo Norte"),
  cada uno con coordenadas y radio de geofence.
- **2 usuarios Administrador** de ejemplo, con su usuario/contraseña de portal.
- **~12 empleados** de ejemplo distribuidos entre los centros de trabajo, cada uno con su número de
  empleado y un PIN de marcaje (4-6 dígitos) ya asignado, para poder probar el marcaje de inmediato.
- **2-3 turnos** de ejemplo (fijo diurno, nocturno, flexible), asignados a los empleados de ejemplo.
- **Un calendario de festivos** de ejemplo con 2-3 fechas (incluyendo una próxima, para poder probar la
  clasificación de "día festivo" en el portal de consulta).
- **Algunas marcas de asistencia** de ejemplo de días anteriores, para que el "Portal de consulta" no se
  vea vacío la primera vez que un administrador entra a probarlo.

**En palabras de negocio**: apenas se publica la aplicación, cualquier persona del negocio puede entrar,
iniciar sesión con un usuario de ejemplo y probar los 6 módulos de punta a punta sin tener que cargar
manualmente centros de trabajo, empleados o turnos antes de empezar.

## Technical Context

**Language/Version**: C# / .NET 10 (LTS).

**Primary Dependencies**: ASP.NET Core 10 (MVC + Razor Views), Entity Framework Core 10 (SQLite
provider), ASP.NET Core Identity (2 roles: Empleado, Administrador), Bootstrap 5 vía CDN (responsivo,
sin paso de build de frontend). Sin Redis, sin API Gateway, sin SignalR, sin motor de trabajos en
segundo plano (no hay exportación programada en v1.0).

**Storage**: SQLite (archivo único `timeclock.db`), sembrado con datos mock al primer arranque.

**Testing**: xUnit + `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) para los 6 módulos de
v1.0; pruebas unitarias de las reglas de negocio que sí aplican en v1.0 (validación de geofence,
tolerancia de turno para el indicador puntual/tardío, unicidad de número de empleado y PIN).

**Target Platform**: Aplicación web ASP.NET Core, un solo contenedor, desplegable en cualquier PaaS.
Interfaz responsiva (Bootstrap) para escritorio y móvil — sin app nativa, **sin modo offline** en v1.0
(requiere conexión a internet para marcar).

**Project Type**: Web — proyecto único (Opción 1 de la plantilla).

**Performance Goals**: SC-001 (marcaje <15s), SC-002 (marcas fuera de geofence excluidas), SC-010 (hasta
500 empleados). Los demás Success Criteria de `spec.md` (SC-003 a SC-009) corresponden a módulos
diferidos a v1.1 y no aplican a esta entrega.

**Constraints**: Un solo proceso desplegable; requiere conexión a internet (sin cola offline); sin
conexión en vivo a ERP/HRIS/proveedor biométrico (ninguno de los dos aplica a los 6 módulos de v1.0);
interfaz en español de México; sin secretos en código fuente.

**Scale/Scope**: Hasta 500 empleados activos; 6 módulos; una sola entidad legal (México).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Evaluación |
|---|---|
| I. Simplicidad Ante Todo | ✅ PASA — y de forma más contundente que la revisión anterior: menos módulos, sin modo offline, sin motor de trabajos en segundo plano, solo 2 roles. |
| II. Idioma y Mercado | ✅ PASA. Interfaz en español de México, MXN donde aplique. |
| III. Cero Alcance Fantasma | ✅ PASA. Cada módulo de v1.0 mapea a requisitos ya existentes en `spec.md` (tabla de alcance arriba); nada se construye que no esté ya en la spec. Lo diferido se documenta explícitamente, no se descarta. |
| IV. Verificable por una Persona No Técnica | ✅ PASA. Cada uno de los 6 módulos tiene un flujo de prueba manual en `quickstart.md`, verificable desde la interfaz. |
| V. Datos del Usuario: Mínimos y Sin Secretos | ✅ PASA. PIN y contraseña de portal con hash; sin secretos en código; los datos mock son ficticios, no datos reales de empleados. |

**Resultado**: Sin violaciones. No se requiere tabla de Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-timeclock-spec-v1/
├── plan.md              # Este archivo
├── research.md          # Fase 0 — decisiones técnicas y su razón de negocio
├── data-model.md         # Fase 1 — entidades de v1.0 + referencia a las diferidas
├── quickstart.md         # Fase 1 — validación manual módulo por módulo
├── contracts/            # Fase 1 — mapa de acciones (export-import.md queda diferido)
└── tasks.md              # Fase 2 (/speckit-tasks) — no se crea en este comando
```

### Source Code (repository root)

```text
src/
└── TimeClockSystem.Web/
    ├── Areas/
    │   ├── CentrosTrabajo/            # Módulo 1
    │   ├── Empleados/                 # Módulo 2 (incluye credenciales de marcaje)
    │   ├── Turnos/                    # Módulo 3
    │   ├── DiasFestivos/              # Módulo 4
    │   ├── Marcaje/                   # Módulo 5
    │   └── ConsultaAsistencias/       # Módulo 6
    ├── Domain/
    ├── Views/
    │   └── Shared/                    # _Layout.cshtml compartido (T007)
    ├── Infrastructure/
    │   ├── Data/                      # DbContext, migraciones, DbSeeder (datos mock)
    │   └── Identity/                  # ASP.NET Core Identity, 2 roles, hashing de PIN
    ├── wwwroot/
    └── Program.cs

tests/
└── TimeClockSystem.Tests/
    ├── Unit/
    └── Integration/
```

Las carpetas `Incidencias/`, `Supervisor/`, `Integraciones/` y `Reportes/` (de la versión anterior de
este plan) se posponen a v1.1; no se crean todavía para no dejar código o pantallas a medio terminar.

**Structure Decision**: Un único proyecto web, organizado por los 6 módulos de v1.0. Cada `Area` puede
extenderse en v1.1 con las funciones diferidas (p. ej. `Marcaje` gana soporte offline; `Turnos` gana el
motor de pre-nómina) sin necesitar una reestructuración previa.

## Complexity Tracking

*No aplica — el Constitution Check no encontró violaciones.*
