using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetComplaintNotesQuery(Guid ComplaintId) ;

public record ComplaintNoteDto(
    Guid Id,
    string Body,
    string Visibility,
    Guid AuthorUserId,
    DateTimeOffset CreatedAt
);
