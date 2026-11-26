
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using Portfolio.DTOs.Contact;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services;

public sealed class ContactEmailService : IContactEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactEmailService> _logger;

    public ContactEmailService(
        IConfiguration configuration,
        ILogger<ContactEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(
        ContactFormDto form,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(form);

        // ==========================================
        // 1. CONFIGURAZIONE SMTP
        // ==========================================

        string GetRequired(string key)
        {
            string? value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Configurazione mancante: {key}");
            }

            return value;
        }

        string host = GetRequired("Email:Host");
        string username = GetRequired("Email:Username");
        string password = GetRequired("Email:Password");
        string from = GetRequired("Email:From");
        string recipient = GetRequired("Email:Recipient");

        int port = _configuration.GetValue<int?>("Email:Port") ?? 587;

        // 465 = SSL/TLS immediato
        // 587 = STARTTLS
        SecureSocketOptions security = port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,

            _ => throw new InvalidOperationException(
                "Porta SMTP non supportata. Usa 465 o 587.")
        };

        // ==========================================
        // 2. CREAZIONE EMAIL
        // ==========================================

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                "Portfolio - Contatti",
                from));

        message.To.Add(
            MailboxAddress.Parse(recipient));

        // Consente di rispondere al visitatore
        message.ReplyTo.Add(
            new MailboxAddress(
                form.Name,
                form.Email));

        message.Subject = "Nuovo messaggio dal Portfolio";

        message.Body = new TextPart("plain")
        {
            Text =
                $"NUOVO MESSAGGIO DAL PORTFOLIO\n\n" +
                $"Nome: {form.Name}\n" +
                $"Email: {form.Email}\n\n" +
                $"Messaggio:\n{form.Message}"
        };

        // ==========================================
        // 3. CONNESSIONE SMTP
        // ==========================================

        using var client = new SmtpClient();

        try
        {
            _logger.LogInformation(
                "Connessione al server SMTP sulla porta {Port}",
                port);

            await client.ConnectAsync(
                host,
                port,
                security,
                cancellationToken);

            _logger.LogInformation(
                "Connessione SMTP stabilita.");

            // Evita di tentare l'autenticazione OAuth
            // quando usiamo una password per le app.
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.AuthenticationMechanisms.Remove("OAUTHBEARER");

            // ======================================
            // 4. AUTENTICAZIONE
            // ======================================

            _logger.LogInformation(
                "Avvio autenticazione SMTP.");

            await client.AuthenticateAsync(
                username,
                password,
                cancellationToken);

            _logger.LogInformation(
                "Autenticazione SMTP completata.");

            // ======================================
            // 5. INVIO
            // ======================================

            await client.SendAsync(
                message,
                cancellationToken);

            _logger.LogInformation(
                "Email del Portfolio inviata.");

            // ======================================
            // 6. DISCONNESSIONE
            // ======================================

            await client.DisconnectAsync(
                true,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Errore durante la comunicazione SMTP.");

            throw;
        }
    }
}
