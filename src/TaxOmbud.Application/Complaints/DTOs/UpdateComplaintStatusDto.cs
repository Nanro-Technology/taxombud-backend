using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record UpdateComplaintStatusCommand(
    Guid ComplaintId,
    ComplaintStatus Status,
    string? Reason
) ;

public record UpdateComplaintStatusRequest(ComplaintStatus Status, string? Reason);