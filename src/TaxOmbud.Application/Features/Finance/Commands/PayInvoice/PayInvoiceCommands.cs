using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.PayInvoice;

public record PayInvoiceCommands(Guid InvoiceId) : IRequest<Result<bool>>;

public class PayInvoiceCommandsHandler : IRequestHandler<PayInvoiceCommands, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public PayInvoiceCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(PayInvoiceCommands request, CancellationToken cancellationToken)
    {
        var entity = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken);
        if(entity == null) return Result<bool>.NotFound($"Invoice {request.InvoiceId} not found.");
        
        entity.Status = "Paid";
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}