using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Dashboard.DTOs;

public record SaveWidgetRequest(string Name, string Description, string ComponentName, string? RequiredPermission, bool IsActive);
