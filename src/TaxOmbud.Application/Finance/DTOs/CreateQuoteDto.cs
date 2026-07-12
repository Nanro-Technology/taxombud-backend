using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Finance;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Finance.DTOs;

public record QuoteItemDto(
    string Name,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record CreateQuoteCommands(
    string QuoteNumber,
    string Title,
    string Status,
    string Currency,
    string ParentType,
    Guid? ParentId,
    DateTime? IssuedDate,
    DateTime? ExpiryDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? Notes,
    List<QuoteItemDto> Items
);

public record UpdateQuoteCommand(
    Guid Id,
    string Title,
    string Status,
    string Currency,
    string ParentType,
    Guid? ParentId,
    DateTime? IssuedDate,
    DateTime? ExpiryDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? Notes,
    List<QuoteItemDto> Items
);

public record DeleteQuoteCommand(Guid Id);
