using System;
using FluentValidation;
using TaxOmbud.Application.Taxpayers.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Taxpayers.Validators;

public class VerifyTaxpayerCommandValidator : AbstractValidator<VerifyTaxpayerCommand>
{
    public VerifyTaxpayerCommandValidator()
    {
        RuleFor(x => x.TaxpayerId).NotEmpty();
    }
}