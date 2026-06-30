# RegenerateAndFix.ps1
$rootDir = "c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend"
$servicesDir = "$rootDir\src\TaxOmbud.Application\Services"

Write-Host "1. Deleting existing service files..."
Remove-Item -Path "$servicesDir\*.cs" -Force -ErrorAction SilentlyContinue

Write-Host "2. Deleting old DTO and Validator directories..."
Get-ChildItem -Path "$rootDir\src\TaxOmbud.Application" -Directory | ForEach-Object {
    Remove-Item -Path "$($_.FullName)\DTOs" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$($_.FullName)\Validators" -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "3. Running ScaffoldApp to generate clean service, DTO, and Validator files..."
dotnet run --project "$rootDir\ScaffoldApp\ScaffoldApp.csproj"

Write-Host "4. Wrapping methods in try/catch..."
$files = Get-ChildItem -Path $servicesDir -Filter "*.cs"

function Fix-MethodBody {
    param(
        [string]$body,
        [string]$header
    )
    
    # Get the return type from header
    if ($header -match 'Response\s*<\s*(.*?)\s*>\s*>') {
        $retType = $Matches[1]
    } else {
        $retType = "object?"
    }
    
    # Initialize response if not already present
    if ($body -notmatch 'var\s+response\s*=' -and $body -notmatch 'Response\s*<') {
        $initCode = "`r`n        var response = new Response<$retType>();"
        $body = $initCode + $body
    }
    
    # Split lines and find the response initialization
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
        
        if ($braceCount -gt 0) {
            Write-Warning "Mismatched braces in method $methodName of file $($file.Name)"
            continue
        }
        
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
        Write-Host "  Processed: $($file.Name)"
    }
}

Write-Host "All clean services generated and wrapped in try/catch."
