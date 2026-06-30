using System;
using System.Collections.Generic;
using TaxOmbud.Domain.Entities.Finance;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Finance.DTOs;

public record CreateContractCommands(string ContractNumber, string Title) ;