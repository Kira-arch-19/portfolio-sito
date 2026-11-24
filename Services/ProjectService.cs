using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services;
public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

  public async Task<Project> CreateAsync(Project project)
{
    bool exists = await _context.Projects
        .AnyAsync(p => p.Id == project.Id);

    if (exists)
    {
        throw new InvalidOperationException(
            "Esiste già un progetto con questo id"
        );
    }

    _context.Projects.Add(project);

    await _context.SaveChangesAsync();

    return project;
}

   public async Task<bool> DeleteAsync(int id)
{
    Project? project = await _context.Projects
        .FirstOrDefaultAsync(p => p.Id == id);

    if (project == null)
        return false;

    _context.Projects.Remove(project);

    await _context.SaveChangesAsync();

    return true;
}

    public async Task<List<Project>> GetAllAsync()
    {
        return await _context.Projects
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByNameAsync(string name)
    {
        Project? project = await _context.Projects
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.Name == name);
    return project;
    }

    public async Task<Project?> UpdateAsync(int id, Project project)
{
    Project? existingProject = await _context.Projects
        .FirstOrDefaultAsync(p => p.Id == id);

    if (existingProject == null)
        return null;

    existingProject.Name = project.Name;
    existingProject.ShortDescription = project.ShortDescription;
    existingProject.Description = project.Description;
    existingProject.Technologies = project.Technologies;
    existingProject.GithubLink = project.GithubLink;
    existingProject.SkillImproved = project.SkillImproved;

    await _context.SaveChangesAsync();

    return existingProject;
}
}
