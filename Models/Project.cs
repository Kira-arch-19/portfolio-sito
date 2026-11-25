
using System.ComponentModel.DataAnnotations;
using Portfolio.Models.Interfaces;

namespace Portfolio.Models;

public class Project : IEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "La descrizione breve è obbligatoria")]
    [MaxLength(200, ErrorMessage = "Massimo 200 caratteri")]
    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Technologies { get; set; } = string.Empty;

    public string GithubLink { get; set; } = string.Empty;

    public string SkillImproved { get; set; } = string.Empty;
}
