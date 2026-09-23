| Campo | Valor |
|---|---|
| Estado | Borrador para revisión |
| Autor | Software Architect / Lead Requirements Engineer |
| Fecha | 2026-09-22 |
| Insumos | `docs/architecture/c4-containers.md`, `docs/architecture/domain-model.md`, `docs/specs/functional/01-vision-document.md`, `docs/specs/functional/02-user-stories.md`, `docs/specs/functional/VALIDATION.md` |

# Especificación: TimeClockSystem v1.0

## 1. Objetivo y contexto de negocio

Las organizaciones con fuerza laboral distribuida (oficina, planta, campo y teletrabajo) hoy resuelven el control de asistencia de forma fragmentada: relojes biométricos físicos que no cubren al personal remoto, marcas manuales propensas a error y fraude ("buddy punching"), y el cálculo de horas extra, recargos y ausencias en hojas de cálculo que generan reprocesos y reclamos laborales.

**TimeClockSystem** resuelve esto centralizando tres cosas: (1) la **captura** de marcas de entrada, salida y receso por cualquier canal (biometría, RFID, web, móvil, incluso sin conexión), (2) el **cálculo automático** de horas trabajadas, horas extra, recargos, atrasos y ausencias contra los turnos asignados, y (3) la **gestión de excepciones** (permisos, vacaciones, incapacidades) con un flujo de aprobación trazable — todo bajo un marco de auditoría inalterable y de protección de datos biométricos.

En la vida real, esto se traduce en: un empleado de campo marca su entrada desde el celular sin necesitar ir a una oficina; un supervisor ve en un panel quién de su equipo está presente, ausente o de vacaciones y reubica cobertura si alguien falta sin avisar; un analista de RRHH ya no arma manualmente el consolidado de horas extra del mes; el encargado de nómina recibe ese consolidado ya calculado y lo procesa en el ERP; y un auditor puede reconstruir, ante una inspección laboral, quién modificó qué marca o permiso y cuándo.

## 2. Usuarios

| Perfil | Qué usa | Qué sabe | Qué NO sabe / asume el sistema |
|---|---|---|---|
| **Empleado** (oficina, planta, campo o teletrabajo) | App móvil, portal web (ESS), terminal biométrico/RFID | Su rutina diaria de entrada/salida; a veces tiene conectividad inestable en campo | No conoce (ni debería necesitar conocer) las reglas legales de cálculo de horas extra/recargos, ni el flujo interno de aprobación de su solicitud — solo ve el resultado (aprobado/rechazado) |
| **Supervisor / Jefatura** | Portal web (MSS) | Quién de su equipo debería estar trabajando hoy y en qué turno | No necesariamente conoce el detalle de cómo se calculan los montos de nómina; solo necesita aprobar/rechazar con criterio operativo |
| **Analista de Recursos Humanos** | Portal web (configuración de turnos, incidencias) | Las políticas de tolerancia, turnos y flujos de aprobación de la organización | No opera dispositivos biométricos ni conectores técnicos con el ERP |
| **Encargado de Nómina** | Consulta de exportaciones/consolidado | Cómo se procesa el pago en el ERP destino | **Recibe el resultado del sistema (el consolidado de horas) sin haber usado la app de marcaje** — su interacción es solo con el dato ya calculado |
| **Administrador del Sistema** | Configuración de dispositivos, geofences, roles | Aspectos técnicos de configuración del sistema | No define reglas legales de nómina (eso es de RRHH) |
| **Auditor de Cumplimiento** | Consulta de bitácora de auditoría | Requisitos legales de trazabilidad y protección de datos | No participa en la operación diaria de marcaje ni aprobación |
| **Especialista de TI / Integraciones** | Monitoreo de conectores ERP/HRIS | Aspectos técnicos de integración | No conoce necesariamente las reglas de negocio de asistencia |
| **Ejecutivo / Gerencia** | Reportes y dashboards | Indicadores agregados (ausentismo, costo de horas extra) | **También recibe el resultado (reportes) sin operar la app día a día** |
| *(externo)* **Inspector laboral** | Ninguno — recibe evidencia exportada | Legislación laboral aplicable | Nunca abre el sistema; recibe únicamente el archivo de evidencia de auditoría que el Auditor le entrega |

## 3. Escenarios de usuario (historias)

- **HU1** (US-001) — Como empleado de campo o teletrabajador, quiero registrar mi marca de entrada y salida desde la app móvil o el portal web, para validar mi jornada laboral sin estar físicamente en la oficina.
- **HU2** (US-002) — Como administrador del sistema, quiero definir perímetros geográficos (geofencing) y validación facial para los marcajes, para evitar suplantaciones de identidad y asegurar que los registros se realicen en ubicaciones autorizadas.
- **HU3** (US-003) — Como empleado en zonas con baja conectividad, quiero registrar mi asistencia de manera offline en la app, para que mis marcas se sincronicen automáticamente cuando vuelva a tener red.
- **HU4** (US-004) — Como analista de Recursos Humanos, quiero definir plantillas de turnos (fijos, nocturnos, rotativos) con reglas de margen/tolerancia, para adaptarme a los esquemas operativos de cada departamento.
- **HU5** (US-005) — Como supervisor de área, quiero asignar calendarios y turnos a un grupo de empleados de forma masiva, para optimizar el tiempo de planificación semanal o mensual.
- **HU6** (US-006) — Como encargado de nómina, quiero que el sistema clasifique automáticamente las horas extra (diurnas, nocturnas, festivas) y recargos según la legislación local, para reducir inconsistencias y tiempo en el procesamiento de salarios.
- **HU7** (US-007) — Como analista de Recursos Humanos, quiero que el sistema identifique inmediatamente cuando un empleado sobrepase la tolerancia de llegada o no presente marcas, para aplicar los descuentos o alertas oportunas.
- **HU8** (US-008) — Como empleado, quiero solicitar días de vacaciones o permisos con/sin goce de sueldo adjuntando justificantes, para regularizar mis faltas o ausencias programadas.
- **HU9** (US-009) — Como supervisor, quiero recibir notificaciones para aprobar o rechazar solicitudes de permisos, horas extra o correcciones de marcas de mi equipo, para mantener actualizada la pre-nómina.
- **HU10** (US-010) — Como empleado, quiero revisar mi historial de marcas, acumulado de horas trabajadas y saldo de vacaciones en tiempo real, para tener transparencia sobre mi cumplimiento laboral.
- **HU11** (US-011) — Como supervisor, quiero visualizar un dashboard en tiempo real con el estado actual de mi equipo (presentes, ausentes, en receso o de vacaciones), para tomar decisiones operativas inmediatas ante faltas imprevistas.
- **HU12** (US-011b) — Como supervisor, quiero reasignar rápidamente la cobertura de un turno ante una ausencia imprevista, para garantizar la continuidad operativa del equipo.
- **HU13** (US-012) — Como especialista de TI/Nómina, quiero integrar el sistema mediante API REST o archivos estructurados con el ERP de nómina, para automatizar la transferencia del consolidado de horas sin intervención manual.
- **HU14** (US-013) — Como oficial de cumplimiento/auditor de TI, quiero consultar un registro inalterable de todas las modificaciones manuales sobre marcas o permisos, para garantizar la integridad del sistema ante inspecciones laborales.

## 4. Requisitos funcionales

### 4.1 Marcaje y captura de asistencia

- **RF1**. El sistema debe permitir registrar una marca de entrada, salida, inicio de receso o fin de receso desde biometría física, RFID, portal web o app móvil.
- **RF2**. El sistema debe rechazar el registro de una nueva entrada si el empleado ya tiene una entrada abierta sin su salida correspondiente.
- **RF3**. El sistema debe validar, para cada marca con geolocalización, que las coordenadas capturadas estén dentro del perímetro autorizado (geofence) configurado para el empleado o su centro de trabajo.
- **RF4**. El sistema debe verificar, mediante prueba de vida (liveness), que el reconocimiento facial corresponde a una persona presente en el momento de la marca, antes de aceptarla.
- **RF5**. El sistema debe permitir capturar y almacenar marcas localmente en la app móvil cuando no haya conectividad, y sincronizarlas automáticamente al recuperarla.
- **RF6**. El sistema debe descartar automáticamente, durante la sincronización, cualquier marca duplicada, sin generar doble conteo de horas.
- **RF7**. El sistema debe registrar como evento de seguridad todo intento de marcaje rechazado por geofencing o por fallo de liveness.

### 4.2 Horarios y turnos

- **RF8**. El sistema debe permitir crear y mantener un catálogo de turnos fijos, rotativos, nocturnos, flexibles y de disponibilidad (on-call), cada uno con su horario, duración de receso y márgenes de tolerancia.
- **RF9**. El sistema debe distribuir correctamente las horas trabajadas de un turno que cruza la medianoche entre las dos fechas calendario correspondientes.
- **RF10**. El sistema debe permitir asignar turnos y calendarios de forma individual o de forma masiva a un grupo de empleados, departamento o centro de costo.
- **RF11**. El sistema debe respetar las restricciones horarias individuales aprobadas (p. ej. médicas) al procesar una asignación masiva, sin requerir reconfiguración manual posterior.

### 4.3 Pre-nómina y motor de reglas

- **RF12**. El sistema debe calcular automáticamente las horas trabajadas, los atrasos, las ausencias y las horas extra de cada empleado a partir de sus marcas y su turno asignado.
- **RF13**. El sistema debe clasificar automáticamente las horas extra en diurnas, nocturnas, festivas o dominicales, según el calendario de festivos y la legislación configurada para la entidad legal del empleado.
- **RF14**. El sistema debe permitir configurar, por entidad legal o país, los factores de pago y los rangos horarios que definen cada tipo de hora y recargo.
- **RF15**. El sistema debe recalcular el consolidado de un periodo ya procesado cuando se apruebe, con posterioridad, una solicitud que afecte una incidencia de ese periodo.

### 4.4 Incidencias, justificaciones y permisos

- **RF16**. El sistema debe permitir a un empleado solicitar vacaciones, permisos con o sin goce de sueldo, incapacidades médicas y días compensatorios, adjuntando comprobantes cuando corresponda.
- **RF17**. El sistema debe validar el saldo de vacaciones disponible antes de permitir el envío de una solicitud de vacaciones.
- **RF18**. El sistema debe conducir cada solicitud a través de un flujo de aprobación configurable (uno o más niveles: supervisor, RRHH) según el tipo de solicitud.
- **RF19**. El sistema debe escalar automáticamente una solicitud al siguiente nivel de aprobación si no se resuelve dentro del plazo configurado.
- **RF20**. El sistema debe registrar el motivo de todo rechazo y notificarlo al solicitante.

### 4.5 Portal del Empleado (ESS)

- **RF21**. El sistema debe permitir a cada empleado consultar su historial de marcas, su acumulado de horas y sus incidencias, con información actualizada en tiempo casi real.
- **RF22**. El sistema debe permitir a cada empleado consultar su saldo de vacaciones disponible, reservado y utilizado.
- **RF23**. El sistema debe señalar visualmente al empleado cualquier marca omitida y permitirle iniciar desde ahí una solicitud de corrección.
- **RF24**. El sistema debe notificar al empleado sobre marcas omitidas y sobre la aprobación o rechazo de sus solicitudes.

### 4.6 Portal del Supervisor (MSS)

- **RF25**. El sistema debe mostrar al supervisor el estado de presencia en tiempo real de cada integrante de su equipo (presente, en receso, ausente, ausente sin justificar, de vacaciones).
- **RF26**. El sistema debe permitir al supervisor aprobar o rechazar, desde un solo lugar, las solicitudes de su equipo (permisos, horas extra, correcciones de marca).
- **RF27**. El sistema debe permitir al supervisor reasignar la cobertura de un turno ante una ausencia imprevista, eligiendo a un empleado disponible.
- **RF28**. El sistema debe advertir al supervisor si el empleado elegido para cubrir un turno ya tiene otro turno asignado en el mismo horario, exigiendo su confirmación explícita antes de aplicar el cambio.

### 4.7 Integración y exportación

- **RF29**. El sistema debe exportar, de forma automatizada y programada, el consolidado de horas e incidencias del periodo hacia el sistema de nómina/ERP configurado.
- **RF30**. El sistema debe reintentar automáticamente una exportación fallida según una política de reintentos configurable, y notificar si finalmente no se puede completar.
- **RF31**. El sistema debe sincronizar de forma automatizada las altas y bajas de empleados desde el directorio activo/HRIS de la organización.

### 4.8 Reportes y Business Intelligence

- **RF32**. El sistema debe generar reportes operativos de asistencia, puntualidad, ausentismo y horas extra por periodo, empleado, departamento o centro de costo.
- **RF33**. El sistema debe presentar indicadores (KPIs) de tasa de ausentismo, costo de horas extra y tendencia de tardanzas.
- **RF34**. El sistema debe permitir exportar cualquier reporte en formatos PDF, Excel y CSV.

### 4.9 Administración, seguridad y auditoría

- **RF35**. El sistema debe permitir definir roles con permisos diferenciados por perfil de usuario (empleado, supervisor, RRHH, nómina, administrador, auditor, TI, ejecutivo).
- **RF36**. El sistema debe registrar, de forma inalterable, quién modificó qué dato, cuándo y con qué valor anterior y nuevo, para toda modificación manual sobre marcas, turnos o solicitudes.
- **RF37**. El sistema debe impedir cualquier modificación o eliminación posterior de un registro ya insertado en la bitácora de auditoría.
- **RF38**. El sistema debe solicitar y registrar el consentimiento explícito del empleado antes de capturar sus datos biométricos o de geolocalización.

## 5. Reglas de negocio (con ejemplos)

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

## 6. Criterios de aceptación (verificables sin código)

- **CA1**. Al marcar entrada con éxito, la app muestra de inmediato la hora registrada.
- **CA2**. Un intento de marcar una segunda entrada sin salida previa muestra un mensaje explicando que ya existe una entrada abierta.
- **CA3**. Una marca capturada fuera del perímetro autorizado nunca aparece como "válida" en el historial del empleado.
- **CA4**. Un intento de marcaje facial con una foto o video (no persona real) es rechazado y no se registra como marca válida.
- **CA5**. Una marca registrada sin conexión aparece en la app como "pendiente de sincronización" hasta recuperar la red, y luego cambia a "sincronizada" o "rechazada".
- **CA6**. Un turno configurado con un valor de tolerancia negativo o mayor a su duración no puede guardarse.
- **CA7**. Al asignar turnos de forma masiva a un grupo, el resumen final muestra cuántos empleados fueron asignados y cuántos quedaron excluidos por restricciones.
- **CA8**. El consolidado de un periodo muestra las horas extra separadas por tipo (diurna, nocturna, festiva, dominical).
- **CA9**. Un día calificado como festivo en el calendario legal hace que toda la jornada trabajada ese día aparezca clasificada como "hora festiva".
- **CA10**. Una solicitud de vacaciones que excede el saldo disponible se rechaza en pantalla antes de poder enviarse.
- **CA11**. Una solicitud de incapacidad médica no puede enviarse si no tiene al menos un archivo adjunto.
- **CA12**. Una solicitud rechazada siempre muestra al empleado el motivo del rechazo.
- **CA13**. Una solicitud sin resolver después del plazo configurado cambia de estado a "escalada" y aparece en la bandeja del siguiente nivel de aprobación.
- **CA14**. La tarjeta de asistencia del empleado resalta visualmente cualquier día con una marca omitida.
- **CA15**. El panel de presencia del supervisor cambia el estado de un empleado a "presente" apenas se registra su marca de entrada, sin recargar la página.
- **CA16**. Un empleado con turno iniciado hace más de 30 minutos, sin marca ni solicitud aprobada, aparece como "ausente sin justificar" en el panel del supervisor.
- **CA17**. Al reasignar cobertura a un empleado que ya tiene otro turno en el mismo horario, el sistema muestra una advertencia antes de permitir confirmar.
- **CA18**. El estado de cada exportación hacia el ERP (pendiente, exportada, fallida) es consultable después de cada ejecución programada.
- **CA19**. Un reporte de auditoría puede filtrarse por empleado o por periodo, y muestra para cada cambio quién lo hizo y cuándo.
- **CA20**. Ningún usuario, incluido un administrador, puede editar o borrar un registro ya existente de la bitácora de auditoría desde ninguna pantalla del sistema.

## 7. Casos límite

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

## 8. Fuera de alcance

- **Cálculo completo de nómina** (bruto a neto, deducciones legales, impuestos): el sistema exporta el consolidado de horas e incidencias ya calculado; el cálculo final de la nómina permanece en el ERP/sistema de nómina destino.
- **Gestión de reclutamiento, evaluación de desempeño** u otros módulos de HRIS no relacionados con asistencia.
- **Fabricación o suministro de hardware biométrico**: el sistema se integra con terminales de terceros vía SDK/API; no fabrica ni distribuye dispositivos.
- **Negociación o modelado de convenios colectivos complejos** más allá de las reglas de horas extra y recargos parametrizables ya definidas en el motor de pre-nómina.
- **Una plataforma de Business Intelligence con modelado analítico avanzado** (data warehouse, minería de datos, machine learning predictivo): en esta versión los reportes y KPIs se sirven directamente desde los datos operativos.
- **Soporte multi-idioma de la interfaz**: en esta versión el sistema opera en español.
- **Integración simultánea con más de un ERP/Nómina o más de un directorio activo** por instancia de la organización.
- **Negociación o firma electrónica de documentos legales** más allá de adjuntar comprobantes a una solicitud.

---

## Documentos relacionados

- `docs/specs/functional/01-vision-document.md` — Objetivos de negocio y perfiles de usuario que fundamentan las secciones 1 y 2.
- `docs/specs/functional/02-user-stories.md` — Historias de usuario en Gherkin de las que se derivan las secciones 3, 5, 6 y 7.
- `docs/specs/functional/VALIDATION.md` — Evidencia de validación sintáctica de los diagramas de secuencia de las historias de usuario.
- `docs/architecture/domain-model.md` — Modelo de dominio (agregados, invariantes) del que se derivan las reglas de negocio de la sección 5.
- `docs/architecture/c4-containers.md` — Arquitectura de contenedores que implementa estos requisitos funcionales.
