# FixControllers.ps1
# Controllers have two corruption patterns:
# 1. "using" statements embedded inside the class body
# 2. Calls to _aiChatbotService.commandAsync() instead of the correct service
# This script strips corrupt using lines from class bodies and replaces wrong service calls.

$controllersDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.API\Controllers"
$files = Get-ChildItem -Path $controllersDir -Filter "*.cs"

Write-Host "Processing $($files.Count) controller files..."
$fixed = 0

foreach ($file in $files) {
    $original = Get-Content $file.FullName -Raw
    $content = $original

    # ── Step 1: Remove "using" lines that appear INSIDE class body (after first "{") ──
    # We do a line-by-line pass, tracking whether we are inside the class body
    $lines = $content -split "`r?`n"
    $classBodyStarted = $false
    $braceDepth = 0
    $newLines = @()
    $classLinePattern = '^\s*public\s+(abstract\s+)?class\s+'

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        if (!$classBodyStarted) {
            # detect when class brace opens
            if ($line -match $classLinePattern) {
                $classBodyStarted = $true
            }
            $newLines += $line
            continue
        }

        # Inside or after class declaration: count braces
        # Skip "using" lines that appear BEFORE we see fields/methods (inside class body)
        # They manifest as "using TaxOmbud..." lines with no leading method/field structure
        if ($line -match '^\s*using\s+TaxOmbud' -or $line -match '^\s*using\s+System' -or $line -match '^\s*using\s+Microsoft' -or $line -match '^\s*using\s+Hangfire' -or $line -match '^\s*using\s+FluentValidation') {
            # Check if this line is INSIDE the class (after the opening brace)
            # Count braces seen so far
            $bracesSoFar = ($newLines -join "`n" | Select-String -Pattern '\{' -AllMatches).Matches.Count -
                           ($newLines -join "`n" | Select-String -Pattern '\}' -AllMatches).Matches.Count
            if ($bracesSoFar -gt 0) {
                # This using is inside the class body — skip it
                continue
            }
        }

        # Remove duplicate private field declarations: if same field name appears more than once
        $newLines += $line
    }

    $content = $newLines -join "`r`n"

    # ── Step 2: Remove duplicate private field declarations ──
    # Pattern: two consecutive occurrences of "private readonly IXxx _xxx;"
    # We'll do a regex pass to find duplicates
    $fieldPattern = '(?m)([ \t]+private readonly \S+ \S+;[ \t]*\r?\n)([ \t]*\r?\n)*([ \t]+private readonly \S+ \S+;[ \t]*\r?\n)'
    # Simpler: find exact duplicate lines and remove the second one
    $lines2 = $content -split "`r?`n"
    $seenFields = @{}
    $deduped = @()
    foreach ($line in $lines2) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^private readonly \S+ \S+;$') {
            if ($seenFields.ContainsKey($trimmed)) {
                continue  # Skip duplicate field declaration
            }
            $seenFields[$trimmed] = $true
        }
        $deduped += $line
    }
    $content = $deduped -join "`r`n"

    # ── Step 3: Determine this controller's primary service ──
    # Parse the constructor to find the injected service name
    $ctorMatch = [regex]::Match($content, 'public \w+Controller\(\s*(\w+ \w+)')
    $primaryServiceField = $null
    $primaryServiceType = $null

    $fieldMatches = [regex]::Matches($content, 'private readonly (I\w+Service) (_\w+);')
    foreach ($m in $fieldMatches) {
        # Use first service that is NOT IAiChatbotService
        if ($m.Groups[1].Value -ne 'IAiChatbotService') {
            $primaryServiceType = $m.Groups[1].Value
            $primaryServiceField = $m.Groups[2].Value
            break
        }
    }

    # ── Step 4: Remove IAiChatbotService field if it exists and is not in constructor ──
    # Check if constructor injects IAiChatbotService
    $ctorBody = [regex]::Match($content, 'public \w+Controller\([^)]*\)')
    $ctorHasAiService = $ctorBody.Value -match 'IAiChatbotService'

    if (!$ctorHasAiService) {
        # Remove the _aiChatbotService field declaration
        $content = [regex]::Replace($content, '[ \t]+private readonly IAiChatbotService _aiChatbotService;\r?\n', '')
        # Remove the IAiChatbotService assignment in constructor if present
        $content = [regex]::Replace($content, '[ \t]+_aiChatbotService = \w+;\r?\n', '')

        # ── Step 5: Replace _aiChatbotService.commandAsync(cmd, ct) with primary service calls ──
        if ($primaryServiceField) {
            # Replace _aiChatbotService.commandAsync(X, ct) with _primaryService.HandleAsync(X, ct)
            # We'll route to the correct method based on the command name
            $content = [regex]::Replace($content,
                '_aiChatbotService\.commandAsync\(([^,]+),\s*ct\)',
                { param($m)
                    $arg = $m.Groups[1].Value.Trim()
                    "$primaryServiceField.HandleAsync($arg, ct)"
                })
        }
    }

    # ── Step 6: Remove IAiChatbotService from DI parameter list if not needed ──
    if (!$ctorHasAiService) {
        $content = [regex]::Replace($content,
            ',?\s*IAiChatbotService \w+\s*,?',
            '')
        # Clean up any trailing comma left in constructor
        $content = [regex]::Replace($content,
            '\(\s*,',
            '(')
        $content = [regex]::Replace($content,
            ',\s*\)',
            ')')
    }

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "FIXED: $($file.Name)"
        $fixed++
    } else {
        Write-Host "SKIP (no changes): $($file.Name)"
    }
}

Write-Host ""
Write-Host "Done. Fixed: $fixed controllers."
