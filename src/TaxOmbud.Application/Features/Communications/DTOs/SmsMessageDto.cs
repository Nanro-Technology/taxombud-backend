using System;

namespace TaxOmbud.Application.Features.Communications.DTOs;

public class SmsMessageDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public string? SenderId { get; set; }
    public string Body { get; set; } = null!;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string RecipientType { get; set; } = null!;
    public string? PhoneNumbers { get; set; }
    public string Mode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Direction { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
