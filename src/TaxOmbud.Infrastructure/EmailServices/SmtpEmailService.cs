using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Common.Config;

namespace TaxOmbud.Infrastructure.EmailServices;

public class SmtpEmailService : IEmailService
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
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Determine SecureSocketOptions based on port:
            // Port 465 = implicit SSL (SslOnConnect), Port 587/25 = STARTTLS (StartTls / Auto)
            var socketOptions = _options.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : _options.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {To} — subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} — subject: {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken cancellationToken = default)
    {
        var body = $"<p>Template: {templateName}</p><pre>{System.Text.Json.JsonSerializer.Serialize(model)}</pre>";
        await SendAsync(to, templateName, body, cancellationToken);
    }
}
