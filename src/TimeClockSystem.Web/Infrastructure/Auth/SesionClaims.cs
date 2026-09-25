namespace TimeClockSystem.Web.Infrastructure.Auth;

public static class SesionClaims
{
    /// <summary>Claim propio (no estándar) donde se guarda el JWT del Backend dentro de la cookie cifrada.</summary>
    public const string AccessToken = "access_token";
}
