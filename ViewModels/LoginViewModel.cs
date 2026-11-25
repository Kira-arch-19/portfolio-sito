
using System.ComponentModel.DataAnnotations;

namespace Portfolio.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Inserisci la tua email")]
    [EmailAddress(ErrorMessage = "Email non valida")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Inserisci la password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
