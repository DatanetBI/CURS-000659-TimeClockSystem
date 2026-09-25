namespace TimeClockSystem.Infrastructure.Auth;

public record JwtOptions(string SigningKey, string Issuer, string Audience, int ExpirationMinutes);
