
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers;

public class HomeController : Controller
{
    private readonly IProjectService _projectService;

    public HomeController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        List<Project> projects =
            await _projectService.GetAllAsync();

        return View(projects);
    }
}
