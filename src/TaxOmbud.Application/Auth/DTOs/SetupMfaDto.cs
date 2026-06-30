using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Auth.DTOs;

public record SetupMfaCommand(Guid UserId) ;

public record SetupMfaResponse(
    string QrCodeUri,
    string SecretKey,
    IReadOnlyList<string> BackupCodes
);