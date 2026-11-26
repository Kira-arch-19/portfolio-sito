using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Controllers;
using Portfolio.DTOs.Contact;
using Portfolio.Models;
using Portfolio.Tests.Helpers;

namespace Portfolio.Tests.Controllers;

public sealed class HomeControllerTests
{
    private static ContactFormDto ValidForm() => new()
    {
        Name = "Mario Rossi",
        Email = "mario@example.com",
        Message = "Vorrei saperne di più sui tuoi progetti."
    };

    private static HomeController CreateController(
        FakeProjectService projects,
        FakeContactEmailService email)
    {
        var context = new DefaultHttpContext();
        return new HomeController(
            projects,
            email,
            NullLogger<HomeController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = context },
            TempData = new TempDataDictionary(context, new FakeTempDataProvider())
        };
    }

    [Fact]
    public async Task Index_MostraElencoProgetti()
    {
        var projects = new FakeProjectService();
        projects.Items.Add(new Project { Id = 1, Name = "Portfolio" });
        var controller = CreateController(projects, new FakeContactEmailService());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Project>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Contact_Valido_InviaMessaggioEReindirizza()
    {
        var projects = new FakeProjectService();
        var email = new FakeContactEmailService();
        var controller = CreateController(projects, email);

        var result = await controller.Contact(ValidForm(), CancellationToken.None);

        Assert.Equal(1, email.CallCount);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("contatti", redirect.Fragment);
        Assert.Equal("Messaggio inviato correttamente!", controller.TempData["ContactSuccess"]);
    }

    [Fact]
    public async Task Contact_HoneypotCompilato_NonInviaEmail()
    {
        var email = new FakeContactEmailService();
        var controller = CreateController(new FakeProjectService(), email);
        var form = ValidForm();
        form.Website = "https://spam.invalid";

        var result = await controller.Contact(form, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(0, email.CallCount);
    }

    [Fact]
    public async Task Contact_ModelStateNonValido_MostraDiNuovoLaHome()
    {
        var projects = new FakeProjectService();
        projects.Items.Add(new Project { Id = 1, Name = "Portfolio" });
        var email = new FakeContactEmailService();
        var controller = CreateController(projects, email);
        // Nei test di Controller la validazione MVC non parte automaticamente:
        // simuliamo l'errore che aggiungerebbe il model binder.
        controller.ModelState.AddModelError("Email", "Email non valida");

        var result = await controller.Contact(ValidForm(), CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.IsType<List<Project>>(view.Model);
        Assert.Equal(0, email.CallCount);
        Assert.True(controller.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task Contact_SmtpFallisce_NonEsponeDettagliTecniciAlVisitatore()
    {
        var email = new FakeContactEmailService { ShouldFail = true };
        var controller = CreateController(new FakeProjectService(), email);

        var result = await controller.Contact(ValidForm(), CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal(1, email.CallCount);
        var text = string.Join(" ", controller.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        Assert.Contains("Impossibile inviare", text);
        Assert.DoesNotContain("Simulazione guasto SMTP", text);
    }
}
