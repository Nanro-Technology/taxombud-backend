using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record CloseComplaintCommand(Guid ComplaintId, string Reason, Guid ClosedByUserId)
    ;

public record CloseComplaintRequest(string Reason);
