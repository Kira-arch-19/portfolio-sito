
namespace Portfolio.Models;

public class ProjectImage
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsCover { get; set; }

    public int Position { get; set; }
}
