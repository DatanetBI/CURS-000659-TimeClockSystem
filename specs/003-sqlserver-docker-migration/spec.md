# Feature Specification: Migración a SQL Server y Contenerización con Docker

**Feature Branch**: `003-sqlserver-docker-migration`

**Created**: 2026-09-24

**Status**: Draft

**Input**: User description: "Migrar el motor de base de datos del Backend de SQLite a Microsoft SQL Server, y contenerizar con Docker los tres componentes del sistema: la base de datos, el Backend WebAPI y el Frontend MVC, de modo que todo el sistema pueda levantarse con un solo comando en un entorno local/de desarrollo. El Backend debe usar Microsoft SQL Server como motor de base de datos en lugar de SQLite, incluyendo las migraciones EF Core necesarias para el proveedor de SQL Server. Debe existir una imagen de contenedor para el Backend y otra para el Frontend, cada una construida a partir de su propio Dockerfile. La base de datos SQL Server debe correr en su propio contenedor (imagen oficial de Microsoft), con sus datos persistidos en un volumen para que no se pierdan al reiniciar el contenedor. Debe existir una forma de levantar los tres contenedores juntos con un solo comando, con el Backend esperando a que la base de datos esté lista antes de aplicar migraciones, y el Frontend apuntando al Backend por su nombre de servicio dentro de la red de contenedores. Ningún secreto (contraseña de SQL Server, clave de firma del token JWT, cadenas de conexión) debe quedar en texto plano dentro de las imágenes ni en los archivos versionados. Los datos de ejemplo/mock (DbSeeder) deben seguir sembrándose igual que hoy al iniciar el Backend contenedorizado. Fuera de alcance: orquestación en Kubernetes o cualquier plataforma de nube específica, pipelines de CI/CD para construir/publicar las imágenes, alta disponibilidad o clustering de SQL Server, y cualquier cambio funcional a las reglas de negocio o pantallas existentes más allá de lo necesario para el cambio de motor de base de datos y la contenerización."

## Clarifications

### Session 2026-09-24

- Q: ¿El entorno objetivo de estos contenedores es solo desarrollo/demo local, o también debe funcionar como base para un despliegue de producción? → A: Solo desarrollo/demo local. No se requiere HTTPS entre contenedores, ni una edición específica de SQL Server orientada a producción, ni límites de recursos definidos en esta funcionalidad.
- Q: ¿Se debe conservar la posibilidad de seguir ejecutando el Backend/Frontend sin Docker (por ejemplo con "dotnet run" para desarrollo diario), o Docker pasa a ser la única forma soportada de levantar el sistema? → A: Sí, se conserva la posibilidad de ejecutar Backend y Frontend directamente en el host con "dotnet run" para desarrollo diario, apuntando a una instancia de SQL Server accesible localmente.
- Q: ¿Los datos ya existentes en el archivo SQLite actual (timeclock.db) deben migrarse/importarse a SQL Server, o basta con que la base de datos en SQL Server inicie vacía y se resiembre con el DbSeeder de datos mock? → A: No se migran datos existentes; la base de datos SQL Server inicia vacía y se resiembra únicamente con los datos de ejemplo del DbSeeder.
- Q: ¿Debe existir un límite máximo de tiempo/reintentos para que el Backend espere a que la base de datos esté lista, después del cual falle con un error claro en vez de reintentar para siempre? → A: Sí, límite acotado con error claro. El Backend reintenta durante una ventana razonable y, si la base de datos sigue sin responder, falla explícitamente con un mensaje claro en los logs del contenedor, en vez de quedar reintentando indefinidamente sin señal de error.
- Q: ¿Qué debe hacer el Backend contenedorizado si al aplicar las migraciones de EF Core sobre SQL Server ocurre un error (migración incompatible o esquema inconsistente)? → A: Fallar y detener el contenedor con error claro. El Backend detiene su arranque, no queda escuchando peticiones, y registra en los logs un error claro que identifica la migración fallida, en vez de operar con un esquema parcial.
- Q: ¿Se debe mantener el objetivo de rendimiento previo (95% de acciones típicas del usuario en menos de 5 segundos, definido en la funcionalidad 002) pese a la latencia adicional de la red entre contenedores y del nuevo motor SQL Server? → A: Sí, se mantiene el mismo objetivo de 5 segundos; si el cambio de motor de base de datos o la contenerización lo degradan, se considera una regresión a corregir.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Levantar todo el sistema con un solo comando (Priority: P1)

Como desarrollador o responsable de QA que necesita preparar un ambiente completo de TimeClockSystem (base de datos, Backend y Frontend) en una máquina local, quiero ejecutar un único comando y obtener los tres componentes funcionando y comunicados entre sí, sin pasos manuales adicionales de instalación o configuración.

**Why this priority**: Es el objetivo de negocio explícito de esta funcionalidad: eliminar la fricción de preparar el ambiente a mano, que hoy requiere instalar y configurar cada componente por separado.

**Independent Test**: Se puede probar clonando el repositorio en una máquina limpia con Docker instalado, ejecutando el comando único documentado, y confirmando que los tres servicios quedan accesibles y operativos sin intervención manual adicional.

**Acceptance Scenarios**:

1. **Given** una máquina con Docker instalado y el repositorio clonado, **When** se ejecuta el comando único de arranque, **Then** los tres componentes (base de datos SQL Server, Backend, Frontend) quedan corriendo y accesibles sin pasos manuales adicionales.
2. **Given** los contenedores se están iniciando, **When** la base de datos SQL Server aún no está lista para aceptar conexiones, **Then** el Backend espera y reintenta hasta que la base de datos esté disponible antes de aplicar migraciones, en lugar de fallar de inmediato.
3. **Given** el sistema completo está arriba, **When** el Frontend necesita comunicarse con el Backend, **Then** lo hace usando el nombre del servicio del Backend dentro de la red de contenedores, sin depender de "localhost" ni de una dirección fija del equipo host.

---

### User Story 2 - Continuidad de comportamiento tras el cambio de motor de base de datos (Priority: P2)

Como Administrador o Empleado que ya usa TimeClockSystem, quiero que iniciar sesión, marcar asistencia, y administrar empleados/turnos/centros de trabajo funcione exactamente igual que hoy, aun cuando por debajo el sistema ahora guarde los datos en SQL Server en lugar de SQLite.

**Why this priority**: Cambiar el motor de base de datos es una operación de alto riesgo de regresión; sin esta continuidad garantizada, la migración no puede considerarse segura, pero depende de que el arranque contenedorizado (Historia 1) ya funcione.

**Independent Test**: Se puede probar ejecutando los flujos existentes de cada módulo (Empleados, CentrosTrabajo, Turnos, AsignacionTurno, DiasFestivos, Marcaje, ConsultaAsistencias) contra el sistema corriendo sobre SQL Server contenedorizado, y confirmando que el resultado observado por el usuario es idéntico al que se obtenía con SQLite.

**Acceptance Scenarios**:

1. **Given** el Backend contenedorizado se inicia por primera vez contra una base de datos SQL Server vacía, **When** arranca, **Then** aplica automáticamente las migraciones pendientes y siembra los datos de ejemplo (DbSeeder), quedando listo para usarse de inmediato.
2. **Given** un usuario realiza operaciones (inicio de sesión, marcaje, consulta de asistencias, administración de empleados/turnos/centros de trabajo) contra el sistema corriendo con SQL Server, **Then** obtiene los mismos resultados y comportamiento que existían cuando el sistema usaba SQLite.
3. **Given** el Backend se reinicia contra una base de datos SQL Server que ya tiene datos y migraciones aplicadas, **When** vuelve a arrancar, **Then** no falla ni duplica datos de ejemplo, y continúa operando con la información ya existente.

---

### User Story 3 - Persistencia de datos entre reinicios (Priority: P3)

Como operador del ambiente local, quiero que los datos registrados en el sistema (empleados, marcas, turnos, etc.) sobrevivan cuando detengo y vuelvo a iniciar los contenedores, para no perder información de prueba entre sesiones de trabajo.

**Why this priority**: Sin persistencia garantizada, cada reinicio obligaría a resembrar y volver a configurar datos de prueba, lo cual reduce la utilidad práctica de la contenerización, aunque no bloquea el arranque inicial descrito en la Historia 1.

**Independent Test**: Se puede probar registrando datos a través del sistema contenedorizado, deteniendo y reiniciando el contenedor de la base de datos (o el conjunto completo), y confirmando que los datos previamente registrados siguen presentes.

**Acceptance Scenarios**:

1. **Given** existen datos registrados (empleados, marcas, turnos, etc.) a través del sistema contenedorizado, **When** se detiene y reinicia el contenedor de la base de datos, **Then** los datos siguen presentes sin necesidad de volver a sembrarlos.
2. **Given** el volumen de datos de la base de datos existe y no ha sido eliminado explícitamente, **When** se recrean los contenedores del Backend o del Frontend, **Then** los datos previamente registrados permanecen intactos.

---

### Edge Cases

- ¿Qué sucede si el contenedor de la base de datos tarda más de lo esperado en estar listo? El Backend MUST seguir reintentando la conexión (no fallar de inmediato) mientras esté dentro de la ventana acotada de espera definida; si esa ventana se agota sin que la base de datos responda, el Backend MUST fallar de forma explícita con un mensaje de error claro en los logs, en vez de reintentar indefinidamente.
- ¿Qué ocurre si se vuelve a levantar el sistema una segunda vez sobre una base de datos que ya tiene migraciones y datos de ejemplo aplicados? El sistema MUST detectar ese estado y continuar sin duplicar datos de ejemplo ni fallar por migraciones ya aplicadas.
- ¿Qué pasa si falta alguna variable de entorno con un secreto requerido (contraseña de SQL Server, clave de firma del token) al levantar los contenedores? El sistema MUST fallar de forma clara y explicable en vez de arrancar con un valor por defecto inseguro embebido en la imagen.
- ¿Qué sucede si se elimina explícitamente el volumen de datos de la base de datos? Se acepta la pérdida de datos como resultado esperado de esa acción explícita (equivalente a reinstalar el ambiente desde cero).
- ¿Qué pasa si una migración de EF Core falla al aplicarse contra SQL Server (migración incompatible o esquema inconsistente)? El Backend MUST detener su arranque sin quedar escuchando peticiones, registrando en los logs un error claro que identifique la migración fallida, en vez de operar con un esquema parcial.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El Backend MUST usar Microsoft SQL Server como motor de base de datos en lugar de SQLite, incluyendo el conjunto de migraciones necesarias para ese proveedor de base de datos.
- **FR-002**: MUST existir una imagen de contenedor para el Backend (TimeClockSystem.Api), construida a partir de su propio Dockerfile.
- **FR-003**: MUST existir una imagen de contenedor para el Frontend (TimeClockSystem.Web), construida a partir de su propio Dockerfile.
- **FR-004**: La base de datos SQL Server MUST correr en su propio contenedor, basado en una imagen oficial de Microsoft.
- **FR-005**: Los datos de la base de datos SQL Server MUST persistir en un volumen que sobreviva a la detención y reinicio del contenedor de base de datos.
- **FR-006**: MUST existir una forma de levantar los tres componentes (base de datos, Backend, Frontend) juntos mediante un único comando.
- **FR-007**: El Backend contenedorizado MUST esperar a que la base de datos esté lista para aceptar conexiones antes de aplicar migraciones, reintentando en lugar de fallar de inmediato si la base de datos aún no responde, pero MUST limitar esa espera a una ventana acotada de tiempo/reintentos, tras la cual MUST fallar con un mensaje de error claro en vez de reintentar indefinidamente.
- **FR-008**: El Frontend contenedorizado MUST comunicarse con el Backend usando el nombre del servicio del Backend dentro de la red de contenedores, no mediante "localhost" ni una dirección fija del equipo host.
- **FR-009**: Ningún secreto (contraseña de SQL Server, clave de firma del token JWT, cadenas de conexión completas) MUST quedar en texto plano dentro de las imágenes de contenedor ni en archivos versionados del repositorio; MUST inyectarse mediante variables de entorno (u otro mecanismo equivalente) al momento de levantar los contenedores.
- **FR-010**: El Backend contenedorizado MUST aplicar automáticamente las migraciones pendientes de la base de datos al iniciar, sin requerir pasos manuales adicionales. Si la aplicación de una migración falla (por ejemplo, por una migración incompatible o un esquema inconsistente), el Backend MUST detener su arranque sin quedar escuchando peticiones, registrando en los logs un error claro que identifique la migración fallida.
- **FR-011**: El Backend contenedorizado MUST ejecutar el mismo proceso de siembra de datos de ejemplo (DbSeeder) que existe hoy, al iniciar contra una base de datos recién creada, para permitir probar el sistema de inmediato.
- **FR-012**: El entorno objetivo de esta contenerización MUST ser exclusivamente desarrollo/demo local; no se requiere HTTPS entre contenedores, edición de SQL Server orientada a producción, ni límites de recursos por contenedor como parte de esta funcionalidad.
- **FR-013**: El sistema MUST conservar la posibilidad de ejecutar el Backend y el Frontend directamente en el sistema operativo host (por ejemplo con "dotnet run") para desarrollo diario, apuntando a una instancia de SQL Server accesible localmente, además de la forma contenedorizada.
- **FR-014**: El sistema MUST iniciar la base de datos SQL Server vacía (sin migrar ni importar los datos existentes del archivo SQLite actual) y resembrarla únicamente con los datos de ejemplo del DbSeeder.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Una persona con Docker instalado puede levantar el sistema completo (base de datos, Backend y Frontend) ejecutando un único comando documentado, en menos de 10 minutos desde un ambiente limpio, sin editar código ni ejecutar pasos manuales adicionales.
- **SC-002**: El 100% de los flujos funcionales existentes (inicio de sesión, marcaje, consulta de asistencias, administración de empleados/turnos/centros de trabajo) producen el mismo resultado observable para el usuario después de migrar de SQLite a SQL Server.
- **SC-003**: Los datos registrados en el sistema sobreviven al 100% de los reinicios del contenedor de base de datos, sin pérdida de información mientras el volumen de datos no se elimine explícitamente.
- **SC-004**: El sistema queda utilizable con datos de ejemplo inmediatamente después de levantarse por primera vez, sin que ninguna persona deba cargar datos de forma manual.
- **SC-005**: 0 secretos (contraseñas, claves de firma de token, cadenas de conexión) aparecen en texto plano en las imágenes de contenedor generadas o en los archivos versionados del repositorio.
- **SC-006**: El 100% de los arranques desde cero completan la espera de la base de datos y la aplicación de migraciones sin intervención manual ni errores por conexión prematura.
- **SC-007**: El 95% de las acciones típicas del usuario (registrar una marca, guardar un cambio de empleado/turno) se completan en menos de 5 segundos, pese a la latencia adicional de la red entre contenedores y del nuevo motor de base de datos.

## Assumptions

- El alcance de esta funcionalidad reemplaza conscientemente la decisión previa (funcionalidad 002) de mantener SQLite; a partir de aquí SQL Server es el motor de base de datos soportado por el Backend.
- El entorno objetivo es exclusivamente desarrollo/demo local; no se diseña como base lista para producción (sin HTTPS interno, sin edición de SQL Server orientada a producción, sin límites de recursos definidos).
- Se conserva la posibilidad de ejecutar el Backend y el Frontend fuera de Docker (con "dotnet run") para desarrollo diario, apuntando a una instancia de SQL Server accesible localmente; Docker no es la única forma soportada de levantar el sistema.
- Los datos existentes en el archivo SQLite actual (timeclock.db) no se migran; la base de datos SQL Server inicia vacía y se resiembra únicamente con los datos de ejemplo del DbSeeder.
- El mecanismo para levantar los tres contenedores con un solo comando puede implementarse con una herramienta estándar de orquestación local de contenedores (por ejemplo, un archivo de composición de servicios); la elección exacta de herramienta se decide en la fase de planificación técnica.
- Los secretos requeridos (contraseña de SQL Server, clave de firma del token JWT) se proveen al momento de levantar el ambiente (por ejemplo, mediante variables de entorno o un archivo de variables no versionado) y no se documentan con valores reales en el repositorio.
- Quedan fuera de alcance: orquestación en Kubernetes o cualquier plataforma de nube específica, pipelines de CI/CD para construir o publicar las imágenes, alta disponibilidad o clustering de SQL Server, y cualquier cambio funcional a las reglas de negocio o pantallas existentes más allá de lo estrictamente necesario para el cambio de motor de base de datos y la contenerización.
