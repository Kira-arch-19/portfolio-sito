
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Services.Interfaces;
using Portfolio.DTOs.Projects;
namespace Portfolio.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IProjectService _projectService;

    public AdminController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<Project> projects =
            await _projectService.GetAllAsync();

        return View(projects);
    }

[HttpGet("projects/create")]
public IActionResult Create()
{
    return View(new CreateProjectDto());
}

[HttpPost("projects/create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    [FromForm] CreateProjectDto model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    Project project = new Project
    {
        Name = model.Name,
        ShortDescription = model.ShortDescription,
        Description = model.Description,
        Technologies = model.Technologies,
        GithubLink = model.GithubLink,
        SkillImproved = model.SkillImproved
    };

    try
    {
        await _projectService.CreateAsync(project);

        TempData["SuccessMessage"] =
            "Progetto creato correttamente!";

        return RedirectToAction(nameof(Index));
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(
            nameof(model.Name), ex.Message);

        return View(model);
    }
}

}
