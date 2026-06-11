using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.GenerateInvoice;

public record InvoiceItemDto(string ItemName, string Description, decimal Quantity, decimal UnitPrice);

public record GenerateInvoiceCommands(
    string InvoiceTitle,
    string Currency,
    string ParentType,
    Guid? AccountId,
    Guid? ContractId,
    DateTime IssuedDate,
    DateTime DueDate,
    decimal TaxAmount,
    decimal DiscountAmount,
    string Notes,
    global::System.Collections.Generic.List<InvoiceItemDto> Items
) : IRequest<Result<Guid>>;

public class GenerateInvoiceCommandsHandler : IRequestHandler<GenerateInvoiceCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public GenerateInvoiceCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommands request, CancellationToken cancellationToken)
    {
        decimal subTotal = 0;
        var invoiceItems = new global::System.Collections.Generic.List<InvoiceItem>();
        
        foreach (var item in request.Items ?? new global::System.Collections.Generic.List<InvoiceItemDto>())
        {
            var amount = item.Quantity * item.UnitPrice;
            subTotal += amount;
            invoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                ItemName = item.ItemName,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = amount,
                CreatedAt = DateTime.UtcNow
            });
        }

        var totalAmount = subTotal + request.TaxAmount - request.DiscountAmount;

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
            InvoiceTitle = request.InvoiceTitle,
            Currency = request.Currency,
            ParentType = request.ParentType,
            AccountId = request.AccountId,
            ContractId = request.ContractId,
            IssuedDate = request.IssuedDate,
            DueDate = request.DueDate,
            TaxAmount = request.TaxAmount,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = totalAmount,
            Notes = request.Notes,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            Items = invoiceItems
        };

        _context.Invoices.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(entity.Id);
    }
}