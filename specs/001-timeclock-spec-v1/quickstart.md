# Quickstart — TimeClockSystem v1.0 (6 módulos)

Guía para levantar la aplicación localmente y validar manualmente cada módulo en su orden de
construcción, sin necesidad de leer código (Principio IV de la constitución).

## Requisitos previos

- .NET 10 SDK instalado.
- Conexión a internet (v1.0 no tiene modo offline). No se requiere instalar ninguna base de datos aparte:
  la aplicación crea y siembra un archivo SQLite (`timeclock.db`) automáticamente en el primer arranque.

## Levantar la aplicación

```bash
cd src/TimeClockSystem.Web
dotnet run
```

Al iniciar por primera vez, el `DbSeeder` crea automáticamente los datos mock descritos en `plan.md`
§"Datos mock para la prueba de concepto": centros de trabajo, usuarios administradores, empleados con
PIN, turnos, festivos y algunas marcas de ejemplo. La consola imprime la URL local y las credenciales de
ejemplo (usuario/contraseña de Administrador, número de empleado/PIN de un Empleado de ejemplo).

## Validación manual, módulo por módulo

### Módulo 1 — Centros de trabajo

1. Iniciar sesión como Administrador y abrir "Centros de trabajo" → deben verse los 3 centros de
   ejemplo.
2. Crear un nuevo centro de trabajo (nombre, coordenadas, radio) → debe aparecer en la lista.

### Módulo 2 — Empleados y credenciales de marcaje

1. Abrir "Empleados" → deben verse los ~12 empleados de ejemplo, cada uno con su centro de trabajo.
2. Dar de alta un nuevo empleado, asociarlo a un centro de trabajo del módulo 1.
3. Asignarle un PIN de marcaje (4 a 6 dígitos) → debe quedar listo para marcar (módulo 5).

### Módulo 3 — Turnos y asignación de turnos

1. Abrir "Turnos" → deben verse los turnos de ejemplo (fijo, nocturno, flexible).
2. Crear un turno con hora de entrada 08:00 y 10 minutos de tolerancia.
3. Asignarlo al empleado creado en el módulo 2 (asignación individual) y a un grupo de empleados
   (asignación masiva) → el resumen debe indicar cuántos quedaron asignados.
4. Intentar guardar un turno con tolerancia negativa o mayor a su duración → debe rechazarse.

### Módulo 4 — Días festivos

1. Abrir "Días festivos" → deben verse las 2-3 fechas de ejemplo.
2. Agregar una fecha festiva próxima (útil para probar el módulo 6 después).

### Módulo 5 — Registro de asistencias

1. Iniciar sesión como el empleado creado en el módulo 2 (o uno de ejemplo) y registrar una marca de
   entrada estando dentro del geofence de su centro de trabajo → debe verse de inmediato la hora
   registrada.
2. Intentar una segunda entrada sin haber marcado salida → debe rechazarse con el mensaje de "entrada ya
   abierta".
3. Registrar una marca simulando estar fuera del geofence → debe rechazarse y no aparecer como válida.
4. En la pantalla de marcaje por credencial (modo kiosco), ingresar número de empleado + PIN correctos →
   debe registrar la marca; repetir con PIN incorrecto → debe mostrar "credenciales inválidas" sin
   indicar cuál dato falló.

### Módulo 6 — Portal de consulta de asistencias

1. Iniciar sesión como Administrador y abrir "Consulta de asistencias".
2. Filtrar por el empleado del módulo 5 → debe verse la marca registrada, con su indicador
   puntual/tardío según el turno del módulo 3.
3. Filtrar por la fecha festiva del módulo 4 → las marcas de ese día deben mostrarse señaladas como
   "día festivo".
4. Filtrar por centro de trabajo → deben verse solo las marcas de los empleados de ese centro.

## Fuera de esta guía (diferido a v1.1)

Incidencias/solicitudes, exportación/importación, marcaje sin conexión, panel de presencia en tiempo
real, reportes exportables y bitácora de auditoría — ver `plan.md` §"Diferido explícitamente a v1.1".

## Pruebas automatizadas (complementarias, no sustituyen la validación manual anterior)

```bash
dotnet test
```

Ejecuta las pruebas unitarias (unicidad de número de empleado/PIN, validación de tolerancia de turno,
validación de geofence) y de integración de los 6 módulos, descritas en `tasks.md`.
