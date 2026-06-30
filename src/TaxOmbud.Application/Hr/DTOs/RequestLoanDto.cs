using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record RequestLoanCommand(decimal Amount, int TermMonths, string Purpose) ;

public record RequestLoanRequest(decimal Amount, int TermMonths, string Purpose);