# FixInfrastructureAndApi.ps1
# 1. Fix namespace references in Infrastructure/Services files
# 2. Delete old Infrastructure/DependencyInjection.cs, Options/, Persistence/ folders
# 3. Delete API/EventHandlers/ folder
# 4. Update ApiControllerBase.cs to remove Result<T> dependency

$root = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend"
$infraDir = Join-Path $root "src\TaxOmbud.Infrastructure"
$apiDir   = Join-Path $root "src\TaxOmbud.API"

# ── 1. Fix namespace imports in all Infrastructure .cs files ─────────────────
Write-Host "=== Fixing Infrastructure namespace imports ==="
$infraFiles = Get-ChildItem -Path $infraDir -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

$infraReplaced = 0
foreach ($file in $infraFiles) {
    $content = Get-Content $file.FullName -Raw
    $original = $content

    # Old → New interface namespace
    $content = $content -replace 'TaxOmbud\.Application\.Common\.Interfaces', 'TaxOmbud.Application.Interfaces.InfrastructureService'

    # Old Options namespace → Common.Config
    $content = $content -replace 'TaxOmbud\.Infrastructure\.Options', 'TaxOmbud.Common.Config'
    $content = $content -replace 'using TaxOmbud\.Infrastructure\.Options;', 'using TaxOmbud.Common.Config;'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  FIXED imports: $($file.Name)"
        $infraReplaced++
    }
}
Write-Host "  Total fixed: $infraReplaced"

# ── 2. Delete old Infrastructure files / folders ─────────────────────────────
Write-Host ""
Write-Host "=== Cleaning up old Infrastructure files ==="

$oldDI = Join-Path $infraDir "DependencyInjection.cs"
if (Test-Path $oldDI) { Remove-Item $oldDI -Force; Write-Host "  DELETED: DependencyInjection.cs" }

$oldClass1 = Join-Path $infraDir "Class1.cs"
if (Test-Path $oldClass1) { Remove-Item $oldClass1 -Force; Write-Host "  DELETED: Class1.cs" }

$oldOpts = Join-Path $infraDir "Options"
if (Test-Path $oldOpts) { Remove-Item $oldOpts -Recurse -Force; Write-Host "  DELETED: Options/ folder" }

# Remove the old Persistence folder (ApplicationDbContext now lives in TaxOmbud.Persistence)
$oldPersistence = Join-Path $infraDir "Persistence"
if (Test-Path $oldPersistence) { Remove-Item $oldPersistence -Recurse -Force; Write-Host "  DELETED: Infrastructure/Persistence/ folder" }

# Remove old Services/SmtpEmailService.cs (moved to EmailServices/)
$oldSmtp = Join-Path $infraDir "Services\SmtpEmailService.cs"
if (Test-Path $oldSmtp) { Remove-Item $oldSmtp -Force; Write-Host "  DELETED: Services/SmtpEmailService.cs (moved to EmailServices/)" }

# Remove old Migrations folder from Infrastructure (now in Persistence)
$oldMigrations = Join-Path $infraDir "Migrations"
if (Test-Path $oldMigrations) {
    Write-Host "  NOTE: Infrastructure/Migrations/ still exists - keeping (EF migrations may still be here)"
}

# ── 3. Delete API/EventHandlers/ ─────────────────────────────────────────────
Write-Host ""
Write-Host "=== Cleaning up API EventHandlers ==="
$apiEventHandlers = Join-Path $apiDir "EventHandlers"
if (Test-Path $apiEventHandlers) { Remove-Item $apiEventHandlers -Recurse -Force; Write-Host "  DELETED: API/EventHandlers/ folder" }

# ── 4. Fix Application Common.Interfaces references project-wide ──────────────
Write-Host ""
Write-Host "=== Fixing Application.Common.Interfaces refs in Services/ ==="
$appServicesDir = Join-Path $root "src\TaxOmbud.Application\Services"
$appFiles = Get-ChildItem -Path $appServicesDir -Filter "*.cs" -Recurse 2>$null
$appReplaced = 0
foreach ($file in $appFiles) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    $content = $content -replace 'TaxOmbud\.Application\.Common\.Interfaces', 'TaxOmbud.Application.Interfaces.InfrastructureService'
    $content = $content -replace 'TaxOmbud\.Application\.Common\.Models', 'TaxOmbud.Common.Responses'
    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  FIXED: $($file.Name)"
        $appReplaced++
    }
}
Write-Host "  Total fixed: $appReplaced"

Write-Host ""
Write-Host "=== All done! ==="
