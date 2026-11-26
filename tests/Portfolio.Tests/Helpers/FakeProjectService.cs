using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Tests.Helpers;

/// <summary>
/// Servizio finto per isolare i Controller dal database.
/// NON viene registrato nell'applicazione reale.
/// </summary>
internal sealed class FakeProjectService : IProjectService
{
    public List<Project> Items { get; } = new();
    public bool ThrowOnDuplicate { get; set; }
    public int ReadCount { get; private set; }

    public Task<List<Project>> GetAllAsync()
    {
        ReadCount++;
        return Task.FromResult(Items.ToList());
    }

    public Task<Project?> GetByIdAsync(int id) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<Project?> GetByNameAsync(string name) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Name == name));

    public Task<Project> CreateAsync(Project entity)
    {
        if (ThrowOnDuplicate || Items.Any(p => p.Name == entity.Name))
            throw new InvalidOperationException("Esiste già un progetto con questo nome");

        entity.Id = Items.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1;
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Project?> UpdateAsync(int id, Project entity)
    {
        var existing = Items.FirstOrDefault(p => p.Id == id);
        if (existing is null)
            return Task.FromResult<Project?>(null);

        existing.Name = entity.Name;
        existing.ShortDescription = entity.ShortDescription;
        existing.Description = entity.Description;
        existing.Technologies = entity.Technologies;
        existing.GithubLink = entity.GithubLink;
        existing.SkillImproved = entity.SkillImproved;
        return Task.FromResult<Project?>(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var existing = Items.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(existing is not null && Items.Remove(existing));
    }
}
