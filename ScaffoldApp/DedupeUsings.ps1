# DedupeUsings.ps1
# Removes duplicate "using X;" lines from every .cs file in the Application project.

$root = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application"
$files = Get-ChildItem -Path $root -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

Write-Host "Scanning $($files.Count) files for duplicate usings..."
$fixed = 0

foreach ($file in $files) {
    $lines    = Get-Content $file.FullName
    $seenUsings = [System.Collections.Generic.HashSet[string]]::new()
    $result   = [System.Collections.Generic.List[string]]::new()
    $changed  = $false

    foreach ($line in $lines) {
        $trimmed = $line.Trim()

        # If it's a using directive, deduplicate
        if ($trimmed -match '^using\s+[\w\.]+;$') {
            if ($seenUsings.Add($trimmed)) {
                $result.Add($line)
            } else {
                $changed = $true  # Duplicate — skip it
            }
        } else {
            # Reset seen usings once we're past the using block (hit a blank or code line)
            $result.Add($line)
        }
    }

    if ($changed) {
        Set-Content -Path $file.FullName -Value $result -Encoding UTF8
        Write-Host "  DEDUPED: $($file.Name)"
        $fixed++
    }
}

Write-Host ""
Write-Host "Done. Deduplicated $fixed files."
