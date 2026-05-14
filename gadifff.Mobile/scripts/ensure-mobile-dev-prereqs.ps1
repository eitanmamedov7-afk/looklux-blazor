param()

$ErrorActionPreference = 'Stop'

function Write-Info([string]$message) {
    Write-Host "[mobile-setup] $message"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir
$repoRoot = Split-Path -Parent $projectDir
$backendProject = Join-Path $repoRoot 'gadifff\gadifff.csproj'
$backendUrl = 'http://127.0.0.1:7164'
$backendPort = 7164

if (-not (Test-Path $backendProject)) {
    Write-Info "Backend project not found at $backendProject"
    exit 0
}

function Is-PortListening([int]$port) {
    try {
        $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop
        return $null -ne $listener
    }
    catch {
        return $false
    }
}

function Wait-ForBackend([string]$url, [int]$timeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                return $true
            }
        }
        catch {
            Start-Sleep -Milliseconds 750
        }
    }

    return $false
}

if (-not (Is-PortListening -port $backendPort)) {
    Write-Info "Starting backend on $backendUrl"
    $args = @(
        'run'
        '--project'
        ('"' + $backendProject + '"')
        '--launch-profile'
        'http'
    )

    Start-Process -FilePath 'dotnet' -ArgumentList ($args -join ' ') -WorkingDirectory $repoRoot -WindowStyle Hidden | Out-Null

    if (Wait-ForBackend -url $backendUrl -timeoutSeconds 25) {
        Write-Info 'Backend is reachable.'
    }
    else {
        Write-Info 'Backend process started but did not become reachable within timeout.'
    }
}
else {
    Write-Info "Backend already listening on port $backendPort"
}

$adbCandidates = @()
if ($env:ANDROID_SDK_ROOT) {
    $adbCandidates += (Join-Path $env:ANDROID_SDK_ROOT 'platform-tools\adb.exe')
}
if ($env:ANDROID_HOME) {
    $adbCandidates += (Join-Path $env:ANDROID_HOME 'platform-tools\adb.exe')
}
$adbCandidates += (Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe')
$programFilesX86 = [Environment]::GetEnvironmentVariable('ProgramFiles(x86)')
if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
    $adbCandidates += (Join-Path $programFilesX86 'Android\android-sdk\platform-tools\adb.exe')
}
if ($env:ProgramFiles) {
    $adbCandidates += (Join-Path $env:ProgramFiles 'Android\android-sdk\platform-tools\adb.exe')
}
$adbCandidates += 'C:\Android\platform-tools\adb.exe'

$adbPath = $adbCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $adbPath) {
    Write-Info 'adb.exe not found; skipping adb reverse setup.'
    exit 0
}

& $adbPath start-server | Out-Null
$deviceLines = & $adbPath devices | Select-String '\tdevice$'
if (-not $deviceLines) {
    Write-Info 'No adb device in "device" state; skipping adb reverse.'
    exit 0
}

foreach ($line in $deviceLines) {
    $serial = ($line.Line -split "`t")[0].Trim()
    if ([string]::IsNullOrWhiteSpace($serial)) {
        continue
    }

    & $adbPath -s $serial reverse tcp:7164 tcp:7164 | Out-Null
    Write-Info "Configured adb reverse for $serial (tcp:7164 -> tcp:7164)"
}

exit 0
