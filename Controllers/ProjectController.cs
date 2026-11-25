using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Services.Interfaces;
using Portfolio.DTOs.Projects;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Portfolio.Services;
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
    
    
[HttpPost("")]
public async Task<IActionResult> Create(
    [FromBody] CreateProjectDto request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    Project project = new Project
    {
        Name = request.Name,
        ShortDescription = request.ShortDescription,
        Description = request.Description,
        Technologies = request.Technologies,
        GithubLink = request.GithubLink,
        SkillImproved = request.SkillImproved
    };

    try
    {
        Project createdProject =
            await _projectService.CreateAsync(project);

        return CreatedAtAction(
            nameof(Details),
            new { id = createdProject.Id },
            createdProject
        );
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { message = ex.Message });
    }
}

[HttpPut("{id:int}")]
public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateProjectDto request)
{
    if (id <= 0)
    {
        return BadRequest("ID non valido");
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    Project project = new Project
    {
        Name = request.Name,
        Description = request.Description,
        ShortDescription = request.ShortDescription,
        GithubLink = request.GithubLink,
        Technologies = request.Technologies,
        SkillImproved = request.SkillImproved
    };

    Project? updatedProject =
        await _projectService.UpdateAsync(id, project);

    if (updatedProject == null)
    {
        return NotFound("Progetto non trovato");
    }

    return Ok(updatedProject);
}

[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
    {
        if(id <= 0)
        {
            return BadRequest("id non valido");
        }
        bool deleted = await _projectService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Progetto non trovato");
        }
        return NoContent();
    }


[HttpGet("{id:int}/pagina")]
public async Task<IActionResult> Page(int id)
{
    if (id <= 0)
    {
        return BadRequest("ID non valido");
    }

    Project? project =
        await _projectService.GetByIdAsync(id);

    if (project == null)
    {
        return NotFound("Progetto non trovato");
    }

    return View("Details", project);
}

}
