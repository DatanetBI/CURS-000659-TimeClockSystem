# Research & Decisiones — TimeClockSystem v1.0

Cada decisión resuelve una pregunta abierta ("NEEDS CLARIFICATION") del Technical Context de `plan.md`.
Todas están redactadas primero en lenguaje de negocio y luego con el detalle técnico necesario para
diseñar `data-model.md` y `contracts/`.

---

## 1. Arquitectura de despliegue: una aplicación vs. microservicios

**Decisión**: Un único proyecto ASP.NET Core desplegable como un solo contenedor/proceso.

**En palabras de negocio**: Se puede publicar en línea en días. Solo hay una cosa que desplegar,
respaldar y monitorear, por lo que el costo de operación (tiempo y dinero) es el de una aplicación
pequeña, no el de una plataforma de 9 servicios.

**Rationale técnico**: Los documentos `docs/architecture/c4-containers.md` (v3.0) y `ADR-001` proponen 9
microservicios + API Gateway + Redis + SignalR + Kubernetes, justificados por necesidades de escala y
autonomía de equipos que v1 no tiene (un solo equipo, hasta 500 empleados — ver `spec.md` SC-010). La
constitución del proyecto (Principio I) y la instrucción explícita de negocio para este plan
("prioriza la simplicidad... no agregues infraestructura que la spec no requiera") toman precedencia
sobre ese documento de arquitectura para esta versión.

**Alternativas consideradas**:
- *Adoptar los 9 microservicios tal cual*: descartada — viola el Principio I de la constitución y la
  instrucción explícita de negocio; ningún requisito funcional de v1 exige escalado independiente por
  módulo.
- *Monolito modular con proyectos separados por bounded context, ensamblados en un solo despliegue*:
  considerada pero descartada por añadir complejidad de compilación (múltiples `.csproj`, contratos
  entre ensamblados) sin beneficio para una v1 de un solo equipo; se logra el mismo límite de dominio
  organizando por carpetas dentro de un único proyecto.

---

## 2. Almacenamiento de datos

**Decisión**: SQLite (archivo único) vía Entity Framework Core.

**En palabras de negocio**: No hay que contratar ni administrar un servidor de base de datos aparte; la
base de datos viaja con la aplicación. Esto simplifica publicar en línea "enseguida" y reduce el costo
de infraestructura a casi cero para una prueba de concepto de hasta 500 empleados.

**Rationale técnico**: El volumen esperado (SC-010: ≤500 empleados, unas pocas marcas/solicitudes por
empleado al día) está muy por debajo de lo que SQLite maneja cómodamente en un solo proceso. EF Core
abstrae el proveedor, por lo que migrar a PostgreSQL más adelante (si el volumen real lo justifica) es
un cambio de cadena de conexión y proveedor, no un rediseño del modelo de datos.

**Alternativas consideradas**:
- *PostgreSQL desde v1* (como en `c4-containers.md`): descartada para v1 porque exige aprovisionar,
  parchear y respaldar un servidor de base de datos separado — infraestructura que la spec no requiere
  para validar el producto con hasta 500 empleados.

---

## 3. Autenticación y control de acceso

**Decisión**: ASP.NET Core Identity (cookies de sesión) para el acceso al portal (empleado, supervisor,
RRHH, nómina, administrador, auditor, TI, ejecutivo — FR-043), con roles nativos del framework. El
marcaje por número de empleado + PIN (FR-002/003/004) es un flujo separado y más ligero, pensado para un
terminal/kiosco compartido, no una sesión de portal completa.

**En palabras de negocio**: Se reutiliza el mecanismo de acceso estándar de la plataforma en vez de
construir uno propio desde cero, lo cual reduce tiempo de desarrollo y riesgo de errores de seguridad.
El marcaje rápido por PIN queda separado del inicio de sesión del portal porque son dos necesidades
distintas: un empleado en un kiosco compartido solo necesita marcar en segundos; el mismo empleado
consultando su historial en el portal (US5) sí necesita una sesión propia.

**Rationale técnico**: `Microsoft.AspNetCore.Identity` provee gestión de roles, hashing de contraseñas
(`PasswordHasher<T>`) y cookies de sesión sin dependencias externas. El PIN de marcaje se guarda como un
hash independiente en el registro del empleado (mismo mecanismo de hashing, campo separado de la
contraseña de portal) — nunca en texto plano, cumpliendo el Principio V de la constitución.

**Alternativas consideradas**:
- *OAuth2/OpenID Connect con un Identity Provider externo* (como insinúa `c4-containers.md` con JWT):
  descartado para v1 por ser infraestructura adicional (un servidor de identidad) no requerida por
  ningún requisito funcional de la spec; la app es un solo proceso, no una red de servicios que
  necesiten validar tokens entre sí.

---

## 4. Panel de presencia en "tiempo casi real" (SC-003, FR-031)

**Decisión**: Sondeo (*polling*) del navegador cada 5-8 segundos a un endpoint de solo lectura, en vez de
WebSockets/SignalR con backplane de Redis.

**En palabras de negocio**: El requisito de negocio es que un supervisor vea el cambio de estado de su
equipo en menos de 10 segundos (SC-003); no se pide que sea instantáneo al milisegundo. Una actualización
automática cada pocos segundos ya cumple ese objetivo sin necesitar una pieza de infraestructura adicional
(Redis) que solo tendría sentido si hubiera varias instancias de la aplicación corriendo a la vez.

**Rationale técnico**: SignalR con backplane de Redis (como en `c4-containers.md`) resuelve un problema
que no existe en v1 (sincronizar el estado de tiempo real entre *varias* instancias del servicio). Con un
único proceso, el polling simple desde el cliente cumple el SLA de <10s sin necesidad de conexiones
persistentes ni infraestructura de mensajería.

**Alternativas consideradas**:
- *SignalR sin backplane* (una sola instancia): viable técnicamente, pero se prefiere el polling por ser
  más simple de implementar y depurar para un equipo pequeño, y porque no hay ningún requisito de
  latencia sub-segundo que lo justifique.

---

## 5. Marcaje sin conexión (FR-008, CL10, CL14)

**Decisión**: JavaScript del lado del cliente guarda el intento de marca en `localStorage` cuando la
petición falla (sin red, o el backend no responde), y reintenta automáticamente en segundo plano hasta
confirmarla. No se construye una app instalable (PWA con *service worker*) en v1.

**En palabras de negocio**: Cubre el caso real descrito en la spec — un empleado de campo marca su
entrada y en ese momento no hay señal — sin construir una aplicación móvil instalable aparte, que sería
un esfuerzo de desarrollo mucho mayor y no fue pedido para v1 (v1 es una web responsiva).

**Rationale técnico**: La página debe haberse cargado mientras había conectividad (típico si el empleado
la deja abierta); a partir de ahí, el JS intercepta fallos de red al enviar el formulario, guarda el
payload localmente y reintenta cuando detecta que la conexión se restableció (evento `online` del
navegador) o cada cierto intervalo. Esto satisface FR-008/CL10/CL14 sin requerir *service worker* ni
cola de mensajes en el servidor.

**Alternativas consideradas**:
- *PWA instalable con Service Worker y Background Sync API*: descartada para v1 por ser un esfuerzo de
  desarrollo mayor no exigido explícitamente por ningún criterio de aceptación de la spec (los
  escenarios piden que la marca quede "pendiente" y luego se sincronice, no que la página cargue sin
  red en absoluto).

---

## 6. Reconocimiento facial / liveness (FR-007)

**Decisión**: Se define una interfaz de extensión (`IBiometricVerificationProvider`) con una
implementación por defecto que simplemente no aplica esa validación (deshabilitada), sin integrar ningún
proveedor real.

**En palabras de negocio**: Deja la puerta abierta para conectar un proveedor de reconocimiento facial en
una fase futura, sin construir ni pagar por esa pieza ahora — exactamente lo que la spec pide (FR-007,
"Fuera de Alcance").

**Alternativas consideradas**: Ninguna — la spec ya resolvió esta decisión; este punto solo documenta
cómo se traduce a código sin convertirse en trabajo adicional no solicitado.

---

## 7. Exportación a nómina e importación de altas/bajas (FR-036 a FR-039)

**Decisión**: Un `BackgroundService` (incluido en ASP.NET Core, sin dependencias nuevas) ejecuta la
exportación de forma programada y genera un archivo CSV descargable con el consolidado del periodo; una
pantalla de administración permite subir un archivo CSV de altas/bajas para importarlas. El estado de
cada lote (pendiente/exitoso/fallido, reintentos) se guarda en una tabla de la misma base de datos.

**En palabras de negocio**: Cumple lo que pide la spec — un contrato de archivo, no una conexión en vivo
a un ERP o directorio activo concreto — sin instalar un motor de trabajos en segundo plano de terceros
(como Hangfire), que sería infraestructura adicional no necesaria para el volumen de v1.

**Alternativas consideradas**:
- *Hangfire/Quartz.NET*: descartadas para v1 — añaden una dependencia y, en el caso de Hangfire, tablas
  y un panel propios, para un volumen de tareas (una exportación programada, reintentos simples) que el
  `BackgroundService` nativo resuelve sin dependencias externas.

---

## 8. Datos de ejemplo (mock) para la prueba de concepto

**Decisión**: Un `DbSeeder` se ejecuta al iniciar la aplicación (si la base de datos está vacía) y crea:
empleados de ejemplo con cada rol (empleado, supervisor, RRHH, nómina, administrador, auditor, TI,
ejecutivo), turnos de ejemplo (fijo, nocturno, rotativo), geofences, algunas marcas históricas, saldos de
vacaciones calculados, y solicitudes en distintos estados (pendiente, aprobada, rechazada, escalada).

**En palabras de negocio**: Cualquier persona del negocio puede entrar a la aplicación recién publicada y
probar de inmediato los criterios de aceptación de la spec (por ejemplo, aprobar una solicitud pendiente
o ver el panel de presencia con datos), sin necesidad de dar de alta manualmente decenas de registros
antes de poder evaluar el sistema.

**Rationale técnico**: `DbSeeder` corre en el arranque del proceso, revisa si ya existen datos y, si no,
inserta el conjunto de ejemplo dentro de una transacción; es idempotente (no duplica datos en reinicios
posteriores).

---

## 9. Responsividad móvil

**Decisión**: Bootstrap 5 cargado por CDN, con un layout base "mobile-first" (Razor `_Layout.cshtml`);
no se construye una app nativa ni un proyecto frontend separado (React/Angular/SPA).

**En palabras de negocio**: Un empleado puede marcar su asistencia o un supervisor puede aprobar una
solicitud desde el navegador de su teléfono sin instalar nada, y sin que eso implique construir y
mantener dos frontends distintos.

**Rationale técnico**: Al no requerir un *build step* de frontend (Node/npm), se simplifica el pipeline
de despliegue: `dotnet publish` produce el artefacto completo (servidor + vistas + CSS/JS estáticos).

---

## Resumen de resolución de "NEEDS CLARIFICATION"

Ninguna casilla del Technical Context de `plan.md` quedó como `NEEDS CLARIFICATION`; las 9 decisiones
anteriores cubren Language/Version, Primary Dependencies, Storage, Testing, Target Platform, Project
Type, Performance Goals, Constraints y Scale/Scope.
