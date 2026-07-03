using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.CustomException;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record AcknowledgeCommunicationCommand(Guid CommunicationId) ;
