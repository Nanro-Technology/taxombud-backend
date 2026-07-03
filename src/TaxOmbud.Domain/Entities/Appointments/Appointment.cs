using System;
using TaxOmbud.Domain.Common;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Domain.Entities.Appointments;

public class Appointment : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public Guid? OfficerId { get; set; }
    public Officer? Officer { get; set; }

    public Guid? TaxpayerId { get; set; }
    public Taxpayer? Taxpayer { get; set; }

    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
}
