using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Operations;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record AddVendorCommands(
    string Name, 
    string Company, 
    string Email, 
    string Phone,
    string? Designation,
    string? Scope,
    string? ScopeTarget,
    string? Notes
) ;