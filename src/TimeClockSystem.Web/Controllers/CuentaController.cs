using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeClockSystem.Web.Infrastructure.Identity;
using TimeClockSystem.Web.Models;

namespace TimeClockSystem.Web.Controllers;

public class CuentaController(SignInManager<ApplicationUser> signInManager) : Controller
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

        var resultado = await signInManager.PasswordSignInAsync(
            modelo.UserName, modelo.Password, isPersistent: true, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();
}
