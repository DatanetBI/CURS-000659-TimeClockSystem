# Feature Specification: TimeClockSystem v1.0 — Gestión de Asistencias

**Feature Branch**: `001-timeclock-spec-v1`

**Created**: 2026-09-22

**Status**: Draft

**Input**: User description: "Especificación TimeClockSystem v1.0 — sistema web para centralizar la captura de marcas de
asistencia (biometría, RFID, web, móvil, incluso sin conexión), el cálculo automático de horas trabajadas, horas
extra, recargos, atrasos y ausencias, y la gestión de excepciones (permisos, vacaciones, incapacidades) con flujo
de aprobación trazable y bitácora de auditoría inalterable."

## Aclaraciones resueltas antes de esta versión

Estas decisiones de alcance se resolvieron con el negocio antes de redactar esta especificación, para mantener
consistencia con la constitución del proyecto (`.specify/memory/constitution.md` v1.0.0 — Simplicidad Ante Todo,
Idioma y Mercado):

1. **Alcance legal/geográfico**: v1 opera para una sola entidad legal, México, bajo la Ley Federal del Trabajo
   (LFT). No se construye un motor multi-país/multi-entidad legal en esta versión.
2. **Biometría facial**: v1 valida geolocalización (geofencing) de forma real. El reconocimiento facial y la
   prueba de vida (liveness) se modelan como un punto de integración con un proveedor biométrico externo; el
   algoritmo de matching/liveness en sí **no se construye** en v1 (ver Fuera de Alcance).
3. **Integraciones externas (ERP de nómina, directorio activo/HRIS)**: v1 entrega el contrato de exportación e
   importación (archivo/API) del consolidado de horas y de altas/bajas de empleados. La conexión en vivo contra
   un sistema específico de un proveedor queda fuera de esta versión.
4. **Saldo de vacaciones**: se calcula automáticamente según la antigüedad del empleado, conforme a la tabla de
   días de vacaciones de la LFT (incluida la reforma de "vacaciones dignas").

## Clarifications

### Session 2026-09-22

- Q: ¿Qué tipo de contraseña debe usarse para el nuevo método de marcaje por número de empleado (FR-002/FR-003)? →
  A: PIN numérico corto (4 a 6 dígitos), sin caducidad obligatoria en v1.
- Q: ¿Qué tamaño de organización debe soportar el sistema en esta versión, para calibrar las métricas de éxito? →
  A: Pequeña/mediana empresa — hasta 500 empleados activos.
- Q: ¿Qué debe pasar con una marca si el sistema central (backend) está caído, no solo el dispositivo del
  empleado? → A: Se extiende el mismo mecanismo de "pendiente de sincronización" ya definido para falta de
  conectividad del dispositivo, sin definir un SLA de disponibilidad numérico formal en esta versión.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar marca de asistencia validada por ubicación, con soporte sin conexión (Priority: P1)

Un empleado (de oficina, planta, campo o teletrabajo) registra su entrada, salida o receso desde la app móvil, el
portal web o un terminal RFID/biométrico. El sistema valida que la marca ocurra dentro del perímetro autorizado
(geofence) de su centro de trabajo antes de aceptarla como válida. Si el empleado no tiene conexión, la marca se
guarda localmente en la app y se sincroniza automáticamente al recuperar la red. Cuando el canal no cuenta con
biometría facial habilitada (por ejemplo, un terminal sin ese hardware), el empleado puede identificarse
alternativamente con su número de empleado y un PIN numérico (4 a 6 dígitos).

**Why this priority**: Es el flujo del que depende todo lo demás (cálculo de horas, presencia, pre-nómina). Sin
captura de marcas confiable no hay sistema.

**Independent Test**: Puede probarse por completo dando de alta un empleado con un geofence configurado y
verificando que: (a) una marca dentro del perímetro se acepta y aparece en su historial, (b) una marca fuera del
perímetro se rechaza y nunca aparece como válida, (c) una marca capturada sin conexión aparece como "pendiente de
sincronización" y luego pasa a "sincronizada" al recuperar la red — todo desde la interfaz, sin leer código.

**Acceptance Scenarios**:

1. **Given** un empleado con un turno asignado hoy y dentro del geofence de su centro de trabajo, **When**
   registra su marca de entrada, **Then** la aplicación muestra de inmediato la hora registrada y la marca queda
   visible en su historial como válida.
2. **Given** un empleado que ya registró una entrada sin haber registrado su salida, **When** intenta registrar
   una segunda entrada, **Then** el sistema rechaza el registro y muestra un mensaje explicando que ya existe una
   entrada abierta.
3. **Given** un empleado ubicado fuera del perímetro autorizado de su centro de trabajo, **When** intenta
   registrar una marca con geolocalización, **Then** la marca se rechaza, nunca aparece como válida en su
   historial, y se registra un evento de seguridad.
4. **Given** un empleado sin conexión a internet, **When** registra una marca desde la app móvil, **Then** la
   marca se guarda localmente y se muestra como "pendiente de sincronización" hasta que la app recupera la red,
   momento en el cual cambia a "sincronizada" o "rechazada".
5. **Given** una marca que ya fue sincronizada exitosamente, **When** la app reintenta enviar esa misma marca
   (por ejemplo, tras una reconexión intermitente), **Then** el sistema la descarta como duplicada sin generar
   doble conteo de horas.
6. **Given** un terminal sin biometría facial habilitada, **When** el empleado ingresa su número de empleado y su
   PIN correctos, **Then** el sistema lo identifica y registra la marca correspondiente, sujeta a las mismas
   validaciones de geofence y de entrada duplicada que las demás formas de marcaje.
7. **Given** un intento de marcaje con número de empleado o PIN incorrectos, **When** el empleado envía el
   formulario, **Then** el sistema rechaza el registro mostrando un mensaje genérico de "credenciales inválidas",
   sin indicar cuál de los dos datos es incorrecto.

---

### User Story 2 - Configurar turnos y calcular automáticamente horas, atrasos y horas extra (Priority: P1)

Un analista de Recursos Humanos define plantillas de turno (fijo, rotativo, nocturno, flexible, on-call) con su
horario, receso y margen de tolerancia, y las asigna —individualmente o de forma masiva— a empleados, departamentos
o centros de costo. A partir de las marcas registradas y el turno asignado, el sistema calcula automáticamente
horas trabajadas, atrasos, ausencias y horas extra (clasificadas en diurnas, nocturnas, festivas o dominicales).

**Why this priority**: Es el motor de cálculo que convierte marcas crudas en el consolidado que RRHH y Nómina
necesitan; sin él, el registro de marcas por sí solo no genera valor de negocio.

**Independent Test**: Puede probarse creando un turno con tolerancia configurada, asignándolo a un grupo de
empleados, registrando marcas de prueba y verificando en el consolidado del periodo que las horas, atrasos y
horas extra aparecen clasificadas correctamente, sin necesidad de inspeccionar la base de datos.

**Acceptance Scenarios**:

1. **Given** un turno con hora de entrada 08:00 y 10 minutos de tolerancia, **When** un empleado marca entrada a
   las 08:16, **Then** el consolidado del periodo reporta 6 minutos de atraso.
2. **Given** el mismo turno, **When** un empleado marca entrada a las 08:07, **Then** el consolidado no registra
   ninguna tardanza para ese día.
3. **Given** un turno nocturno de 22:00 a 06:00 que inicia un lunes, **When** el empleado completa su jornada,
   **Then** el consolidado distribuye 2 horas al lunes y 6 horas al martes.
4. **Given** un día declarado festivo en el calendario legal, **When** un empleado trabaja su turno completo de 8
   horas ese día, **Then** las 8 horas se clasifican como "horas festivas" en el consolidado (no solo el
   excedente).
5. **Given** un intento de guardar un turno con un valor de tolerancia negativo o mayor a la duración del turno,
   **When** RRHH intenta guardarlo, **Then** el sistema rechaza el guardado.
6. **Given** una asignación masiva de turno a un grupo de 50 empleados donde 3 tienen una restricción horaria
   individual aprobada (p. ej. médica), **When** se ejecuta la asignación, **Then** el resumen final indica
   cuántos empleados fueron asignados y cuántos quedaron excluidos por esa restricción, sin requerir
   reconfiguración manual posterior.
7. **Given** un periodo de pre-nómina ya calculado, **When** se aprueba, con posterioridad, una solicitud que
   corrige una incidencia de ese periodo, **Then** el consolidado de ese periodo se recalcula automáticamente.

---

### User Story 3 - Solicitar y aprobar permisos, vacaciones e incapacidades (Priority: P2)

Un empleado solicita vacaciones, un permiso con o sin goce de sueldo, una incapacidad médica o un día
compensatorio, adjuntando comprobantes cuando corresponde. La solicitud recorre un flujo de aprobación
configurable (supervisor y, si aplica, RRHH), con escalamiento automático si nadie la resuelve a tiempo. El saldo
de vacaciones disponible se calcula automáticamente según la antigüedad del empleado.

**Why this priority**: Es el segundo generador de datos de la pre-nómina (ausencias justificadas) y el punto de
mayor fricción humana (aprobaciones); es necesario para que el consolidado distinga ausencia justificada de
injustificada.

**Independent Test**: Puede probarse enviando una solicitud desde el portal del empleado, aprobándola o
rechazándola desde el portal del supervisor, y verificando que el saldo de vacaciones, el estado de la solicitud
y las notificaciones se actualizan correctamente en pantalla.

**Acceptance Scenarios**:

1. **Given** un empleado con 3 días de vacaciones disponibles, **When** solicita 5 días, **Then** el sistema
   rechaza el envío de la solicitud antes de que llegue a aprobación, indicando el saldo disponible.
2. **Given** un empleado que inicia una solicitud de incapacidad médica sin adjuntar ningún comprobante, **When**
   intenta enviarla, **Then** el sistema bloquea el envío hasta que se adjunte al menos un archivo.
3. **Given** una solicitud pendiente de aprobación por más del plazo máximo configurado (por defecto, 3 días
   hábiles), **When** el plazo se cumple sin que el supervisor decida, **Then** la solicitud se escala
   automáticamente al siguiente nivel de aprobación (RRHH) y aparece en su bandeja.
4. **Given** una solicitud rechazada, **When** el empleado la consulta, **Then** el sistema le muestra el motivo
   del rechazo registrado por quien la rechazó.
5. **Given** un empleado con un año de antigüedad, **When** consulta su saldo de vacaciones, **Then** el sistema
   muestra el saldo calculado automáticamente según la tabla de antigüedad vigente en la Ley Federal del Trabajo,
   sin que RRHH lo haya capturado manualmente.
6. **Given** un día sin marcas para un empleado, **When** no existe ninguna solicitud aprobada vigente para esa
   fecha, **Then** el día se clasifica como "ausencia injustificada" y se notifica al supervisor.

---

### User Story 4 - Panel de presencia y cobertura del supervisor (Priority: P2)

Un supervisor visualiza en tiempo casi real el estado de presencia de su equipo (presente, en receso, ausente,
ausente sin justificar, de vacaciones) y, desde el mismo lugar, aprueba o rechaza las solicitudes pendientes de su
equipo. Ante una ausencia imprevista, reasigna la cobertura del turno a otro empleado disponible.

**Why this priority**: Da al supervisor la capacidad operativa de reaccionar el mismo día ante ausencias, que es
uno de los dolores centrales descritos en el objetivo de negocio.

**Independent Test**: Puede probarse haciendo que un empleado marque entrada y verificando que su estado cambia
en el panel del supervisor sin recargar la página; y reasignando la cobertura de un turno vacante a otro empleado
desde esa misma pantalla.

**Acceptance Scenarios**:

1. **Given** un supervisor con el panel de presencia abierto, **When** uno de sus empleados registra su marca de
   entrada, **Then** el estado de ese empleado cambia a "presente" en el panel sin necesidad de recargar la
   página.
2. **Given** un empleado cuyo turno inició hace más de 30 minutos, sin marca registrada ni solicitud aprobada para
   ese día, **When** el supervisor consulta el panel, **Then** ese empleado aparece como "ausente sin justificar".
3. **Given** una ausencia imprevista, **When** el supervisor elige a otro empleado para cubrir el turno vacante,
   **Then** el sistema aplica la reasignación y la refleja en el panel.
4. **Given** un empleado elegido para cubrir un turno que ya tiene otro turno asignado en el mismo horario,
   **When** el supervisor intenta confirmar la reasignación, **Then** el sistema muestra una advertencia de
   traslape y exige confirmación explícita antes de aplicar el cambio.
5. **Given** una solicitud de permiso, vacaciones o corrección de marca de su equipo, **When** el supervisor la
   aprueba o rechaza desde el panel, **Then** el estado de la solicitud se actualiza y el solicitante es
   notificado.

---

### User Story 5 - Autoservicio del empleado (Priority: P3)

Un empleado consulta, desde el portal o la app, su historial de marcas, su acumulado de horas, sus incidencias y
su saldo de vacaciones (disponible, reservado y utilizado), y recibe notificaciones sobre marcas omitidas y sobre
la resolución de sus solicitudes.

**Why this priority**: Da transparencia al empleado y reduce las consultas manuales a RRHH, pero el sistema ya
genera valor de negocio (cálculo de nómina) sin este portal.

**Independent Test**: Puede probarse iniciando sesión como empleado y verificando que su historial, su saldo de
vacaciones y sus notificaciones reflejan la información esperada, sin intervención de RRHH.

**Acceptance Scenarios**:

1. **Given** un empleado con marcas e incidencias registradas, **When** abre su historial, **Then** ve su
   acumulado de horas y sus incidencias actualizados en tiempo casi real.
2. **Given** un día con una marca omitida, **When** el empleado revisa su tarjeta de asistencia, **Then** ese día
   aparece resaltado visualmente y el empleado puede iniciar desde ahí una solicitud de corrección.
3. **Given** una solicitud del empleado que cambia de estado (aprobada, rechazada o escalada), **When** ocurre ese
   cambio, **Then** el empleado recibe una notificación.

---

### User Story 6 - Exportar consolidado a nómina y sincronizar altas/bajas (Priority: P3)

Un especialista de TI/Nómina configura la exportación programada y automatizada del consolidado de horas e
incidencias del periodo (en el formato de archivo o API definido por el contrato de integración), y la
sincronización de altas y bajas de empleados a partir de un archivo o API de directorio de personal.

**Why this priority**: Cierra el ciclo con el sistema de nómina, pero requiere que los tres flujos anteriores ya
funcionen; no bloquea el valor operativo diario del sistema.

**Independent Test**: Puede probarse ejecutando una exportación programada y verificando, desde una pantalla de
estado, que el lote quedó "exportado" o "fallido"; y cargando un archivo de altas/bajas y verificando que los
empleados correspondientes se reflejan en el sistema.

**Acceptance Scenarios**:

1. **Given** un periodo de pre-nómina cerrado, **When** se ejecuta la exportación programada, **Then** el
   consolidado de horas e incidencias se genera en el formato de exportación configurado.
2. **Given** una exportación que falla, **When** el sistema reintenta según la política configurada (por defecto,
   3 reintentos) y todos los reintentos fallan, **Then** el lote se marca como "Fallido" y se notifica al área de
   TI.
3. **Given** un archivo de altas/bajas de empleados recibido, **When** se procesa, **Then** el sistema refleja las
   altas y bajas correspondientes y el estado de la sincronización queda consultable después de cada ejecución.

---

### User Story 7 - Reportes operativos y auditoría inalterable (Priority: P3)

Un ejecutivo o analista consulta reportes e indicadores (KPIs) de asistencia, puntualidad, ausentismo y horas
extra, exportables a PDF, Excel o CSV. En paralelo, un auditor de cumplimiento consulta una bitácora inalterable
de todas las modificaciones manuales sobre marcas, turnos o solicitudes.

**Why this priority**: Aporta visibilidad de gestión y cumplimiento normativo, pero depende de que los datos
operativos (marcas, turnos, solicitudes) ya existan.

**Independent Test**: Puede probarse generando un reporte de un periodo con datos conocidos y verificando sus
totales en pantalla; y realizando una corrección manual sobre una marca y verificando que queda registrada en la
bitácora de auditoría, incluyendo el intento de editar o borrar esa entrada de bitácora.

**Acceptance Scenarios**:

1. **Given** un periodo con datos de asistencia registrados, **When** un ejecutivo genera el reporte de
   ausentismo y horas extra de ese periodo, **Then** el reporte muestra los indicadores agregados y puede
   exportarse en PDF, Excel o CSV.
2. **Given** una corrección manual sobre una marca, un turno o una solicitud, **When** ocurre esa modificación,
   **Then** queda registrada en la bitácora de auditoría con quién la hizo, cuándo, el valor anterior y el valor
   nuevo.
3. **Given** un registro ya insertado en la bitácora de auditoría, **When** cualquier usuario —incluido un
   administrador— intenta editarlo o eliminarlo desde cualquier pantalla del sistema, **Then** el sistema rechaza
   la operación.
4. **Given** un reporte de auditoría abierto, **When** el auditor lo filtra por empleado o por periodo, **Then**
   el reporte muestra únicamente los cambios correspondientes a ese filtro.

---

### Edge Cases

- **CL1**: Un empleado sin ningún turno asignado para el día intenta marcar asistencia — el sistema debe rechazar
  o marcar la marca como "sin turno asociado" y no debe generarse ningún cálculo de horas para ese registro.
- **CL2**: La misma marca llega dos veces al servidor (por ejemplo, sincronizada offline y también registrada por
  otro canal) — debe detectarse como duplicada y no debe contarse dos veces (ver User Story 1, escenario 5).
- **CL3**: Un turno nocturno que cruza la medianoche coincide exactamente con el cierre del periodo de
  pre-nómina — las horas deben distribuirse entre los dos periodos calendario correspondientes, sin perder ni
  duplicar horas.
- **CL4**: Dos personas intentan aprobar/rechazar la misma solicitud casi al mismo tiempo — el sistema debe
  aceptar solo la primera decisión y notificar a la segunda persona que la solicitud ya fue resuelta.
- **CL5**: Se aprueba una solicitud de vacaciones para un periodo de pre-nómina que ya fue calculado y
  exportado — el sistema debe recalcular ese periodo y señalar que requiere una nueva exportación (o
  reexportación) del periodo afectado.
- **CL6**: La sincronización de altas/bajas reporta la baja de un empleado con una solicitud pendiente de
  aprobación o un turno futuro ya asignado — las solicitudes pendientes deben cancelarse o marcarse para revisión,
  y los turnos futuros deben liberarse, en lugar de fallar silenciosamente.
- **CL7**: El sistema de nómina/ERP no responde durante toda la ventana de reintentos configurada — el lote se
  marca "Fallido" y TI recibe una notificación con el detalle del error (ver User Story 6, escenario 2).
- **CL8**: Un empleado solicita un permiso para una fecha que ya pasó — el sistema debe permitirlo solo como
  "solicitud retroactiva" identificable como tal, sujeta a las mismas reglas de aprobación.
- **CL9**: Un empleado sin consentimiento biométrico/de geolocalización registrado intenta marcar en un canal que
  requiere ese consentimiento — el sistema debe bloquear la marca y solicitar el consentimiento antes de
  aceptarla.
- **CL10**: Se pierde la conectividad de la app móvil en medio de la sincronización de un lote grande de marcas
  pendientes — las marcas ya confirmadas por el servidor no deben reenviarse ni duplicarse cuando la sincronización
  se reanude.
- **CL11**: Un supervisor deja de tener acceso al sistema mientras su equipo tiene solicitudes pendientes — esas
  solicitudes deben poder reasignarse a otro aprobador o seguir la regla de escalamiento por vencimiento de plazo.
- **CL12**: Se intenta cargar de forma masiva un calendario de turnos con filas inválidas o con identificadores de
  empleados inexistentes — el sistema debe procesar las filas válidas y devolver un reporte claro de las filas
  rechazadas y su motivo, sin abortar toda la carga.
- **CL13**: Un empleado ingresa su número de empleado o PIN incorrectos de forma repetida — cada intento se
  rechaza individualmente como credenciales inválidas (ver FR-003); esta versión no define una política de bloqueo
  de cuenta por intentos fallidos (ver Assumptions).
- **CL14**: El sistema central (backend) no está disponible en el momento de un marcaje, aunque el dispositivo del
  empleado sí tenga conectividad — la marca se trata igual que una marca sin conectividad del dispositivo: queda
  como "pendiente de sincronización" y se reintenta automáticamente hasta que el sistema central se restablece
  (ver FR-008).

## Requirements *(mandatory)*

### Functional Requirements

**Marcaje y captura de asistencia**

- **FR-001**: El sistema MUST permitir registrar una marca de entrada, salida, inicio de receso o fin de receso
  desde RFID, portal web, app móvil o terminal físico.
- **FR-002**: El sistema MUST permitir identificar y autenticar a un empleado mediante su número de empleado y un
  PIN numérico de 4 a 6 dígitos, sin caducidad obligatoria en v1, como método de marcaje alternativo a la
  biometría facial, disponible en cualquier canal habilitado (terminal, portal web o app móvil).
- **FR-003**: El sistema MUST rechazar un intento de marcaje por número de empleado y PIN cuando cualquiera de los
  dos datos sea incorrecto, mostrando un mensaje genérico de "credenciales inválidas" sin indicar cuál de los dos
  datos falló.
- **FR-004**: El sistema MUST permitir a RRHH o a un Administrador asignar y restablecer el PIN de marcaje de un
  empleado.
- **FR-005**: El sistema MUST rechazar el registro de una nueva entrada si el empleado ya tiene una entrada abierta
  sin su salida correspondiente.
- **FR-006**: El sistema MUST validar, para cada marca con geolocalización, que las coordenadas capturadas estén
  dentro del perímetro autorizado (geofence) configurado para el empleado o su centro de trabajo, y rechazar la
  marca en caso contrario.
- **FR-007**: El sistema MUST exponer un punto de integración para un proveedor externo de reconocimiento facial y
  prueba de vida (liveness), de modo que una futura fase pueda habilitar esa validación sin rediseñar el flujo de
  marcaje. El algoritmo de matching/liveness en sí queda fuera de esta versión (ver Fuera de Alcance).
- **FR-008**: El sistema MUST permitir capturar y almacenar marcas localmente en la app móvil cuando no haya
  conectividad del dispositivo o cuando el sistema central no esté disponible temporalmente, y sincronizarlas
  automáticamente al restablecerse la conectividad o el servicio.
- **FR-009**: El sistema MUST descartar automáticamente, durante la sincronización, cualquier marca duplicada, sin
  generar doble conteo de horas.
- **FR-010**: El sistema MUST registrar como evento de seguridad todo intento de marcaje rechazado por geofencing.

**Horarios y turnos**

- **FR-011**: El sistema MUST permitir crear y mantener un catálogo de turnos fijos, rotativos, nocturnos,
  flexibles y de disponibilidad (on-call), cada uno con su horario, duración de receso y margen de tolerancia.
- **FR-012**: El sistema MUST distribuir correctamente las horas trabajadas de un turno que cruza la medianoche
  entre las dos fechas calendario correspondientes.
- **FR-013**: El sistema MUST permitir asignar turnos y calendarios de forma individual o de forma masiva a un
  grupo de empleados, departamento o centro de costo.
- **FR-014**: El sistema MUST respetar las restricciones horarias individuales aprobadas (p. ej. médicas) al
  procesar una asignación masiva, sin requerir reconfiguración manual posterior.
- **FR-015**: El sistema MUST rechazar el guardado de un turno cuyo margen de tolerancia sea negativo o mayor que
  la duración del turno.

**Pre-nómina y motor de reglas**

- **FR-016**: El sistema MUST calcular automáticamente las horas trabajadas, los atrasos, las ausencias y las
  horas extra de cada empleado a partir de sus marcas y su turno asignado.
- **FR-017**: El sistema MUST clasificar automáticamente las horas extra en diurnas, nocturnas, festivas o
  dominicales, según el calendario de festivos de México y las reglas configuradas para la organización.
- **FR-018**: El sistema MUST permitir a RRHH configurar los factores de pago y los rangos horarios que definen
  cada tipo de hora y recargo para la organización (una única entidad legal, México), con valores por defecto
  basados en la Ley Federal del Trabajo.
- **FR-019**: El sistema MUST recalcular el consolidado de un periodo ya procesado cuando se apruebe, con
  posterioridad, una solicitud que afecte una incidencia de ese periodo.

**Incidencias, justificaciones y permisos**

- **FR-020**: El sistema MUST permitir a un empleado solicitar vacaciones, permisos con o sin goce de sueldo,
  incapacidades médicas y días compensatorios, adjuntando comprobantes cuando corresponda.
- **FR-021**: El sistema MUST calcular automáticamente el saldo anual de vacaciones de cada empleado según su
  antigüedad, conforme a la tabla vigente de la Ley Federal del Trabajo (incluida la reforma de "vacaciones
  dignas"), y MUST validar ese saldo antes de permitir el envío de una solicitud de vacaciones.
- **FR-022**: El sistema MUST exigir al menos un comprobante adjunto antes de permitir el envío de una solicitud
  de incapacidad médica.
- **FR-023**: El sistema MUST conducir cada solicitud a través de un flujo de aprobación configurable (uno o más
  niveles: supervisor, RRHH) según el tipo de solicitud.
- **FR-024**: El sistema MUST escalar automáticamente una solicitud al siguiente nivel de aprobación si no se
  resuelve dentro del plazo configurado (por defecto, 3 días hábiles).
- **FR-025**: El sistema MUST registrar el motivo de todo rechazo y notificarlo al solicitante.
- **FR-026**: El sistema MUST clasificar un día sin marcas como "ausencia justificada" solo si existe una
  solicitud aprobada vigente para esa fecha; en caso contrario, MUST clasificarlo como "ausencia injustificada" y
  notificar al supervisor.

**Portal del Empleado (autoservicio)**

- **FR-027**: El sistema MUST permitir a cada empleado consultar su historial de marcas, su acumulado de horas y
  sus incidencias, con información actualizada en tiempo casi real.
- **FR-028**: El sistema MUST permitir a cada empleado consultar su saldo de vacaciones disponible, reservado y
  utilizado.
- **FR-029**: El sistema MUST señalar visualmente al empleado cualquier marca omitida y permitirle iniciar desde
  ahí una solicitud de corrección.
- **FR-030**: El sistema MUST notificar al empleado sobre marcas omitidas y sobre la aprobación, rechazo o
  escalamiento de sus solicitudes.

**Panel del Supervisor**

- **FR-031**: El sistema MUST mostrar al supervisor el estado de presencia, en tiempo casi real, de cada
  integrante de su equipo (presente, en receso, ausente, ausente sin justificar, de vacaciones).
- **FR-032**: El sistema MUST marcar como "ausente sin justificar" a un empleado cuyo turno inició hace más de 30
  minutos sin marca registrada ni solicitud aprobada para ese día.
- **FR-033**: El sistema MUST permitir al supervisor aprobar o rechazar, desde un solo lugar, las solicitudes de
  su equipo (permisos, horas extra, correcciones de marca).
- **FR-034**: El sistema MUST permitir al supervisor reasignar la cobertura de un turno ante una ausencia
  imprevista, eligiendo a un empleado disponible.
- **FR-035**: El sistema MUST advertir al supervisor si el empleado elegido para cubrir un turno ya tiene otro
  turno asignado en el mismo horario, y MUST exigir confirmación explícita antes de aplicar el cambio.

**Integración y exportación**

- **FR-036**: El sistema MUST generar, de forma automatizada y programada, un archivo o respuesta de API con el
  consolidado de horas e incidencias del periodo, en un formato de exportación definido y documentado, sin
  requerir una conexión en vivo a un sistema de nómina/ERP específico.
- **FR-037**: El sistema MUST reintentar automáticamente una exportación fallida según una política de reintentos
  configurable (por defecto, 3 intentos), y MUST notificar al área de TI si finalmente no se puede completar.
- **FR-038**: El sistema MUST permitir importar altas y bajas de empleados a partir de un archivo o respuesta de
  API en un formato definido y documentado, sin requerir una conexión en vivo a un directorio activo/HRIS
  específico.
- **FR-039**: El sistema MUST dejar consultable, después de cada ejecución programada, el estado de cada
  exportación o importación (pendiente, exitosa, fallida).

**Reportes**

- **FR-040**: El sistema MUST generar reportes operativos de asistencia, puntualidad, ausentismo y horas extra por
  periodo, empleado, departamento o centro de costo.
- **FR-041**: El sistema MUST presentar indicadores (KPIs) de tasa de ausentismo, costo de horas extra y tendencia
  de tardanzas.
- **FR-042**: El sistema MUST permitir exportar cualquier reporte en formatos PDF, Excel y CSV.

**Administración, seguridad y auditoría**

- **FR-043**: El sistema MUST permitir definir roles con permisos diferenciados por perfil de usuario (empleado,
  supervisor, RRHH, nómina, administrador, auditor, TI, ejecutivo).
- **FR-044**: El sistema MUST registrar, de forma inalterable, quién modificó qué dato, cuándo, y con qué valor
  anterior y nuevo, para toda modificación manual sobre marcas, turnos o solicitudes.
- **FR-045**: El sistema MUST impedir, desde cualquier pantalla y para cualquier perfil (incluido un
  administrador), la modificación o eliminación de un registro ya insertado en la bitácora de auditoría.
- **FR-046**: El sistema MUST solicitar y registrar el consentimiento explícito del empleado antes de capturar sus
  datos de geolocalización, y antes de habilitar cualquier captura biométrica cuando esa integración esté
  disponible.
- **FR-047**: El sistema MUST permitir a un auditor filtrar la bitácora de auditoría por empleado o por periodo.

### Key Entities *(include if feature involves data)*

- **Empleado**: persona cuya asistencia se controla; incluye centro de trabajo/geofence asociado, antigüedad
  (para cálculo de vacaciones), rol y estado (activo/baja).
- **Credencial de Marcaje**: número de empleado y PIN numérico (4 a 6 dígitos) que permiten identificar y
  autenticar a un empleado al registrar una marca cuando no se usa biometría; asignada y restablecible por RRHH o
  Administrador.
- **Turno**: plantilla de horario (fijo, rotativo, nocturno, flexible, on-call) con hora de entrada/salida,
  receso y margen de tolerancia; se asigna a uno o varios empleados en un calendario.
- **Marca de Asistencia**: evento de entrada, salida, inicio o fin de receso, con canal de origen, geolocalización,
  estado (válida, rechazada, pendiente de sincronización) y vínculo al empleado y al turno del día.
- **Solicitud**: permiso, vacaciones, incapacidad o día compensatorio solicitado por un empleado; incluye tipo,
  fechas, comprobantes adjuntos, estado (pendiente, aprobada, rechazada, escalada) y el historial de aprobación.
- **Aprobación**: decisión tomada sobre una solicitud por un nivel de aprobación (supervisor o RRHH), con motivo
  cuando corresponde.
- **Saldo de Vacaciones**: acumulado disponible, reservado y utilizado de un empleado, derivado de su antigüedad.
- **Consolidado de Periodo (Pre-nómina)**: resumen calculado de horas trabajadas, atrasos, ausencias y horas
  extra clasificadas, por empleado y periodo.
- **Calendario de Festivos**: fechas declaradas festivas para México, usadas en la clasificación de horas extra.
- **Geofence / Centro de Trabajo**: perímetro geográfico autorizado para el marcaje de un grupo de empleados.
- **Exportación / Importación**: lote programado de salida (consolidado hacia nómina) o entrada (altas/bajas de
  empleados), con estado y reintentos.
- **Rol**: conjunto de permisos asociado a un perfil de usuario (empleado, supervisor, RRHH, nómina, administrador,
  auditor, TI, ejecutivo).
- **Registro de Auditoría**: entrada inmutable que documenta quién modificó qué dato, cuándo, y los valores
  anterior y nuevo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un empleado puede completar un registro de marca de asistencia (entrada o salida) en menos de 15
  segundos desde que abre la app o el portal.
- **SC-002**: El 100% de las marcas fuera del perímetro autorizado quedan excluidas del cálculo de horas
  trabajadas.
- **SC-003**: El estado de presencia de un empleado se refleja en el panel del supervisor en menos de 10 segundos
  después de registrada su marca, sin recargar la página.
- **SC-004**: El 100% de las solicitudes de vacaciones que exceden el saldo disponible se rechazan antes de llegar
  a un aprobador.
- **SC-005**: El 100% de las solicitudes pendientes más allá del plazo configurado se escalan automáticamente al
  siguiente nivel, sin intervención manual.
- **SC-006**: El consolidado de un periodo reprocesado por una aprobación tardía queda disponible con los valores
  corregidos en menos de 5 minutos desde la aprobación.
- **SC-007**: El 100% de los intentos de modificar o eliminar un registro de la bitácora de auditoría, desde
  cualquier perfil, son rechazados por el sistema.
- **SC-008**: Un supervisor puede reasignar la cobertura de un turno ante una ausencia imprevista en menos de 2
  minutos desde el panel de presencia.
- **SC-009**: Un ejecutivo puede obtener el reporte de ausentismo y horas extra de un periodo, exportado a PDF,
  Excel o CSV, en menos de 1 minuto.
- **SC-010**: El sistema soporta organizaciones de hasta 500 empleados activos sin degradar los tiempos definidos
  en SC-001, SC-003, SC-006, SC-008 y SC-009.

## Assumptions

- El sistema opera para una única entidad legal en México; los factores de pago de horas extra/recargos se
  configuran una sola vez para la organización, con valores por defecto basados en la Ley Federal del Trabajo, y
  RRHH puede ajustarlos si la legislación cambia.
- El reconocimiento facial y la prueba de vida (liveness) no se implementan en v1; el marcaje se valida mediante
  geofencing. El sistema deja un punto de integración explícito para incorporar un proveedor biométrico externo en
  una fase posterior.
- El saldo de vacaciones se calcula con la tabla de antigüedad de la Ley Federal del Trabajo vigente al momento de
  esta especificación (12 días el primer año, incrementos conforme a la reforma de "vacaciones dignas"); RRHH
  puede corregir manualmente un saldo individual en casos excepcionales.
- La exportación hacia nómina/ERP y la importación de altas/bajas se entregan como un contrato de archivo/API
  documentado; la conexión en vivo contra un proveedor de ERP o HRIS específico no es parte de esta versión.
- "Tiempo casi real" en los paneles de presencia y en el portal del empleado significa una actualización visible
  en la interfaz sin recargar la página, con una latencia objetivo menor a 10 segundos (ver SC-003), sin implicar
  un requisito técnico de transporte específico.
- El plazo por defecto para el escalamiento automático de solicitudes es de 3 días hábiles, ajustable por RRHH por
  tipo de solicitud.
- La política de reintentos por defecto para una exportación fallida es de 3 intentos antes de marcarla como
  "Fallida".
- Todo el producto (interfaz, mensajes, reportes) se entrega en español de México, con montos monetarios en Pesos
  Mexicanos, conforme a la constitución del proyecto.
- El marcaje por número de empleado y PIN es un método adicional de identificación, no reemplaza las validaciones
  ya existentes (geofence, entrada duplicada) que siguen aplicando según el canal usado; su gestión (asignación y
  restablecimiento) es responsabilidad de RRHH o Administrador, sin autoservicio de restablecimiento por parte del
  propio empleado en v1. El PIN es numérico (4 a 6 dígitos) y no tiene caducidad obligatoria en esta versión. Esta
  versión no define una política de bloqueo de cuenta por intentos fallidos repetidos (ver CL13); cada intento
  inválido simplemente se rechaza.
- El sistema está dimensionado para organizaciones de hasta 500 empleados activos en v1 (ver SC-010); un
  crecimiento significativo por encima de ese volumen puede requerir revisar las metas de tiempo de SC-003 y
  SC-006 en una fase posterior.
- Una caída temporal del sistema central (backend) se trata con el mismo mecanismo de "pendiente de
  sincronización" ya definido para la falta de conectividad del dispositivo (FR-008, CL14); esta versión no define
  un SLA de disponibilidad numérico formal.

## Fuera de Alcance

- Cálculo completo de nómina (bruto a neto, deducciones legales, impuestos): el sistema exporta el consolidado de
  horas e incidencias ya calculado; el cálculo final de la nómina permanece en el sistema de nómina/ERP destino.
- Gestión de reclutamiento, evaluación de desempeño u otros módulos de HRIS no relacionados con asistencia.
- Fabricación o suministro de hardware biométrico o de terminales RFID: el sistema se integra con dispositivos de
  terceros vía un punto de integración definido; no fabrica ni distribuye dispositivos.
- El algoritmo de reconocimiento facial y de prueba de vida (liveness) en sí mismo: v1 deja el punto de
  integración listo, pero no implementa ni certifica un proveedor biométrico.
- Soporte para más de una entidad legal o país en la misma instancia (multi-país/multi-tenant legal).
- Conexión en vivo contra un ERP de nómina o un directorio activo/HRIS específico: v1 entrega el contrato de
  archivo/API; la conexión con un proveedor concreto se evalúa en una fase posterior.
- Negociación o modelado de convenios colectivos complejos más allá de las reglas de horas extra y recargos
  parametrizables ya definidas.
- Una plataforma de Business Intelligence con modelado analítico avanzado (data warehouse, minería de datos,
  machine learning predictivo): los reportes y KPIs se sirven directamente desde los datos operativos.
- Soporte multi-idioma de la interfaz: en esta versión el sistema opera en español de México.
- Firma electrónica de documentos legales más allá de adjuntar comprobantes a una solicitud.
