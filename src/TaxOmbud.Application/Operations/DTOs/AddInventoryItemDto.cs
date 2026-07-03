using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Operations;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record AddInventoryItemCommands(
    string Name,
    string Category,
    string Description,
    string SKU,
    Guid? DepartmentId,
    Guid? AssignedUserId,
    string Location,
    string Mode,
    int Quantity,
    string SerialNumber,
    string ImageUrl,
    string Status,
    string Note
) ;
