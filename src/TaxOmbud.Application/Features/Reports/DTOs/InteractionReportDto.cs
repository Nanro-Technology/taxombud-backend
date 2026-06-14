using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Reports.DTOs;

public class InteractionReportDto
{
    public int TotalInteractions { get; set; }
    
    // Channel distribution (Email, Call, SMS, Portal, etc.)
    public Dictionary<string, int> InteractionsByChannel { get; set; } = new();
    
    public Dictionary<string, int> InteractionsByDirection { get; set; } = new(); // Inbound, Outbound
}
