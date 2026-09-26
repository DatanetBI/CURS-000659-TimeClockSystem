namespace TimeClockSystem.Domain;

/// <summary>
/// Punto de integración para un proveedor externo de reconocimiento facial y prueba de vida
/// (FR-007). El algoritmo de matching/liveness en sí queda fuera de v1.0 (spec.md - Fuera de
/// Alcance); esta interfaz solo deja lista la extensión para una fase posterior.
/// </summary>
public interface IBiometricVerificationProvider
{
    Task<bool> VerificarAsync(int empleadoId, CancellationToken cancellationToken = default);
}
