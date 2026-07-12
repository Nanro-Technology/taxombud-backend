using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Finance;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Finance.DTOs;

public record CreateContractCommands(
    string ContractNumber,
    string Title,
    string Status,
    Guid? SourceQuoteId,
    Guid? AssignedAgentId,
    string? ParentType,
    Guid? ParentId,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? RenewalDate,
    int ReminderCycleDays,
    string? Notes
);

public record UpdateContractCommand(
    Guid Id,
    string Title,
    string Status,
    Guid? AssignedAgentId,
    string? ParentType,
    Guid? ParentId,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? RenewalDate,
    int ReminderCycleDays,
    string? Notes
);

public record DeleteContractCommand(Guid Id);
