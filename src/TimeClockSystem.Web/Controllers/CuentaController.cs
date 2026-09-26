using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;
using TimeClockSystem.Web.Models;

namespace TimeClockSystem.Web.Controllers;

public class CuentaController(AuthApiClient authApi) : Controller
{
    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new IniciarSesionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(IniciarSesionViewModel modelo, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await authApi.LoginAsync(modelo.UserName, modelo.Password);
        if (resultado is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // El JWT del Backend se guarda solo del lado servidor, dentro de la cookie de autenticación
        // cifrada; el navegador nunca lo ve directamente (FR-002a).
        List<Claim> claims =
        [
            new(ClaimTypes.Name, resultado.Nombre),
            new(ClaimTypes.Role, resultado.Rol),
            new(SesionClaims.AccessToken, resultado.Token),
        ];
        if (resultado.EmpleadoId is { } empleadoId)
        {
            claims.Add(new Claim("empleadoId", empleadoId.ToString()));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = resultado.ExpiraEnUtc,
        });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();
}
