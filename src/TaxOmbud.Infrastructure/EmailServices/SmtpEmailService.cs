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

                var effectiveUser = !string.IsNullOrWhiteSpace(user) ? user.Trim() : _options.Username;
                var effectiveFrom = (!string.IsNullOrWhiteSpace(from) && !from.StartsWith("no-reply") && from.Contains("@")) ? from.Trim() : effectiveUser;

                return new SmtpOptions
                {
                    Host = !string.IsNullOrWhiteSpace(host) ? host.Trim() : _options.Host,
                    Port = int.TryParse(portStr, out var p) ? p : _options.Port,
                    UseSsl = bool.TryParse(sslStr, out var s) ? s : _options.UseSsl,
                    Username = effectiveUser,
                    Password = !string.IsNullOrWhiteSpace(pass) ? pass : _options.Password,
                    FromAddress = effectiveFrom,
                    FromName = !string.IsNullOrWhiteSpace(name) ? name.Trim() : _options.FromName,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load SMTP settings from database. Falling back to appsettings.json configuration.");
        }

        return _options;
    }

    private async Task SendWithConfigAsync(SmtpOptions opts, string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        
        // cPanel Exim requires the From header to match a valid mailbox on the server.
        // If FromAddress is missing, invalid, or "no-reply", use the authenticated Username.
        var fromEmail = (!string.IsNullOrWhiteSpace(opts.FromAddress) && opts.FromAddress.Contains("@") && !opts.FromAddress.StartsWith("no-reply"))
            ? opts.FromAddress.Trim()
            : opts.Username.Trim();

        var fromName = !string.IsNullOrWhiteSpace(opts.FromName) ? opts.FromName.Trim() : "Tax Ombud System";

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        // cPanel port 465 = SslOnConnect (implicit SSL). Port 587/25 = STARTTLS / Auto.
        var socketOptions = opts.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : opts.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

        await client.ConnectAsync(opts.Host.Trim(), opts.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(opts.Username.Trim(), opts.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var effectiveOpts = await GetEffectiveOptionsAsync(cancellationToken);
        try
        {
            await SendWithConfigAsync(effectiveOpts, to, subject, htmlBody, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To} — subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email using primary DB SMTP configuration ({User}). Attempting fallback to appsettings config ({FallbackUser})...", effectiveOpts.Username, _options.Username);
            
            if (effectiveOpts.Username != _options.Username || effectiveOpts.Host != _options.Host || effectiveOpts.Password != _options.Password || effectiveOpts.FromAddress != _options.FromAddress)
            {
                try
                {
                    await SendWithConfigAsync(_options, to, subject, htmlBody, cancellationToken);
                    _logger.LogInformation("Email sent successfully to {To} via fallback appsettings config — subject: {Subject}", to, subject);
                    return;
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback SMTP send also failed for {To}", to);
                }
            }
            throw;
        }
    }

    public async Task SendTemplatedAsync(string to, string templateName, object model, CancellationToken cancellationToken = default)
    {
        var body = $"<p>Template: {templateName}</p><pre>{System.Text.Json.JsonSerializer.Serialize(model)}</pre>";
        await SendAsync(to, templateName, body, cancellationToken);
    }
}
