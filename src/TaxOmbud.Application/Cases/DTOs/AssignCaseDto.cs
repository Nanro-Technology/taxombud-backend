using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.ValueObjects;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record AssignCaseCommand(Guid CaseId, Guid OfficerId) ;