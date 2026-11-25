
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services;

public class ProjectService
    : CrudService<Project>, IProjectService
{
    public ProjectService(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Project?> GetByNameAsync(string name)
    {
        return await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public override async Task<Project> CreateAsync(Project project)
    {
        bool exists = await _context.Projects
            .AnyAsync(p => p.Name == project.Name);

        if (exists)
        {
            throw new InvalidOperationException(
                "Esiste già un progetto con questo nome");
        }

        return await base.CreateAsync(project);
    }
}
