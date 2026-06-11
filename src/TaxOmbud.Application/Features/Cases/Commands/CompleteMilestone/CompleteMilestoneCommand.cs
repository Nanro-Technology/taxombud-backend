using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.CompleteMilestone;

public record CompleteMilestoneCommand(Guid CaseId, Guid MilestoneId) : IRequest<Result<object?>>;

public class CompleteMilestoneCommandValidator : AbstractValidator<CompleteMilestoneCommand>
{
    public CompleteMilestoneCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.MilestoneId).NotEmpty();
    }
}

public class CompleteMilestoneCommandHandler : IRequestHandler<CompleteMilestoneCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public CompleteMilestoneCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(CompleteMilestoneCommand request, CancellationToken cancellationToken)
    {
        var @case = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (@case is null)
            return Result<object?>.NotFound($"Case '{request.CaseId}' was not found.");

        if (@case.Status == CaseStatus.Closed)
            return Result<object?>.Failure("Cannot modify milestones on a closed case.");

        var milestone = await _context.CaseMilestones
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId && m.CaseId == request.CaseId, cancellationToken);

        if (milestone is null)
            return Result<object?>.NotFound($"Milestone '{request.MilestoneId}' was not found on Case '{request.CaseId}'.");

        if (milestone.IsCompleted)
            return Result<object?>.Success(null); // Already completed

        milestone.IsCompleted = true;
        milestone.CompletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
