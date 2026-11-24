
using Portfolio.Models;

namespace Portfolio.Services.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync();

    Task<Project?> GetByIdAsync(int id);
    
    Task<Project> CreateAsync(Project project);

}
