using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.PostRecommendation;

// ─── Command ─────────────────────────────────────────────────────────────────

public record PostRecommendationCommand(Guid CaseId, string RecommendationText) : IRequest<Result<PostRecommendationResponse>>;

public record PostRecommendationResponse(Guid Id, string RecommendationText, DateTimeOffset CreatedAt);

// ─── Validator ────────────────────────────────────────────────────────────────

public class PostRecommendationCommandValidator : AbstractValidator<PostRecommendationCommand>
{
    public PostRecommendationCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.RecommendationText).NotEmpty().MaximumLength(4000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class PostRecommendationCommandHandler : IRequestHandler<PostRecommendationCommand, Result<PostRecommendationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public PostRecommendationCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PostRecommendationResponse>> Handle(PostRecommendationCommand request, CancellationToken cancellationToken)
    {
        var kase = await _context.Cases.FirstOrDefaultAsync(c => c.ComplaintId == request.CaseId, cancellationToken);
        if (kase == null)
            return Result<PostRecommendationResponse>.NotFound("Case not found.");

        var actorUserId = _currentUser.UserId ?? Guid.Empty;
        var rec = new CaseRecommendation
        {
            Id = Guid.NewGuid(),
            CaseId = kase.Id,
            RecommendationText = request.RecommendationText,
            ApprovedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Cases.Entry(kase).Collection(c => c.Recommendations).Query().Cast<CaseRecommendation>().ToList();
        kase.Recommendations.Add(rec);
        kase.UpdateStatus(CaseStatus.UnderReview, "approval", actorUserId);

        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (complaint != null)
        {
            complaint.UpdateStage("approval");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<PostRecommendationResponse>.Success(new PostRecommendationResponse(rec.Id, rec.RecommendationText, rec.CreatedAt));
    }
}
