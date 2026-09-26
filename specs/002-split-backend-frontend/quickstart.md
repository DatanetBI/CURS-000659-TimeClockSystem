# Quickstart: Validar la separación Backend/Frontend

Esta guía valida de extremo a extremo que la separación funciona, cubriendo las 3 Historias de
Usuario de `spec.md`.

## Prerrequisitos

- .NET 10 SDK instalado.
- Repositorio en la rama `002-split-backend-frontend`, con `TimeClockSystem.Api` y
  `TimeClockSystem.Web` ya implementados según este plan.
- Base de datos SQLite migrada (`dotnet ef database update` desde `TimeClockSystem.Infrastructure`,
  o migración automática al iniciar la API, igual que hoy).

## 1. Levantar el Backend de forma independiente

```powershell
dotnet run --project src/TimeClockSystem.Api
```

- Abrir `https://localhost:<puerto>/swagger`.
- **Validación (Historia de Usuario 3 / SC-003)**: usando solo Swagger, ejecutar el flujo
  `POST /api/auth/login` (admin de prueba) → `POST /api/empleados` → `POST /api/asignaciones-turno`
  → `POST /api/marcaje` → `GET /api/consulta-asistencias`, sin abrir el Frontend. Debe completarse
  en menos de 15 minutos.

## 2. Levantar el Frontend apuntando al Backend

```powershell
dotnet run --project src/TimeClockSystem.Web
```

- Configurar la URL base del Backend en `appsettings.Development.json` del Frontend (ej.
  `Api:BaseUrl`).
- **Validación (Historia de Usuario 1 / SC-001)**: iniciar sesión como Administrador y como
  Empleado (credenciales mock existentes) y repetir los flujos de cada Area (Empleados,
  CentrosTrabajo, Turnos, AsignacionTurno, DiasFestivos, Marcaje, ConsultaAsistencias) verificando
  que el resultado en pantalla es idéntico al del sistema previo a la separación.
- **Validación (FR-002a)**: inspeccionar las herramientas de desarrollador del navegador (pestaña
  Application/Cookies y Network) y confirmar que el JWT del Backend no aparece nunca en el cuerpo de
  las respuestas recibidas por el navegador ni en `localStorage`/`sessionStorage`.

## 3. Validar el aislamiento de despliegue (Historia de Usuario 2 / SC-002)

1. Con ambos proyectos corriendo, detener solo `TimeClockSystem.Api`, hacer un cambio trivial (ej.
   un mensaje de log) y volver a iniciarlo, **sin** recompilar ni reiniciar `TimeClockSystem.Web`.
2. Repetir una acción en el Frontend (ej. consultar empleados) y confirmar que sigue funcionando.
3. **Validación (SC-004 / FR-009)**: detener `TimeClockSystem.Api` y, con el Frontend corriendo,
   intentar registrar una marca. Debe mostrarse un mensaje de error claro y no técnico, sin que la
   página falle de forma no controlada.

## 4. Validar auditoría (FR-011, FR-011a, SC-006)

1. Provocar un login fallido, un marcaje rechazado (por geofence) y un intento de un Empleado de
   consultar la asistencia de otro empleado.
2. Iniciar sesión como Administrador y abrir la nueva pantalla **Auditoría**.
3. Filtrar por fecha y por usuario y confirmar que los 3 eventos anteriores aparecen con el detalle
   esperado, sin necesidad de consultar la base de datos directamente.

## 5. Validar cero pérdida de datos (FR-010)

1. Antes de migrar, respaldar el archivo SQLite existente.
2. Tras migrar `TimeClockSystem.Infrastructure` y aplicar solo la migración aditiva de auditoría,
   confirmar en el Frontend que todos los empleados, marcas, turnos, asignaciones y días festivos
   previos a la separación siguen visibles y consultables.

## 6. Validar rendimiento percibido (SC-007)

1. Con ambos proyectos corriendo, abrir las herramientas de desarrollador del navegador (pestaña
   Network) antes de repetir estas acciones:
   - Registrar una marca de entrada/salida (Marcaje).
   - Guardar un cambio de un Empleado o de un Turno.
2. Confirmar que cada acción se completa (respuesta HTTP recibida + pantalla actualizada) en menos
   de 5 segundos, pese al salto de red adicional introducido por la separación.
3. Repetir al menos 5 veces por acción; si alguna supera los 5 segundos de forma consistente,
   investigar antes de dar por válido SC-007.
