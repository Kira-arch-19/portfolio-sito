
using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs.Projects;

public class UpdateProjectDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Technologies { get; set; } = string.Empty;

    [Url]
    public string GithubLink { get; set; } = string.Empty;

    public string SkillImproved { get; set; } = string.Empty;
}
