using TimeClockSystem.Domain;

namespace TimeClockSystem.Infrastructure.Biometria;

/// <summary>
/// Implementación por defecto: no aplica ninguna validación biométrica real. Se usa mientras no
/// haya un proveedor biométrico conectado (fuera de alcance en v1.0 — relocalizada tal cual desde
/// TimeClockSystem.Web.Domain, FR-005).
/// </summary>
public class NullBiometricVerificationProvider : IBiometricVerificationProvider
{
    public Task<bool> VerificarAsync(int empleadoId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
