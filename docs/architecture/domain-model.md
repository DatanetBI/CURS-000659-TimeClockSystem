# Modelo de Dominio — Time Clock System

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Estado | Borrador para revisión |
| Autor | Software Architect |
| Fecha | 2026-09-21 |
| Insumos | `docs/specs/functional/01-vision-document.md`, `docs/specs/functional/02-user-stories.md` |

## 1. Enfoque

El dominio se modela con **Domain-Driven Design (DDD)**: se identifican **bounded contexts** alineados 1:1 con los módulos funcionales del documento de visión, cada uno con sus propios **aggregate roots**, invariantes y ciclo de vida. Los contextos no comparten entidades directamente entre sí — se referencian por **identidad (ID)** y se comunican mediante **eventos de dominio** (ver §11) — precisamente porque cada bounded context se implementa como un **microservicio independiente** (ver `c4-containers.md` §1), este desacoplamiento deja de ser una preferencia de diseño interno y pasa a ser un requisito de arquitectura de código: dos servicios en despliegues distintos se integran por red (API síncrona) o por mensajería (eventos asíncronos) para todo lo que sea **comando** (mutar el estado de otro contexto aplicando sus reglas de negocio).

Esto es independiente de que, a nivel de infraestructura, los servicios compartan una única base de datos física (`c4-containers.md` §1.2–§1.3, ver **ADR-001**) con acceso abierto entre esquemas: los esquemas son una agrupación lógica de tablas, no una frontera de aislamiento, y cualquier servicio puede leer, unir (`JOIN`) o incluso transaccionar directamente contra el esquema de otro cuando el caso de uso es una **consulta** (CQRS) o exige **consistencia atómica inmediata**. Lo que no cambia es quién es responsable de codificar las invariantes de cada agregado: ese acceso abierto se usa para leer o para transacciones puntuales de consistencia, nunca para que un servicio reimplemente en su propio código las reglas de negocio de un bounded context ajeno.

El **Identity Service** (§2) es quien posee los datos de **Empleado** y **Usuario**; los demás servicios no los duplican como fuente de verdad — los consumen por referencia (`EmpleadoId`) vía llamada síncrona a su API o mediante una réplica local de solo lectura mantenida por eventos (*Open Host Service* + *Published Language*, no un *shared kernel* de código compartido).

### 1.1 Convenciones de los diagramas

- `classDiagram` de Mermaid, un diagrama por bounded context.
- `<<AggregateRoot>>`: entidad raíz que garantiza sus invariantes y es la única puerta de entrada para modificar el agregado.
- `<<ValueObject>>`: objeto inmutable sin identidad propia, comparado por valor.
- `<<enumeration>>`: conjunto cerrado de valores.
- `-->`: asociación / referencia por ID hacia otro agregado.
- `*--`: composición (el value object no existe fuera del agregado que lo contiene).
- `..>`: dependencia de tipo (p. ej. un atributo tipado con un enum).

### 1.2 Trazabilidad Historia de Usuario → Bounded Context → Agregado

| Historia | Bounded Context | Agregado(s) raíz principal(es) |
|---|---|---|
| US-001 Marcaje Web/Móvil | Clocking & Attendance Capture (§3) | `Marca` |
| US-002 Marcaje Biométrico con Geofencing | Clocking & Attendance Capture (§3) | `Marca`, `Geofence` |
| US-003 Marcaje Offline y Sincronización | Clocking & Attendance Capture (§3) | `Marca` |
| US-004 Configuración de Turnos Rotativos y Fijos | Scheduling & Shift Management (§4) | `Turno` |
| US-005 Asignación Masiva de Calendarios | Scheduling & Shift Management (§4) | `CargaMasivaCalendario`, `AsignacionTurno` |
| US-006 Cálculo Automático de Horas Extra y Recargos | Pre-Payroll Engine (§5) | `ConsolidadoPreNomina` |
| US-007 Detección Automática de Tardanzas y Ausencias | Pre-Payroll Engine (§5) | `Incidencia` |
| US-008 Solicitud de Permisos y Vacaciones | Absence Management (§6) | `Solicitud`, `SaldoVacaciones` |
| US-009 Aprobación de Incidencias por Jefatura | Absence Management (§6) | `Solicitud`, `DecisionAprobacion` |
| US-010 Consulta de Tarjeta de Asistencia | Employee Self-Service (§7) | `TarjetaAsistenciaView` (read model) |
| US-011 Monitor de Presencia en Tiempo Real | Manager Self-Service (§8) | `EstadoPresenciaEmpleado` (read model) |
| US-011b Reasignación Rápida de Turnos | Manager Self-Service (§8) | `ReasignacionCobertura` |
| US-012 Exportación e Integración con Sistema de Nómina | Integration & Payroll Export (§9) | `LoteExportacion` |
| US-013 Bitácora de Auditoría | Audit & Compliance (§10) | `RegistroAuditoria` |

## 2. Identity Service — Contexto de Identidad (Open Host Service)

Microservicio dueño de `Empleado` y `Usuario`. Los demás servicios lo consumen por referencia (`EmpleadoId`, `UsuarioId`), ya sea mediante llamada síncrona a su API o suscribiéndose al evento `EmpleadoSincronizado` (§11) para mantener una proyección local de solo lectura; en ningún caso otro servicio escribe estos datos.

```mermaid
classDiagram
    class Empleado {
        <<AggregateRoot>>
        +EmpleadoId id
        +string nombre
        +string documentoIdentidad
        +string email
        +CentroCostoId centroCostoId
        +EntidadLegalId entidadLegalId
        +EstadoEmpleado estado
        +DateOnly fechaIngreso
    }
    class EstadoEmpleado {
        <<enumeration>>
        ACTIVO
        INACTIVO
        SUSPENDIDO
    }
    class CentroCosto {
        +CentroCostoId id
        +string nombre
        +GeofenceId geofenceId
    }
    class EntidadLegal {
        +EntidadLegalId id
        +string pais
        +string razonSocial
    }
    class Usuario {
        <<AggregateRoot>>
        +UsuarioId id
        +EmpleadoId empleadoId
        +string username
        +List~RolUsuario~ roles
        +bool activo
    }
    class RolUsuario {
        <<enumeration>>
        EMPLEADO
        SUPERVISOR
        ANALISTA_RRHH
        ENCARGADO_NOMINA
        ADMINISTRADOR
        AUDITOR_CUMPLIMIENTO
        TI_INTEGRACIONES
        EJECUTIVO
    }
    class ConsentimientoBiometrico {
        +ConsentimientoId id
        +EmpleadoId empleadoId
        +bool aceptaBiometria
        +bool aceptaGeolocalizacion
        +DateTimeOffset fechaConsentimiento
    }

    Empleado "1" --> "1" CentroCosto : pertenece a
    Empleado "1" --> "1" EntidadLegal : rige por
    Empleado ..> EstadoEmpleado
    Usuario "1" --> "0..1" Empleado : representa a
    Usuario ..> RolUsuario
    ConsentimientoBiometrico "1" --> "1" Empleado : otorgado por
```

**Notas de diseño:**
- `Usuario` (identidad de acceso/RBAC) se modela separado de `Empleado` (sujeto de control de asistencia): un supervisor o auditor puede no estar sujeto a marcaje, y un mismo `Empleado` puede tener cero o un `Usuario` asociado.
- `ConsentimientoBiometrico` es prerrequisito de negocio para que `Marca` (§3) acepte captura biométrica o geolocalización, en línea con el principio de "privacidad por diseño" del documento de visión.

## 3. Clocking & Attendance Capture — US-001, US-002, US-003

```mermaid
classDiagram
    class Marca {
        <<AggregateRoot>>
        +MarcaId id
        +EmpleadoId empleadoId
        +TipoMarca tipo
        +DateTimeOffset timestampCaptura
        +DateTimeOffset timestampSincronizacion
        +CanalCaptura canal
        +EstadoMarca estado
        +UbicacionGPS ubicacion
        +ValidacionBiometrica validacionBiometrica
        +string motivoRechazo
        +registrar()
        +validarGeofencing(Geofence) bool
        +sincronizar()
        +marcarDuplicada()
    }
    class TipoMarca {
        <<enumeration>>
        ENTRADA
        SALIDA
        INICIO_RECESO
        FIN_RECESO
    }
    class CanalCaptura {
        <<enumeration>>
        BIOMETRICO_FISICO
        RFID
        PORTAL_WEB
        APP_MOVIL
    }
    class EstadoMarca {
        <<enumeration>>
        PENDIENTE_SINCRONIZACION
        VALIDA
        RECHAZADA
        DUPLICADA
    }
    class UbicacionGPS {
        <<ValueObject>>
        +decimal latitud
        +decimal longitud
        +decimal precisionMetros
    }
    class ValidacionBiometrica {
        <<ValueObject>>
        +bool livenessAprobado
        +decimal scoreConfianza
        +bool identidadConfirmada
    }
    class Geofence {
        <<AggregateRoot>>
        +GeofenceId id
        +CentroCostoId centroCostoId
        +UbicacionGPS centro
        +int radioMetros
        +estaDentroDelPerimetro(UbicacionGPS) bool
    }
    class DispositivoCaptura {
        +DispositivoId id
        +TipoDispositivo tipo
        +string identificadorHardware
        +CentroCostoId centroCostoId
    }
    class EventoSeguridad {
        <<AggregateRoot>>
        +EventoId id
        +MarcaId marcaId
        +TipoEventoSeguridad tipo
        +DateTimeOffset fecha
    }
    class TipoEventoSeguridad {
        <<enumeration>>
        FUERA_DE_PERIMETRO
        LIVENESS_FALLIDO
        INTENTO_SUPLANTACION
    }

    Marca "0..*" --> "1" Empleado : pertenece a
    Marca ..> TipoMarca
    Marca ..> CanalCaptura
    Marca ..> EstadoMarca
    Marca *-- UbicacionGPS : contiene
    Marca *-- ValidacionBiometrica : contiene
    Marca "0..*" --> "0..1" Geofence : validada contra
    Marca "0..*" --> "0..1" DispositivoCaptura : capturada por
    Marca "1" --> "0..*" EventoSeguridad : genera
    EventoSeguridad ..> TipoEventoSeguridad
```

**Invariantes clave (trazadas a criterios de aceptación):**
- Una `Marca` de tipo `ENTRADA` no puede registrarse si el empleado ya tiene una `ENTRADA` sin `SALIDA` de cierre (US-001, escenario "Intento de doble marca de entrada").
- Una `Marca` capturada por canal `BIOMETRICO_FISICO` o `APP_MOVIL` con geolocalización requiere `ValidacionBiometrica.livenessAprobado = true` antes de evaluar `Geofence`; si falla, la marca pasa a `RECHAZADA` y genera `EventoSeguridad` (US-002).
- Una `Marca` en estado `PENDIENTE_SINCRONIZACION` conserva `timestampCaptura` (hora local del dispositivo) distinto de `timestampSincronizacion`; las reglas de negocio (geofencing, turno, duplicados) se evalúan contra `timestampCaptura` (US-003).
- La sincronización debe detectar y descartar duplicados (mismo `empleadoId` + `tipo` + `timestampCaptura` ya persistido) sin doble conteo de horas (US-003).

## 4. Scheduling & Shift Management — US-004, US-005

```mermaid
classDiagram
    class Turno {
        <<AggregateRoot>>
        +TurnoId id
        +string nombre
        +TimeSpan horaInicio
        +TimeSpan horaFin
        +TimeSpan duracionReceso
        +int toleranciaEntradaMin
        +int toleranciaSalidaMin
        +ClasificacionTurno clasificacion
        +bool cruzaMedianoche
        +validarTolerancia(minutosDiferencia) EstadoPuntualidad
    }
    class ClasificacionTurno {
        <<enumeration>>
        FIJO
        ROTATIVO
        NOCTURNO
        FLEXIBLE
        ON_CALL
    }
    class PatronRotacion {
        +PatronId id
        +TurnoId turnoBaseId
        +List~string~ secuenciaDias
        +int duracionCicloDias
    }
    class AsignacionTurno {
        <<AggregateRoot>>
        +AsignacionId id
        +EmpleadoId empleadoId
        +TurnoId turnoId
        +DateOnly fecha
        +OrigenAsignacion origen
    }
    class OrigenAsignacion {
        <<enumeration>>
        INDIVIDUAL
        MASIVA
        REASIGNACION_COBERTURA
    }
    class CargaMasivaCalendario {
        <<AggregateRoot>>
        +LoteId id
        +CentroCostoId centroCostoId
        +DateOnly fechaInicio
        +DateOnly fechaFin
        +EstadoCarga estado
        +int totalRegistros
        +int registrosConError
        +procesar()
    }
    class EstadoCarga {
        <<enumeration>>
        VALIDANDO
        PROCESADA_CON_ERRORES
        COMPLETADA
    }
    class RestriccionHoraria {
        <<AggregateRoot>>
        +RestriccionId id
        +EmpleadoId empleadoId
        +TipoRestriccion tipo
        +DateOnly vigenciaDesde
        +DateOnly vigenciaHasta
        +bool aprobada
    }

    Turno "1" --> "0..1" PatronRotacion : define rotacion
    Turno "1" --> "0..*" AsignacionTurno : es asignado en
    AsignacionTurno "0..*" --> "1" Empleado : asignado a
    AsignacionTurno ..> OrigenAsignacion
    CargaMasivaCalendario "1" --> "0..*" AsignacionTurno : genera
    RestriccionHoraria "0..*" --> "1" Empleado : aplica a
    CargaMasivaCalendario ..> RestriccionHoraria : respeta al generar
```

**Invariantes clave:**
- `toleranciaEntradaMin`/`toleranciaSalidaMin` deben estar en el rango `[0, duración del turno]`; un valor negativo o excesivo se rechaza en la configuración (US-004).
- Un `Turno` con `cruzaMedianoche = true` distribuye las horas trabajadas entre dos fechas calendario y marca el segmento nocturno para efectos de recargo, consumido por `ReglaCalculoLegal` (§5) (US-004).
- `CargaMasivaCalendario.procesar()` genera una `AsignacionTurno` por empleado del alcance, **excepto** para empleados con `RestriccionHoraria` activa y aprobada vigente en el rango de fechas, que se excluyen o ajustan automáticamente (US-005).

## 5. Pre-Payroll Engine — US-006, US-007

```mermaid
classDiagram
    class ConsolidadoPreNomina {
        <<AggregateRoot>>
        +ConsolidadoId id
        +EmpleadoId empleadoId
        +PeriodoId periodoId
        +decimal horasTrabajadas
        +decimal horasExtraDiurnas
        +decimal horasExtraNocturnas
        +decimal horasExtraFestivas
        +decimal horasExtraDominicales
        +decimal montoRecargoNocturno
        +int minutosAtrasoTotal
        +EstadoConsolidado estado
        +calcular()
        +recalcular(motivo)
    }
    class EstadoConsolidado {
        <<enumeration>>
        CALCULADO
        RECALCULADO
        EXPORTADO
    }
    class Periodo {
        +PeriodoId id
        +DateOnly fechaInicio
        +DateOnly fechaFin
        +bool cerrado
    }
    class ReglaCalculoLegal {
        <<AggregateRoot>>
        +ReglaId id
        +EntidadLegalId entidadLegalId
        +TipoHora tipoHora
        +decimal factorPago
        +TimeSpan rangoNocturnoInicio
        +TimeSpan rangoNocturnoFin
    }
    class TipoHora {
        <<enumeration>>
        DIURNA
        NOCTURNA
        FESTIVA
        DOMINICAL
    }
    class DiaFestivo {
        +DateOnly fecha
        +EntidadLegalId entidadLegalId
        +string descripcion
    }
    class Incidencia {
        <<AggregateRoot>>
        +IncidenciaId id
        +EmpleadoId empleadoId
        +DateOnly fecha
        +TipoIncidencia tipo
        +int minutosAtraso
        +bool justificada
        +SolicitudId solicitudRelacionadaId
        +clasificar()
    }
    class TipoIncidencia {
        <<enumeration>>
        TARDANZA
        SALIDA_ANTICIPADA
        AUSENCIA_JUSTIFICADA
        AUSENCIA_INJUSTIFICADA
    }

    ConsolidadoPreNomina "0..*" --> "1" Empleado : de
    ConsolidadoPreNomina "0..*" --> "1" Periodo : corresponde a
    ConsolidadoPreNomina "0..*" ..> "0..*" ReglaCalculoLegal : aplica
    ReglaCalculoLegal ..> TipoHora
    ReglaCalculoLegal "0..*" --> "1" EntidadLegal : definida por
    ConsolidadoPreNomina ..> DiaFestivo : consulta
    ConsolidadoPreNomina "1" --> "0..*" Incidencia : consolida
    Incidencia "0..*" --> "1" Empleado : afecta a
    Incidencia ..> TipoIncidencia
    Incidencia "0..1" --> "0..1" Solicitud : justificada por
```

> `Solicitud` pertenece al contexto **Absence Management** (§6) y se referencia aquí únicamente por `SolicitudId` — el motor de pre-nómina no es dueño de su ciclo de vida, solo consulta si existe una aprobación vigente para la fecha de la incidencia.

**Invariantes clave:**
- Los factores de `ReglaCalculoLegal` son datos de configuración por `EntidadLegal`, nunca lógica embebida, permitiendo distintos porcentajes de recargo por país/entidad (US-006).
- Una jornada en fecha presente en `DiaFestivo` clasifica **todas** las horas trabajadas ese día como `FESTIVA`, con su propio factor de pago (US-006).
- `Incidencia.minutosAtraso` se calcula descontando la tolerancia del `Turno` asignado; un ingreso dentro de tolerancia no genera `Incidencia` (US-007).
- El cierre diario clasifica una ausencia como `AUSENCIA_JUSTIFICADA` únicamente si existe una `Solicitud` aprobada vigente para esa fecha; en caso contrario, `AUSENCIA_INJUSTIFICADA` y notifica al supervisor (US-007).
- Toda aprobación tardía de una `Solicitud` que afecta un `Periodo` ya calculado dispara `ConsolidadoPreNomina.recalcular()`, dejando trazabilidad del motivo.

## 6. Absence Management — US-008, US-009

```mermaid
classDiagram
    class Solicitud {
        <<AggregateRoot>>
        +SolicitudId id
        +EmpleadoId empleadoId
        +TipoSolicitud tipo
        +DateOnly fechaInicio
        +DateOnly fechaFin
        +EstadoSolicitud estado
        +List~Adjunto~ adjuntos
        +enviar()
        +aprobar(aprobadorId, nivel)
        +rechazar(aprobadorId, motivo)
        +escalar()
    }
    class TipoSolicitud {
        <<enumeration>>
        VACACIONES
        PERMISO_CON_GOCE
        PERMISO_SIN_GOCE
        INCAPACIDAD_MEDICA
        DIA_COMPENSATORIO
        CORRECCION_MARCA
    }
    class EstadoSolicitud {
        <<enumeration>>
        PENDIENTE
        PENDIENTE_RRHH
        APROBADA
        RECHAZADA
        ESCALADA
    }
    class Adjunto {
        <<ValueObject>>
        +AdjuntoId id
        +string nombreArchivo
        +string tipoContenido
        +string urlAlmacenamiento
    }
    class DecisionAprobacion {
        <<ValueObject>>
        +DecisionId id
        +UsuarioId aprobadorId
        +NivelAprobacion nivel
        +bool aprobado
        +string motivoRechazo
        +DateTimeOffset fecha
    }
    class NivelAprobacion {
        <<enumeration>>
        SUPERVISOR
        RRHH
    }
    class FlujoAprobacion {
        <<AggregateRoot>>
        +FlujoId id
        +TipoSolicitud tipoSolicitud
        +List~NivelAprobacion~ nivelesRequeridos
        +int plazoMaximoDiasHabiles
    }
    class SaldoVacaciones {
        <<AggregateRoot>>
        +EmpleadoId empleadoId
        +decimal diasDisponibles
        +decimal diasReservados
        +decimal diasUtilizados
        +reservar(dias)
        +confirmar(dias)
        +liberar(dias)
    }

    Solicitud "0..*" --> "1" Empleado : solicitada por
    Solicitud ..> TipoSolicitud
    Solicitud ..> EstadoSolicitud
    Solicitud "1" *-- "0..*" Adjunto : incluye
    Solicitud "1" *-- "0..*" DecisionAprobacion : registra
    DecisionAprobacion ..> NivelAprobacion
    Solicitud "0..*" --> "1" FlujoAprobacion : sigue
    Solicitud "0..*" --> "0..1" SaldoVacaciones : reserva/consume
```

**Invariantes clave:**
- `Solicitud.enviar()` para `TipoSolicitud.VACACIONES` valida `SaldoVacaciones.diasDisponibles >= diasSolicitados` antes de crear la solicitud; si se aprueba, pasa de reservado a utilizado, si se rechaza, se libera la reserva (US-008).
- `Solicitud` de tipo `INCAPACIDAD_MEDICA` exige al menos un `Adjunto` antes de permitir `enviar()` (US-008).
- El número de `NivelAprobacion` requeridos y sus roles participantes provienen de `FlujoAprobacion`, configurable por `TipoSolicitud` y unidad organizativa — no está codificado en `Solicitud` (US-009).
- Si el tiempo transcurrido desde la creación supera `FlujoAprobacion.plazoMaximoDiasHabiles` sin decisión, `Solicitud.escalar()` cambia el estado y notifica al siguiente nivel (US-009).
- Toda `DecisionAprobacion` (aprobación o rechazo) es inmutable una vez registrada y alimenta la Bitácora de Auditoría (§10).

## 7. Employee Self-Service (ESS) — US-010

Contexto de **solo lectura** (read model / CQRS query side): compone datos de `Marca` (§3), `Incidencia` (§5), `Turno` (§4) y `SaldoVacaciones` (§6) sin ser dueño de ninguno.

```mermaid
classDiagram
    class TarjetaAsistenciaView {
        <<ReadModel>>
        +EmpleadoId empleadoId
        +PeriodoId periodoId
        +List~MarcaResumen~ marcas
        +decimal horasAcumuladas
        +List~IncidenciaResumen~ incidencias
        +decimal saldoVacacionesDisponible
    }
    class MarcaResumen {
        <<ValueObject>>
        +DateOnly fecha
        +TipoMarca tipo
        +DateTimeOffset hora
        +CanalCaptura canal
        +bool esOmitida
    }
    class IncidenciaResumen {
        <<ValueObject>>
        +DateOnly fecha
        +TipoIncidencia tipo
        +bool resuelta
    }
    class Notificacion {
        <<AggregateRoot>>
        +NotificacionId id
        +EmpleadoId destinatarioId
        +TipoNotificacion tipo
        +string mensaje
        +bool leida
        +DateTimeOffset fecha
    }
    class TipoNotificacion {
        <<enumeration>>
        MARCA_OMITIDA
        SOLICITUD_APROBADA
        SOLICITUD_RECHAZADA
        RECORDATORIO
    }

    TarjetaAsistenciaView "1" *-- "0..*" MarcaResumen : compone
    TarjetaAsistenciaView "1" *-- "0..*" IncidenciaResumen : compone
    Notificacion ..> TipoNotificacion
    Notificacion "0..*" --> "1" Empleado : notifica a
```

**Invariantes clave:**
- `TarjetaAsistenciaView` se reconstruye de forma casi en tiempo real ante cada evento `MarcaRegistrada` o `IncidenciaClasificada` (§11), reflejando incidencias aún no resueltas por RRHH (US-010).
- `MarcaResumen.esOmitida = true` quando un turno asignado no tiene su marca de cierre correspondiente registrada, habilitando al empleado a iniciar una `Solicitud` de tipo `CORRECCION_MARCA` desde la misma vista (US-010).

## 8. Manager Self-Service (MSS) — US-011, US-011b

```mermaid
classDiagram
    class EstadoPresenciaEmpleado {
        <<ReadModel>>
        +EmpleadoId empleadoId
        +CentroCostoId centroCostoId
        +EstadoPresencia estado
        +DateTimeOffset ultimaActualizacion
        +MarcaId ultimaMarcaId
    }
    class EstadoPresencia {
        <<enumeration>>
        PRESENTE
        EN_RECESO
        AUSENTE
        AUSENTE_SIN_JUSTIFICAR
        DE_VACACIONES
    }
    class ReasignacionCobertura {
        <<AggregateRoot>>
        +ReasignacionId id
        +AsignacionId turnoOriginalId
        +EmpleadoId empleadoAusenteId
        +EmpleadoId empleadoCoberturaId
        +DateOnly fecha
        +string motivo
        +EstadoReasignacion estado
        +confirmar()
    }
    class EstadoReasignacion {
        <<enumeration>>
        PROPUESTA
        CONFIRMADA
        RECHAZADA_POR_TRASLAPE
    }

    EstadoPresenciaEmpleado ..> EstadoPresencia
    EstadoPresenciaEmpleado "0..*" --> "1" Empleado : refleja a
    ReasignacionCobertura "0..*" --> "1" Empleado : ausente
    ReasignacionCobertura "0..*" --> "1" Empleado : cobertura
    ReasignacionCobertura ..> EstadoReasignacion
```

**Invariantes clave:**
- `EstadoPresenciaEmpleado` se actualiza por push ante cada evento `MarcaRegistrada` (§11), sin requerir recarga de página; el filtrado por centro de costo opera sobre esta proyección (US-011).
- Un empleado con turno iniciado hace más de 30 minutos sin `Marca` de entrada y sin `Solicitud` aprobada vigente se proyecta como `AUSENTE_SIN_JUSTIFICAR` (US-011).
- `ReasignacionCobertura.confirmar()` valida que `empleadoCoberturaId` no tenga ya una `AsignacionTurno` (§4) en el mismo horario; de existir traslape, exige confirmación explícita del supervisor antes de persistir (US-011b).
- Toda `ReasignacionCobertura` queda vinculada al evento de ausencia que la originó para trazabilidad en reportes de cobertura (US-011b).

## 9. Integration & Payroll Export — US-012

```mermaid
classDiagram
    class LoteExportacion {
        <<AggregateRoot>>
        +LoteId id
        +PeriodoId periodoId
        +SistemaDestino sistemaDestino
        +FormatoExportacion formato
        +EstadoExportacion estado
        +int intentos
        +DateTimeOffset fechaEjecucion
        +ejecutar()
        +reintentar()
    }
    class SistemaDestino {
        <<enumeration>>
        SAP
        ORACLE
        WORKDAY
        SOFTLAND
    }
    class FormatoExportacion {
        <<enumeration>>
        API_REST
        ARCHIVO_PLANO_SFTP
    }
    class EstadoExportacion {
        <<enumeration>>
        PENDIENTE
        EXPORTADO
        FALLIDO
    }
    class ConectorIntegracion {
        <<AggregateRoot>>
        +ConectorId id
        +SistemaDestino sistema
        +FormatoExportacion formato
        +string endpointOCarpeta
        +PoliticaReintento politicaReintento
    }
    class PoliticaReintento {
        <<ValueObject>>
        +int maxIntentos
        +TimeSpan backoffInicial
    }
    class RegistroSincronizacionDirectorio {
        <<AggregateRoot>>
        +SincronizacionId id
        +DateTimeOffset fecha
        +int altasProcesadas
        +int bajasProcesadas
        +EstadoSincronizacion estado
    }
    class EstadoSincronizacion {
        <<enumeration>>
        EXITOSA
        PARCIAL
        FALLIDA
    }

    LoteExportacion "0..*" --> "1" Periodo : exporta
    LoteExportacion ..> SistemaDestino
    LoteExportacion ..> FormatoExportacion
    LoteExportacion ..> EstadoExportacion
    LoteExportacion "0..*" --> "1" ConectorIntegracion : usa
    ConectorIntegracion *-- PoliticaReintento : define
    RegistroSincronizacionDirectorio ..> EstadoSincronizacion
```

**Invariantes clave:**
- Cada `LoteExportacion` tiene un identificador único trazable end-to-end hacia el ERP destino; el estado solo transiciona `PENDIENTE → EXPORTADO` o `PENDIENTE → FALLIDO` tras agotar `PoliticaReintento.maxIntentos` (US-012).
- `RegistroSincronizacionDirectorio` es la fuente autoritativa de altas/bajas de `Empleado`: mientras esté activa, el TCS no permite creación manual de empleados (US-012, ver también §2).

## 10. Audit & Compliance — US-013 (contexto transversal)

```mermaid
classDiagram
    class RegistroAuditoria {
        <<AggregateRoot>>
        +RegistroId id
        +string entidadAfectada
        +string entidadId
        +UsuarioId usuarioId
        +TipoAccion accion
        +string valorAnterior
        +string valorNuevo
        +DateTimeOffset timestamp
        +string hashIntegridad
    }
    class TipoAccion {
        <<enumeration>>
        CREACION
        MODIFICACION
        APROBACION
        RECHAZO
        INTENTO_ALTERACION_BLOQUEADO
    }
    class ExportacionEvidencia {
        <<AggregateRoot>>
        +ExportacionId id
        +DateOnly periodoDesde
        +DateOnly periodoHasta
        +List~EmpleadoId~ empleadosIncluidos
        +string formato
        +string hashArchivo
    }

    RegistroAuditoria ..> TipoAccion
    RegistroAuditoria "0..*" --> "1" Usuario : ejecutado por
    ExportacionEvidencia "1" --> "0..*" RegistroAuditoria : incluye
```

**Invariantes clave (append-only por diseño):**
- `RegistroAuditoria` **no expone operaciones de actualización ni borrado**, ni siquiera a nivel administrativo; solo `registrar()` (inserción) y consultas de lectura (US-013).
- Todo cambio manual sobre `Marca`, `AsignacionTurno` o `Solicitud` genera un `RegistroAuditoria` de forma síncrona a la operación que lo origina, nunca diferida (US-013).
- `ExportacionEvidencia.hashArchivo` permite verificar la integridad del archivo exportado ante una inspección laboral (US-013).
- Este contexto es **transversal**: todos los demás contextos publican eventos de cambio manual hacia él (ver §11), pero él nunca depende de ellos.

## 11. Eventos de dominio entre contextos

Los bounded contexts se mantienen desacoplados y se comunican mediante eventos de dominio (publicados por el agregado que los origina, consumidos por los contextos interesados). Como cada contexto es un microservicio independiente (`c4-containers.md` §1), estos eventos ya no son un detalle interno de implementación sino **contratos de integración entre servicios**, publicados y consumidos a través del bus de eventos compartido (Redis) descrito en `c4-containers.md` §5.

| Evento | Publicado por | Consumido por | Efecto |
|---|---|---|---|
| `MarcaRegistrada` | Clocking (§3) | Manager Self-Service (§8), Employee Self-Service (§7), Pre-Payroll (§5) | Actualiza `EstadoPresenciaEmpleado`; recalcula `TarjetaAsistenciaView`; insumo para cierre diario |
| `MarcaRechazada` | Clocking (§3) | Audit (§10) | Registra intento de fraude/geofencing como evento de seguridad |
| `CierreDiarioEjecutado` | Pre-Payroll (§5) | Absence Management (§6), Employee/Manager SS (§7, §8) | Clasifica ausencias; actualiza vistas de presencia e incidencias |
| `SolicitudAprobada` / `SolicitudRechazada` | Absence Management (§6) | Pre-Payroll (§5), Employee Self-Service (§7) | Marca `Incidencia.justificada`; dispara `recalcular()` si el periodo ya fue calculado; actualiza `SaldoVacaciones` |
| `ConsolidadoCalculado` | Pre-Payroll (§5) | Integration (§9), Reportes/BI | Habilita el `LoteExportacion` del periodo |
| `LoteExportado` / `LoteFallido` | Integration (§9) | Audit (§10) | Trazabilidad de exportaciones hacia el ERP |
| `AltaBajaDetectada` (comando síncrono, no evento) | Integration (§9) → Identidad (§2) | Identidad (§2) | Integration detecta el cambio en el directorio activo y ordena la creación/baja; Identidad es la única que escribe `Empleado`/`Usuario` |
| `EmpleadoSincronizado` (alta/baja) | Identidad (§2) | Todos los demás contextos | Publicado por Identidad tras persistir el alta/baja; actualiza las proyecciones locales de `EmpleadoId` en cada contexto |
| `CambioManualRegistrado` | Cualquier contexto (Clocking, Scheduling, Absence) | Audit (§10) | Inserción síncrona de `RegistroAuditoria` |

## 12. Mapa de bounded contexts

```mermaid
graph TB
    subgraph Compartido["Identidad (Shared Kernel)"]
        ID["Empleado / Usuario"]
    end

    subgraph Clocking["Clocking & Attendance Capture<br/>US-001, US-002, US-003"]
        C1["Marca / Geofence"]
    end

    subgraph Scheduling["Scheduling & Shift Management<br/>US-004, US-005"]
        C2["Turno / AsignacionTurno"]
    end

    subgraph PrePayroll["Pre-Payroll Engine<br/>US-006, US-007"]
        C3["ConsolidadoPreNomina / Incidencia"]
    end

    subgraph Absence["Absence Management<br/>US-008, US-009"]
        C4["Solicitud / SaldoVacaciones"]
    end

    subgraph ESS["Employee Self-Service<br/>US-010"]
        C5["TarjetaAsistenciaView"]
    end

    subgraph MSS["Manager Self-Service<br/>US-011, US-011b"]
        C6["EstadoPresenciaEmpleado / ReasignacionCobertura"]
    end

    subgraph Integration["Integration & Payroll Export<br/>US-012"]
        C7["LoteExportacion"]
    end

    subgraph Audit["Audit & Compliance<br/>US-013"]
        C8["RegistroAuditoria"]
    end

    ID -.->|referenciado por ID| Clocking
    ID -.->|referenciado por ID| Scheduling
    ID -.->|referenciado por ID| PrePayroll
    ID -.->|referenciado por ID| Absence
    ID -.->|referenciado por ID| MSS

    Clocking -->|MarcaRegistrada| PrePayroll
    Clocking -->|MarcaRegistrada| MSS
    Clocking -->|MarcaRegistrada| ESS
    Scheduling -->|turno asignado| PrePayroll
    Scheduling -->|turno asignado| MSS
    Absence -->|SolicitudAprobada| PrePayroll
    Absence -->|SolicitudAprobada| ESS
    PrePayroll -->|ConsolidadoCalculado| Integration
    MSS -->|reasignacion| Scheduling
    Clocking -.->|CambioManualRegistrado| Audit
    Scheduling -.->|CambioManualRegistrado| Audit
    Absence -.->|CambioManualRegistrado| Audit
    Integration -.->|LoteExportado/Fallido| Audit
```

> Cada subgrafo de este mapa es un **servicio desplegable independiente** (ver el mapeo 1:1 en `c4-containers.md` §2–§3). Las flechas sólidas representan eventos de dominio publicados de forma asíncrona a través del bus de eventos compartido (Redis); las flechas punteadas hacia `Identity` representan una llamada síncrona a su API o una réplica local mantenida por el evento `EmpleadoSincronizado`.

## 13. Documentos relacionados

- `docs/specs/functional/02-user-stories.md` — Historias de usuario e insumo funcional de este modelo.
- `docs/architecture/c4-containers.md` — Arquitectura de contenedores (C4) que implementa estos bounded contexts.
- `docs/architecture/decisions/ADR-001-clean-architecture-cqrs-vs-microservicios-multiples-bd.md` — Justificación de por qué los servicios comparten una única base de datos con acceso abierto entre esquemas.
