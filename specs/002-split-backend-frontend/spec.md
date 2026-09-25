# Feature Specification: Separación de TimeClockSystem en Backend WebAPI y Frontend MVC

**Feature Branch**: `002-split-backend-frontend`

**Created**: 2026-09-24

**Status**: Draft

**Input**: User description: "Separar TimeClockSystem en dos proyectos independientes: un Backend WebAPI (TimeClockSystem.Api) que exponga toda la lógica de negocio y acceso a datos actualmente embebida en la app MVC, y un Frontend (TimeClockSystem.Web) que conserve las mismas pantallas y flujos actuales pero que deje de acceder directamente a EF Core/SQLite y en su lugar consuma la información exclusivamente vía HTTP contra la nueva API. Debe mantenerse el mismo comportamiento funcional y reglas de negocio ya existentes (cálculo de puntualidad, validación de geocerca, validación de consentimiento, verificación biométrica) sin regresiones. La API debe exponer documentación (Swagger/OpenAPI) para poder probarse de forma independiente del frontend. Ambos proyectos deben poder ejecutarse y desplegarse de forma independiente, comunicándose solo por HTTP. Fuera de alcance: cambiar el motor de base de datos, agregar nuevos módulos funcionales, soporte multi-tenant, apps móviles nativas, y cambios en las reglas de negocio existentes más allá de lo necesario para exponerlas vía API."

## Clarifications

### Session 2026-09-24

- Q: ¿Cómo debe el Frontend MVC conservar la sesión del navegador (cookie de inicio de sesión) mientras usa el nuevo token JWT para llamar al Backend? → A: El Frontend conserva su propia cookie de sesión (como hoy); internamente guarda el JWT del lado servidor (cifrado dentro de la cookie de autenticación o en estado de servidor) y lo adjunta a cada llamada al Backend. El navegador nunca ve el JWT directamente.
- Q: ¿Qué debe hacer el Frontend cuando una llamada al Backend para registrar una marca de asistencia falla por timeout o pérdida de red? → A: Fallar sin reintento automático — un único intento; si falla, se muestra el error de inmediato y el Empleado debe volver a marcar manualmente, para evitar marcas duplicadas por reintentos sobre una operación no garantizada como idempotente.
- Q: ¿El Backend debe registrar un log de auditoría para eventos sensibles (inicios de sesión, intentos de marcaje rechazados, accesos denegados por rol)? → A: Sí, registrar auditoría de eventos sensibles — el Backend guarda quién, cuándo y qué se intentó en eventos de login, marcaje rechazado y accesos denegados, dando trazabilidad para disputas laborales.
- Q: ¿Qué tan rápido debe percibirse una acción típica del usuario (registrar una marca, guardar un cambio de empleado/turno) ahora que hay una llamada de red adicional entre el Frontend y el Backend? → A: Menos de 5 segundos en el 95% de los casos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Continuidad de uso sin cambios para los usuarios finales (Priority: P1)

Como Administrador o Empleado que ya usa TimeClockSystem, quiero seguir marcando mi asistencia, consultando mis registros y administrando empleados, centros de trabajo y turnos exactamente igual que hoy, sin percibir ningún cambio en pantallas ni flujos, aun cuando por debajo el sistema ahora dependa de un backend independiente.

**Why this priority**: Es el requisito mínimo para que la separación sea segura: si los usuarios finales notan una regresión, la migración no puede considerarse exitosa sin importar qué tan bien se haya desacoplado el backend.

**Independent Test**: Se puede probar ejecutando los escenarios existentes de cada módulo (Empleados, CentrosTrabajo, Turnos, AsignacionTurno, DiasFestivos, Marcaje, ConsultaAsistencias) contra la nueva arquitectura separada y confirmando que el resultado observado por el usuario es idéntico al del sistema actual.

**Acceptance Scenarios**:

1. **Given** un Administrador ha iniciado sesión, **When** crea, edita o elimina un Centro de Trabajo, un Turno o un Empleado, **Then** el cambio se guarda y se refleja en pantalla exactamente igual que antes de la separación.
2. **Given** un Empleado ha iniciado sesión, **When** registra entrada, salida, inicio de receso o fin de receso, **Then** la marca se registra aplicando las mismas validaciones de puntualidad, geocerca y consentimiento que existían antes.
3. **Given** cualquier usuario autenticado, **When** el Backend no está disponible momentáneamente, **Then** el Frontend muestra un mensaje de error claro en vez de fallar de forma silenciosa o inesperada.

---

### User Story 2 - Despliegue y operación independiente (Priority: P2)

Como responsable de operación del sistema, quiero poder construir, actualizar y desplegar el Backend y el Frontend por separado, para poder liberar cambios en uno sin verse obligado a redesplegar el otro.

**Why this priority**: Es el motivo de negocio principal detrás de la separación (ciclos de despliegue independientes), pero depende de que la Historia 1 ya funcione correctamente.

**Independent Test**: Se puede probar actualizando y redesplegando únicamente el Backend (dejando el Frontend sin tocar) y viceversa, y confirmando que el sistema completo sigue funcionando de extremo a extremo.

**Acceptance Scenarios**:

1. **Given** ambos proyectos están en ejecución, **When** se despliega una corrección en el Backend, **Then** el Frontend continúa funcionando sin necesitar una nueva compilación ni despliegue.
2. **Given** ambos proyectos están en ejecución, **When** se despliega un cambio visual en el Frontend, **Then** no se requiere ningún cambio en el Backend para que el sistema siga operando.

---

### User Story 3 - Verificación independiente del Backend (Priority: P3)

Como desarrollador o responsable de QA, quiero poder explorar y probar todas las capacidades del Backend (empleados, turnos, marcaje, etc.) directamente, sin pasar por las pantallas del Frontend, para validar reglas de negocio y detectar errores más rápido.

**Why this priority**: Aporta velocidad de desarrollo y calidad, pero no es indispensable para que los usuarios finales operen el sistema el primer día.

**Independent Test**: Se puede probar usando únicamente la documentación/herramientas del Backend (sin el Frontend) para ejecutar un flujo completo: crear empleado, asignar turno, registrar marca y consultar asistencia.

**Acceptance Scenarios**:

1. **Given** credenciales válidas, **When** un cliente llama directamente a las operaciones del Backend, **Then** puede completar el flujo crear empleado → asignar turno → marcar asistencia → consultar asistencia sin usar el Frontend.
2. **Given** el Backend desplegado, **When** un desarrollador busca conocer sus capacidades disponibles, **Then** existe documentación legible por humanos que describe cada operación sin necesidad de leer el código fuente.

---

### Edge Cases

- ¿Qué sucede cuando el Frontend recibe un token de autenticación vencido o inválido desde el Backend? Debe forzar un nuevo inicio de sesión limpio sin exponer detalles técnicos del error.
- ¿Cómo se comporta el sistema si el Frontend no puede alcanzar al Backend justo durante el intento de marcaje de un Empleado? El Frontend MUST hacer un único intento (sin reintento automático) y, si falla por timeout o pérdida de red, MUST informar de inmediato al Empleado que la marca no se registró, para que la repita manualmente y evitar registros duplicados.
- ¿Qué ocurre con los datos históricos existentes (empleados, marcas, turnos ya almacenados) durante la migración? Deben seguir siendo accesibles en su totalidad tras la separación.
- ¿Qué sucede si los permisos de un usuario cambian durante una sesión activa (por ejemplo, un Administrador es reasignado a Empleado)? El Backend debe validar el rol vigente en cada operación en lugar de confiar indefinidamente en un dato de rol obsoleto.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST exponer todas las capacidades hoy disponibles en la aplicación (gestión de empleados, centros de trabajo, turnos, asignaciones de turno, días festivos, marcaje/asistencias y credenciales de marcaje) como un conjunto documentado de operaciones accesibles por red, independientes de cualquier capa de presentación específica.
- **FR-002**: El sistema MUST autenticar las solicitudes entre el Frontend y el Backend mediante un token con expiración fija emitido tras el inicio de sesión; al vencer, el usuario MUST volver a iniciar sesión (sin renovación silenciosa), preservando los roles Administrador y Empleado existentes y sus permisos actuales.
- **FR-002a**: El Frontend MUST conservar su propia cookie de sesión de navegador (como hoy) y MUST mantener el token del Backend únicamente del lado servidor (nunca expuesto al navegador/JavaScript), adjuntándolo internamente a cada llamada al Backend en nombre del usuario autenticado.
- **FR-003**: El Backend MUST aplicar de forma independiente las restricciones de permisos por rol en cada operación que expone (por ejemplo, solo solicitudes con rol Administrador pueden gestionar centros de trabajo, turnos o empleados), sin depender de que el Frontend también las valide.
- **FR-004**: El Frontend MUST conservar todas las pantallas, rutas de navegación y flujos de trabajo actuales para Administrador y Empleado, obteniendo todos los datos y decisiones de negocio exclusivamente del Backend (sin acceso directo a la base de datos desde el Frontend).
- **FR-005**: El sistema MUST preservar el comportamiento exacto de las reglas de negocio existentes (cálculo de puntualidad, validación de geocerca, validación de consentimiento, verificación biométrica): las mismas entradas deben producir los mismos resultados que antes de la separación.
- **FR-006**: El Backend MUST ofrecer documentación autoservicio y legible por humanos de sus operaciones disponibles, de forma que pueda entenderse y probarse sin necesidad del Frontend.
- **FR-007**: El Backend y el Frontend MUST poder compilarse, ejecutarse y desplegarse de forma independiente, comunicándose solo por red, entregándose ambos como proyectos separados dentro de la misma solución/repositorio actual (sin repositorios ni pipelines separados).
- **FR-008**: El Frontend MUST comunicarse con el Backend mediante llamadas HTTP directas a sus endpoints, sin una capa intermedia tipo gateway/BFF.
- **FR-009**: El sistema MUST mostrar al usuario final un mensaje de error claro y no técnico cada vez que el Backend no esté disponible o devuelva un error, sin exponer detalles internos.
- **FR-010**: El sistema MUST conservar el acceso a todos los datos históricos existentes (empleados, marcas, turnos, asignaciones, días festivos, credenciales) después de la separación, sin pérdida de información.
- **FR-011**: El Backend MUST registrar un log de auditoría de eventos sensibles (inicios de sesión, intentos de marcaje rechazados, accesos denegados por rol), incluyendo quién, cuándo y qué se intentó, para dar trazabilidad ante disputas laborales.
- **FR-012**: El Frontend MUST intentar cada llamada de registro de marca al Backend una única vez (sin reintento automático) y, si falla por timeout o pérdida de red, MUST informar de inmediato al Empleado que la marca no se registró.

### Key Entities *(include if feature involves data)*

- **Empleado**: persona que registra marcas de asistencia; puede estar asignada a centros de trabajo y turnos.
- **CentroTrabajo**: ubicación física o lógica a la que se asignan empleados, con una geocerca asociada.
- **Turno**: horario de trabajo definido que puede asignarse a empleados.
- **AsignacionTurno**: vínculo entre un Empleado y un Turno para un periodo determinado.
- **DiaFestivo**: fecha del calendario que afecta los cálculos de puntualidad/asistencia.
- **Marca**: evento de asistencia registrado (entrada, salida, inicio/fin de receso) asociado a un Empleado.
- **CredencialDeMarcaje**: credencial (PIN u otro medio) usada para autenticar una marca.
- **Usuario/Rol**: identidad (Administrador o Empleado) usada para acceder al sistema, que ahora debe viajar entre Frontend y Backend como un token.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de los flujos de trabajo actuales (en todos los módulos existentes) se completan exitosamente tras la separación, con el mismo resultado observado por el usuario que antes.
- **SC-002**: El Backend puede actualizarse y redesplegarse sin requerir ningún redespliegue del Frontend (y viceversa) en al menos el 95% de los cambios rutinarios.
- **SC-003**: Una persona de desarrollo o QA puede completar un flujo de negocio de extremo a extremo (crear empleado → asignar turno → registrar marca → consultar asistencia) usando únicamente la documentación del Backend, sin abrir el Frontend, en menos de 15 minutos.
- **SC-004**: Cuando el Backend no está disponible, el 100% de las acciones afectadas en el Frontend muestran un mensaje de error comprensible en vez de una falla no controlada.
- **SC-005**: No se observa ninguna pérdida de registros históricos (empleados, marcas, turnos, asignaciones, días festivos) después de migrar a la arquitectura separada.
- **SC-006**: El 100% de los inicios de sesión, intentos de marcaje rechazados y accesos denegados por rol quedan registrados en un log de auditoría consultable, con suficiente detalle (quién, cuándo, qué) para resolver una disputa laboral.
- **SC-007**: El 95% de las acciones típicas del usuario (registrar una marca, guardar un cambio de empleado/turno) se completan en menos de 5 segundos, pese al salto de red adicional introducido por la separación.

## Assumptions

- El Backend y el Frontend se entregan como dos proyectos separados dentro de la misma solución/repositorio actual (no repositorios ni pipelines independientes).
- La autenticación Frontend-Backend usa un token de expiración fija; al vencer, el usuario debe reautenticarse (no hay renovación silenciosa vía refresh token).
- El Frontend llama directamente a los endpoints del Backend, sin una capa de gateway/BFF intermedia.
- SQLite se mantiene como motor de base de datos del Backend (cambiarlo queda explícitamente fuera de alcance).
- No se requiere versionado de la API desde el primer lanzamiento; puede introducirse más adelante sin afectar la finalización de esta funcionalidad.
- El conjunto de roles (Administrador, Empleado) y sus permisos no cambian; lo que cambia es dónde se aplica la validación de autorización (pasa a residir por completo en el Backend).
- Las reglas de negocio existentes (puntualidad, geocerca, consentimiento, verificación biométrica) se consideran funcionalmente congeladas: esta migración las reubica pero no las rediseña.
- Quedan fuera de alcance: cambiar el motor de base de datos, agregar nuevos módulos funcionales, soporte multi-tenant, apps móviles nativas, y cualquier cambio en las reglas de negocio existentes más allá de lo necesario para exponerlas vía API.
