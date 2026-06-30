using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Entities.Complaints;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Cases.DTOs;

public record AddCaseNoteCommand(Guid CaseId, string Text, bool IsExternal) ;

public record AddCaseNoteResponse(Guid Id, string NoteText, bool IsExternal, DateTimeOffset CreatedAt);