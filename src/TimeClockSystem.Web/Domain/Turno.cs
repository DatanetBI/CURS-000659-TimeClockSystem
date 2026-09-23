using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Web.Domain;

public enum TipoTurno
{
    Fijo,
    Rotativo,
    Nocturno,
    Flexible,
    OnCall,
}

/// <summary>
/// Plantilla de horario (data-model.md - Turno). El reparto de horas de un turno nocturno entre
/// dos fechas calendario (FR-012) y el motor de horas extra (FR-016 a FR-019) quedan diferidos a
/// v1.1 (plan.md).
/// </summary>
public class Turno
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del turno es obligatorio.")]
    [StringLength(80)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Tipo de turno")]
    public TipoTurno Tipo { get; set; } = TipoTurno.Fijo;

    [Required(ErrorMessage = "La hora de entrada es obligatoria.")]
    [Display(Name = "Hora de entrada")]
    public TimeSpan HoraEntrada { get; set; }

    [Required(ErrorMessage = "La hora de salida es obligatoria.")]
    [Display(Name = "Hora de salida")]
    public TimeSpan HoraSalida { get; set; }

    [Range(0, 480, ErrorMessage = "La duración del receso debe estar entre 0 y 480 minutos.")]
    [Display(Name = "Duración del receso (minutos)")]
    public int DuracionRecesoMinutos { get; set; }

    [Range(0, 1440, ErrorMessage = "La tolerancia debe estar entre 0 y 1440 minutos.")]
    [Display(Name = "Tolerancia (minutos)")]
    public int ToleranciaMinutos { get; set; }

    /// <summary>
    /// Duración total del turno en minutos, considerando que puede cruzar la medianoche
    /// (HoraSalida menor que HoraEntrada).
    /// </summary>
    public int DuracionTotalMinutos()
    {
        var duracion = HoraSalida - HoraEntrada;
        if (duracion <= TimeSpan.Zero)
        {
            duracion += TimeSpan.FromDays(1);
        }
        return (int)duracion.TotalMinutes;
    }
}
