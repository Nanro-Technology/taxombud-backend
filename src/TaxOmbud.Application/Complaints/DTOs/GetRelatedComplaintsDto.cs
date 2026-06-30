using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record GetRelatedComplaintsQuery(Guid ComplaintId) ;

public record RelatedComplaintDto(
    Guid LinkId,
    Guid ComplaintId,
    string ReferenceNumber,
    string Subject,
    string Status,
    string LinkType
);