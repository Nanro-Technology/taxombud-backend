using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Taxpayers.DTOs;

public record VerifyTaxpayerCommand(Guid TaxpayerId, bool IsVerified) ;

public record VerifyTaxpayerRequest(bool IsVerified);