# FixServices.ps1
# Fixes try/catch blocks in C# services using brace matching.

$servicesDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend\src\TaxOmbud.Application\Services"
$files = Get-ChildItem -Path $servicesDir -Filter "*.cs"

function Fix-MethodBody {
    param(
        [string]$body,
        [string]$header
    )
    
    if ($body -notmatch 'catch\s*\(\s*Exception\s+ex\s*\)') {
        # If it doesn't have try/catch at all, but uses response., let's wrap it in try/catch!
        if ($body -match 'response\.' -and $body -notmatch '\btry\b') {
            # Ensure response is initialized
            if ($body -notmatch 'var\s+response\s*=' -and $body -notmatch 'Response\s*<') {
                # Get the return type from header
                if ($header -match 'Response\s*<\s*(.*?)\s*>\s*>') {
                    $retType = $Matches[1]
                } else {
                    $retType = "object?"
                }
                $initCode = "`r`n        var response = new Response<$retType>();"
                $body = $initCode + $body
            }
            
            # Find response initialization line to wrap after it
            $lines = $body -split "\r?\n"
            $initIdx = -1
            for ($j = 0; $j -lt $lines.Length; $j++) {
                if ($lines[$j] -match 'var\s+response\s*=' -or $lines[$j] -match 'Response\s*<') {
                    $initIdx = $j
                    break
                }
            }
            
            if ($initIdx -ne -1) {
                $before = $lines[0..$initIdx] -join "`r`n"
                if ($initIdx + 1 -lt $lines.Length) {
                    $after = $lines[($initIdx+1)..($lines.Length-1)] -join "`r`n"
                } else {
                    $after = ""
                }
                $wrapped = "$before`r`n        try`r`n        {`r`n$after`r`n        }`r`n        catch (Exception ex)`r`n        {`r`n            response.StatusCode = StatusCodes.Status500InternalServerError;`r`n            response.Message = Constants.Messages.ServerError;`r`n            return response;`r`n        }"
                return $wrapped
            } else {
                $wrapped = "        try`r`n        {`r`n$body`r`n        }`r`n        catch (Exception ex)`r`n        {`r`n            response.StatusCode = StatusCodes.Status500InternalServerError;`r`n            response.Message = Constants.Messages.ServerError;`r`n            return response;`r`n        }"
                return $wrapped
            }
        }
        return $body
    }
    
    # It has catch (Exception ex)
    $bodyClean = $body
    
    # 1. Remove the "try" block start (e.g. try { )
    $bodyClean = $bodyClean -replace '(?m)^[ \t]*try[ \t]*\r?\n?[ \t]*\{[ \t]*\r?\n?', ''
    
    # 2. Remove the premature catch block:
    # catch (Exception ex) { ... } }
    $catchPattern = [regex]'(?s)catch\s*\(\s*Exception\s+ex\s*\)\s*\{.*?\}\s*\}'
    $bodyClean = $catchPattern.Replace($bodyClean, '')
    
    # 3. Now wrap the entire clean body (after the response declaration) in try/catch.
    $lines = $bodyClean -split "\r?\n"
    $initIdx = -1
    for ($j = 0; $j -lt $lines.Length; $j++) {
        if ($lines[$j] -match 'var\s+response\s*=' -or $lines[$j] -match 'Response\s*<') {
            $initIdx = $j
            break
        }
    }
    
    if ($initIdx -ne -1) {
        $before = $lines[0..$initIdx] -join "`r`n"
        if ($initIdx + 1 -lt $lines.Length) {
            $after = $lines[($initIdx+1)..($lines.Length-1)] -join "`r`n"
        } else {
            $after = ""
        }
        $wrapped = "$before`r`n        try`r`n        {`r`n$after`r`n        }`r`n        catch (Exception ex)`r`n        {`r`n            response.StatusCode = StatusCodes.Status500InternalServerError;`r`n            response.Message = Constants.Messages.ServerError;`r`n            return response;`r`n        }"
        return $wrapped
    } else {
        $wrapped = "        try`r`n        {`r`n$bodyClean`r`n        }`r`n        catch (Exception ex)`r`n        {`r`n            response.StatusCode = StatusCodes.Status500InternalServerError;`r`n            response.Message = Constants.Messages.ServerError;`r`n            return response;`r`n        }"
        return $wrapped
    }
}

Write-Host "Scanning $($files.Count) files in $servicesDir..."
$fixedCount = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    # Match public [async] Task<Response<...>> MethodName(...) {
    $pattern = [regex]'(?s)(\bpublic\s+(?:async\s+)?Task\s*<\s*Response\s*<.*?\s+(\w+)\s*\(.*?\{)'
    
    $matches = $pattern.Matches($content)
    $newContent = ""
    $lastEnd = 0
    
    for ($idx = 0; $idx -lt $matches.Count; $idx++) {
        $m = $matches[$idx]
        $startIdx = $m.Index
        
        # Skip if this match starts before the end of the last matched method body
        if ($startIdx -lt $lastEnd) {
            continue
        }
        
        $header = $m.Groups[1].Value
        $methodName = $m.Groups[2].Value
        $startBraceIdx = $startIdx + $header.Length - 1 # index of '{'
        
        # Brace count to find matching '}' for the method
        $braceCount = 1
        $i = $startBraceIdx + 1
        while ($i -lt $content.Length -and $braceCount -gt 0) {
            $char = $content[$i]
            if ($char -eq '{') {
                $braceCount++
            } elseif ($char -eq '}') {
                $braceCount--
            }
            $i++
        }
        
        # If braces are mismatched, log a warning and skip
        if ($braceCount -gt 0) {
            Write-Warning "Mismatched braces in method $methodName of file $($file.Name)"
            continue
        }
        
        # Append anything from the last end to the start of this match
        $newContent += $content.Substring($lastEnd, $startIdx - $lastEnd)
        
        $methodEndIdx = $i
        $body = $content.Substring($startBraceIdx + 1, $methodEndIdx - $startBraceIdx - 2)
        
        # Fix the body
        $fixedBody = Fix-MethodBody $body $header
        
        $newContent += $header + $fixedBody + "}"
        $lastEnd = $methodEndIdx
    }
    
    $newContent += $content.Substring($lastEnd)
    
    if ($newContent -ne $original) {
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "  Fixed: $($file.Name)"
        $fixedCount++
    }
}

Write-Host "Completed. Fixed $fixedCount files."
