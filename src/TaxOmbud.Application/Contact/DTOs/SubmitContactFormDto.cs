using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Contact.DTOs;

public record SubmitContactFormCommand(
    string Name,
    string Email,
    string Subject,
    string Message
) ;
