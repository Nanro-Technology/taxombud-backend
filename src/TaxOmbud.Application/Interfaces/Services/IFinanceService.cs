using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Finance;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IFinanceService
{
    Task<Response<Guid>> CreateContractAsync(CreateContractCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UpdateContractAsync(UpdateContractCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteContractAsync(DeleteContractCommand request, CancellationToken cancellationToken = default);

    Task<Response<Guid>> CreateQuoteAsync(CreateQuoteCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> UpdateQuoteAsync(UpdateQuoteCommand request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteQuoteAsync(DeleteQuoteCommand request, CancellationToken cancellationToken = default);

    Task<Response<Guid>> GenerateInvoiceAsync(GenerateInvoiceCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> PayInvoiceAsync(PayInvoiceCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> DeleteInvoiceAsync(DeleteInvoiceCommand request, CancellationToken cancellationToken = default);

    Task<Response<List<Contract>>> GetContractsAsync(GetContractsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Invoice>>> GetInvoicesAsync(GetInvoicesQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Quote>>> GetQuotesAsync(GetQuotesQueries request, CancellationToken cancellationToken = default);
}
