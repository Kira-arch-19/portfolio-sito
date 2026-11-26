using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.DTOs.Contact;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers;

public class HomeController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IContactEmailService _contactEmailService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProjectService projectService,
        IContactEmailService contactEmailService,
        ILogger<HomeController> logger)
    {
        _projectService = projectService;
        _contactEmailService = contactEmailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<Project> projects = await _projectService.GetAllAsync();
        return View(projects);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Contact(
        [FromForm] ContactFormDto form,
        CancellationToken cancellationToken)
    {
        // Honeypot: rispondi come se il form fosse andato a buon fine,
        // senza inviare nulla ai bot che compilano il campo nascosto.
        if (!string.IsNullOrWhiteSpace(form.Website))
            return RedirectToAction(nameof(Index), "Home", null, "contatti");

        if (!ModelState.IsValid)
            return await RenderHomeWithFormAsync(form);

        try
        {
            await _contactEmailService.SendAsync(form, cancellationToken);
            TempData["ContactSuccess"] = "Messaggio inviato correttamente!";
            return RedirectToAction(nameof(Index), "Home", null, "contatti");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Non mostrare messaggi di errore SMTP o credenziali al visitatore.
            _logger.LogError(ex, "Errore invio messaggio dal modulo Contatti");
            ModelState.AddModelError(string.Empty,
                "Impossibile inviare il messaggio. Riprova più tardi o contattami via LinkedIn.");
            return await RenderHomeWithFormAsync(form);
        }
    }

    private async Task<IActionResult> RenderHomeWithFormAsync(ContactFormDto form)
    {
        ViewBag.ContactForm = form;
        List<Project> projects = await _projectService.GetAllAsync();
        // La Home mantiene il suo @model List<Project>.
        return View("Index", projects);
    }
}
