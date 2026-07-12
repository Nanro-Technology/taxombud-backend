using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Hr;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.HrRequests.DTOs;

public record GetLoanRequestsQueries { public bool? IsSalaryAdvance { get; init; } }
