using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record UpdateComplaintCommand(
    Guid Id,
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Priority
) ;

public record UpdateComplaintRequest(
    string Subject,
    string Description,
    string TaxType,
    string TaxPeriod,
    string ComplaintCategory,
    string? TaxOfficeRef,
    string? TinNumber,
    string Priority
);