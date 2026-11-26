using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Portfolio.DTOs.Contact;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services;

public sealed class ContactEmailService : IContactEmailService
{
    private readonly IConfiguration _configuration;

    public ContactEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(ContactFormDto form, CancellationToken cancellationToken = default)
    {
        string GetRequired(string key) =>
            _configuration[key] is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException($"Configurazione mancante: {key}");

        var host = GetRequired("Email:Host");
        var username = GetRequired("Email:Username");
        var password = GetRequired("Email:Password");
        var from = GetRequired("Email:From");
        var recipient = GetRequired("Email:Recipient");
        var port = _configuration.GetValue<int?>("Email:Port") ?? 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Portfolio - Contatti", from));
        message.To.Add(MailboxAddress.Parse(recipient));

        // Importante: il mittente SMTP rimane il proprietario dell'account.
        // L'indirizzo del visitatore va in Reply-To, non in From.
        message.ReplyTo.Add(new MailboxAddress(form.Name, form.Email));
        message.Subject = "Nuovo messaggio dal Portfolio";
        message.Body = new TextPart("plain")
        {
            Text = $"Nome: {form.Name}\nEmail: {form.Email}\n\nMessaggio:\n{form.Message}"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(username, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
