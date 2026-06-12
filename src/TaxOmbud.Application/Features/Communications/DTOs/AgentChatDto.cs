using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.Communications.DTOs;

public class AgentChatDto
{
    public Guid Id { get; set; }
    public string? Topic { get; set; }
    public bool IsGroupChat { get; set; }
    public List<AgentSummaryDto> Participants { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
