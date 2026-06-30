using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Complaints.DTOs;

public record AddComplaintNoteCommand(
    Guid ComplaintId,
    string Body,
    string Visibility,
    Guid AuthorUserId
) ;

public record AddComplaintNoteRequest(string Body, string Visibility);