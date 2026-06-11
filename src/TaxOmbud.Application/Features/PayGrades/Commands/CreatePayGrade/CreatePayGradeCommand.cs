using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.PayGrades.Commands.CreatePayGrade;

// ─── Command ─────────────────────────────────────────────────────────────────

public record CreatePayGradeCommand(
    string Name,
    int Level,
    string BasicSalaryBand
) : IRequest<Result<CreatedPayGradeResponse>>;

public record CreatedPayGradeResponse(
    Guid Id,
    string Name,
    int Level
);

// ─── Validator ────────────────────────────────────────────────────────────────

public class CreatePayGradeCommandValidator : AbstractValidator<CreatePayGradeCommand>
{
    public CreatePayGradeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).GreaterThanOrEqualTo(0).WithMessage("Pay grade level must be greater than or equal to zero.");
        RuleFor(x => x.BasicSalaryBand).NotEmpty().MaximumLength(100);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class CreatePayGradeCommandHandler : IRequestHandler<CreatePayGradeCommand, Result<CreatedPayGradeResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreatePayGradeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreatedPayGradeResponse>> Handle(CreatePayGradeCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await _context.PayGrades.AnyAsync(g => g.Level == request.Level, cancellationToken);
        if (duplicate)
            return Result<CreatedPayGradeResponse>.Failure($"A pay grade at level {request.Level} already exists.");

        var grade = new PayGrade
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Level = request.Level,
            BasicSalaryBand = request.BasicSalaryBand
        };

        _context.PayGrades.Add(grade);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreatedPayGradeResponse(grade.Id, grade.Name, grade.Level);
        return Result<CreatedPayGradeResponse>.Success(response);
    }
}
