using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Hr;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.HrRequests.DTOs;

public record SubmitLoanRequestCommands(
    Guid StaffId, 
    decimal Amount, 
    int RepaymentMonths,
    string Purpose,
    string? DisburseTo,
    string? PayoutReference,
    string? ActionNote
) ;
