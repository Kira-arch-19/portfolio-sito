
using Portfolio.Models;

namespace Portfolio.Services.Interfaces;

public interface IProjectService
{
    // READ
    Task<List<Project>> GetAllAsync();

    Task<Project?> GetByIdAsync(int id);

    Task<Project?> GetByNameAsync(string name);

    // CREATE
    Task<Project> CreateAsync(Project project);

    // UPDATE
    Task<Project?> UpdateAsync(int id, Project project);

    // DELETE
    Task<bool> DeleteAsync(int id);
}
