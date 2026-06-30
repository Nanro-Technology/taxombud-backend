using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.System;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.System.DTOs;

public record CreateAnnouncementCommand(string Title, string Message, string Scope) ;