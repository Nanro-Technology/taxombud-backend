using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.PayGrades.Commands.UpdatePayGrade;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdatePayGradeCommand(
    Guid Id,
    string Name,
    int Level,
    string BasicSalaryBand
) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdatePayGradeCommandValidator : AbstractValidator<UpdatePayGradeCommand>
{
    public UpdatePayGradeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).GreaterThanOrEqualTo(0).WithMessage("Pay grade level must be greater than or equal to zero.");
        RuleFor(x => x.BasicSalaryBand).NotEmpty().MaximumLength(100);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdatePayGradeCommandHandler : IRequestHandler<UpdatePayGradeCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePayGradeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdatePayGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _context.PayGrades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade == null)
            return Result<Unit>.NotFound("Pay grade not found.");

        grade.Name = request.Name;
        grade.Level = request.Level;
        grade.BasicSalaryBand = request.BasicSalaryBand;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
