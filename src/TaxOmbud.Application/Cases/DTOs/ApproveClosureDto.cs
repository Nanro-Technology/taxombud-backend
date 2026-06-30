using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record ApproveClosureCommand(Guid CaseId, bool Approve, string Rationale) ;

public record ApproveClosureRequest(bool Approve, string Rationale);