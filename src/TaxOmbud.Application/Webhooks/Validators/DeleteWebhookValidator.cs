using System;
using FluentValidation;
using TaxOmbud.Application.Webhooks.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaxOmbud.Application.Webhooks.Validators;

public class DeleteWebhookCommandValidator : AbstractValidator<DeleteWebhookCommand>
{
    public DeleteWebhookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}