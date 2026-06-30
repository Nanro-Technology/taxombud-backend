using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record GetCaseCommunicationsQuery(Guid CaseId) ;

public record CaseCommunicationDto(
    Guid Id,
    Guid CaseId,
    string Sender,
    string Recipient,
    string Direction,
    string Subject,
    string Body,
    DateTimeOffset SentAt,
    string Channel
);