# FixDuplicatesAndAliases.ps1
# 1. For each _Stubs.cs file, removes type definitions that already exist in other .cs files in the same folder
# 2. Adds plural type aliases for scaffold-generated pluralized type names
# 3. Adds missing PagedResult using to interface files

$root = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application"

Write-Host "=== Step 1: Remove duplicate types from _Stubs.cs files ==="

$stubFiles = Get-ChildItem -Path $root -Filter "_Stubs.cs" -Recurse
foreach ($stubFile in $stubFiles) {
    $folder = $stubFile.Directory.FullName

    # Collect all type names already defined in OTHER files in the same folder
    $existingTypes = [System.Collections.Generic.HashSet[string]]::new()
    $otherFiles = Get-ChildItem -Path $folder -Filter "*.cs" |
        Where-Object { $_.Name -ne "_Stubs.cs" }

    foreach ($f in $otherFiles) {
        $fc = Get-Content $f.FullName -Raw
        [regex]::Matches($fc, '(?m)^public (?:record|class|interface|struct|enum) (\w+)') | ForEach-Object {
            $existingTypes.Add($_.Groups[1].Value) | Out-Null
        }
    }

    if ($existingTypes.Count -eq 0) { continue }

    # Remove stub lines for types that already exist
    $stubLines = Get-Content $stubFile.FullName
    $newLines  = [System.Collections.Generic.List[string]]::new()
    $removed   = 0

    foreach ($line in $stubLines) {
        $isDuplicate = $false
        if ($line -match '^\s*public (?:record|class) (\w+)') {
            $typeName = $matches[1]
            if ($existingTypes.Contains($typeName)) {
                $isDuplicate = $true
                $removed++
            }
        }
        if (-not $isDuplicate) {
            $newLines.Add($line)
        }
    }

    if ($removed -gt 0) {
        Set-Content -Path $stubFile.FullName -Value $newLines -Encoding UTF8
        Write-Host "  Removed $removed duplicates from: $($stubFile.Name) in $($folder.Split('\')[-2])"
    }
}

Write-Host ""
Write-Host "=== Step 2: Add plural type aliases ==="

# The scaffold generated pluralized type names in interface files.
# Add type aliases in the stubs to handle both forms.
$pluralAliases = @{
    "TaxOmbud.Application.Finance.DTOs" = @"

// Plural aliases for scaffold compatibility
public record CreateContractCommands(string Title, Guid VendorId, decimal Value, DateTime StartDate, DateTime EndDate) : CreateContractCommand(Title, VendorId, Value, StartDate, EndDate);
public record CreateQuoteCommands(string Title, Guid ClientId, decimal TotalAmount) : CreateQuoteCommand(Title, ClientId, TotalAmount);
public record GenerateInvoiceCommands(Guid QuoteId) : GenerateInvoiceCommand(QuoteId);
public record PayInvoiceCommands(Guid InvoiceId, decimal Amount, string PaymentMethod) : PayInvoiceCommand(InvoiceId, Amount, PaymentMethod);
public record GetContractsQueries(int Page = 1, int PageSize = 20) : GetContractsQuery(Page, PageSize);
public record GetInvoicesQueries(int Page = 1, int PageSize = 20) : GetInvoicesQuery(Page, PageSize);
public record GetQuotesQueries(int Page = 1, int PageSize = 20) : GetQuotesQuery(Page, PageSize);
"@

    "TaxOmbud.Application.Operations.DTOs" = @"

// Plural aliases for scaffold compatibility
public record AddInventoryItemCommands(string Name, string Category, int Quantity, decimal UnitCost) : AddInventoryItemCommand(Name, Category, Quantity, UnitCost);
public record AddVendorCommands(string Name, string Email, string Phone, string? Address) : AddVendorCommand(Name, Email, Phone, Address);
public record CreateProjectCommands(string Title, string Description, DateTime StartDate, DateTime EndDate) : CreateProjectCommand(Title, Description, StartDate, EndDate);
public record UpdateProjectStatusCommands(Guid ProjectId, string Status) : UpdateProjectStatusCommand(ProjectId, Status);
public record GetInventoryItemsQueries(int Page = 1, int PageSize = 20) : GetInventoryItemsQuery(Page, PageSize);
public record GetProjectsQueries(int Page = 1, int PageSize = 20) : GetProjectsQuery(Page, PageSize);
public record GetVendorsQueries(int Page = 1, int PageSize = 20) : GetVendorsQuery(Page, PageSize);
"@

    "TaxOmbud.Application.HrRequests.DTOs" = @"

// Plural aliases for scaffold compatibility
public record ApproveLeaveRequestCommands(Guid RequestId, bool Approved, string? Reason) : ApproveLeaveRequestCommand(RequestId, Approved, Reason);
public record SubmitLeaveRequestCommands(DateTime StartDate, DateTime EndDate, string LeaveType, string Reason) : SubmitLeaveRequestCommand(StartDate, EndDate, LeaveType, Reason);
public record SubmitLoanRequestCommands(decimal Amount, string Purpose) : SubmitLoanRequestCommand(Amount, Purpose);
public record GetEwaRequestsQueries(int Page = 1, int PageSize = 20) : GetEwaRequestsQuery(Page, PageSize);
public record GetLeaveRequestsQueries(int Page = 1, int PageSize = 20) : GetLeaveRequestsQuery(Page, PageSize);
public record GetLoanRequestsQueries(int Page = 1, int PageSize = 20) : GetLoanRequestsQuery(Page, PageSize);
"@

    "TaxOmbud.Application.Payroll.DTOs" = @"

// Plural aliases for scaffold compatibility
public record ApprovePayrollCommands(Guid PayrollRunId, bool Approved) : ApprovePayrollCommand(PayrollRunId, Approved);
public record CreateSalaryProfileCommands(Guid EmployeeId, decimal BasicSalary, decimal Allowances) : CreateSalaryProfileCommand(EmployeeId, BasicSalary, Allowances);
public record RunPayrollCommands(DateTime PeriodStart, DateTime PeriodEnd) : RunPayrollCommand(PeriodStart, PeriodEnd);
public record GetPayrollPeriodsQueries(int Page = 1, int PageSize = 20) : GetPayrollPeriodsQuery(Page, PageSize);
public record GetRemittancesQueries(int Page = 1, int PageSize = 20) : GetRemittancesQuery(Page, PageSize);
public record GetSalaryProfilesQueries(int Page = 1, int PageSize = 20) : GetSalaryProfilesQuery(Page, PageSize);
"@

    "TaxOmbud.Application.Wallet.DTOs" = @"

// Plural aliases for scaffold compatibility
public record ProcessWithdrawalCommands(Guid WithdrawalRequestId, bool Approved, string? Reason) : ProcessWithdrawalCommand(WithdrawalRequestId, Approved, Reason);
public record RequestWithdrawalCommands(decimal Amount, string BankName, string AccountNumber) : RequestWithdrawalCommand(Amount, BankName, AccountNumber);
public record GetWalletBalanceQueries(Guid UserId) : GetWalletBalanceQuery(UserId);
public record GetWalletTransactionsQueries(Guid UserId, int Page = 1, int PageSize = 20) : GetWalletTransactionsQuery(UserId, Page, PageSize);
"@
}

foreach ($ns in $pluralAliases.Keys) {
    $parts   = $ns -replace '^TaxOmbud\.Application\.', '' -split '\.'
    $folder  = Join-Path $root ($parts -join '\')
    $stubFile = Join-Path $folder "_Stubs.cs"

    if (Test-Path $stubFile) {
        $existing = Get-Content $stubFile -Raw
        if ($existing -notmatch 'Plural aliases') {
            Add-Content -Path $stubFile -Value $pluralAliases[$ns] -Encoding UTF8
            Write-Host "  Added plural aliases to: $($ns.Split('.')[-2])"
        }
    }
}

Write-Host ""
Write-Host "=== Step 3: Fix PagedResult missing using in interface files ==="

$ifaceDir = Join-Path $root "Interfaces\Services"
$ifaceFiles = Get-ChildItem -Path $ifaceDir -Filter "*.cs"
$fixedIfaces = 0

foreach ($f in $ifaceFiles) {
    $content  = Get-Content $f.FullName -Raw
    $original = $content

    # Add PagedResult using if file uses PagedResult but doesn't have using TaxOmbud.Common.Utilities
    if ($content -match 'PagedResult' -and $content -notmatch 'using TaxOmbud\.Common\.Utilities') {
        $content = "using TaxOmbud.Common.Utilities;`r`n" + $content
    }

    if ($content -ne $original) {
        Set-Content -Path $f.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  Fixed PagedResult using: $($f.Name)"
        $fixedIfaces++
    }
}

Write-Host "  Fixed $fixedIfaces interface files."
Write-Host ""
Write-Host "=== All done! ==="
