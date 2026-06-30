# FixResponsePattern.ps1
# Replaces Response<T>.Success/Fail/NotFound/etc. static factory calls with 
# the direct property-assignment pattern used by Estate-Management-Backend.

$root = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src"

$files = Get-ChildItem -Path $root -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

Write-Host "Scanning $($files.Count) files for Response factory calls..."
$fixed = 0

foreach ($file in $files) {
    $content  = Get-Content $file.FullName -Raw
    $original = $content

    # ── Remove Errors property references ──────────────────────────────────────
    $content = $content -replace '(?m)^\s+public List<string>\? Errors.*\r?\n', ''
    $content = $content -replace ',\s*errors\s*=\s*\w+', ''
    $content = $content -replace '\s*Errors\s*=\s*[^,;\r\n]+[,;]?', ''

    # ── Replace static factory calls with direct object-initializer ────────────
    # Response<T>.Success(data, "msg", code) — with all 3 args
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Success\(([^,)]+),\s*"([^"]*)",\s*(\d+)\)',
        'new Response<$1> { StatusCode = $4, Message = "$3", Data = $2 }')

    # Response<T>.Success(data, "msg") — 2 args
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Success\(([^,)]+),\s*"([^"]*)"\)',
        'new Response<$1> { StatusCode = 200, Message = "$3", Data = $2 }')

    # Response<T>.Success(data) — 1 arg (no message)
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Success\(([^)]+)\)',
        'new Response<$1> { StatusCode = 200, Message = "Success", Data = $2 }')

    # Response<T>.Fail("msg", code)
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Fail\("([^"]*)",\s*(\d+)[^)]*\)',
        'new Response<$1> { StatusCode = $3, Message = "$2" }')

    # Response<T>.Fail("msg")
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Fail\("([^"]*)"\)',
        'new Response<$1> { StatusCode = 400, Message = "$2" }')

    # Response<T>.NotFound("msg")
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.NotFound\("([^"]*)"\)',
        'new Response<$1> { StatusCode = 404, Message = "$2" }')

    # Response<T>.NotFound() — no arg
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.NotFound\(\)',
        'new Response<$1> { StatusCode = 404, Message = "Resource not found" }')

    # Response<T>.Unauthorized("msg")
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Unauthorized\("([^"]*)"\)',
        'new Response<$1> { StatusCode = 401, Message = "$2" }')

    # Response<T>.Unauthorized()
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Unauthorized\(\)',
        'new Response<$1> { StatusCode = 401, Message = "Unauthorized" }')

    # Response<T>.Forbidden("msg")
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Forbidden\("([^"]*)"\)',
        'new Response<$1> { StatusCode = 403, Message = "$2" }')

    # Response<T>.Forbidden()
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.Forbidden\(\)',
        'new Response<$1> { StatusCode = 403, Message = "Access denied" }')

    # Response<T>.ServerError("msg")
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.ServerError\("([^"]*)"\)',
        'new Response<$1> { StatusCode = 500, Message = "$2" }')

    # Response<T>.ServerError()
    $content = [regex]::Replace($content,
        'Response<([^>]+)>\.ServerError\(\)',
        'new Response<$1> { StatusCode = 500, Message = "An unexpected error occurred" }')

    # ── Replace PagedResponse<T> with PagedResult<T> in return types ──────────
    # (AuditLogsService was using PagedResponse instead of PagedResult)
    $content = $content -replace 'new PagedResponse<([^>]+)>', 'new PagedResult<$1>'
    $content = $content -replace 'Response<PagedResponse<([^>]+)>>', 'Response<PagedResult<$1>>'

    # ── Remove TaxOmbud.Common.Responses.Result leftover refs ─────────────────
    $content = $content -replace 'TaxOmbud\.Common\.Responses\.Result<', 'Response<'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "  FIXED: $($file.Name)"
        $fixed++
    }
}

Write-Host ""
Write-Host "Done. Fixed $fixed files."
