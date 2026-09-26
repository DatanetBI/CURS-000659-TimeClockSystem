using Microsoft.AspNetCore.Identity;
using TimeClockSystem.Application.Abstractions;
using TimeClockSystem.Domain;

namespace TimeClockSystem.Infrastructure.Auth;

/// <summary>
/// El PIN de marcaje nunca se guarda en texto plano (Principio V). Usa el mismo formato de hash
/// que <c>DbSeeder</c> (<see cref="PasswordHasher{TUser}"/>, independiente del tipo genérico).
/// </summary>
public class PinHasher : IPinHasher
{
    private readonly PasswordHasher<CredencialDeMarcaje> hasher = new();

    public string Hash(string pin) => hasher.HashPassword(null!, pin);

    public bool Verificar(string pin, string hash) =>
        hasher.VerifyHashedPassword(null!, hash, pin) != PasswordVerificationResult.Failed;
}
