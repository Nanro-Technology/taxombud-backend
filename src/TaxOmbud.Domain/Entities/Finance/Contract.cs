using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Domain.Entities.Finance;

public class Contract : BaseAuditableEntity
{
    public string? ContractNumber { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; } = "Draft"; // Draft, Active, Expired, Terminated
    
    public Guid? SourceQuoteId { get; set; }
    public Guid? AssignedAgentId { get; set; }
    
    public string? ParentType { get; set; } // Account, Organization
    public Guid? ParentId { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    
    public int ReminderCycleDays { get; set; } // e.g., 7, 14, 30
    public string? Notes { get; set; }
}

public class ContractReview : BaseAuditableEntity
{
    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    
    public Guid? ReviewTicketId { get; set; }
    public Guid? ReviewDepartmentId { get; set; }
    
    public string? Notes { get; set; }
}
