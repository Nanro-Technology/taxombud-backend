using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Hr;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.HrRequests.Commands.SubmitLoanRequest;

public record SubmitLoanRequestCommands(
    Guid StaffId, 
    decimal Amount, 
    int RepaymentMonths,
    string Purpose,
    string? DisburseTo,
    string? PayoutReference,
    string? ActionNote
) : IRequest<Result<Guid>>;

public class SubmitLoanRequestCommandsHandler : IRequestHandler<SubmitLoanRequestCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public SubmitLoanRequestCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(SubmitLoanRequestCommands request, CancellationToken cancellationToken)
    {
        var entity = new LoanRequest
        {
            Id = Guid.NewGuid(),
            UserId = request.StaffId,
            Amount = request.Amount,
            TermMonths = request.RepaymentMonths,
            Purpose = request.Purpose,
            DisburseTo = request.DisburseTo,
            PayoutReference = request.PayoutReference,
            ActionNote = request.ActionNote,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.LoanRequests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}