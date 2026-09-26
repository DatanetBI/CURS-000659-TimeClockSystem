using System.Net.Http.Headers;

namespace TimeClockSystem.Web.Infrastructure.Auth;

/// <summary>
/// Adjunta el token del Backend (guardado como claim dentro de la cookie de autenticación del
/// Frontend) como <c>Authorization: Bearer</c> en cada llamada saliente a la API. El navegador
/// nunca ve este token (FR-002a).
/// </summary>
public class TokenForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.User.FindFirst(SesionClaims.AccessToken)?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
