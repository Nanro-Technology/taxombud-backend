using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record UpdateVisitorStatusCommand(Guid Id, string Status);
