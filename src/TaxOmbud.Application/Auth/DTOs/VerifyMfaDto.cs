using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Auth.DTOs;

public record VerifyMfaCommand(Guid UserId, string TotpCode) ;