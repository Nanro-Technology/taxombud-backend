using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record SubmitPublicCaseCommand(
    string SubmitterType, // Personal or Corporate
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string CountryId,
    string StateId,
    string Description
) ;

public record SubmitPublicCaseResponse(Guid CaseId, string TrackingNumber);