using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Application.Interfaces.Persistence;
using TaxOmbud.Common.Config;

namespace TaxOmbud.Infrastructure.EmailServices;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<SmtpOptions> options, 
        IApplicationDbContext dbContext,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    private async Task<SmtpOptions> GetEffectiveOptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _dbContext.SystemSettings
                .Where(s => s.Key.StartsWith("Smtp:"))
                .ToListAsync(cancellationToken);

            if (settings.Count > 0)
            {
                var host = settings.FirstOrDefault(s => s.Key == "Smtp:Host")?.Value;
                var portStr = settings.FirstOrDefault(s => s.Key == "Smtp:Port")?.Value;
                var sslStr = settings.FirstOrDefault(s => s.Key == "Smtp:UseSsl")?.Value;
                var user = settings.FirstOrDefault(s => s.Key == "Smtp:Username")?.Value;
                var pass = settings.FirstOrDefault(s => s.Key == "Smtp:Password")?.Value;
                var from = settings.FirstOrDefault(s => s.Key == "Smtp:FromAddress")?.Value;
                var name = settings.FirstOrDefault(s => s.Key == "Smtp:FromName")?.Value;

                return new SmtpOptions
                {
                    Host = !string.IsNullOrWhiteSpace(host) ? host : _options.Host,
                    Port = int.TryParse(portStr, out var p) ? p : _options.Port,
                    UseSsl = bool.TryParse(sslStr, out var s) ? s : _options.UseSsl,
                    Username = !string.IsNullOrWhiteSpace(user) ? user : _options.Username,
                    Password = !string.IsNullOrWhiteSpace(pass) ? pass : _options.Password,
                    FromAddress = !string.IsNullOrWhiteSpace(from) ? from : _options.FromAddress,
                    FromName = !string.IsNullOrWhiteSpace(name) ? name : _options.FromName,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load SMTP settings from database. Falling back to appsettings.json configuration.");
        }

        return _options;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveOpts = await GetEffectiveOptionsAsync(cancellationToken);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(effectiveOpts.FromName, effectiveOpts.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Determine SecureSocketOptions based on port:
            // Port 465 = implicit SSL (SslOnConnect), Port 587/25 = STARTTLS (StartTls / Auto)
            var socketOptions = effectiveOpts.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : effectiveOpts.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(effectiveOpts.Host, effectiveOpts.Port, socketOptions, cancellationToken);
            await client.AuthenticateAsync(effectiveOpts.Username, effectiveOpts.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To} — subject: {Subject}", to, subject);
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
