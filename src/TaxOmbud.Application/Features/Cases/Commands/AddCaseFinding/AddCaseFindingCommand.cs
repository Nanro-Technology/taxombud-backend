using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Cases.Commands.AddCaseFinding;

public record AddCaseFindingCommand(Guid CaseId, string Description) : IRequest<Result<Guid>>;

public class AddCaseFindingCommandValidator : AbstractValidator<AddCaseFindingCommand>
{
    public AddCaseFindingCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class AddCaseFindingCommandHandler : IRequestHandler<AddCaseFindingCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddCaseFindingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddCaseFindingCommand request, CancellationToken cancellationToken)
    {
        var @case = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (@case is null)
            return Result<Guid>.NotFound($"Case '{request.CaseId}' was not found.");

        if (@case.Status == CaseStatus.Closed)
            return Result<Guid>.Failure("Cannot add findings to a closed case.");

        var finding = new CaseFinding
        {
            Id = Guid.NewGuid(),
            CaseId = request.CaseId,
            Description = request.Description
        };

        _context.CaseFindings.Add(finding);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(finding.Id);
    }
}
