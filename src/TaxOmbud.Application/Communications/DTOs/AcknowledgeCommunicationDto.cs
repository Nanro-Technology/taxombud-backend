using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Domain.Entities.Communications;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Communications.DTOs;

public record AcknowledgeCommunicationCommand(Guid CommunicationId) ;