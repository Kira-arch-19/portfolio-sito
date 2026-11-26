using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Controllers;
using Portfolio.DTOs.Projects;
using Portfolio.Models;
using Portfolio.Tests.Helpers;
using System.Reflection;

namespace Portfolio.Tests.Controllers;

public sealed class ProjectControllerTests
{
    [Fact]
    public async Task Details_IdNonValido_Restituisce400()
    {
        var controller = new ProjectController(new FakeProjectService());

        var result = await controller.Details(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Details_ProgettoAssente_Restituisce404()
    {
        var controller = new ProjectController(new FakeProjectService());

        var result = await controller.Details(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ProgettoEsistente_Restituisce200ConProgetto()
    {
        var projects = new FakeProjectService();
        projects.Items.Add(new Project { Id = 3, Name = "Portfolio" });
        var controller = new ProjectController(projects);

        var result = await controller.Details(3);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Portfolio", Assert.IsType<Project>(ok.Value).Name);
    }

    [Fact]
    public async Task Page_ProgettoEsistente_UsaViewDetails()
    {
        var projects = new FakeProjectService();
        projects.Items.Add(new Project { Id = 2, Name = "Life Project" });
        var controller = new ProjectController(projects);

        var result = await controller.Page(2);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Details", view.ViewName);
        Assert.Equal(2, Assert.IsType<Project>(view.Model).Id);
    }

    [Fact]
    public async Task Create_ProgettoDuplicato_Restituisce409()
    {
        var projects = new FakeProjectService { ThrowOnDuplicate = true };
        var controller = new ProjectController(projects);
        var request = new CreateProjectDto
        {
            Name = "Portfolio",
            ShortDescription = "Il mio sito personale"
        };

        var result = await controller.Create(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Update_ProgettoAssente_Restituisce404()
    {
        var controller = new ProjectController(new FakeProjectService());
        var request = new UpdateProjectDto
        {
            Name = "Portfolio",
            ShortDescription = "Descrizione"
        };

        var result = await controller.Update(500, request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ProgettoAssente_Restituisce404()
    {
        var controller = new ProjectController(new FakeProjectService());

        var result = await controller.Delete(500);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void AzioniModifica_RichiedonoRuoloAdmin()
    {
        foreach (string methodName in new[] { "Create", "Update", "Delete" })
        {
            var method = typeof(ProjectController).GetMethod(methodName);
            Assert.NotNull(method);
            var authorization = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authorization);
            Assert.Equal("Admin", authorization.Roles);
        }
    }

    [Fact]
    public void AdminController_ProtettoDalRuoloAdmin()
    {
        var authorization = typeof(AdminController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorization);
        Assert.Equal("Admin", authorization.Roles);
    }
}
