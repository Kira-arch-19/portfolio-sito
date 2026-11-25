
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Tabelle
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectImage> ProjectImages { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tabella progetti
        modelBuilder.Entity<Project>()
            .ToTable("projects");

        // Tabella immagini dei progetti
        modelBuilder.Entity<ProjectImage>(entity =>
        {
            entity.ToTable("project_images");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            // Relazione uno-a-molti
            entity.HasOne<Project>()
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
