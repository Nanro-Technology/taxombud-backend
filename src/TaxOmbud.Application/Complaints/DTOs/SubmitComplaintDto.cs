using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Common.Utilities;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record SubmitComplaintCommand(
    Guid TaxpayerId,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string Subject,
    string Description,
    string? TaxOfficeRef,
    string? TinNumber
) ;

public record SubmitComplaintResponse(Guid ComplaintId, string ReferenceNumber, string Status);
