# Use Cases — Time Clock System

> **Entregable**: `use-cases.md` — Actores, flujos principales y alternativos.
> **Fuentes originales**: no existe un documento de casos de uso UML clásico en el material fuente. Este entregable se construyó **reorganizando** contenido ya existente, sin reescribir el texto original:
> - Tabla de actores: [`docs/specs/functional/01-vision-document.md`](../docs/specs/functional/01-vision-document.md) §5.
> - Flujos principales/alternativos: los mismos escenarios Gherkin de [`docs/specs/functional/02-user-stories.md`](../docs/specs/functional/02-user-stories.md), reagrupados bajo encabezados "Flujo principal" / "Flujo(s) alternativo(s)" en vez de "Scenario", **sin modificar el texto de los `Given/When/Then`**.
>
> **Criterio de clasificación** (decisión editorial de este entregable, ya que el original no distingue explícitamente principal/alternativo): el primer escenario de éxito ("camino feliz") de cada historia se marca como *Flujo principal*; el resto (rechazos, validaciones fallidas, variantes de canal o de consulta) se marca como *Flujo(s) alternativo(s)*. Cada caso de uso conserva el ID `CU-0XX`, alineado 1:1 con el ID de historia `US-0XX` correspondiente para trazabilidad (ver [`ENTREGABLES/traceability.md`](./traceability.md)).

---

## 1. Actores

*Fuente: `01-vision-document.md` §5 (tabla de perfiles de usuario, reproducida íntegra).*

| Actor | Descripción | Objetivos principales | Módulos que utiliza principalmente |
|---|---|---|---|
| **Empleado (Employee)** | Colaborador operativo, administrativo, de campo o teletrabajador sujeto a control de asistencia | Registrar marcas correctamente, consultar su historial, solicitar permisos/vacaciones, corregir omisiones | 1, 5 |
| **Supervisor / Jefatura (Manager)** | Líder de equipo o de centro de costo responsable de la cobertura operativa | Aprobar/rechazar solicitudes, monitorear presencia en tiempo real, reasignar turnos ante ausencias | 4, 6, 2 |
| **Analista de Recursos Humanos (HR Analyst)** | Responsable de la administración de asistencia a nivel organizacional | Configurar turnos, tolerancias, revisar incidencias, preparar la pre-nómina | 2, 3, 4, 8 |
| **Encargado de Nómina (Payroll Specialist)** | Responsable de procesar el pago de remuneraciones | Consumir el consolidado de horas/incidencias calculado, validar exportaciones al ERP | 3, 7, 8 |
| **Administrador del Sistema (System Administrator)** | Responsable de la configuración técnica y de seguridad del TCS | Gestionar dispositivos, geofencing, roles y permisos, integración con directorio activo | 1, 7, 9 |
| **Oficial de Cumplimiento / Auditor de TI (Compliance Officer / IT Auditor)** | Responsable de verificar el cumplimiento normativo y la integridad de los datos | Consultar bitácoras de auditoría inalterables, verificar consentimientos biométricos | 9, 8 |
| **Especialista de TI / Integraciones (IT Integration Specialist)** | Responsable de la interoperabilidad con otros sistemas corporativos | Configurar y monitorear conectores ERP/HRIS, resolver incidentes de sincronización | 7 |
| **Ejecutivo / Gerencia (Executive Sponsor)** | Consumidor de reportes estratégicos sobre costo laboral y cumplimiento | Visualizar KPIs de ausentismo, costo de horas extra y tendencias por departamento | 8 |

---

## 2. Casos de uso

### CU-001 — Marcaje Web/Móvil

**Actor principal**: Empleado. **Historia origen**: US-001.

**Flujo principal**:
```gherkin
  Scenario: Registro exitoso de marca de entrada desde la app móvil
    Given el empleado tiene sesión iniciada en la app móvil
    And el empleado no tiene una marca de entrada abierta en el turno actual
    When el empleado presiona "Marcar Entrada"
    Then el sistema registra la marca con fecha, hora y canal "App Móvil"
    And el sistema muestra una confirmación visual con la hora registrada

  Scenario: Registro exitoso de marca de salida desde el portal web
    Given el empleado tiene una marca de entrada abierta
    When el empleado presiona "Marcar Salida" desde el portal web
    Then el sistema registra la marca de salida con fecha, hora y canal "Portal Web"
    And el sistema calcula el tiempo transcurrido desde la entrada
```

**Flujo alternativo**:
```gherkin
  Scenario: Intento de doble marca de entrada
    Given el empleado ya tiene una marca de entrada abierta sin salida registrada
    When el empleado intenta marcar entrada nuevamente
    Then el sistema rechaza la operación
    And el sistema muestra el mensaje "Ya existe una entrada abierta, registre su salida primero"
```

---

### CU-002 — Marcaje Biométrico con Geofencing

**Actor principal**: Administrador del Sistema (configura); Empleado (ejecuta el marcaje). **Historia origen**: US-002.

**Flujo principal**:
```gherkin
  Scenario: Marca válida dentro del perímetro autorizado con reconocimiento facial exitoso
    Given el empleado tiene configurado un geofence de 150 metros alrededor de su centro de trabajo
    And el empleado se encuentra dentro del perímetro autorizado
    When el empleado realiza el marcaje mediante reconocimiento facial
    And la prueba de vida (liveness detection) confirma que es una persona real
    And el motor de reconocimiento facial confirma una coincidencia de identidad válida
    Then el sistema registra la marca como válida
    And el sistema almacena las coordenadas GPS y el score de confianza biométrica
```

**Flujos alternativos**:
```gherkin
  Scenario: Intento de marcaje fuera del perímetro autorizado
    Given el empleado tiene configurado un geofence para su centro de trabajo
    And el empleado se encuentra a 500 metros fuera del perímetro autorizado
    When el empleado intenta marcar asistencia
    Then el sistema rechaza la marca
    And el sistema notifica al supervisor sobre el intento fuera de perímetro
    And el sistema registra el intento fallido en la bitácora de seguridad

  Scenario: Intento de suplantación detectado por liveness detection
    Given el empleado intenta marcar usando reconocimiento facial
    When el sistema detecta que la imagen proviene de una fotografía o video (no persona en vivo)
    Then el sistema rechaza el marcaje
    And el sistema clasifica el intento como "posible fraude"
    And el sistema notifica al administrador de seguridad
```

---

### CU-003 — Marcaje Offline y Sincronización

**Actor principal**: Empleado. **Historia origen**: US-003.

**Flujo principal**:
```gherkin
  Scenario: Registro de marca sin conexion a internet
    Given el empleado no tiene conectividad a internet
    When el empleado registra una marca de entrada desde la app movil
    Then el sistema almacena la marca localmente en el dispositivo con estado "Pendiente de sincronizacion"
    And el sistema muestra al empleado un indicador de "marca guardada localmente"

  Scenario: Sincronizacion automatica al recuperar conectividad
    Given el empleado tiene una o mas marcas pendientes de sincronizacion almacenadas localmente
    When el dispositivo recupera conectividad a internet
    Then la app sincroniza automaticamente las marcas pendientes con el servidor en orden cronologico
    And el servidor aplica las validaciones de geofencing y reglas de negocio correspondientes a la hora original de captura
    And el estado de cada marca cambia a "Sincronizada" o "Rechazada" segun el resultado de la validacion
```

**Flujo alternativo**:
```gherkin
  Scenario: Conflicto de sincronizacion por marca duplicada
    Given una marca fue capturada offline con la misma hora y tipo de una marca ya sincronizada previamente desde otro dispositivo
    When la app intenta sincronizar la marca offline
    Then el sistema detecta el duplicado
    And el sistema descarta la marca duplicada sin generar doble conteo de horas
    And el sistema notifica al empleado sobre el duplicado descartado
```

---

### CU-004 — Configuración de Turnos Rotativos y Fijos

**Actor principal**: Analista de Recursos Humanos. **Historia origen**: US-004.

**Flujo principal** (tres variantes de configuración exitosa):
```gherkin
  Scenario: Creacion de un turno fijo diurno con tolerancia
    Given el analista de RRHH accede al catalogo de turnos
    When crea un turno "Administrativo 08:00-17:00" con 10 minutos de tolerancia de entrada
    Then el sistema guarda el turno como disponible para asignacion
    And el sistema aplica la tolerancia configurada en el calculo de atrasos para ese turno

  Scenario: Creacion de un turno nocturno que cruza medianoche
    Given el analista de RRHH crea un turno "Nocturno 22:00-06:00"
    When el turno es asignado a un empleado
    Then el sistema calcula correctamente las horas trabajadas distribuidas entre dos dias calendario
    And el sistema marca las horas comprendidas en el rango nocturno legal para efectos de recargo

  Scenario: Creacion de un turno rotativo con patron de rotacion
    Given el analista de RRHH define un turno rotativo con patron "Manana-Tarde-Noche-Descanso" de 4 dias
    When el patron se activa para un grupo de empleados a partir de una fecha
    Then el sistema genera automaticamente el calendario proyectado de cada empleado segun el patron
    And el sistema permite visualizar la proyeccion de turnos futuros por empleado
```

**Flujo alternativo**:
```gherkin
  Scenario: Intento de crear un turno con tolerancia invalida
    Given el analista de RRHH esta creando un nuevo turno
    When ingresa un valor de tolerancia negativo o mayor a la duracion del turno
    Then el sistema rechaza la configuracion
    And el sistema muestra un mensaje de validacion indicando el rango permitido
```

---

### CU-005 — Asignación Masiva de Calendarios

**Actor principal**: Supervisor / Jefatura. **Historia origen**: US-005.

**Flujo principal**:
```gherkin
  Scenario: Asignacion masiva exitosa por departamento
    Given el supervisor selecciona el departamento "Logistica" con 25 empleados activos
    When asigna el turno "Rotativo Logistica" para el periodo del mes siguiente
    Then el sistema asigna el turno a los 25 empleados seleccionados
    And el sistema muestra un resumen de la asignacion con la cantidad de empleados afectados
```

**Flujos alternativos**:
```gherkin
  Scenario: Asignacion masiva con excepciones individuales
    Given el supervisor realiza una asignacion masiva de turno a un grupo de empleados
    And 2 de los empleados tienen una restriccion de horario aprobada (ej. lactancia, condicion medica)
    When el supervisor confirma la asignacion masiva
    Then el sistema aplica el turno estandar a los empleados sin restriccion
    And el sistema excluye o ajusta automaticamente a los empleados con restriccion activa
    And el sistema notifica al supervisor sobre las excepciones aplicadas

  Scenario: Carga masiva de calendarios mediante archivo
    Given el supervisor descarga la plantilla de carga masiva de calendarios
    When sube un archivo con la asignacion de turnos por empleado y fecha
    Then el sistema valida el formato y los identificadores de empleado
    And el sistema muestra un reporte de filas procesadas exitosamente y filas con error antes de confirmar la carga
```

---

### CU-006 — Cálculo Automático de Horas Extra y Recargos

**Actor principal**: Encargado de Nómina. **Historia origen**: US-006.

**Flujo principal**:
```gherkin
  Scenario: Clasificacion de horas extra diurnas
    Given un empleado tiene un turno de 8 horas de 08:00 a 17:00
    And el empleado registra marcas de 08:00 a 19:00
    When el motor de calculo procesa la jornada
    Then el sistema calcula 2 horas extra diurnas
    And el sistema aplica el factor de pago configurado para horas extra diurnas
```

**Flujos alternativos** (variantes de clasificación):
```gherkin
  Scenario: Clasificacion de horas extra en dia festivo
    Given la fecha del turno corresponde a un dia festivo configurado en el calendario legal
    And el empleado registra marcas completas de su turno asignado
    When el motor de calculo procesa la jornada
    Then el sistema clasifica todas las horas trabajadas ese dia como "Horas festivas"
    And el sistema aplica el factor de recargo festivo configurado para la entidad legal correspondiente

  Scenario: Clasificacion de horas extra nocturnas dentro de un turno diurno extendido
    Given un empleado con turno diurno permanece trabajando mas alla de las 22:00
    When el motor de calculo procesa la jornada
    Then las horas trabajadas despues de las 22:00 se clasifican como "Horas extra nocturnas"
    And se aplica el factor combinado de hora extra y recargo nocturno segun configuracion legal

  Scenario: Reglas parametrizables por pais o entidad legal
    Given dos entidades legales de la organizacion tienen porcentajes distintos de recargo por hora extra festiva
    When el motor de calculo procesa jornadas de empleados de ambas entidades
    Then el sistema aplica el porcentaje de recargo configurado para la entidad legal correspondiente a cada empleado
```

---

### CU-007 — Detección Automática de Tardanzas y Ausencias

**Actor principal**: Analista de Recursos Humanos. **Historia origen**: US-007.

**Flujo principal**:
```gherkin
  Scenario: Deteccion de tardanza fuera de tolerancia
    Given el empleado tiene un turno de 08:00 con 10 minutos de tolerancia
    When el empleado marca su entrada a las 08:16
    Then el sistema clasifica la marca como "Tardanza"
    And el sistema calcula el minutaje de atraso descontando la tolerancia
```

**Flujos alternativos**:
```gherkin
  Scenario: Marca dentro del margen de tolerancia
    Given el empleado tiene un turno de 08:00 con 10 minutos de tolerancia
    When el empleado marca su entrada a las 08:07
    Then el sistema no genera incidencia de tardanza

  Scenario: Ausencia por falta de marcas
    Given el empleado tiene un turno asignado para el dia actual
    And no existe ninguna marca de entrada registrada para ese turno al cierre del dia
    And no existe una justificacion o permiso aprobado para esa fecha
    When el proceso de cierre diario se ejecuta
    Then el sistema clasifica el dia como "Ausencia injustificada"
    And el sistema notifica al supervisor del empleado

  Scenario: Ausencia justificada por permiso aprobado
    Given el empleado tiene un permiso con goce de sueldo aprobado para la fecha del turno
    And no existen marcas registradas para ese turno
    When el proceso de cierre diario se ejecuta
    Then el sistema clasifica el dia como "Ausencia justificada"
    And el dia no genera descuento ni alerta de incumplimiento
```

---

### CU-008 — Solicitud de Permisos y Vacaciones

**Actor principal**: Empleado. **Historia origen**: US-008.

**Flujo principal**:
```gherkin
  Scenario: Solicitud de vacaciones con saldo disponible
    Given el empleado tiene un saldo de 12 dias de vacaciones disponibles
    When solicita 5 dias de vacaciones desde el portal ESS
    Then el sistema valida que el saldo disponible cubre los dias solicitados
    And el sistema crea la solicitud en estado "Pendiente de aprobacion"
    And el saldo disponible se reserva provisionalmente en 5 dias hasta la resolucion de la solicitud
```

**Flujos alternativos**:
```gherkin
  Scenario: Solicitud de vacaciones sin saldo suficiente
    Given el empleado tiene un saldo de 3 dias de vacaciones disponibles
    When solicita 5 dias de vacaciones
    Then el sistema rechaza la solicitud antes de enviarla a aprobacion
    And el sistema muestra el saldo disponible actual al empleado

  Scenario: Solicitud de incapacidad medica con adjunto obligatorio
    Given el empleado registra una solicitud de tipo "Incapacidad medica"
    When intenta enviar la solicitud sin adjuntar un certificado medico
    Then el sistema exige el adjunto como requisito obligatorio antes de permitir el envio
    And al adjuntar el certificado en PDF o imagen, la solicitud queda habilitada para su envio

  Scenario: Solicitud de permiso sin goce de sueldo
    Given el empleado solicita un permiso sin goce de sueldo para una fecha especifica
    When la solicitud es enviada
    Then el sistema la registra en estado "Pendiente de aprobacion"
    And marca la solicitud para que, de ser aprobada, no genere pago para ese dia en la pre-nomina
```

---

### CU-009 — Aprobación de Incidencias por Jefatura

**Actor principal**: Supervisor / Jefatura. **Historia origen**: US-009.

**Flujo principal**:
```gherkin
  Scenario: Aprobacion directa por supervisor
    Given el empleado envia una solicitud de permiso
    And el flujo de aprobacion configurado requiere unicamente aprobacion del supervisor directo
    When el supervisor aprueba la solicitud desde el portal MSS
    Then el sistema actualiza el estado de la solicitud a "Aprobada"
    And el sistema notifica al empleado sobre la aprobacion
    And la incidencia queda disponible para el motor de pre-nomina
```

**Flujos alternativos**:
```gherkin
  Scenario: Flujo de aprobacion en dos niveles (Jefatura y Recursos Humanos)
    Given la politica de la organizacion requiere aprobacion de Jefatura y luego de Recursos Humanos para incapacidades medicas
    When el supervisor aprueba la solicitud
    Then el sistema mantiene el estado en "Pendiente de aprobacion RRHH"
    And el sistema notifica al analista de RRHH correspondiente
    When el analista de RRHH aprueba la solicitud
    Then el sistema actualiza el estado final a "Aprobada"

  Scenario: Rechazo de solicitud con motivo obligatorio
    Given el supervisor revisa una solicitud de horas extra de un colaborador
    When el supervisor rechaza la solicitud
    Then el sistema exige el ingreso de un motivo de rechazo
    And el sistema notifica al empleado con el motivo indicado
    And la solicitud no se refleja en el calculo de pre-nomina

  Scenario: Escalamiento por vencimiento de plazo de aprobacion
    Given una solicitud de permiso lleva 3 dias habiles sin resolucion por parte del supervisor directo
    When se cumple el plazo maximo configurado para la aprobacion
    Then el sistema escala automaticamente la solicitud al siguiente nivel definido en el flujo
    And el sistema notifica a RRHH sobre el escalamiento
```

---

### CU-010 — Consulta de Tarjeta de Asistencia

**Actor principal**: Empleado. **Historia origen**: US-010.

> Este caso de uso es enteramente de consulta (no existe una condición de error o rechazo en el original); se marca como "flujo principal" la consulta base y como "flujos alternativos" las demás vistas de consulta que ofrece la misma pantalla.

**Flujo principal**:
```gherkin
  Scenario: Consulta de historial de marcas del mes actual
    Given el empleado tiene marcas registradas durante el mes en curso
    When accede a la seccion "Mi tarjeta de asistencia" en el portal ESS
    Then el sistema muestra el listado de marcas con fecha, hora, tipo y canal de captura
    And el sistema muestra el acumulado de horas trabajadas del periodo
```

**Flujos alternativos**:
```gherkin
  Scenario: Consulta de saldo de vacaciones actualizado
    Given el empleado tiene solicitudes de vacaciones aprobadas y pendientes
    When accede a la seccion "Mi saldo de vacaciones"
    Then el sistema muestra el saldo disponible, el saldo reservado por solicitudes pendientes y el saldo utilizado
    And la informacion refleja los movimientos aprobados hasta el momento de la consulta

  Scenario: Identificacion de marca omitida en el historial
    Given el empleado tiene un turno asignado para una fecha pasada
    And falta el registro de la marca de salida correspondiente
    When el empleado consulta su tarjeta de asistencia
    Then el sistema resalta visualmente el dia con la incidencia "Marca omitida"
    And ofrece al empleado la opcion de solicitar la correccion desde la misma vista
```

---

### CU-011 — Monitor de Presencia en Tiempo Real

**Actor principal**: Supervisor / Jefatura. **Historia origen**: US-011.

**Flujo principal**:
```gherkin
  Scenario: Visualizacion del estado actual del equipo
    Given el supervisor tiene un equipo de 15 empleados a cargo
    When accede al panel de presencia en tiempo real
    Then el sistema muestra el estado actual de cada empleado: "Presente", "En receso", "Ausente" o "De vacaciones"
    And el estado se actualiza automaticamente ante un nuevo marcaje sin requerir recargar la pagina
```

**Flujos alternativos**:
```gherkin
  Scenario: Alerta por ausencia imprevista
    Given un empleado tiene un turno programado que inicio hace mas de 30 minutos sin marca de entrada registrada
    And el empleado no tiene una incidencia justificada aprobada para el dia
    When el supervisor visualiza el panel de presencia
    Then el sistema resalta al empleado con un indicador de "Ausencia sin justificar"
    And permite al supervisor iniciar directamente una accion de reasignacion de cobertura

  Scenario: Filtrado del panel por centro de costo
    Given el supervisor tiene visibilidad sobre multiples centros de costo
    When filtra el panel de presencia por un centro de costo especifico
    Then el sistema muestra unicamente el estado de los empleados que pertenecen a ese centro de costo
```

---

### CU-011b — Reasignación Rápida de Turnos

**Actor principal**: Supervisor / Jefatura. **Historia origen**: US-011b.

**Flujo principal**:
```gherkin
  Scenario: Reasignacion de cobertura desde el panel de presencia
    Given el supervisor identifica a un empleado ausente sin justificar en un turno critico
    When selecciona "Reasignar cobertura" y elige a un empleado disponible del mismo centro de costo
    Then el sistema asigna el turno de cobertura al empleado seleccionado para la fecha correspondiente
    And notifica al empleado reasignado sobre el cambio de turno
```

**Flujo alternativo**:
```gherkin
  Scenario: Intento de reasignar a un empleado sin disponibilidad
    Given el supervisor intenta reasignar la cobertura a un empleado que ya tiene un turno asignado en el mismo horario
    When confirma la reasignacion
    Then el sistema advierte sobre el traslape de turnos
    And solicita confirmacion explicita antes de permitir el traslape
```

---

### CU-012 — Exportación e Integración con Sistema de Nómina (ERP/API)

**Actor principal**: Especialista de TI / Integraciones. **Historia origen**: US-012.

**Flujo principal**:
```gherkin
  Scenario: Exportacion automatizada exitosa via API REST
    Given el periodo de pre-nomina se encuentra cerrado y validado
    When se ejecuta el proceso de exportacion programado hacia el ERP
    Then el sistema envia el consolidado de horas e incidencias mediante la API REST configurada
    And el ERP confirma la recepcion con un codigo de exito
    And el sistema registra la exportacion como "Completada" con marca de tiempo
```

**Flujos alternativos**:
```gherkin
  Scenario: Exportacion mediante archivo plano estructurado
    Given el ERP destino no soporta integracion via API en tiempo real
    When se ejecuta el proceso de exportacion
    Then el sistema genera un archivo en el formato estructurado configurado (ej. CSV de ancho fijo)
    And deposita el archivo en la ubicacion de intercambio configurada (ej. SFTP)

  Scenario: Fallo de comunicacion con el ERP durante la exportacion
    Given se ejecuta el proceso de exportacion automatizada
    When el ERP destino no responde dentro del tiempo de espera configurado
    Then el sistema reintenta la exportacion segun la politica de reintentos configurada
    And si los reintentos se agotan, marca la exportacion como "Fallida" y notifica al especialista de TI

  Scenario: Sincronizacion de empleados desde Active Directory/HRIS
    Given existe un nuevo empleado dado de alta en el Active Directory corporativo
    When se ejecuta la sincronizacion programada con el directorio activo
    Then el sistema crea automaticamente el registro del empleado en el TCS con sus atributos base
    And si un empleado es dado de baja en el directorio, el sistema desactiva automaticamente su acceso al TCS
```
> Nota: la "Sincronización de empleados desde Active Directory/HRIS" es una capacidad relacionada pero distinta (alta/baja de empleados, no exportación de pre-nómina) que la historia US-012 agrupa junto al caso de exportación; se conserva aquí como flujo alternativo tal como aparece en el original.

---

### CU-013 — Bitácora de Auditoría (Audit Trail)

**Actor principal**: Oficial de Cumplimiento / Auditor de TI. **Historia origen**: US-013.

**Flujo principal**:
```gherkin
  Scenario: Registro automatico de una modificacion manual de marca
    Given un analista de RRHH corrige manualmente la hora de una marca de salida
    When la correccion se guarda en el sistema
    Then el sistema registra en la bitacora de auditoria: usuario, fecha/hora del cambio, valor anterior, valor nuevo y entidad afectada
    And el registro de auditoria no puede ser editado ni eliminado por ningun usuario, incluyendo administradores
```

**Flujos alternativos**:
```gherkin
  Scenario: Consulta de historial de cambios sobre una entidad especifica
    Given el auditor de TI necesita revisar el historial de una solicitud de permiso especifica
    When busca el identificador de la solicitud en el modulo de auditoria
    Then el sistema muestra la linea de tiempo completa de cambios asociados a esa solicitud
    And cada evento muestra el usuario responsable y el detalle del cambio

  Scenario: Intento de alteracion directa de un registro de auditoria
    Given un usuario con permisos administrativos intenta modificar un registro existente en la bitacora de auditoria
    When intenta ejecutar la operacion mediante cualquier interfaz del sistema
    Then el sistema rechaza la operacion
    And genera una alerta de seguridad por intento de alteracion de la bitacora

  Scenario: Exportacion de evidencia de auditoria para inspeccion laboral
    Given el oficial de cumplimiento necesita presentar evidencia ante una inspeccion laboral
    When exporta el historial de auditoria de un periodo y un conjunto de empleados especifico
    Then el sistema genera un archivo exportable (PDF o CSV) con la informacion solicitada
    And el archivo exportado incluye un hash o firma que permite verificar su integridad
```

---

## 3. Documentos relacionados

- [`docs/specs/functional/01-vision-document.md`](../docs/specs/functional/01-vision-document.md) §5 — Fuente de la tabla de actores.
- [`docs/specs/functional/02-user-stories.md`](../docs/specs/functional/02-user-stories.md) — Fuente de todos los escenarios Gherkin reorganizados en este documento.
- [`ENTREGABLES/user-stories.md`](./user-stories.md) — Las mismas historias en su formato original (con reglas de negocio y diagramas de secuencia completos).
- [`ENTREGABLES/traceability.md`](./traceability.md) — Matriz RF → HU → CU → Bounded Context/Endpoint.
