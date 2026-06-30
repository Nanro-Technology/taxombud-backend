# FixServiceStubs.ps1
# Scaffold-generated service methods are missing bodies: "public async Task<...> XyzAsync(...)\n\n"
# This adds "{ throw new NotImplementedException(); }" after each naked method signature.

$servicesDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application\Services"
$files = Get-ChildItem -Path $servicesDir -Filter "*.cs"

Write-Host "Processing $($files.Count) service files..."
$fixed = 0

foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw
    $original = $content

    # Pattern: a public async/non-async method signature that ends the line
    # without a { ... } body — followed immediately by a blank line or closing brace.
    # We need to add "{ throw new NotImplementedException(); }" after it.
    #
    # Match: line ending with "= default)" or "ct)" or "cancellationToken)" followed by \r\n\r\n or \r\n}
    # We'll use a broad approach: if a method declaration line doesn't have "{" after it on the same line
    # and the next non-blank line is "}" or another method signature, inject a body.

    # Strategy: split to lines and scan
    $lines   = $content -split "`r?`n"
    $result  = [System.Collections.Generic.List[string]]::new()
    $i = 0

    while ($i -lt $lines.Count) {
        $line = $lines[$i]

        # Detect a method signature line (public ... Task<...> MethodAsync(...)):
        # - starts with whitespace + "public"
        # - contains "Task<" or "Task " or "void"
        # - ends with ")" (no trailing "{")
        # - line does NOT contain "=>" (not a lambda or property)
        $isMethodSig = (
            $line -match '^\s+public\s' -and
            ($line -match 'Task[<\s]' -or $line -match '\bvoid\b') -and
            $line.TrimEnd() -match '\)$' -and
            $line -notmatch '=>' -and
            $line -notmatch '//' -and
            $line -notmatch 'public\s+\w+Controller'  # not constructor
        )

        if ($isMethodSig) {
            # Look ahead: is the next non-blank line "}" or another public method?
            $nextNonBlank = $null
            for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                if ($lines[$j].Trim() -ne '') {
                    $nextNonBlank = $lines[$j].Trim()
                    break
                }
            }

            $missingBody = (
                $nextNonBlank -eq '}' -or
                ($nextNonBlank -ne $null -and $nextNonBlank -match '^\s*public\s') -or
                $nextNonBlank -eq $null
            )

            $result.Add($line)
            if ($missingBody) {
                $result.Add('    {')
                $result.Add('        throw new NotImplementedException();')
                $result.Add('    }')
            }
        } else {
            $result.Add($line)
        }

        $i++
    }

    $newContent = ($result -join "`r`n")

    if ($newContent -ne $original) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline -Encoding UTF8
        Write-Host "  FIXED: $($file.Name)"
        $fixed++
    }
}

Write-Host ""
Write-Host "Done. Fixed $fixed service files."
