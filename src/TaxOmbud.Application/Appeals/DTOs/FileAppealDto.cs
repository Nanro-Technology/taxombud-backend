using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appeals.DTOs;

public record FileAppealCommand(Guid CaseId, string Reason) ;

public record FileAppealResponse(Guid Id, Guid CaseId, string Reason, DateTimeOffset CreatedAt);