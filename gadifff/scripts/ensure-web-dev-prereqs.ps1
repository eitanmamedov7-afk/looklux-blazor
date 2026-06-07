
param(
    [int[]]$Ports = @(7166, 7167),
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

function Write-Info([string]$message) {
    Write-Host "[web-setup] $message"
}

function Get-ListenerProcessIds([int]$port) {
    try {
        return Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop |
            Select-Object -ExpandProperty OwningProcess -Unique
    }
    catch {
        return @()
    }
}

function Test-IsLookLuxWebProcess([int]$processId, [string]$repoRoot) {
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if (-not $process) {
        return $false
    }

    $normalizedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\')
    if (-not [string]::IsNullOrWhiteSpace($process.Path)) {
        $processPath = [System.IO.Path]::GetFullPath($process.Path)
        if ($processPath.StartsWith((Join-Path $normalizedRepoRoot 'gadifff\bin'), [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    $cimProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$processId" -ErrorAction SilentlyContinue
    $commandLine = if ($cimProcess) { $cimProcess.CommandLine } else { '' }
    if ([string]::IsNullOrWhiteSpace($commandLine)) {
        return $false
    }

    return $commandLine.IndexOf((Join-Path $normalizedRepoRoot 'gadifff\gadifff.csproj'), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $commandLine.IndexOf((Join-Path $normalizedRepoRoot 'gadifff\bin'), [StringComparison]::OrdinalIgnoreCase) -ge 0
}

foreach ($port in $Ports) {
    $processIds = Get-ListenerProcessIds -port $port
    foreach ($processId in $processIds) {
        if ($processId -eq $PID) {
            continue
        }

        if (Test-IsLookLuxWebProcess -processId $processId -repoRoot $RepoRoot) {
            Write-Info "Stopping stale web process on port $port (pid $processId)."
            Stop-Process -Id $processId -ErrorAction SilentlyContinue
            Start-Sleep -Milliseconds 500
        }
        else {
            Write-Info "Port $port is used by another process (pid $processId); leaving it alone."
        }
    }
}
