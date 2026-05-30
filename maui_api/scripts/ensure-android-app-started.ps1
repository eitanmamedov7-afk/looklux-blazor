param(
    [string]$PackageName = "com.companyname.maui_api",
    [int]$WaitSeconds = 20
)

$ErrorActionPreference = "Stop"

function Write-Info([string]$message) {
    Write-Host "[mobile-run] $message"
}

function Get-AdbPath {
    $adbCandidates = @()

    if ($env:ANDROID_SDK_ROOT) {
        $adbCandidates += (Join-Path $env:ANDROID_SDK_ROOT "platform-tools\\adb.exe")
    }
    if ($env:ANDROID_HOME) {
        $adbCandidates += (Join-Path $env:ANDROID_HOME "platform-tools\\adb.exe")
    }

    $adbCandidates += (Join-Path $env:LOCALAPPDATA "Android\\Sdk\\platform-tools\\adb.exe")

    $programFilesX86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
    if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
        $adbCandidates += (Join-Path $programFilesX86 "Android\\android-sdk\\platform-tools\\adb.exe")
    }
    if ($env:ProgramFiles) {
        $adbCandidates += (Join-Path $env:ProgramFiles "Android\\android-sdk\\platform-tools\\adb.exe")
    }

    $adbCandidates += "C:\\Android\\platform-tools\\adb.exe"
    return $adbCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

function Get-ConnectedDevices([string]$adbPath) {
    $lines = & $adbPath devices
    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    $devices = @()
    foreach ($line in $lines) {
        if ($line -match "^(?<serial>\S+)\s+device$") {
            $devices += $Matches.serial
        }
    }

    return $devices
}

function Is-AppRunning([string]$adbPath, [string]$serial, [string]$packageName) {
    $appPid = (& $adbPath -s $serial shell pidof $packageName 2>$null).Trim()
    return -not [string]::IsNullOrWhiteSpace($appPid)
}

function Launch-App([string]$adbPath, [string]$serial, [string]$packageName) {
    & $adbPath -s $serial shell monkey -p $packageName -c android.intent.category.LAUNCHER 1 | Out-Null
    return $LASTEXITCODE -eq 0
}

$adbPath = Get-AdbPath
if (-not $adbPath) {
    Write-Info "adb.exe not found; skipping app start check."
    exit 0
}

$devices = Get-ConnectedDevices -adbPath $adbPath
if (-not $devices -or $devices.Count -eq 0) {
    Write-Info "No connected Android device/emulator; skipping app start check."
    exit 0
}

foreach ($serial in $devices) {
    if (Is-AppRunning -adbPath $adbPath -serial $serial -packageName $PackageName) {
        Write-Info "App already running on $serial."
        continue
    }

    Write-Info "App not running on $serial, launching $PackageName."
    [void](Launch-App -adbPath $adbPath -serial $serial -packageName $PackageName)

    $started = $false
    for ($i = 0; $i -lt $WaitSeconds; $i++) {
        Start-Sleep -Seconds 1
        if (Is-AppRunning -adbPath $adbPath -serial $serial -packageName $PackageName) {
            $started = $true
            break
        }
    }

    if ($started) {
        Write-Info "App started on $serial."
    }
    else {
        Write-Info "App still not running on $serial after launch attempt."
    }
}

exit 0
