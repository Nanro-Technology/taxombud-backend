using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record LinkComplaintsCommand(
    Guid SourceComplaintId,
    Guid TargetComplaintId,
    string? LinkType
) ;

public record LinkComplaintRequest(Guid TargetComplaintId, string? LinkType);