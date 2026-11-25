
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

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
}
