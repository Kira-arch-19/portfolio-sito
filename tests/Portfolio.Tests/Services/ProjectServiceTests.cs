using Microsoft.EntityFrameworkCore;
using Portfolio.Models;
using Portfolio.Services;
using Portfolio.Tests.Helpers;

namespace Portfolio.Tests.Services;

public sealed class ProjectServiceTests
{
    private static Project NewProject(string name) => new()
    {
        Name = name,
        ShortDescription = $"Descrizione breve di {name}",
        Description = "Descrizione tecnica",
        Technologies = "C#, PostgreSQL"
    };

    [Fact]
    public async Task CreateAsync_SalvaProgettoNelDatabase()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);

        var created = await service.CreateAsync(NewProject("Portfolio"));

        Assert.True(created.Id > 0);
        var persisted = await db.Context.Projects
            .AsNoTracking()
            .SingleAsync(p => p.Id == created.Id);
        Assert.Equal("Portfolio", persisted.Name);
    }

    [Fact]
    public async Task CreateAsync_RifiutaNomeDuplicato()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        await service.CreateAsync(NewProject("Gestionale"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(NewProject("Gestionale")));

        Assert.Contains("Esiste già", exception.Message);
        Assert.Equal(1, await db.Context.Projects.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_CaricaAncheImmagini()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        var project = await service.CreateAsync(NewProject("Unity"));
        db.Context.ProjectImages.Add(new ProjectImage
        {
            ProjectId = project.Id,
            ImageUrl = "/uploads/projects/1/copertina.png",
            IsCover = true,
            Position = 0
        });
        await db.Context.SaveChangesAsync();

        var loaded = await service.GetByIdAsync(project.Id);

        Assert.NotNull(loaded);
        Assert.Single(loaded.Images);
        Assert.True(loaded.Images[0].IsCover);
    }

    [Fact]
    public async Task GetAllAsync_RestituisceProgettiEImmagini()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        var first = await service.CreateAsync(NewProject("Portfolio"));
        await service.CreateAsync(NewProject("Inventory"));
        db.Context.ProjectImages.Add(new ProjectImage
        {
            ProjectId = first.Id,
            ImageUrl = "/uploads/projects/1/immagine.webp",
            Position = 1
        });
        await db.Context.SaveChangesAsync();

        var projects = await service.GetAllAsync();

        Assert.Equal(2, projects.Count);
        Assert.Single(projects.Single(p => p.Id == first.Id).Images);
    }

    [Fact]
    public async Task GetByNameAsync_TrovaProgettoEsistente()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        await service.CreateAsync(NewProject("Life Project"));

        var found = await service.GetByNameAsync("Life Project");

        Assert.NotNull(found);
        Assert.Equal("Life Project", found.Name);
        Assert.Null(await service.GetByNameAsync("Non esiste"));
    }

    [Fact]
    public async Task UpdateAsync_ModificaDatiEsistenti()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        var created = await service.CreateAsync(NewProject("Nome vecchio"));
        var replacement = NewProject("Nome nuovo");
        replacement.ShortDescription = "Descrizione aggiornata";

        var updated = await service.UpdateAsync(created.Id, replacement);

        Assert.NotNull(updated);
        var persisted = await db.Context.Projects.AsNoTracking()
            .SingleAsync(p => p.Id == created.Id);
        Assert.Equal("Nome nuovo", persisted.Name);
        Assert.Equal("Descrizione aggiornata", persisted.ShortDescription);
    }

    [Fact]
    public async Task UpdateAsync_IdAssente_RestituisceNull()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);

        var result = await service.UpdateAsync(999, NewProject("Non trovato"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RimuoveProgettoEImmaginiDalDatabase()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);
        var created = await service.CreateAsync(NewProject("Da eliminare"));
        db.Context.ProjectImages.Add(new ProjectImage
        {
            ProjectId = created.Id,
            ImageUrl = "/uploads/projects/1/screenshot.png"
        });
        await db.Context.SaveChangesAsync();

        Assert.True(await service.DeleteAsync(created.Id));
        Assert.Empty(await db.Context.Projects.AsNoTracking().ToListAsync());
        Assert.Empty(await db.Context.ProjectImages.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_IdAssente_RestituisceFalse()
    {
        using var db = new SqliteTestDatabase();
        var service = new ProjectService(db.Context);

        Assert.False(await service.DeleteAsync(999));
    }
}
