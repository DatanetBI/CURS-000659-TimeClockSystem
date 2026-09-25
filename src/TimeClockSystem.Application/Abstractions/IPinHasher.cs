namespace TimeClockSystem.Application.Abstractions;

/// <summary>
/// El PIN de marcaje nunca se guarda en texto plano (Principio V de la constitución).
/// </summary>
public interface IPinHasher
{
    string Hash(string pin);

    bool Verificar(string pin, string hash);
}
