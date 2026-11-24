
using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Technologies { get; set; } = string.Empty;

    public string GithubLink { get; set; } = string.Empty;

    public string SkillImproved { get; set; } = string.Empty;
}
