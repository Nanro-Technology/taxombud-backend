using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Finance;

namespace TaxOmbud.Application.Services;

public class FinanceService : IFinanceService
{
    private readonly IGenericRepository<Contract> _contractRepo;
    private readonly IGenericRepository<Quote> _quoteRepo;
    private readonly IGenericRepository<Invoice> _invoiceRepo;

    public FinanceService(
        IGenericRepository<Contract> contractRepo,
        IGenericRepository<Quote> quoteRepo,
        IGenericRepository<Invoice> invoiceRepo
    )
    {
        _contractRepo = contractRepo;
        _quoteRepo = quoteRepo;
        _invoiceRepo = invoiceRepo;
    }

    public async Task<Response<Guid>> CreateContractAsync(CreateContractCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = new Contract
            {
                Id = Guid.NewGuid(),
                ContractNumber = request.ContractNumber,
                Title = request.Title,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            await _contractRepo.AddAsync(entity);
            await _contractRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Contract created successfully.";
            response.Data = entity.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the contract.";
            return response;
        }
    }

    public async Task<Response<Guid>> CreateQuoteAsync(CreateQuoteCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = new Quote
            {
                Id = Guid.NewGuid(),
                QuoteNumber = request.QuoteNumber,
                TotalAmount = request.TotalAmount,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };
            await _quoteRepo.AddAsync(entity);
            await _quoteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Quote created successfully.";
            response.Data = entity.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while creating the quote.";
            return response;
        }
    }

    public async Task<Response<Guid>> GenerateInvoiceAsync(GenerateInvoiceCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
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

            await _invoiceRepo.AddAsync(entity);
            await _invoiceRepo.SaveAsync();
            
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Invoice generated successfully.";
            response.Data = entity.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while generating the invoice.";
            return response;
        }
    }

    public async Task<Response<bool>> PayInvoiceAsync(PayInvoiceCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var entity = await _invoiceRepo.FindAsync(x => x.Id == request.InvoiceId);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = $"Invoice {request.InvoiceId} not found.";
                return response;
            }
            
            entity.Status = "Paid";
            entity.LastModifiedAt = DateTime.UtcNow;
            await _invoiceRepo.UpdateAsync(entity);
            await _invoiceRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Invoice paid successfully.";
            response.Data = true;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while paying the invoice.";
            return response;
        }
    }

    public async Task<Response<List<Contract>>> GetContractsAsync(GetContractsQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Contract>>();
        try
        {
            var list = await _contractRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Contracts retrieved successfully.";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving contracts.";
            return response;
        }
    }

    public async Task<Response<List<Invoice>>> GetInvoicesAsync(GetInvoicesQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Invoice>>();
        try
        {
            var list = await _invoiceRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Invoices retrieved successfully.";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving invoices.";
            return response;
        }
    }

    public async Task<Response<List<Quote>>> GetQuotesAsync(GetQuotesQueries request, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<Quote>>();
        try
        {
            var list = await _quoteRepo.GetAllAsync();
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Quotes retrieved successfully.";
            response.Data = list.ToList();
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while retrieving quotes.";
            return response;
        }
    }
}
