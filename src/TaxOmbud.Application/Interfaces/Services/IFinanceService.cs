using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;
using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Domain.Entities.Hr;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.Entities.Officers;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Documents;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Domain.Entities.Appeals;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Entities.Notifications;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Domain.Entities.Finance;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IFinanceService
{
    Task<Response<Guid>> CreateContractAsync(CreateContractCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> CreateQuoteAsync(CreateQuoteCommands request, CancellationToken cancellationToken = default);
    Task<Response<Guid>> GenerateInvoiceAsync(GenerateInvoiceCommands request, CancellationToken cancellationToken = default);
    Task<Response<bool>> PayInvoiceAsync(PayInvoiceCommands request, CancellationToken cancellationToken = default);
    Task<Response<List<Contract>>> GetContractsAsync(GetContractsQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Invoice>>> GetInvoicesAsync(GetInvoicesQueries request, CancellationToken cancellationToken = default);
    Task<Response<List<Quote>>> GetQuotesAsync(GetQuotesQueries request, CancellationToken cancellationToken = default);
}
