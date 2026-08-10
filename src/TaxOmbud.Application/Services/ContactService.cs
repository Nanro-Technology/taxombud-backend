using TaxOmbud.Application.Contact.DTOs;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Application.Interfaces.InfrastructureService;
using TaxOmbud.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace TaxOmbud.Application.Services;


public class ContactService : IContactService
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ContactService> logger)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Response<string>> SubmitContactFormAsync(SubmitContactFormCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<string>();
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Message))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Name, email, and message are required fields.";
                return response;
            }

            var baseUrl = Helper.GetAppBaseUrl(_configuration);

            // Send acknowledgment receipt to user
            var userHtml = $"""
                <div style="font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;">
                  <div style="background:#114a31;padding:24px 32px;text-align:center;border-bottom:4px solid #c9a227;">
                    <h1 style="color:#ffffff;font-size:1.1rem;margin:0;text-transform:uppercase;">OFFICE OF THE TAX OMBUD</h1>
                    <p style="color:rgba(255,255,255,.75);font-size:.8rem;margin:4px 0 0;">Federal Republic of Nigeria</p>
                  </div>
                  <div style="padding:28px 32px;background:#ffffff;color:#333333;font-size:.95rem;line-height:1.7;">
                    <h2 style="color:#114a31;font-size:1.15rem;margin-top:0;">Thank You for Contacting Us</h2>
                    <p>Dear <strong>{request.Name}</strong>,</p>
                    <p>Thank you for reaching out to the Tax Ombud Office. We have received your inquiry regarding <strong>"{request.Subject}"</strong>.</p>
                    <p>Our desk officers will review your message and respond within 1-2 business days.</p>
                    <div style="background:#f8f9fa;border-left:4px solid #114a31;padding:12px 16px;margin:20px 0;font-size:.9rem;">
                      <p style="margin:0;"><strong>Your Message:</strong> {request.Message}</p>
                    </div>
                  </div>
                  <div style="background:#114a31;padding:16px 32px;text-align:center;">
                    <p style="color:#c9a227;font-size:.85rem;font-weight:bold;margin:0;">Office of the Tax Ombud</p>
                  </div>
                </div>
                """;

            try
            {
                await _emailService.SendAsync(request.Email, "Thank You for Contacting Tax Ombud", userHtml, cancellationToken);

                // Send admin alert copy
                var adminHtml = $"""
                    <div style="font-family:'Segoe UI',sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;padding:24px;">
                      <h3 style="color:#114a31;margin-top:0;">New Public Contact Inquiry</h3>
                      <p><strong>From:</strong> {request.Name} ({request.Email})</p>
                      <p><strong>Phone:</strong> {request.Phone ?? "N/A"}</p>
                      <p><strong>Subject:</strong> {request.Subject}</p>
                      <hr style="border:0;border-top:1px solid #eee;margin:16px 0;" />
                      <p><strong>Message:</strong></p>
                      <p>{request.Message}</p>
                    </div>
                    """;

                await _emailService.SendAsync("info@mediate.com.ng", $"[Inquiry] {request.Subject}", adminHtml, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact emails for {Email}", request.Email);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Your message has been received. A confirmation email has been dispatched to your inbox.";
            response.Data = "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process contact form submission from {Email}", request.Email);
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while submitting your message. Please try again.";
        }
        return response;
    }
}

