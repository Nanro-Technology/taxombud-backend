using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record PostRecommendationCommand(Guid CaseId, string RecommendationText) ;

public record PostRecommendationResponse(Guid Id, string RecommendationText, DateTimeOffset CreatedAt);