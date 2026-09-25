namespace TimeClockSystem.Domain;

public enum IndicadorPuntualidad
{
    Puntual,
    Tardio,
    SinTurnoAsignado,
}

/// <summary>
/// Compara una marca de entrada contra el turno asignado ese día + su tolerancia, para el Portal
/// de consulta (Módulo 6). No clasifica tipos de hora extra ni calcula montos — eso es el motor de
/// pre-nómina, diferido a v1.1 (research.md #8).
/// </summary>
public static class PuntualidadCalculator
{
    public static IndicadorPuntualidad Calcular(Marca marcaEntrada, Turno? turnoAsignado)
    {
        if (turnoAsignado is null)
        {
            return IndicadorPuntualidad.SinTurnoAsignado;
        }

        var horaLocal = TimeOnly.FromDateTime(marcaEntrada.Timestamp.ToLocalTime());
        var horaEntradaTurno = TimeOnly.FromTimeSpan(turnoAsignado.HoraEntrada);
        var limiteTolerancia = horaEntradaTurno.AddMinutes(turnoAsignado.ToleranciaMinutos);

        return horaLocal <= limiteTolerancia ? IndicadorPuntualidad.Puntual : IndicadorPuntualidad.Tardio;
    }
}
