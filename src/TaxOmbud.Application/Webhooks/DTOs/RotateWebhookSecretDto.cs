using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Webhooks.DTOs;

public record RotateWebhookSecretCommand(Guid Id, string NewSecret) ;

public record RotateSecretResponseDto(string Message);