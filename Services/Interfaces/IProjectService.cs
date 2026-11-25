
using Portfolio.Models;

namespace Portfolio.Services.Interfaces;

public interface IProjectService : ICrudService<Project>
{
    Task<Project?> GetByNameAsync(string name);
}
