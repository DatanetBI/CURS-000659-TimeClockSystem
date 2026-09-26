using System.ComponentModel.DataAnnotations;

namespace TimeClockSystem.Domain;

public enum EventoAuditoria
{
    InicioSesionExitoso,
    InicioSesionFallido,
    MarcajeRechazado,
    AccesoDenegadoPorRol,
}

/// <summary>
/// Evento sensible registrado por el Backend para trazabilidad ante disputas laborales
/// (data-model.md - RegistroAuditoria, FR-011).
/// </summary>
public class RegistroAuditoria
{
    public int Id { get; set; }

    [Required]
    public EventoAuditoria Evento { get; set; }

    [Required]
    [StringLength(256)]
    public string UsuarioOEmpleadoId { get; set; } = string.Empty;

    [StringLength(500)]
    public string Detalle { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; }
}
