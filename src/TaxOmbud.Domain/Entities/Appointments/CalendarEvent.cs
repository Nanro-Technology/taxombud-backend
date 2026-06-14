using System;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Appointments;

public class CalendarEvent : BaseAuditableEntity
{
    public string Title { get; set; } = null!;
    public string EventType { get; set; } = "Meeting"; // Meeting, Reminder, OutOfOffice
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public string Status { get; set; } = "Scheduled";
    
    public Guid OwnerId { get; set; }
    public Guid? DepartmentId { get; set; }
    
    public string? Location { get; set; }
    public string? Notes { get; set; }
    
    public bool IsPublic { get; set; } = false;
    public string? ReminderMinutes { get; set; } // comma separated e.g. "15,60"
    
    public string? AttendeesList { get; set; } // JSON of attendees
}
