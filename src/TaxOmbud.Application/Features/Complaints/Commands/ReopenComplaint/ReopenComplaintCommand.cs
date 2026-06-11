using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;

namespace TaxOmbud.Application.Features.Complaints.Commands.ReopenComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record ReopenComplaintCommand(Guid ComplaintId, Guid ReopenedByUserId) : IRequest<Result<object?>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class ReopenComplaintCommandHandler : IRequestHandler<ReopenComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public ReopenComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(ReopenComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.ComplaintId}' was not found.");

        try
        {
            complaint.Reopen(request.ReopenedByUserId);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<object?>.Success(null);
        }
        catch (TaxOmbud.Domain.Exceptions.DomainException ex)
        {
            return Result<object?>.Failure(ex.Message);
        }
    }
}
