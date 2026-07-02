using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Common.Config;

namespace TaxOmbud.Infrastructure.EmailServices;

public class SmtpEmailService : TaxOmbud.Application.Interfaces.InfrastructureService.IEmailService, TaxOmbud.Application.Common.Interfaces.IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        using var client = BuildClient();
        var message = new MailMessage
        {
            From       = new MailAddress(_options.FromAddress, _options.FromName),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To} subject: {Subject}", to, subject);
    }

    public async Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken cancellationToken = default)
    {
        var body = $"<p>Template: {templateName}</p><pre>{System.Text.Json.JsonSerializer.Serialize(model)}</pre>";
        await SendAsync(to, templateName, body, cancellationToken);
    }

    private SmtpClient BuildClient() => new(_options.Host, _options.Port)
    {
        EnableSsl   = _options.UseSsl,
        Credentials = new NetworkCredential(_options.Username, _options.Password)
    };
}
