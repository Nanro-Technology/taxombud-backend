using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Complaints.Commands.DeleteComplaint;

// ─── Command ─────────────────────────────────────────────────────────────────

public record DeleteComplaintCommand(Guid Id) : IRequest<Result<object?>>;

// ─── Handler ─────────────────────────────────────────────────────────────────

public class DeleteComplaintCommandHandler : IRequestHandler<DeleteComplaintCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public DeleteComplaintCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(DeleteComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (complaint is null)
            return Result<object?>.NotFound($"Complaint '{request.Id}' was not found.");

        if (complaint.Status != ComplaintStatus.Draft)
            return Result<object?>.Failure("Only draft complaints can be deleted.");

        _context.Complaints.Remove(complaint);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
