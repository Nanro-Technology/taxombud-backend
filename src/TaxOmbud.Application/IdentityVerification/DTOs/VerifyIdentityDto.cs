using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.IdentityVerification.DTOs;

public record VerifyIdentityCommand(
    string IdNumber,
    string IdType // NIN, BVN, Passport, etc.
);

public record IdentityVerificationResponse(
    bool Verified,
    string IdNumber,
    string IdType,
    string? FullName,
    string? DateOfBirth,
    string? FailureReason
);
