using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Auth.DTOs;

public record RefreshTokenCommand(string Token) ;
public record RefreshTokenResponse(string AccessToken, string NewRefreshToken, DateTimeOffset ExpiresAt);