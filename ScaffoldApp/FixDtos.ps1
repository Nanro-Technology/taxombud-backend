# FixDtos.ps1
# Fixes scaffold-generated DTOs that have duplicate namespace blocks.
# Each file has a clean header (lines 1..N) then a second "namespace TaxOmbud.Application.Features.*"
# block containing old MediatR code. We strip the second block entirely.

$appDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application"

$files = Get-ChildItem -Path $appDir -Filter "*.cs" -Recurse |
    Where-Object { (Get-Content $_.FullName -Raw) -match "namespace TaxOmbud\.Application\.Features" }

Write-Host "Found $($files.Count) files to fix."

$fixed = 0
$skipped = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Find index of the second namespace declaration (old MediatR namespace)
    # Strategy: split on line endings, find the line index that starts "namespace TaxOmbud.Application.Features"
    $lines = $content -split "`r?`n"

    # Find the line index of the OLD namespace (Features.*)
    $oldNsIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "^namespace TaxOmbud\.Application\.Features") {
            $oldNsIndex = $i
            break
        }
    }

    if ($oldNsIndex -eq -1) {
        Write-Host "SKIP (no Features namespace found): $($file.Name)"
        $skipped++
        continue
    }

    # Walk backwards from $oldNsIndex to also remove any using/blank lines
    # that belong to the old block (everything after the new namespace declaration line)
    # The new namespace line looks like: "namespace TaxOmbud.Application.{Module}.DTOs;"
    # Find it (should be before $oldNsIndex)
    $newNsIndex = -1
    for ($i = 0; $i -lt $oldNsIndex; $i++) {
        if ($lines[$i] -match "^namespace TaxOmbud\.Application\." -and
            $lines[$i] -notmatch "Features") {
            $newNsIndex = $i
            break
        }
    }

    if ($newNsIndex -eq -1) {
        Write-Host "SKIP (no new namespace found): $($file.Name)"
        $skipped++
        continue
    }

    # Keep only lines up to and including the new namespace line
    # Strip trailing blank lines from the kept block then add newline
    $keepLines = $lines[0..($newNsIndex)]

    # Trim trailing empty lines from kept block
    while ($keepLines.Count -gt 0 -and [string]::IsNullOrWhiteSpace($keepLines[-1])) {
        $keepLines = $keepLines[0..($keepLines.Count - 2)]
    }

    $newContent = ($keepLines -join "`r`n") + "`r`n"
    Set-Content -Path $file.FullName -Value $newContent -NoNewline -Encoding UTF8
    Write-Host "FIXED: $($file.Name)"
    $fixed++
}

Write-Host ""
Write-Host "Done. Fixed: $fixed | Skipped: $skipped"
