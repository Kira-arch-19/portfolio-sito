using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs.Contact;

public class ContactFormDto
{
    [Required(ErrorMessage = "Inserisci il tuo nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Il nome deve contenere da 2 a 100 caratteri.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Inserisci la tua email.")]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Scrivi un messaggio.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Il messaggio deve contenere da 10 a 4000 caratteri.")]
    public string Message { get; set; } = string.Empty;

    // Campo esca antispam: deve restare vuoto.
    [StringLength(150)]
    public string Website { get; set; } = string.Empty;
}
