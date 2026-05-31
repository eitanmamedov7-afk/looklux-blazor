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

function Test-AndroidDeviceReady([string]$adbPath, [string]$serial) {
    try {
        $sysBootCompleted = ((& $adbPath -s $serial shell getprop sys.boot_completed 2>$null) | Out-String).Trim()
        $devBootCompleted = ((& $adbPath -s $serial shell getprop dev.bootcomplete 2>$null) | Out-String).Trim()
        $bootAnimation = ((& $adbPath -s $serial shell getprop init.svc.bootanim 2>$null) | Out-String).Trim()
        $packageManagerOutput = & $adbPath -s $serial shell cmd package list packages android 2>$null
    }
    catch {
        return $false
    }

    $packageManagerReady = @($packageManagerOutput) |
        ForEach-Object { $_.ToString().Trim() } |
        Where-Object { $_ -eq 'package:android' } |
        Select-Object -First 1

    return $sysBootCompleted -eq "1" -and
        $devBootCompleted -eq "1" -and
        $bootAnimation -eq "stopped" -and
        -not [string]::IsNullOrWhiteSpace($packageManagerReady)
}

function Wait-ForAndroidDeviceReady([string]$adbPath, [string]$serial, [int]$timeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-AndroidDeviceReady -adbPath $adbPath -serial $serial) {
            & $adbPath -s $serial shell input keyevent KEYCODE_WAKEUP 2>$null | Out-Null
            & $adbPath -s $serial shell wm dismiss-keyguard 2>$null | Out-Null
            return $true
        }

        Start-Sleep -Seconds 2
    }

    return $false
}

function Is-AppRunning([string]$adbPath, [string]$serial, [string]$packageName) {
    $pidOutput = & $adbPath -s $serial shell pidof $packageName 2>$null
    if ($null -eq $pidOutput) {
        return $false
    }

    $appPid = ($pidOutput | Out-String).Trim()
    return -not [string]::IsNullOrWhiteSpace($appPid)
}

function Launch-App([string]$adbPath, [string]$serial, [string]$packageName) {
    & $adbPath -s $serial shell am force-stop $packageName 2>$null | Out-Null
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
        Write-Info "Stopping stale $PackageName process on $serial."
        & $adbPath -s $serial shell am force-stop $PackageName 2>$null | Out-Null
    }

    if (-not (Wait-ForAndroidDeviceReady -adbPath $adbPath -serial $serial -timeoutSeconds 60)) {
        Write-Info "Android device $serial is connected but not ready; skipping app launch."
        continue
    }

    if (Is-AppRunning -adbPath $adbPath -serial $serial -packageName $PackageName) {
        Write-Info "Restarting $PackageName on $serial."
    }
    else {
        Write-Info "Launching $PackageName on $serial."
    }

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
