using MediatR;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Finance;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Features.Finance.Commands.CreateQuote;

public record CreateQuoteCommands(string QuoteNumber, decimal TotalAmount) : IRequest<Result<Guid>>;

public class CreateQuoteCommandsHandler : IRequestHandler<CreateQuoteCommands, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateQuoteCommandsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateQuoteCommands request, CancellationToken cancellationToken)
    {
        var entity = new Quote
        {
            Id = Guid.NewGuid(),
            QuoteNumber = request.QuoteNumber,
            TotalAmount = request.TotalAmount,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };
        _context.Quotes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}