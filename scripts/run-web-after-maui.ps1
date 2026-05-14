#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet("http", "https")]
    [string]$LaunchProfile = "http",
    [switch]$NoRun
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$webProject = Join-Path $repoRoot "gadifff\gadifff.csproj"

if (-not (Test-Path $webProject)) {
    throw "Web project not found at: $webProject"
}

$processIds = [System.Collections.Generic.HashSet[int]]::new()

Get-Process -Name "gadifff" -ErrorAction SilentlyContinue |
    ForEach-Object { [void]$processIds.Add($_.Id) }

$dotnetHosts = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue |
    Where-Object {
        $_.CommandLine -and (
            $_.CommandLine -like "*gadifff.dll*" -or
            $_.CommandLine -like "*gadifff.csproj*"
        )
    }

$dotnetHosts | ForEach-Object { [void]$processIds.Add([int]$_.ProcessId) }

if ($processIds.Count -gt 0) {
    Write-Host "Stopping stale web host process(es): $($processIds -join ', ')"
    foreach ($procId in $processIds) {
        try { Stop-Process -Id $procId -Force -ErrorAction Stop } catch { }
    }

    $timeout = [DateTime]::UtcNow.AddSeconds(8)
    while ([DateTime]::UtcNow -lt $timeout) {
        $alive = @()
        foreach ($procId in $processIds) {
            if (Get-Process -Id $procId -ErrorAction SilentlyContinue) {
                $alive += $procId
            }
        }

        if ($alive.Count -eq 0) { break }
        Start-Sleep -Milliseconds 250
    }
}

$baseOutputPath = "C:\maui-bin\run-web\"
New-Item -ItemType Directory -Path $baseOutputPath -Force | Out-Null

Set-Location $repoRoot
Write-Host "Starting web app with profile '$LaunchProfile' on isolated output path..."

if ($NoRun) {
    Write-Host "Prepared command:"
    Write-Host "dotnet run --project `"$webProject`" --launch-profile $LaunchProfile -p:UseAppHost=false -p:BaseOutputPath=$baseOutputPath"
    return
}

& dotnet run --project $webProject --launch-profile $LaunchProfile -p:UseAppHost=false -p:BaseOutputPath=$baseOutputPath
