using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Appeals.Commands.FileAppeal;

// ─── Command ─────────────────────────────────────────────────────────────────

public record FileAppealCommand(Guid CaseId, string Reason) : IRequest<Result<FileAppealResponse>>;

public record FileAppealResponse(Guid Id, Guid CaseId, string Reason, DateTimeOffset CreatedAt);

// ─── Validator ────────────────────────────────────────────────────────────────

public class FileAppealCommandValidator : AbstractValidator<FileAppealCommand>
{
    public FileAppealCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class FileAppealCommandHandler : IRequestHandler<FileAppealCommand, Result<FileAppealResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public FileAppealCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FileAppealResponse>> Handle(FileAppealCommand request, CancellationToken cancellationToken)
    {
        var kase = await _context.Cases.FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (kase == null)
            return Result<FileAppealResponse>.Failure("Associated case not found.");

        if (kase.Status != CaseStatus.Closed)
            return Result<FileAppealResponse>.Failure("Appeals can only be filed against closed cases.");

        var actorUserId = _currentUser.UserId ?? Guid.Empty;
        var appeal = new Appeal(request.CaseId, request.Reason);
        appeal.Submit(actorUserId);

        _context.Appeals.Add(appeal);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<FileAppealResponse>.Success(new FileAppealResponse(
            appeal.Id,
            appeal.CaseId,
            appeal.Reason,
            appeal.CreatedAt
        ));
    }
}
