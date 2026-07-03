using FluentValidation;
using TaxOmbud.Application.Cases.DTOs;

namespace TaxOmbud.Application.Cases.Validators;

public class PostRecommendationCommandValidator : AbstractValidator<PostRecommendationCommand>
{
    public PostRecommendationCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.RecommendationText).NotEmpty().MaximumLength(4000);
    }
}
