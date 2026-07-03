using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Finance;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;


namespace TaxOmbud.Application.Finance.DTOs;

public record InvoiceItemDto(string ItemName, string Description, decimal UnitPrice, int Quantity, string? Unit);

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
    List<InvoiceItemDto> Items
);
