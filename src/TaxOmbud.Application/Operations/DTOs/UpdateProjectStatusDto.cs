using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record UpdateProjectStatusCommands(Guid Id, string Status) ;
