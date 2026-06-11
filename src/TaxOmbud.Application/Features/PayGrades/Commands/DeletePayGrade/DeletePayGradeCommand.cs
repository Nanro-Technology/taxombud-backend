using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.PayGrades.Commands.DeletePayGrade;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeletePayGradeCommand(Guid Id) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class DeletePayGradeCommandValidator : AbstractValidator<DeletePayGradeCommand>
{
    public DeletePayGradeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeletePayGradeCommandHandler : IRequestHandler<DeletePayGradeCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeletePayGradeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeletePayGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _context.PayGrades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade == null)
            return Result<Unit>.NotFound("Pay grade not found.");

        _context.PayGrades.Remove(grade);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
