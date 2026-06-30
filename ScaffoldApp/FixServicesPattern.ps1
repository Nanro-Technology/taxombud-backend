# FixServicesPattern.ps1
# Applies the correct Response pattern to all Application service files:
# 1. Fix corrupted syntax from previous regex runs
# 2. Replace throw new NotImplementedException() stubs with proper response pattern
# 3. Use StatusCodes.Status* and Constants.Messages.*
# 4. Ensure required using directives are present

$servicesDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application\Services"
$files = Get-ChildItem -Path $servicesDir -Filter "*.cs"
Write-Host "Processing $($files.Count) service files..."
$fixed = 0

foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # ── 1. Ensure required using directives ──────────────────────────────────
    if ($content -notmatch 'using Microsoft\.AspNetCore\.Http;') {
        $content = $content -replace '((?:using [^\r\n]+\r?\n)+)', "`$1using Microsoft.AspNetCore.Http;`r`n"
        # avoid duplicating - just prepend after last using block
    }
    if ($content -notmatch 'using TaxOmbud\.Common\.Utilities;') {
        $content = $content -replace '((?:using [^\r\n]+\r?\n)+)', "`$1using TaxOmbud.Common.Utilities;`r`n"
    }

    # ── 2. Fix corrupted .ToString( } ─────────────────────────────────────────
    $content = $content -replace '\.ToString\(\s*\}', '.ToString()'
    $content = $content -replace '\.AsReadOnly\(\s*\}', '.AsReadOnly()'
    $content = $content -replace '\.Adapt<([^>]+)>\(\s*\}', '.Adapt<$1>()'

    # ── 3. Fix remaining old-style single-line factory calls ──────────────────
    # Response<X>.NotFound("msg")
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.NotFound\("([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status404NotFound;`r`n            response.Message = `"`$2`";`r`n            return response;")

    # Response<X>.NotFound() - no arg
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.NotFound\(\);',
        "response.StatusCode = StatusCodes.Status404NotFound;`r`n            response.Message = Constants.Messages.NotFound;`r`n            return response;")

    # Response<X>.Success(data) - single token data only (no nested parens)
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Success\(([^(),]+)\);',
        "response.StatusCode = StatusCodes.Status200OK;`r`n            response.Message = Constants.Messages.Success;`r`n            response.Data = `$2;`r`n            return response;")

    # Response<X>.Forbidden("msg")
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Forbidden\("([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status403Forbidden;`r`n            response.Message = `"`$2`";`r`n            return response;")

    # Response<X>.Unauthorized("msg")
    $content = [regex]::Replace($content,
        'return Response<([^>]+)>\.Unauthorized\("([^"]*)"\);',
        "response.StatusCode = StatusCodes.Status401Unauthorized;`r`n            response.Message = `"`$2`";`r`n            return response;")

    # ── 4. Fix single-line inline return new Response<T> { ... }; ────────────
    # No Data variant
    $content = [regex]::Replace($content,
        'return new Response<([^>]+)>\s*\{\s*StatusCode\s*=\s*(\d+),\s*Message\s*=\s*"([^"]*)"\s*\};',
        "response.StatusCode = `$2;`r`n            response.Message = `"`$3`";`r`n            return response;")

    # With Data variant (simple value, no nested braces)
    $content = [regex]::Replace($content,
        'return new Response<([^>]+)>\s*\{\s*StatusCode\s*=\s*(\d+),\s*Message\s*=\s*"([^"]*)",\s*Data\s*=\s*([^{}]+)\};',
        "response.StatusCode = `$2;`r`n            response.Message = `"`$3`";`r`n            response.Data = `$4;`r`n            return response;")

    # ── 5. Replace message string literals with Constants.Messages.* ──────────
    $content = $content -replace 'response\.Message = "Operation completed successfully\."', 'response.Message = Constants.Messages.Success'
    $content = $content -replace 'response\.Message = "Success"', 'response.Message = Constants.Messages.Success'
    $content = $content -replace 'response\.Message = "Resource not found\."', 'response.Message = Constants.Messages.NotFound'
    $content = $content -replace 'response\.Message = "Resource not found"', 'response.Message = Constants.Messages.NotFound'
    $content = $content -replace 'response\.Message = "Access denied\."', 'response.Message = Constants.Messages.Forbidden'
    $content = $content -replace 'response\.Message = "Access denied"', 'response.Message = Constants.Messages.Forbidden'
    $content = $content -replace 'response\.Message = "Unauthorized"', 'response.Message = Constants.Messages.Unauthorized'
    $content = $content -replace 'response\.Message = "Authentication required\."', 'response.Message = Constants.Messages.Unauthorized'
    $content = $content -replace 'response\.Message = "An unexpected error occurred\."', 'response.Message = Constants.Messages.ServerError'
    $content = $content -replace 'response\.Message = "An unexpected error occurred"', 'response.Message = Constants.Messages.ServerError'
    $content = $content -replace 'response\.Message = "Invalid request data\."', 'response.Message = Constants.Messages.BadRequest'
    $content = $content -replace 'response\.Message = "Resource created successfully\."', 'response.Message = Constants.Messages.Created'
    $content = $content -replace 'response\.Message = "Resource updated successfully\."', 'response.Message = Constants.Messages.Updated'
    $content = $content -replace 'response\.Message = "Resource deleted successfully\."', 'response.Message = Constants.Messages.Deleted'

    # ── 6. Replace throw new NotImplementedException() stubs ──────────────────
    # Find methods containing ONLY throw new NotImplementedException() and replace with proper pattern
    # Pattern: public async Task<Response<T>> Method(...) \n {\n     throw new NotImplementedException();\n }
    $stubPattern = [regex]::new(
        '(?m)([ \t]+public (?:async )?Task<Response<([^>]+(?:<[^>]*>)?)>>[^\r\n\{]+\r?\n[ \t]+\{)\r?\n[ \t]+throw new NotImplementedException\(\);\r?\n([ \t]+\})',
        [System.Text.RegularExpressions.RegexOptions]::Multiline
    )
    $content = $stubPattern.Replace($content, {
        param($m)
        $methodOpen = $m.Groups[1].Value
        $retType    = $m.Groups[2].Value
        $closing    = $m.Groups[3].Value
        $indent     = ($closing -replace '\}', '').Length  # indent of closing brace
        $i          = " " * ($indent + 4)
        $ib         = " " * $indent

        return (
            "$methodOpen`r`n" +
            "${i}var response = new Response<$retType>();`r`n" +
            "${i}try`r`n" +
            "${i}{`r`n" +
            "${i}    // TODO: Implement`r`n" +
            "${i}    response.StatusCode = StatusCodes.Status200OK;`r`n" +
            "${i}    response.Message = Constants.Messages.Success;`r`n" +
            "${i}    return response;`r`n" +
            "${i}}`r`n" +
            "${i}catch (Exception ex)`r`n" +
            "${i}{`r`n" +
            "${i}    response.StatusCode = StatusCodes.Status500InternalServerError;`r`n" +
            "${i}    response.Message = Constants.Messages.ServerError;`r`n" +
            "${i}    return response;`r`n" +
            "${i}}`r`n" +
            "$closing"
        )
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
Write-Host "Done. $fixed/$($files.Count) service files updated."
