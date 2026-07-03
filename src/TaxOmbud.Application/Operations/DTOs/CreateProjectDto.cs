using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Operations;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Operations.DTOs;

public record CreateProjectCommands(string Name, string Description) ;
