
using Microsoft.AspNetCore.Http.HttpResults;
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
        .AnyAsync(p => p.Name == project.Name);

    if (exists)
    {
        throw new InvalidOperationException(
            "Esiste già un progetto con questo nome"
        );
    }

    _context.Projects.Add(project);

    await _context.SaveChangesAsync();

    return project;
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
}
