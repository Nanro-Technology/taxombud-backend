using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record GetCommunicationTemplatesQuery();

public record CommunicationTemplateDto(
    Guid Id,
    string Name,
    string Category,
    string Subject,
    string Body
);
