using System;
using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Cases.Validators;

public class PostRecommendationCommandValidator : AbstractValidator<PostRecommendationCommand>
{
    public PostRecommendationCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.RecommendationText).NotEmpty().MaximumLength(4000);
    }
}