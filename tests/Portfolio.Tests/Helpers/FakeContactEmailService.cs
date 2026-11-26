using Portfolio.DTOs.Contact;
using Portfolio.Services.Interfaces;

namespace Portfolio.Tests.Helpers;

/// <summary>Intercetta le email: non apre connessioni SMTP.</summary>
internal sealed class FakeContactEmailService : IContactEmailService
{
    public int CallCount { get; private set; }
    public ContactFormDto? LastForm { get; private set; }
    public bool ShouldFail { get; set; }

    public Task SendAsync(ContactFormDto form, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastForm = form;

        if (ShouldFail)
            throw new InvalidOperationException("Simulazione guasto SMTP: test");

        return Task.CompletedTask;
    }
}
