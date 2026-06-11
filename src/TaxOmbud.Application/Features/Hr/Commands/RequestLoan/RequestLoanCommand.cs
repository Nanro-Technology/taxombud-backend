using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Entities.Hr;

namespace TaxOmbud.Application.Features.Hr.Commands.RequestLoan;

// ─── Command ─────────────────────────────────────────────────────────────────

public record RequestLoanCommand(decimal Amount, int TermMonths, string Purpose) : IRequest<Result<LoanRequest>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class RequestLoanCommandValidator : AbstractValidator<RequestLoanCommand>
{
    public RequestLoanCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Loan amount must be greater than zero.");
        RuleFor(x => x.TermMonths).GreaterThan(0).WithMessage("Term in months must be greater than zero.");
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(500);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class RequestLoanCommandHandler : IRequestHandler<RequestLoanCommand, Result<LoanRequest>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public RequestLoanCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LoanRequest>> Handle(RequestLoanCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? Guid.Empty;

        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Amount = request.Amount,
            TermMonths = request.TermMonths,
            Purpose = request.Purpose,
            Status = "pending"
        };

        _context.LoanRequests.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<LoanRequest>.Success(loan);
    }
}
