using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Controllers;

[Route("project")]
public class ProjectController : Controller
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("")]
public async Task<IActionResult> Index()
{
    List<Project> projects = await _projectService.GetAllAsync();

    return Ok(projects);
}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

     Project? project =
            await _projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        return Ok(project);
    }
}