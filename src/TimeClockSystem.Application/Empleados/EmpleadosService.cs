using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Application.Empleados;

public record EmpleadoDto(int Id, string NumeroEmpleado, string Nombre, int CentroTrabajoId, string? CentroTrabajoNombre, EstadoEmpleado Estado, bool ConsentimientoGeolocalizacion);

public record CrearEmpleadoRequest(string NumeroEmpleado, string Nombre, int CentroTrabajoId, bool ConsentimientoGeolocalizacion);

public record ActualizarEmpleadoRequest(string NumeroEmpleado, string Nombre, int CentroTrabajoId, EstadoEmpleado Estado, bool ConsentimientoGeolocalizacion);

public record CredencialInfo(bool TieneCredencial, DateTime? FechaActualizacion);

/// <summary>Casos de uso de Empleados (contracts/empleados.md).</summary>
public class EmpleadosService(IEmpleadoRepository empleados, ICredencialRepository credenciales, IPinHasher pinHasher)
{
    public async Task<IReadOnlyList<EmpleadoDto>> ListarAsync(CancellationToken cancellationToken = default) =>
        (await empleados.ListarAsync(cancellationToken)).Select(AParaDto).ToList();

    public async Task<EmpleadoDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleados.ObtenerPorIdAsync(id, cancellationToken);
        return empleado is null ? null : AParaDto(empleado);
    }

    public async Task<EmpleadoDto> CrearAsync(CrearEmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        var empleado = new Empleado
        {
            NumeroEmpleado = request.NumeroEmpleado,
            Nombre = request.Nombre,
            CentroTrabajoId = request.CentroTrabajoId,
            ConsentimientoGeolocalizacion = request.ConsentimientoGeolocalizacion,
        };
        await empleados.AgregarAsync(empleado, cancellationToken);
        return AParaDto(empleado);
    }

    public async Task<EmpleadoDto?> ActualizarAsync(int id, ActualizarEmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        var empleado = await empleados.ObtenerPorIdAsync(id, cancellationToken);
        if (empleado is null)
        {
            return null;
        }

        empleado.NumeroEmpleado = request.NumeroEmpleado;
        empleado.Nombre = request.Nombre;
        empleado.CentroTrabajoId = request.CentroTrabajoId;
        empleado.Estado = request.Estado;
        empleado.ConsentimientoGeolocalizacion = request.ConsentimientoGeolocalizacion;
        await empleados.ActualizarAsync(empleado, cancellationToken);
        return AParaDto(empleado);
    }

    public async Task<CredencialInfo?> ObtenerCredencialAsync(int empleadoId, CancellationToken cancellationToken = default)
    {
        if (await empleados.ObtenerPorIdAsync(empleadoId, cancellationToken) is null)
        {
            return null;
        }

        var credencial = await credenciales.ObtenerPorEmpleadoIdAsync(empleadoId, cancellationToken);
        return new CredencialInfo(credencial is not null, credencial?.FechaActualizacion);
    }

    public async Task<bool> ActualizarCredencialAsync(int empleadoId, string pin, string? actualizadoPorUserId, CancellationToken cancellationToken = default)
    {
        if (await empleados.ObtenerPorIdAsync(empleadoId, cancellationToken) is null)
        {
            return false;
        }

        await credenciales.GuardarAsync(empleadoId, pinHasher.Hash(pin), actualizadoPorUserId, cancellationToken);
        return true;
    }

    private static EmpleadoDto AParaDto(Empleado empleado) => new(
        empleado.Id, empleado.NumeroEmpleado, empleado.Nombre, empleado.CentroTrabajoId,
        empleado.CentroTrabajo?.Nombre, empleado.Estado, empleado.ConsentimientoGeolocalizacion);
}
