using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.UpdateCaseFinding;

public record UpdateCaseFindingCommand(Guid CaseId, Guid FindingId, string Description) : IRequest<Result<object?>>;

public class UpdateCaseFindingCommandValidator : AbstractValidator<UpdateCaseFindingCommand>
{
    public UpdateCaseFindingCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.FindingId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class UpdateCaseFindingCommandHandler : IRequestHandler<UpdateCaseFindingCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCaseFindingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(UpdateCaseFindingCommand request, CancellationToken cancellationToken)
    {
        var @case = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (@case is null)
            return Result<object?>.NotFound($"Case '{request.CaseId}' was not found.");

        if (@case.Status == CaseStatus.Closed)
            return Result<object?>.Failure("Cannot modify findings on a closed case.");

        var finding = await _context.CaseFindings
            .FirstOrDefaultAsync(f => f.Id == request.FindingId && f.CaseId == request.CaseId, cancellationToken);

        if (finding is null)
            return Result<object?>.NotFound($"Finding '{request.FindingId}' was not found on Case '{request.CaseId}'.");

        finding.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
