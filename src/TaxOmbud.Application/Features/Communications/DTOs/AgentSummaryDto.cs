using System;

namespace TaxOmbud.Application.Features.Communications.DTOs;

public class AgentSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}
