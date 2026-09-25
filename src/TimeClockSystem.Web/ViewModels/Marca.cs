namespace TimeClockSystem.Web.ViewModels;

public class Marca
{
    public int Id { get; set; }
    public TipoMarca Tipo { get; set; }
    public CanalMarca Canal { get; set; }
    public DateTime Timestamp { get; set; }
    public EstadoMarca Estado { get; set; }
    public MotivoRechazoMarca MotivoRechazo { get; set; } = MotivoRechazoMarca.Ninguno;
}
