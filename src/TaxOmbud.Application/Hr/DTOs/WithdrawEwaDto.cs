using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record WithdrawEwaCommand(decimal Amount) ;

public record EwaWithdrawalResponse(string Message, decimal Amount);

public record EwaWithdrawalRequest(decimal Amount);