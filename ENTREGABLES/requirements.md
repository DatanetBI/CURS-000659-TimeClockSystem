# Requirements — Time Clock System

> **Entregable**: `requirements.md` — Catálogo RF/RNF, reglas, prioridades y preguntas pendientes.
> **Fuentes originales**:
> - RF (§1), RN (§2) y preguntas pendientes (§4): [`docs/specs/functional/03-especificacion-consolidada.md`](../docs/specs/functional/03-especificacion-consolidada.md) §4, §5, §7 (reproducidos íntegros).
> - RNF (§3): **no existe un catálogo de RNF en el material fuente** — esta sección se redactó extrayendo y citando explícitamente el contenido no funcional que sí está documentado en [`docs/architecture/c4-containers.md`](../docs/architecture/c4-containers.md) §7 y otros puntos puntuales de `vision.md`/`domain-model.md`, según se decidió con el solicitante de este entregable.
> - Prioridades (§5): **no existe ninguna asignación de prioridad (P1/P2/P3, MoSCoW, etc.) en ningún documento fuente**. Se deja marcada como pendiente en vez de inventarse, para no introducir información que no proviene del material original.

---

## 1. Catálogo de Requisitos Funcionales (RF)

*Fuente: `03-especificacion-consolidada.md` §4.*

### 1.1 Marcaje y captura de asistencia

- **RF1**. El sistema debe permitir registrar una marca de entrada, salida, inicio de receso o fin de receso desde biometría física, RFID, portal web o app móvil.
- **RF2**. El sistema debe rechazar el registro de una nueva entrada si el empleado ya tiene una entrada abierta sin su salida correspondiente.
- **RF3**. El sistema debe validar, para cada marca con geolocalización, que las coordenadas capturadas estén dentro del perímetro autorizado (geofence) configurado para el empleado o su centro de trabajo.
- **RF4**. El sistema debe verificar, mediante prueba de vida (liveness), que el reconocimiento facial corresponde a una persona presente en el momento de la marca, antes de aceptarla.
- **RF5**. El sistema debe permitir capturar y almacenar marcas localmente en la app móvil cuando no haya conectividad, y sincronizarlas automáticamente al recuperarla.
- **RF6**. El sistema debe descartar automáticamente, durante la sincronización, cualquier marca duplicada, sin generar doble conteo de horas.
- **RF7**. El sistema debe registrar como evento de seguridad todo intento de marcaje rechazado por geofencing o por fallo de liveness.

### 1.2 Horarios y turnos

- **RF8**. El sistema debe permitir crear y mantener un catálogo de turnos fijos, rotativos, nocturnos, flexibles y de disponibilidad (on-call), cada uno con su horario, duración de receso y márgenes de tolerancia.
- **RF9**. El sistema debe distribuir correctamente las horas trabajadas de un turno que cruza la medianoche entre las dos fechas calendario correspondientes.
- **RF10**. El sistema debe permitir asignar turnos y calendarios de forma individual o de forma masiva a un grupo de empleados, departamento o centro de costo.
- **RF11**. El sistema debe respetar las restricciones horarias individuales aprobadas (p. ej. médicas) al procesar una asignación masiva, sin requerir reconfiguración manual posterior.

### 1.3 Pre-nómina y motor de reglas

- **RF12**. El sistema debe calcular automáticamente las horas trabajadas, los atrasos, las ausencias y las horas extra de cada empleado a partir de sus marcas y su turno asignado.
- **RF13**. El sistema debe clasificar automáticamente las horas extra en diurnas, nocturnas, festivas o dominicales, según el calendario de festivos y la legislación configurada para la entidad legal del empleado.
- **RF14**. El sistema debe permitir configurar, por entidad legal o país, los factores de pago y los rangos horarios que definen cada tipo de hora y recargo.
- **RF15**. El sistema debe recalcular el consolidado de un periodo ya procesado cuando se apruebe, con posterioridad, una solicitud que afecte una incidencia de ese periodo.

### 1.4 Incidencias, justificaciones y permisos

- **RF16**. El sistema debe permitir a un empleado solicitar vacaciones, permisos con o sin goce de sueldo, incapacidades médicas y días compensatorios, adjuntando comprobantes cuando corresponda.
- **RF17**. El sistema debe validar el saldo de vacaciones disponible antes de permitir el envío de una solicitud de vacaciones.
- **RF18**. El sistema debe conducir cada solicitud a través de un flujo de aprobación configurable (uno o más niveles: supervisor, RRHH) según el tipo de solicitud.
- **RF19**. El sistema debe escalar automáticamente una solicitud al siguiente nivel de aprobación si no se resuelve dentro del plazo configurado.
- **RF20**. El sistema debe registrar el motivo de todo rechazo y notificarlo al solicitante.

### 1.5 Portal del Empleado (ESS)

- **RF21**. El sistema debe permitir a cada empleado consultar su historial de marcas, su acumulado de horas y sus incidencias, con información actualizada en tiempo casi real.
- **RF22**. El sistema debe permitir a cada empleado consultar su saldo de vacaciones disponible, reservado y utilizado.
- **RF23**. El sistema debe señalar visualmente al empleado cualquier marca omitida y permitirle iniciar desde ahí una solicitud de corrección.
- **RF24**. El sistema debe notificar al empleado sobre marcas omitidas y sobre la aprobación o rechazo de sus solicitudes.

### 1.6 Portal del Supervisor (MSS)

- **RF25**. El sistema debe mostrar al supervisor el estado de presencia en tiempo real de cada integrante de su equipo (presente, en receso, ausente, ausente sin justificar, de vacaciones).
- **RF26**. El sistema debe permitir al supervisor aprobar o rechazar, desde un solo lugar, las solicitudes de su equipo (permisos, horas extra, correcciones de marca).
- **RF27**. El sistema debe permitir al supervisor reasignar la cobertura de un turno ante una ausencia imprevista, eligiendo a un empleado disponible.
- **RF28**. El sistema debe advertir al supervisor si el empleado elegido para cubrir un turno ya tiene otro turno asignado en el mismo horario, exigiendo su confirmación explícita antes de aplicar el cambio.

### 1.7 Integración y exportación

- **RF29**. El sistema debe exportar, de forma automatizada y programada, el consolidado de horas e incidencias del periodo hacia el sistema de nómina/ERP configurado.
- **RF30**. El sistema debe reintentar automáticamente una exportación fallida según una política de reintentos configurable, y notificar si finalmente no se puede completar.
- **RF31**. El sistema debe sincronizar de forma automatizada las altas y bajas de empleados desde el directorio activo/HRIS de la organización.

### 1.8 Reportes y Business Intelligence

- **RF32**. El sistema debe generar reportes operativos de asistencia, puntualidad, ausentismo y horas extra por periodo, empleado, departamento o centro de costo.
- **RF33**. El sistema debe presentar indicadores (KPIs) de tasa de ausentismo, costo de horas extra y tendencia de tardanzas.
- **RF34**. El sistema debe permitir exportar cualquier reporte en formatos PDF, Excel y CSV.

### 1.9 Administración, seguridad y auditoría

- **RF35**. El sistema debe permitir definir roles con permisos diferenciados por perfil de usuario (empleado, supervisor, RRHH, nómina, administrador, auditor, TI, ejecutivo).
- **RF36**. El sistema debe registrar, de forma inalterable, quién modificó qué dato, cuándo y con qué valor anterior y nuevo, para toda modificación manual sobre marcas, turnos o solicitudes.
- **RF37**. El sistema debe impedir cualquier modificación o eliminación posterior de un registro ya insertado en la bitácora de auditoría.
- **RF38**. El sistema debe solicitar y registrar el consentimiento explícito del empleado antes de capturar sus datos biométricos o de geolocalización.

---

## 2. Reglas de negocio (con ejemplos)

*Fuente: `03-especificacion-consolidada.md` §5.*

- **RN1. Tolerancia de marcaje**: el atraso reportado es el tiempo transcurrido desde la hora de entrada del turno, descontando el margen de tolerancia configurado. *Ejemplo*: turno 08:00 con 10 min de tolerancia; el empleado marca 08:16 → se reportan 6 minutos de atraso (16 − 10).
- **RN2. Marca dentro de tolerancia no genera incidencia**. *Ejemplo*: mismo turno anterior; el empleado marca 08:07 → no se registra ninguna tardanza.
- **RN3. Geofencing**: una marca solo es válida si ocurre dentro del radio configurado. *Ejemplo*: geofence de 150 metros; una marca capturada a 500 metros del centro se rechaza y genera un evento de seguridad.
- **RN4. Doble marca de entrada**: no puede registrarse una entrada si ya existe una entrada abierta sin su salida. *Ejemplo*: el empleado marcó entrada a las 08:00 y no ha marcado salida; un segundo intento de marcar entrada a las 12:00 es rechazado.
- **RN5. Turno nocturno que cruza medianoche**: las horas se distribuyen entre las dos fechas calendario. *Ejemplo*: turno 22:00–06:00 iniciado el lunes → 2 horas se contabilizan el lunes y 6 horas el martes.
- **RN6. Horas extra en día festivo**: toda la jornada trabajada en un día declarado festivo se paga como festiva, no solo el excedente. *Ejemplo*: turno de 8 horas (08:00–17:00) en un día festivo → las 8 horas se clasifican como "horas festivas".
- **RN7. Saldo de vacaciones insuficiente**: una solicitud de vacaciones se rechaza antes de enviarse a aprobación si excede el saldo disponible. *Ejemplo*: saldo disponible de 3 días; solicitud de 5 días → se rechaza de inmediato, sin llegar al supervisor.
- **RN8. Adjunto obligatorio para incapacidad médica**: una solicitud de este tipo no puede enviarse sin al menos un comprobante adjunto. *Ejemplo*: un intento de envío sin adjunto queda bloqueado hasta que se adjunte el certificado en PDF o imagen.
- **RN9. Escalamiento por vencimiento de plazo**: una solicitud pendiente más allá del plazo máximo configurado se escala automáticamente al siguiente nivel. *Ejemplo*: plazo de 3 días hábiles; si el supervisor no decide en ese plazo, la solicitud pasa automáticamente a RRHH.
- **RN10. Ausencia justificada vs. injustificada**: un día sin marcas se clasifica como ausencia justificada solo si existe una solicitud aprobada vigente para esa fecha. *Ejemplo*: empleado sin marcas el 15 de marzo y sin solicitud aprobada para esa fecha → se marca "ausencia injustificada" y se notifica al supervisor.
- **RN11. Reasignación con traslape de turno**: no se asigna automáticamente una cobertura si el empleado elegido ya tiene otro turno en el mismo horario. *Ejemplo*: el empleado B ya tiene turno 08:00–17:00; se le intenta asignar cobertura en ese mismo horario → el sistema advierte el traslape antes de permitir confirmar.
- **RN12. Reintentos de exportación**: una exportación fallida se reintenta según la política configurada antes de marcarse como fallida definitivamente. *Ejemplo*: política de 3 reintentos; si los 3 fallan, el lote se marca "Fallido" y se notifica a TI.
- **RN13. Bitácora de auditoría inalterable**: ningún registro de auditoría admite modificación o eliminación posterior, ni siquiera por un administrador. *Ejemplo*: un intento de editar un registro ya insertado es rechazado por el sistema y genera una alerta de seguridad.
- **RN14. Consentimiento biométrico previo**: el sistema no captura datos biométricos ni de geolocalización de un empleado que no haya otorgado su consentimiento explícito. *Ejemplo*: un empleado sin consentimiento registrado no puede marcar por reconocimiento facial hasta otorgarlo.

---

## 3. Catálogo de Requisitos No Funcionales (RNF)

> ⚠️ **Nota de origen**: ninguno de los documentos fuente (`01-vision-document.md`, `02-user-stories.md`, `03-especificacion-consolidada.md`) contiene una sección explícita de "Requisitos No Funcionales". Los siguientes RNF se **extrajeron** de contenido no funcional real ya documentado en la arquitectura y la visión — cada uno cita textualmente su origen — en vez de crearse desde cero, siguiendo la decisión tomada con el solicitante de este entregable.

### 3.1 Seguridad

- **RNF1 (Autenticación/Autorización)**: *"`Identity Service` emite tokens JWT (OpenID Connect); el `API Gateway` valida el token en el borde y lo reenvía; cada microservicio además revalida el token como resource server (defensa en profundidad) y aplica RBAC por rol"*. — Fuente: `c4-containers.md` §7.
- **RNF2 (Privacidad y consentimiento)**: *"El tratamiento de datos biométricos requiere consentimiento explícito y cumplimiento de normativa de protección de datos equivalente a GDPR (o la ley local de datos personales/biométricos aplicable)"*; *"Privacidad por diseño: la captura de datos biométricos y de geolocalización requiere consentimiento explícito, minimización de datos y políticas de retención definidas"*. — Fuente: `vision.md` §4.2 y Principio 6.
- **RNF3 (Integridad de auditoría)**: *"`RegistroAuditoria` no expone operaciones de actualización ni borrado, ni siquiera a nivel administrativo; solo `registrar()` (inserción) y consultas de lectura"*. — Fuente: `domain-model.md` §10.

### 3.2 Observabilidad

- **RNF4 (Trazabilidad distribuida)**: *"Todo request propaga un `traceId`/`correlationId` (W3C Trace Context) desde el Gateway hasta cada servicio y hacia los eventos publicados en Redis, recolectado vía OpenTelemetry hacia un backend de trazas centralizado"*. — Fuente: `c4-containers.md` §7.

### 3.3 Resiliencia y disponibilidad

- **RNF5 (Resiliencia entre servicios)**: *"Reintentos con backoff exponencial y circuit breaker (Polly) en toda llamada REST síncrona entre servicios; un servicio caído no debe agotar los hilos/conexiones del que lo llama"*. — Fuente: `c4-containers.md` §7.
- **RNF6 (Alta disponibilidad de datos)**: *"Se dimensiona con margen y se protege con connection pooling (PgBouncer), alta disponibilidad (réplica de conmutación) y una réplica de solo lectura para las consultas intensivas de ESS Service, MSS Service y reportes"*. — Fuente: `c4-containers.md` §7.
- **RNF7 (Confiabilidad offline)**: *"100% de marcas offline sincronizadas correctamente al recuperar conexión, sin pérdida de datos"* (Objetivo O7, con KPI medible). — Fuente: `vision.md` §3.

### 3.4 Escalabilidad y despliegue

- **RNF8 (Escalado independiente)**: *"Un contenedor Docker/imagen por servicio, orquestado en Kubernetes (o equivalente); cada uno con su propio pipeline de CI/CD y su propia estrategia de escalado horizontal (réplicas independientes por servicio según carga)"*. — Fuente: `c4-containers.md` §7.

### 3.5 Evolución de contratos

- **RNF9 (Versionado de eventos)**: *"Los contratos de eventos se versionan explícitamente (v1, v2, …) en el nombre del stream de Redis; un productor nunca elimina un campo consumido sin antes migrar a todos los consumidores"*. — Fuente: `c4-containers.md` §7.

### 3.6 Precisión / exactitud (métricas de calidad)

- **RNF10 (Exactitud anti-fraude)**: *"Detección y bloqueo de ≥ 99% de intentos de suplantación facial (liveness)"* (Objetivo O3, con KPI medible). — Fuente: `vision.md` §3.
- **RNF11 (Automatización de pre-nómina)**: *"Reducción ≥ 80% del tiempo de procesamiento manual de pre-nómina por ciclo"* (Objetivo O2). — Fuente: `vision.md` §3.

---

## 4. Prioridades

> ⚠️ **Pendiente — no definido en el material fuente.** Ningún documento (`vision.md`, `user-stories.md`, `03-especificacion-consolidada.md`, ni los documentos de arquitectura) asigna una prioridad explícita (P1/P2/P3, MoSCoW u otro esquema) a los RF1–RF38 ni a las historias US-001–US-013. Antes de planificar releases, el equipo de producto debe asignar esta priorización explícitamente; este entregable no la infiere para no introducir contenido que no proviene del material original.

---

## 5. Preguntas pendientes (Casos límite)

*Fuente: `03-especificacion-consolidada.md` §7. Redactadas originalmente como "casos límite" — cumplen la función de preguntas pendientes de resolución antes de construir cada flujo.*

- **CL1**. ¿Qué pasa si un empleado no tiene ningún turno asignado para el día y aun así intenta marcar asistencia?
- **CL2**. ¿Qué pasa si la misma marca llega dos veces al servidor (por ejemplo, se sincronizó offline y también fue registrada por otro canal)?
- **CL3**. ¿Qué pasa si un turno nocturno que cruza la medianoche coincide exactamente con el cierre del periodo de pre-nómina?
- **CL4**. ¿Qué pasa si dos personas intentan aprobar/rechazar la misma solicitud casi al mismo tiempo?
- **CL5**. ¿Qué pasa si se aprueba una solicitud de vacaciones para un periodo de pre-nómina que ya fue calculado y exportado al ERP?
- **CL6**. ¿Qué pasa si el directorio activo reporta la baja de un empleado que tiene una solicitud pendiente de aprobación o un turno futuro ya asignado?
- **CL7**. ¿Qué pasa si el sistema de nómina/ERP no responde durante toda la ventana de reintentos configurada?
- **CL8**. ¿Qué pasa si un empleado solicita un permiso para una fecha que ya pasó?
- **CL9**. ¿Qué pasa si un empleado sin consentimiento biométrico registrado intenta marcar en un terminal que solo tiene reconocimiento facial?
- **CL10**. ¿Qué pasa si se pierde la conectividad de la app móvil justo en medio de la sincronización de un lote grande de marcas pendientes?
- **CL11**. ¿Qué pasa si un supervisor deja de tener acceso al sistema mientras su equipo tiene solicitudes pendientes sin resolver?
- **CL12**. ¿Qué pasa si se intenta cargar de forma masiva un calendario con filas de datos inválidas o con identificadores de empleados inexistentes?

---

## 6. Fuera de alcance

*Fuente: `03-especificacion-consolidada.md` §8 (coincide con `vision.md` §4.1).*

- **Cálculo completo de nómina** (bruto a neto, deducciones legales, impuestos): el sistema exporta el consolidado de horas e incidencias ya calculado; el cálculo final de la nómina permanece en el ERP/sistema de nómina destino.
- **Gestión de reclutamiento, evaluación de desempeño** u otros módulos de HRIS no relacionados con asistencia.
- **Fabricación o suministro de hardware biométrico**: el sistema se integra con terminales de terceros vía SDK/API; no fabrica ni distribuye dispositivos.
- **Negociación o modelado de convenios colectivos complejos** más allá de las reglas de horas extra y recargos parametrizables ya definidas en el motor de pre-nómina.
- **Una plataforma de Business Intelligence con modelado analítico avanzado** (data warehouse, minería de datos, machine learning predictivo): en esta versión los reportes y KPIs se sirven directamente desde los datos operativos.
- **Soporte multi-idioma de la interfaz**: en esta versión el sistema opera en español.
- **Integración simultánea con más de un ERP/Nómina o más de un directorio activo** por instancia de la organización.
- **Negociación o firma electrónica de documentos legales** más allá de adjuntar comprobantes a una solicitud.

## 7. Documentos relacionados

- [`docs/specs/functional/03-especificacion-consolidada.md`](../docs/specs/functional/03-especificacion-consolidada.md) — Documento fuente principal (RF, RN, casos límite).
- [`docs/architecture/c4-containers.md`](../docs/architecture/c4-containers.md) §7 — Fuente de los RNF extraídos.
- [`ENTREGABLES/vision.md`](./vision.md) — Objetivos con KPI de los que se derivan RNF7, RNF10 y RNF11.
- [`ENTREGABLES/user-stories.md`](./user-stories.md) — Historias de las que se derivan los RF.
- [`ENTREGABLES/traceability.md`](./traceability.md) — Matriz RF → HU → Bounded Context → Endpoint.
