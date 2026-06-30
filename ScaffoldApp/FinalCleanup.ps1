# FinalCleanup.ps1
# 1. Remove stale MediatR/Mapster usings and fix Unit return types
# 2. Add remaining missing DTO stubs

$root = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application"

Write-Host "=== Step 1: Remove stale MediatR/Mapster imports ==="

$files = Get-ChildItem -Path $root -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

$fixed = 0
foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw
    $original = $content

    $content = $content -replace 'using Mapster;\r?\n', ''
    $content = $content -replace 'using MediatR;\r?\n', ''

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  CLEANED: $($file.Name)"
        $fixed++
    }
}
Write-Host "  Cleaned $fixed files."

Write-Host ""
Write-Host "=== Step 2: Fix IMediator in CommunicationsService ==="

$commService = Join-Path $root "Services\CommunicationsService.cs"
if (Test-Path $commService) {
    $content = Get-Content $commService -Raw
    # Remove IMediator field and constructor injection
    $content = $content -replace '^\s+private readonly IMediator _mediator;\r?\n', ''
    $content = $content -replace '^\s+IMediator mediator,?\r?\n', ''
    $content = $content -replace '_mediator = mediator;\r?\n', ''
    $content = $content -replace 'using TaxOmbud\.Application\.Interfaces\.InfrastructureService;\r?\n', 'using TaxOmbud.Application.Interfaces.InfrastructureService;'
    Set-Content -Path $commService -Value $content -NoNewline -Encoding UTF8
    Write-Host "  Fixed CommunicationsService.cs"
}

Write-Host ""
Write-Host "=== Step 3: Fix Unit return type in ICommunicationsService + CommunicationsService ==="

# ICommunicationsService: Task<Unit> → Task
$ifaceFile = Join-Path $root "Interfaces\Services\ICommunicationsService.cs"
if (Test-Path $ifaceFile) {
    $content  = Get-Content $ifaceFile -Raw
    $original = $content
    $content  = $content -replace 'Task<Unit>', 'Task'
    if ($content -ne $original) {
        Set-Content -Path $ifaceFile -Value $content -NoNewline -Encoding UTF8
        Write-Host "  Fixed Task<Unit> in ICommunicationsService.cs"
    }
}

# Same for all service files
$allServices = Get-ChildItem -Path (Join-Path $root "Services") -Filter "*.cs"
foreach ($f in $allServices) {
    $content  = Get-Content $f.FullName -Raw
    $original = $content
    $content  = $content -replace 'Task<Unit>', 'Task'
    if ($content -ne $original) {
        Set-Content -Path $f.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  Fixed Task<Unit> in $($f.Name)"
    }
}

Write-Host ""
Write-Host "=== Step 4: Add remaining missing DTO stubs ==="

# Appointments module — missing CalendarEventDto, TimeSlotDto, GetAvailabilityQuery, GetCalendarQuery, UpdateAppointmentCommand
$appointmentsStub = Join-Path $root "Appointments\DTOs\_Stubs.cs"
if (Test-Path $appointmentsStub) {
    $existing = Get-Content $appointmentsStub -Raw
    if ($existing -notmatch 'CalendarEventDto') {
        Add-Content $appointmentsStub -Value @"

// Additional stubs
public record GetAvailabilityQuery(Guid OfficerId, DateTime Date);
public record GetCalendarQuery(Guid? UserId, DateTime From, DateTime To);
public record UpdateAppointmentCommand(Guid AppointmentId, DateTime? Date, string? Status, string? Notes);
public record CalendarEventDto(Guid Id, string Title, DateTime Start, DateTime End, string Type);
public record TimeSlotDto(DateTime Start, DateTime End, bool IsAvailable);
"@
        Write-Host "  Added stubs to Appointments"
    }
}

# Complaints module — missing ComplaintSummaryDto, GetMyComplaintsQuery
$complaintsStub = Join-Path $root "Complaints\DTOs\_Stubs.cs"
if (-not (Test-Path $complaintsStub)) {
    $ns = "TaxOmbud.Application.Complaints.DTOs"
    New-Item -Path (Split-Path $complaintsStub) -ItemType Directory -Force | Out-Null
    Set-Content $complaintsStub -Value @"
namespace $ns;

public record GetMyComplaintsQuery(int Page = 1, int PageSize = 20);
public record ComplaintSummaryDto(Guid Id, string TrackingNumber, string Status, string TaxType, DateTimeOffset CreatedAt);
"@ -Encoding UTF8
    Write-Host "  Created Complaints stubs"
} else {
    $existing = Get-Content $complaintsStub -Raw
    if ($existing -notmatch 'ComplaintSummaryDto') {
        Add-Content $complaintsStub -Value @"

public record GetMyComplaintsQuery(int Page = 1, int PageSize = 20);
public record ComplaintSummaryDto(Guid Id, string TrackingNumber, string Status, string TaxType, DateTimeOffset CreatedAt);
"@
        Write-Host "  Added stubs to Complaints"
    }
}

# Finance module — missing Contract, Invoice, Quote (domain entity aliases)
$financeStub = Join-Path $root "Finance\DTOs\_Stubs.cs"
$financeContent = Get-Content $financeStub -Raw
if ($financeContent -notmatch '^public record Contract') {
    Add-Content $financeStub -Value @"

// View DTOs
public record ContractDto(Guid Id, string Title, string VendorName, decimal Value, DateTime StartDate, DateTime EndDate, string Status);
public record InvoiceDto(Guid Id, string InvoiceNumber, string ClientName, decimal Amount, string Status, DateTime DueDate);
public record QuoteDto(Guid Id, string Title, string ClientName, decimal TotalAmount, string Status);
"@
    Write-Host "  Added Finance view DTOs"
}

# Taxpayers module — missing GetTaxpayerByTinQuery
$taxpayersStub = Join-Path $root "Taxpayers\DTOs\_Stubs.cs"
if (-not (Test-Path $taxpayersStub)) {
    $ns = "TaxOmbud.Application.Taxpayers.DTOs"
    New-Item -Path (Split-Path $taxpayersStub) -ItemType Directory -Force | Out-Null
    Set-Content $taxpayersStub -Value @"
namespace $ns;

public record GetTaxpayerByTinQuery(string Tin);
"@ -Encoding UTF8
    Write-Host "  Created Taxpayers stubs"
} else {
    $existing = Get-Content $taxpayersStub -Raw
    if ($existing -notmatch 'GetTaxpayerByTinQuery') {
        Add-Content $taxpayersStub -Value "`npublic record GetTaxpayerByTinQuery(string Tin);"
        Write-Host "  Added GetTaxpayerByTinQuery to Taxpayers stubs"
    }
}

# Operations — missing VendorContact, InventoryItem, Project (as DTOs)
$operationsStub = Join-Path $root "Operations\DTOs\_Stubs.cs"
$opContent = Get-Content $operationsStub -Raw
if ($opContent -notmatch 'VendorContact') {
    Add-Content $operationsStub -Value @"

public record VendorContactDto(string Name, string Email, string Phone);
public record InventoryItemDto(Guid Id, string Name, string Category, int Quantity, decimal UnitCost);
public record ProjectDto(Guid Id, string Title, string Description, string Status, DateTime StartDate, DateTime EndDate);
"@
    Write-Host "  Added Operations view DTOs"
}

Write-Host ""
Write-Host "=== All done! ==="
