
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Portfolio.Models;
using Portfolio.ViewModels.Account;

namespace Portfolio.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // GET /account/login
    
[AllowAnonymous]
[HttpGet("login")]
public IActionResult Login(string? returnUrl = null)
{
    // Se l'amministratore è già autenticato
    if (User.Identity?.IsAuthenticated == true &&
        User.IsInRole("Admin"))
    {
        return RedirectToAction("Index", "Admin");
    }

    ViewData["ReturnUrl"] = returnUrl;

    return View(new LoginViewModel());
}


    // POST /account/login
    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ApplicationUser? user =
            await _userManager.FindByEmailAsync(model.Email);

        if (user == null ||
            !await _userManager.IsInRoleAsync(user, "Admin"))
        {
            ModelState.AddModelError(
                "", "Credenziali non valide");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                "", "Account temporaneamente bloccato");
        }
        else
        {
            ModelState.AddModelError(
                "", "Credenziali non valide");
        }

        return View(model);
    }

    // POST /account/logout
    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }

    // GET /account/access-denied
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
