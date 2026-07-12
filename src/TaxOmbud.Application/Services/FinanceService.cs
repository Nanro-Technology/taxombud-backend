using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.Finance.DTOs;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Finance;
using Microsoft.AspNetCore.Http;

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

    // ─── Contracts ────────────────────────────────────────────────────────────

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
                Status = request.Status ?? "Draft",
                SourceQuoteId = request.SourceQuoteId,
                AssignedAgentId = request.AssignedAgentId,
                ParentType = request.ParentType,
                ParentId = request.ParentId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RenewalDate = request.RenewalDate,
                ReminderCycleDays = request.ReminderCycleDays,
                Notes = request.Notes,
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

    public async Task<Response<Guid>> UpdateContractAsync(UpdateContractCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = await _contractRepo.GetByIdAsync(request.Id);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Contract not found.";
                return response;
            }

            entity.Title = request.Title;
            entity.Status = request.Status;
            entity.AssignedAgentId = request.AssignedAgentId;
            entity.ParentType = request.ParentType;
            entity.ParentId = request.ParentId;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.RenewalDate = request.RenewalDate;
            entity.ReminderCycleDays = request.ReminderCycleDays;
            entity.Notes = request.Notes;
            entity.LastModifiedAt = DateTime.UtcNow;

            await _contractRepo.UpdateAsync(entity);
            await _contractRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Contract updated successfully.";
            response.Data = entity.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the contract.";
            return response;
        }
    }

    public async Task<Response<bool>> DeleteContractAsync(DeleteContractCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var entity = await _contractRepo.GetByIdAsync(request.Id);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Contract not found.";
                return response;
            }

            await _contractRepo.RemoveAsync(entity);
            await _contractRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Contract deleted successfully.";
            response.Data = true;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the contract.";
            return response;
        }
    }

    // ─── Quotes ───────────────────────────────────────────────────────────────

    public async Task<Response<Guid>> CreateQuoteAsync(CreateQuoteCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = new Quote
            {
                Id = Guid.NewGuid(),
                QuoteNumber = request.QuoteNumber,
                Title = request.Title,
                Status = request.Status ?? "Draft",
                Currency = request.Currency ?? "NGN",
                ParentType = request.ParentType,
                ParentId = request.ParentId,
                IssuedDate = request.IssuedDate,
                ExpiryDate = request.ExpiryDate,
                Subtotal = request.Subtotal,
                TaxAmount = request.TaxAmount,
                DiscountAmount = request.DiscountAmount,
                TotalAmount = request.TotalAmount,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items?.Select(i => new QuoteItem
                {
                    Id = Guid.NewGuid(),
                    Name = i.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal,
                    CreatedAt = DateTime.UtcNow
                }).ToList() ?? new List<QuoteItem>()
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

    public async Task<Response<Guid>> UpdateQuoteAsync(UpdateQuoteCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            var entity = await _quoteRepo.Query()
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
            
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Quote not found.";
                return response;
            }

            entity.Title = request.Title;
            entity.Status = request.Status;
            entity.Currency = request.Currency;
            entity.ParentType = request.ParentType;
            entity.ParentId = request.ParentId;
            entity.IssuedDate = request.IssuedDate;
            entity.ExpiryDate = request.ExpiryDate;
            entity.Subtotal = request.Subtotal;
            entity.TaxAmount = request.TaxAmount;
            entity.DiscountAmount = request.DiscountAmount;
            entity.TotalAmount = request.TotalAmount;
            entity.Notes = request.Notes;
            entity.LastModifiedAt = DateTime.UtcNow;

            // Clear old items and repopulate
            entity.Items.Clear();
            if (request.Items != null)
            {
                foreach (var i in request.Items)
                {
                    entity.Items.Add(new QuoteItem
                    {
                        Id = Guid.NewGuid(),
                        QuoteId = entity.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.LineTotal,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _quoteRepo.UpdateAsync(entity);
            await _quoteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Quote updated successfully.";
            response.Data = entity.Id;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while updating the quote.";
            return response;
        }
    }

    public async Task<Response<bool>> DeleteQuoteAsync(DeleteQuoteCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var entity = await _quoteRepo.GetByIdAsync(request.Id);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Quote not found.";
                return response;
            }

            await _quoteRepo.RemoveAsync(entity);
            await _quoteRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Quote deleted successfully.";
            response.Data = true;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the quote.";
            return response;
        }
    }

    // ─── Invoices ─────────────────────────────────────────────────────────────

    public async Task<Response<Guid>> GenerateInvoiceAsync(GenerateInvoiceCommands request, CancellationToken cancellationToken = default)
    {
        var response = new Response<Guid>();
        try
        {
            decimal subTotal = 0;
            var invoiceItems = new List<InvoiceItem>();
            
            foreach (var item in request.Items ?? new List<InvoiceItemDto>())
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

    public async Task<Response<bool>> DeleteInvoiceAsync(DeleteInvoiceCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<bool>();
        try
        {
            var entity = await _invoiceRepo.GetByIdAsync(request.Id);
            if (entity == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Invoice not found.";
                return response;
            }

            await _invoiceRepo.RemoveAsync(entity);
            await _invoiceRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Invoice deleted successfully.";
            response.Data = true;
            return response;
        }
        catch (Exception)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An error occurred while deleting the invoice.";
            return response;
        }
    }

    // ─── Query Retrieval ──────────────────────────────────────────────────────

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
            var list = await _invoiceRepo.Query()
                .Include(i => i.Items)
                .ToListAsync(cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Invoices retrieved successfully.";
            response.Data = list;
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
            var list = await _quoteRepo.Query()
                .Include(q => q.Items)
                .ToListAsync(cancellationToken);
            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Quotes retrieved successfully.";
            response.Data = list;
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
