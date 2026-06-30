# FixServicesDeep.ps1
# Targeted repair of all remaining corruption in service files:
# 1) Remove misplaced `using` directives inserted inside method bodies
# 2) Add braces to broken single-line if blocks that set response properties
# 3) Fix remaining Response<T>.Method() factory calls
# 4) Fix corrupted object-initializer closings

$servicesDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application\Services"
$files = Get-ChildItem -Path $servicesDir -Filter "*.cs"
Write-Host "Deep-fixing $($files.Count) service files..."
$fixed = 0

foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # ── 1. Remove using directives that landed INSIDE method bodies ───────────
    # Pattern: non-blank code line, then `using Xxx;`, then more code
    # These appear mid-file, after an `await using var` line
    $content = $content -replace '(?m)^[ \t]*using TaxOmbud\.Common\.Utilities;[ \t]*\r?\n', ''
    $content = $content -replace '(?m)^[ \t]*using Microsoft\.AspNetCore\.Http;[ \t]*\r?\n', ''
    # Re-add them at the top if now missing
    if ($content -notmatch 'using Microsoft\.AspNetCore\.Http;') {
        $content = $content -replace '((?:using [^\r\n]+\r?\n)+)', "`$1using Microsoft.AspNetCore.Http;`r`n"
    }
    if ($content -notmatch 'using TaxOmbud\.Common\.Utilities;') {
        $content = $content -replace '((?:using [^\r\n]+\r?\n)+)', "`$1using TaxOmbud.Common.Utilities;`r`n"
    }

    # ── 2. Fix broken if-blocks: `if (cond)\n STMT1;\n STMT2;` → add braces ──
    # Pattern: if (...) followed immediately by response.StatusCode (no brace on same line)
    $content = [regex]::Replace($content,
        '(?m)(\r?\n)([ \t]+)(if \([^\r\n]+\))\r?\n([ \t]+)(response\.StatusCode = [^\r\n]+;\r?\n[ \t]+response\.Message = [^\r\n]+;\r?\n[ \t]+return response;)',
        {
            param($m)
            $nl     = $m.Groups[1].Value
            $outer  = $m.Groups[2].Value
            $cond   = $m.Groups[3].Value
            $inner  = $m.Groups[4].Value
            $body   = $m.Groups[5].Value
            # normalise the indentation of body lines
            $bodyFixed = $body -replace '(?m)^[ \t]+', "${inner}    "
            "${nl}${outer}${cond}`r`n${outer}{`r`n${inner}    ${bodyFixed}`r`n${outer}}"
        })

    # Also handle if blocks that have Data = null in them
    $content = [regex]::Replace($content,
        '(?m)(\r?\n)([ \t]+)(if \([^\r\n]+\))\r?\n([ \t]+)(response\.StatusCode = [^\r\n]+;\r?\n[ \t]+response\.Message = [^\r\n]+;\r?\n[ \t]+response\.Data = [^\r\n]+;\r?\n[ \t]+return response;)',
        {
            param($m)
            $nl     = $m.Groups[1].Value
            $outer  = $m.Groups[2].Value
            $cond   = $m.Groups[3].Value
            $inner  = $m.Groups[4].Value
            $body   = $m.Groups[5].Value
            $bodyFixed = $body -replace '(?m)^[ \t]+', "${inner}    "
            "${nl}${outer}${cond}`r`n${outer}{`r`n${inner}    ${bodyFixed}`r`n${outer}}"
        })

    # ── 3. Remove extra indentation from response.StatusCode/Message/Data ─────
    # These were produced as "            response.Message = ...;" (over-indented)
    # Normalise them to 8 spaces (standard inside a method body)
    $content = [regex]::Replace($content,
        '(?m)^([ \t]{12,})(response\.(StatusCode|Message|Data) = )',
        '        $2')

    # Remove trailing space before semicolons that the regex introduced
    $content = $content -replace '( );', ';'

    # ── 4. Fix remaining single-line Response factory calls ───────────────────
    # Response<X>.NotFound($"...") - interpolated
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.NotFound\(\$"([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status404NotFound;`r`n            response.Message = `$`"$2`";`r`n            return response;")

    # Response<X>.NotFound("msg")
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.NotFound\("([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status404NotFound;`r`n            response.Message = `"$2`";`r`n            return response;")

    # Response<X>.NotFound() no arg
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.NotFound\(\);',
        "response.StatusCode = StatusCodes.Status404NotFound;`r`n            response.Message = Constants.Messages.NotFound;`r`n            return response;")

    # Response<X>.Fail("msg") - single arg
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Fail\("([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status400BadRequest;`r`n            response.Message = `"$2`";`r`n            return response;")

    # Response<X>.Success(data.AsReadOnly()) and similar - simple single-call data
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Success\(([^()]+(?:\(\))?[^()]*)\);',
        "response.StatusCode = StatusCodes.Status200OK;`r`n            response.Message = Constants.Messages.Success;`r`n            response.Data = $2;`r`n            return response;")

    # Response<X>.Success(new PagedResult<Y>(items, total, page, size))
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Success\((new PagedResult<[^>]+>\([^)]+\))\);',
        "response.StatusCode = StatusCodes.Status200OK;`r`n            response.Message = Constants.Messages.Success;`r`n            response.Data = $2;`r`n            return response;")

    # ── 5. Fix corrupted object initialisers ──────────────────────────────────
    # `Data = new SomeDto(arg1, arg2 })` → `Data = new SomeDto(arg1, arg2) }`
    # Pattern: closing brace immediately after args with no closing paren before it
    $content = [regex]::Replace($content,
        '(?m)(Data = new \w+<[^>]+>\([^)]+)\s*\}\);',
        '$1) };')

    # Simple: `new Dto(a, b, c })` → `new Dto(a, b, c) }`
    $content = [regex]::Replace($content,
        '(new \w+\([^)]+)\s*\}\);',
        '$1) };')

    # ── 6. Fix doubly-wrapped Response type from GetOverdueCasesAsync etc ─────
    $content = $content -replace 'TaxOmbud\.Common\.Responses\.Response<TaxOmbud\.Common\.Responses\.Response<([^>]+)>>',
        'Response<$1>'

    # ── 7. Fix multiline return new Response<T> { ...Data = new Ctor( ────────
    # `return new Response<X> { StatusCode = N, Message = "Y", Data = new Z(`
    # (the closing `});` on a subsequent line is `)`→ closing Z, `}` → closing init, `;` → end)
    # These are actually VALID - the issue was `});` closing them with an extra `)`
    # Correct form: ...CreatedAt\r\n    )};  where ) closes Ctor and } closes init
    # Check for `});` that closes a multiline Response initializer
    $content = [regex]::Replace($content,
        '(return new Response<[^>]+>\s*\{[^{}]*Data\s*=\s*new\s+\w+<?[^>]*>?\s*\()([^{}]+)(\s*\}\s*\)\s*;)',
        '$1$2$3')
    
    # ── 8. Ensure response variable exists for methods using it without init ──
    # Look for method bodies that use `response.StatusCode` but DON'T have `var response = new Response`
    # This is complex - do it per-method
    $methodPattern = [regex]::new(
        '(?ms)(    public (?:async )?Task<Response<([^>]+(?:<[^>]*>)?)>>[^\{]+\{)(.*?)(    \})',
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    )
    $content = $methodPattern.Replace($content, {
        param($m)
        $header  = $m.Groups[1].Value
        $retType = $m.Groups[2].Value
        $body    = $m.Groups[3].Value
        $footer  = $m.Groups[4].Value

        # If body uses response.XXX but doesn't initialize it
        if ($body -match 'response\.' -and $body -notmatch 'var response\s*=' -and $body -notmatch 'Response\<[^>]+\> response\s*=') {
            # Inject initialization at top of body, before first statement
            $initLine = "`r`n        var response = new Response<$retType>();`r`n        try`r`n        {"
            $closeTry = "`r`n        }`r`n        catch (Exception ex)`r`n        {`r`n            response.StatusCode = StatusCodes.Status500InternalServerError;`r`n            response.Message = Constants.Messages.ServerError;`r`n            return response;`r`n        }"
            
            # Only inject try/catch if not already wrapped
            if ($body -notmatch '\btry\b') {
                $newBody = $initLine + $body + $closeTry + "`r`n"
                return $header + $newBody + $footer
            }
        }
        return $m.Value
    })

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  FIXED: $($file.Name)"
        $fixed++
    } else {
        Write-Host "  OK:    $($file.Name)"
    }
}

Write-Host ""
Write-Host "Done. $fixed/$($files.Count) files updated."
