using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.DTOs.Contact;
using Portfolio.Services;

namespace Portfolio.Tests.Services;

public sealed class ContactEmailServiceTests
{
    private static ContactFormDto SampleForm() => new()
    {
        Name = "Mario Rossi",
        Email = "mario@example.com",
        Message = "Messaggio valido per il controllo."
    };

    [Fact]
    public async Task SendAsync_HostMancante_ProduceErroreChiaro()
    {
        var configuration = new ConfigurationBuilder().Build();
        var service = new ContactEmailService(
            configuration,
            NullLogger<ContactEmailService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SendAsync(SampleForm()));

        Assert.Contains("Email:Host", exception.Message);
    }

    [Fact]
    public async Task SendAsync_PortaNonSupportata_NonTentaConnessione()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:Host"] = "smtp.example.invalid",
                ["Email:Username"] = "service@example.invalid",
                ["Email:Password"] = "fake-password",
                ["Email:From"] = "service@example.invalid",
                ["Email:Recipient"] = "service@example.invalid",
                ["Email:Port"] = "2525"
            })
            .Build();

        var service = new ContactEmailService(
            configuration,
            NullLogger<ContactEmailService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SendAsync(SampleForm()));

        Assert.Contains("Porta SMTP non supportata", exception.Message);
    }
}
