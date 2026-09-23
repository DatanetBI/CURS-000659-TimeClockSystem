# Quickstart — TimeClockSystem v1.0

Guía para levantar la aplicación localmente y validar manualmente los escenarios clave de `spec.md`, sin
necesidad de leer código (Principio IV de la constitución).

## Requisitos previos

- .NET 10 SDK instalado.
- No se requiere instalar ni configurar ninguna base de datos aparte: la aplicación crea y siembra un
  archivo SQLite (`timeclock.db`) automáticamente en el primer arranque.

## Levantar la aplicación

```bash
cd src/TimeClockSystem.Web
dotnet run
```

Al iniciar por primera vez, el `DbSeeder` crea automáticamente datos de ejemplo: empleados con cada rol,
turnos, geofences, marcas históricas, saldos de vacaciones y solicitudes en distintos estados. La
consola imprime la URL local (por ejemplo `https://localhost:5001`) y las credenciales de ejemplo para
cada rol (número de empleado / contraseña de portal / PIN de marcaje).

## Validación manual por historia de usuario

Cada bloque se puede ejecutar completamente desde la interfaz, sin inspeccionar la base de datos.

### US1 — Marcaje con geofence, PIN y soporte offline

1. Iniciar sesión como el empleado de ejemplo "dentro de geofence" y registrar una marca de entrada →
   debe verse de inmediato la hora registrada (CA/AS1).
2. Intentar una segunda entrada sin haber marcado salida → debe rechazarse con el mensaje de "entrada ya
   abierta" (AS2).
3. Iniciar sesión como el empleado de ejemplo "fuera de geofence" y marcar → debe rechazarse y no
   aparecer como válida en su historial (AS3).
4. Simular pérdida de red (desactivar la conexión del navegador/dispositivo) y marcar → debe verse como
   "pendiente de sincronización"; reactivar la red → debe cambiar a "sincronizada" (AS4).
5. En el terminal de marcaje por credencial, ingresar número de empleado + PIN correctos → debe
   registrar la marca (AS6); repetir con PIN incorrecto → debe mostrar "credenciales inválidas" sin
   indicar cuál dato falló (AS7).

### US2 — Turnos y cálculo automático

1. Crear un turno 08:00 con 10 minutos de tolerancia, asignarlo a un empleado de ejemplo.
2. Registrar una marca de entrada a las 08:16 → el consolidado del periodo debe reportar 6 minutos de
   atraso (AS1).
3. Ver el consolidado de un empleado con turno nocturno de ejemplo → las horas deben aparecer repartidas
   entre las dos fechas calendario (AS3).
4. Intentar guardar un turno con tolerancia negativa o mayor a su duración → debe rechazarse (AS5).

### US3 — Solicitudes y aprobación

1. Iniciar sesión como el empleado de ejemplo con saldo bajo de vacaciones y solicitar más días de los
   disponibles → debe rechazarse antes de llegar a aprobación (AS1).
2. Iniciar sesión como supervisor y aprobar/rechazar una solicitud pendiente de ejemplo → el estado debe
   actualizarse y debe verse el motivo si se rechaza (AS4).
3. Revisar una solicitud de ejemplo ya escalada por vencimiento de plazo en la bandeja de RRHH (AS3).

### US4 — Panel de supervisor

1. Iniciar sesión como supervisor y abrir el panel de presencia.
2. Desde otra sesión (o pestaña), marcar entrada como uno de los empleados de su equipo → el estado debe
   cambiar a "presente" en el panel sin recargar la página, en menos de 10 segundos (AS1, SC-003).
3. Reasignar la cobertura de un turno vacante a un empleado que ya tiene otro turno en el mismo horario →
   debe mostrarse la advertencia de traslape antes de confirmar (AS4).

### US5 — Autoservicio del empleado

1. Iniciar sesión como empleado y revisar la tarjeta de asistencia → un día con marca omitida debe verse
   resaltado y permitir iniciar una corrección (AS2).

### US6 — Exportación e importación

1. Desde la pantalla de Integraciones, disparar manualmente la exportación del periodo de ejemplo →
   descargar el CSV generado y verificar las columnas de `contracts/export-import.md`.
2. Subir un archivo CSV de altas/bajas de ejemplo (incluido en `wwwroot/sample-data/`) → verificar que
   las filas válidas se procesan y las inválidas se reportan con su motivo.

### US7 — Reportes y auditoría

1. Generar el reporte de ausentismo/horas extra del periodo de ejemplo y exportarlo a PDF/Excel/CSV.
2. Editar manualmente una marca desde la vista de administración → verificar que la bitácora de
   auditoría registra el cambio (quién, cuándo, valor anterior/nuevo).
3. Intentar editar o borrar esa misma entrada de la bitácora (como Administrador) → debe rechazarse
   (FR-045).

## Pruebas automatizadas (complementarias, no sustituyen la validación manual anterior)

```bash
dotnet test
```

Ejecuta las pruebas unitarias (reglas de cálculo: tolerancia, horas extra, saldo de vacaciones) y de
integración (flujos de extremo a extremo vía `WebApplicationFactory`) descritas en `tasks.md`.
