
using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs.Projects;

public class CreateProjectDto
{
    [Required(ErrorMessage = "Il nome è obbligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descrizione breve è obbligatoria")]
    [MaxLength(200)]
    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Technologies { get; set; } = string.Empty;

    [Url(ErrorMessage = "Inserisci un URL valido")]
    public string GithubLink { get; set; } = string.Empty;

    public string SkillImproved { get; set; } = string.Empty;
}
