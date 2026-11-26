using Portfolio.DTOs.Contact;

namespace Portfolio.Services.Interfaces;

public interface IContactEmailService
{
    Task SendAsync(ContactFormDto form, CancellationToken cancellationToken = default);
}
