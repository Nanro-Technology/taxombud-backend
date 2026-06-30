using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Communications.DTOs;

public record RenderTemplateRequest(Dictionary<string, string> Payload);
