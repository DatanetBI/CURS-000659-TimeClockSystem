# Manual de Usuario — TimeClockSystem v1.0

23 de septiembre de 2026

## Introducción

TimeClockSystem v1.0 es la primera versión del sistema de gestión de asistencias de la empresa, construida a partir de la especificación técnica en `specs/001-timeclock-spec-v1`. Este manual explica, paso a paso, cómo usar la aplicación según tu rol.

Esta versión cubre 6 módulos: centros de trabajo, empleados y credenciales de marcaje, turnos y asignación, días festivos, registro de asistencias, y consulta de asistencias. Funciona **100% en línea** (requiere conexión a internet) y es **responsiva** (funciona igual en computadora y en celular). Las funciones de vacaciones/permisos, exportación a nómina, modo sin conexión y reportes avanzados llegan en una versión posterior — ver la última sección de este manual.

## Roles del sistema

v1.0 tiene 2 roles:

| Rol | Qué puede hacer |
| --- | --- |
| Empleado | Marcar su propia entrada, salida y receso desde el portal o desde un kiosco con PIN; ver sus marcas del día |
| Administrador | Gestionar centros de trabajo, empleados, turnos y días festivos, y consultar las asistencias de todos los empleados |

Los roles de Supervisor, RRHH, Nómina, Auditor, TI y Ejecutivo llegan en una versión posterior, junto con las funciones que los necesitan (aprobaciones, exportación, auditoría).

## Acceso al sistema

**Portal (Empleado o Administrador)**: abre la aplicación → "Iniciar sesión" → ingresa tu usuario y contraseña.

**Kiosco por PIN (solo Empleado, sin iniciar sesión)**: desde la pantalla de inicio, abre "Marcar por PIN (kiosco)" → ingresa tu número de empleado y tu PIN.

### Usuarios y contraseñas mock

Datos ficticios que se cargan automáticamente la primera vez que se levanta la aplicación, únicamente para poder probarla de inmediato. **No son datos reales y no deben usarse en un entorno de producción.**

**Administradores** (acceso al portal):

| Usuario | Contraseña |
| --- | --- |
| admin1 | Admin123! |
| admin2 | Admin123! |

**Empleados**: todos comparten la misma contraseña de portal (`Empleado123!`) y el mismo PIN de marcaje (`1234`):

| Número de empleado | Nombre | Centro de trabajo |
| --- | --- | --- |
| E001 | Juan Pérez | Oficina Central CDMX |
| E002 | María García | Oficina Central CDMX |
| E003 | Carlos López | Oficina Central CDMX |
| E004 | Ana Martínez | Oficina Central CDMX |
| E005 | Luis Hernández | Planta Querétaro |
| E006 | Sofía Ramírez | Planta Querétaro |
| E007 | Diego Torres | Planta Querétaro |
| E008 | Valentina Flores | Planta Querétaro |
| E009 | Miguel Sánchez | Campo Norte |
| E010 | Camila Rivera | Campo Norte |
| E011 | Jorge Díaz | Campo Norte |
| E012 | Fernanda Cruz | Campo Norte |

Todos estos empleados ya tienen el turno "Fijo diurno" asignado para hoy y ya otorgaron su consentimiento de geolocalización, para poder probar el marcaje de inmediato.

## Manual del rol Administrador

### 1. Centros de trabajo

Menú "Centros de trabajo".

1. Ver la lista: cada centro muestra su nombre, coordenadas y radio del geofence.
2. Crear uno nuevo: "Nuevo centro de trabajo" → nombre, latitud, longitud y radio en metros → "Guardar".
3. Editar: clic en "Editar" junto al centro que quieras modificar.

El radio del geofence define el perímetro dentro del cual un empleado de ese centro puede marcar su asistencia con geolocalización.

### 2. Empleados y credenciales de marcaje

Menú "Empleados".

1. Ver la lista: número, nombre, centro de trabajo y estado (activo/baja) de cada empleado.
2. Dar de alta un empleado: "Nuevo empleado" → número de empleado, nombre, centro de trabajo, estado, y si otorga su consentimiento de geolocalización → "Guardar".
3. Asignar o restablecer un PIN: "Asignar PIN" junto al empleado → PIN numérico de 4 a 6 dígitos → "Guardar PIN". Este PIN es lo que el empleado usa para marcar en un kiosco compartido.

El número de empleado debe ser único; el sistema no permite repetirlo.

### 3. Turnos y asignación de turnos

Menú "Turnos".

1. Crear un turno: "Nuevo turno" → nombre, tipo (fijo, rotativo, nocturno, flexible, on-call), hora de entrada, hora de salida, minutos de receso y minutos de tolerancia → "Guardar". La tolerancia no puede ser mayor o igual que la duración del turno.
2. Asignar a un solo empleado: "Asignar (individual)" → empleado, turno y fecha → "Asignar".
3. Asignar a un grupo: "Asignar (masiva)" → turno y fecha, marca la casilla de cada empleado a incluir → "Asignar al grupo"; el sistema confirma cuántos empleados quedaron asignados.

### 4. Días festivos

Menú "Días festivos".

1. Ver el calendario de fechas festivas ya registradas.
2. Agregar una nueva: "Nuevo día festivo" → fecha y descripción → "Guardar".

Estas fechas se usan solo de forma informativa en el Portal de consulta, para señalar qué marcas cayeron en día festivo.

### 5. Consulta de asistencias

Menú "Consulta de asistencias".

1. Filtra por empleado, centro de trabajo y/o fecha → "Filtrar".
2. Cada marca muestra: fecha y hora, empleado, centro de trabajo, tipo (entrada/salida/receso), si fue válida o rechazada, si fue puntual o tardía (comparada contra el turno asignado ese día), y si el día fue festivo.

## Manual del rol Empleado

### Marcar asistencia desde el portal

1. Inicia sesión con tu número de empleado y tu contraseña de portal.
2. En "Marcar asistencia", tu navegador te pedirá permiso para usar tu ubicación — acéptalo para que tu marca se valide contra el geofence de tu centro de trabajo.
3. Elige el tipo de marca (Entrada, Salida, Inicio de receso o Fin de receso) y da clic en "Marcar".
4. Verás de inmediato la hora registrada, y el historial de tus marcas del día en la misma pantalla.

Si tu marca es rechazada, el sistema te explica por qué: ya tienes una entrada abierta sin salida, estás fuera del perímetro de tu centro de trabajo, o falta tu consentimiento de geolocalización.

### Marcar asistencia por PIN (kiosco)

Útil en un terminal compartido, sin necesidad de iniciar sesión:

1. Abre "Marcar por PIN (kiosco)" desde la pantalla de inicio.
2. Ingresa tu número de empleado y tu PIN, elige el tipo de marca, y da clic en "Marcar".
3. Si tu número de empleado o tu PIN son incorrectos, el sistema muestra "Credenciales inválidas" sin indicar cuál de los dos datos falló (por seguridad).

## Fuera de alcance de v1.0

Estas funciones son parte de la visión completa del producto, pero llegan en una versión posterior (v1.1):

- Solicitudes de vacaciones, permisos e incapacidades, y su flujo de aprobación
- Exportación del consolidado de horas hacia el sistema de nómina, e importación de altas/bajas
- Marcaje sin conexión a internet
- Cálculo de horas extra, recargos y clasificación legal de horas (motor de pre-nómina)
- Panel de presencia en tiempo real y reasignación de cobertura del supervisor
- Reportes con KPIs exportables y bitácora de auditoría
- Roles de Supervisor, RRHH, Nómina, Auditor, TI y Ejecutivo
- Reconocimiento facial (el sistema ya deja lista la integración, pero no la implementa)

## Preguntas frecuentes

**¿Puedo cambiar mi propia contraseña o PIN?** No en v1.0 — solo un Administrador puede asignar o restablecer tu PIN de marcaje.

**¿Qué pasa si pierdo la conexión a internet al marcar?** v1.0 requiere conexión a internet; ese caso se resuelve en la v1.1.

**¿Los usuarios y contraseñas de este manual son datos reales?** No, son datos ficticios que se cargan automáticamente la primera vez que se levanta la aplicación, únicamente para poder probarla de inmediato.

---

*Este manual también existe como documento vivo editable en: https://claude.ai/artifact/VdufmTCGVtLquBh7WuAXMC*
