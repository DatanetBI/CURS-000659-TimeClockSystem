namespace TimeClockSystem.Web.ViewModels;

// Modelos propios del Frontend que reflejan la forma de las respuestas del Backend (research.md #4:
// sin proyecto de contratos compartido). Se mantienen los mismos nombres que el Domain original
// para que las Vistas Razor existentes seguir compilando sin cambios (FR-004).

public enum EstadoEmpleado
{
    Activo,
    Baja,
}

public enum TipoTurno
{
    Fijo,
    Rotativo,
    Nocturno,
    Flexible,
    OnCall,
}

public enum TipoMarca
{
    Entrada,
    Salida,
    InicioReceso,
    FinReceso,
}

public enum CanalMarca
{
    PortalWeb,
    Pin,
}

public enum EstadoMarca
{
    Valida,
    Rechazada,
}

public enum MotivoRechazoMarca
{
    Ninguno,
    Geofence,
    EntradaDuplicada,
    CredencialesInvalidas,
    SinConsentimientoGeolocalizacion,
}

public enum IndicadorPuntualidad
{
    Puntual,
    Tardio,
    SinTurnoAsignado,
}
