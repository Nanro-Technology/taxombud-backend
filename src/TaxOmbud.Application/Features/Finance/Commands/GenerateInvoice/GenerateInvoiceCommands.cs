using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.GenerateInvoice;

public record GenerateInvoiceCommands(string InvoiceNumber, decimal TotalAmount) : IRequest<Result<Guid>>;

public class GenerateInvoiceCommandsHandler : IRequestHandler<GenerateInvoiceCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public GenerateInvoiceCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommands request, CancellationToken cancellationToken)
    {
        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = request.InvoiceNumber,
            TotalAmount = request.TotalAmount,
            Status = "Unpaid",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}