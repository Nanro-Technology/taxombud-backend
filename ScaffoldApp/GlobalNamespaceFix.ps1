# GlobalNamespaceFix.ps1
# Sweeps all .cs files in Application and fixes stale namespace references.

$root   = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src"
$total  = 0

$dirs = @(
    "$root\TaxOmbud.Application",
    "$root\TaxOmbud.Infrastructure",
    "$root\TaxOmbud.API"
)

# Map of old → new namespace strings
$replacements = [ordered]@{
    # Old interface namespace
    'TaxOmbud\.Application\.Common\.Interfaces'      = 'TaxOmbud.Application.Interfaces.InfrastructureService'
    # Old models namespace
    'TaxOmbud\.Application\.Common\.Models'          = 'TaxOmbud.Common.Responses'
    # Old Infrastructure Options
    'TaxOmbud\.Infrastructure\.Options'              = 'TaxOmbud.Common.Config'
    # MediatR leftover using
    'using MediatR;'                                  = ''
    # Old Features namespace leftovers in using lines
    'using TaxOmbud\.Application\.Features\.[^;]+;'  = ''
    # Application.Common.Interfaces import
    'using TaxOmbud\.Application\.Common\.Interfaces;' = 'using TaxOmbud.Application.Interfaces.InfrastructureService;'
    # Application.Common.Models import
    'using TaxOmbud\.Application\.Common\.Models;'    = 'using TaxOmbud.Common.Responses;'
}

foreach ($dir in $dirs) {
    $files = Get-ChildItem -Path $dir -Filter "*.cs" -Recurse |
        Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

    foreach ($file in $files) {
        $content  = Get-Content $file.FullName -Raw
        $original = $content

        foreach ($old in $replacements.Keys) {
            $new     = $replacements[$old]
            $content = [regex]::Replace($content, $old, $new)
        }

        # Clean up consecutive blank lines (more than 2 in a row)
        $content = [regex]::Replace($content, "(\r?\n){3,}", "`r`n`r`n")

        if ($content -ne $original) {
            Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
            Write-Host "FIXED: $($file.FullName.Replace($root, '').TrimStart('\'))"
            $total++
        }
    }
}

Write-Host ""
Write-Host "Global namespace sweep complete. Files updated: $total"
