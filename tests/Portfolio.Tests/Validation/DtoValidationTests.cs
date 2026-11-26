using System.ComponentModel.DataAnnotations;
using Portfolio.DTOs.Contact;
using Portfolio.DTOs.Projects;

namespace Portfolio.Tests.Validation;

public sealed class DtoValidationTests
{
    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            value,
            new ValidationContext(value),
            results,
            validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ContactForm_DatiValidi_NessunErrore()
    {
        var form = new ContactFormDto
        {
            Name = "Mario Rossi",
            Email = "mario@example.com",
            Message = "Vorrei conoscere il tuo portfolio."
        };

        Assert.Empty(Validate(form));
    }

    [Fact]
    public void ContactForm_WebsiteVuotoONull_NonEObbligatorio()
    {
        var form = new ContactFormDto
        {
            Name = "Mario Rossi",
            Email = "mario@example.com",
            Message = "Messaggio valido per il test.",
            Website = null
        };

        Assert.Empty(Validate(form));
        form.Website = "";
        Assert.Empty(Validate(form));
    }

    [Theory]
    [InlineData("non-una-email")]
    [InlineData("")]
    public void ContactForm_EmailNonValida_GeneraErrore(string email)
    {
        var form = new ContactFormDto
        {
            Name = "Mario Rossi",
            Email = email,
            Message = "Messaggio valido per il test."
        };
        Assert.NotEmpty(Validate(form));
    }

    [Fact]
    public void ContactForm_MessaggioTroppoCorto_GeneraErrore()
    {
        var form = new ContactFormDto
        {
            Name = "Mario Rossi",
            Email = "mario@example.com",
            Message = "Ciao"
        };
        Assert.NotEmpty(Validate(form));
    }

    [Fact]
    public void CreateProject_NomeObbligatorio_GeneraErrore()
    {
        var dto = new CreateProjectDto
        {
            Name = "",
            ShortDescription = "Descrizione presente"
        };
        Assert.NotEmpty(Validate(dto));
    }

    [Fact]
    public void CreateProject_DescrizioneBreveTroppoLunga_GeneraErrore()
    {
        var dto = new CreateProjectDto
        {
            Name = "Portfolio",
            ShortDescription = new string('A', 201)
        };
        Assert.NotEmpty(Validate(dto));
    }

    [Fact]
    public void UpdateProject_UrlGitHubNonValido_GeneraErrore()
    {
        var dto = new UpdateProjectDto
        {
            Name = "Portfolio",
            ShortDescription = "Aggiornamento",
            GithubLink = "non e un url"
        };
        Assert.NotEmpty(Validate(dto));
    }
}
