
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

    // Ricerca per nome
    public async Task<Project?> GetByNameAsync(string name)
    {
        return await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    // CREATE - Controllo nome duplicato
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

    // READ - Tutti i progetti con immagini
    public override async Task<List<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Images)
            .AsNoTracking()
            .ToListAsync();
    }

    // READ - Singolo progetto con immagini
    public override async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
