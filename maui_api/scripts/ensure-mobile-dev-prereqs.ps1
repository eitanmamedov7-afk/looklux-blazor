
param()

$ErrorActionPreference = 'Stop'

function Write-Info([string]$message) {
    Write-Host "[mobile-setup] $message"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir
$repoRoot = Split-Path -Parent $projectDir
$backendProject = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_BACKEND_CSPROJ)) {
    'C:\Users\Eitan\looklux-blazor\gadifff\gadifff.csproj'
}
else {
    $env:LOOKLUX_BACKEND_CSPROJ
}
$backendRunDir = Join-Path $env:TEMP 'looklux-mobile-backend'
$defaultBackendPort = 7166
$configuredBackendPort = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_MOBILE_BACKEND_PORT)) {
    $defaultBackendPort
}
else {
    $env:LOOKLUX_MOBILE_BACKEND_PORT
}
$backendPort = $defaultBackendPort
if ([int]::TryParse($configuredBackendPort, [ref]$backendPort) -and $backendPort -gt 0) {
    $backendPort = [int]$backendPort
}
else {
    $backendPort = $defaultBackendPort
}
$backendUrl = "http://0.0.0.0:$backendPort"
$backendProbeUrl = "http://127.0.0.1:$backendPort"
$autoStartBackend = $env:LOOKLUX_AUTO_START_BACKEND -eq '1'
$preferredAvd = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_ANDROID_AVD)) { 'Pixel_9_API_35' } else { $env:LOOKLUX_ANDROID_AVD }

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

function Get-PortListenerProcessId([int]$port) {
    try {
        return Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop |
            Select-Object -First 1 -ExpandProperty OwningProcess
    }
    catch {
        return $null
    }
}

# Attempts to find a backend process that is running from the backend project's bin output.
function Get-BackendProcessFromProjectOutput([string]$projectPath) {
    $projectDir = Split-Path -Parent $projectPath
    $outputRoot = [System.IO.Path]::GetFullPath((Join-Path $projectDir 'bin')).TrimEnd('\')
    $processName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)

    $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
    foreach ($process in $processes) {
        if ([string]::IsNullOrWhiteSpace($process.Path)) {
            continue
        }

        $processPath = [System.IO.Path]::GetFullPath($process.Path)
        if ($processPath.StartsWith($outputRoot, [StringComparison]::OrdinalIgnoreCase)) {
            return $process
        }
    }

    return $null
}

# Returns unique listening TCP ports for a process id.
function Get-ListeningPortsForProcessId([int]$processId) {
    try {
        return Get-NetTCPConnection -State Listen -OwningProcess $processId -ErrorAction Stop |
            Select-Object -ExpandProperty LocalPort -Unique
    }
    catch {
        return @()
    }
}

function Stop-KnownMobileBackendIfListening([int]$port) {
    $listenerPid = Get-PortListenerProcessId -port $port
    if (-not $listenerPid) {
        return
    }

    $process = Get-Process -Id $listenerPid -ErrorAction SilentlyContinue
    if (-not $process -or [string]::IsNullOrWhiteSpace($process.Path)) {
        return
    }

    $tempBackendDir = [System.IO.Path]::GetFullPath($backendRunDir).TrimEnd('\')

    $cimProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$listenerPid" -ErrorAction SilentlyContinue
    $commandLine = if ($cimProcess) { $cimProcess.CommandLine } else { '' }
    $isTempBackend = -not [string]::IsNullOrWhiteSpace($commandLine) -and
        $commandLine.IndexOf($tempBackendDir, [StringComparison]::OrdinalIgnoreCase) -ge 0

    if ($isTempBackend) {
        Write-Info "Stopping existing mobile backend on port $port (pid $listenerPid)."
        Stop-Process -Id $listenerPid -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 750
    }
}

function Stop-TempMobileBackendProcesses() {
    $tempBackendDir = [System.IO.Path]::GetFullPath($backendRunDir).TrimEnd('\')
    $stoppedAny = $false
    $processes = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue

    foreach ($proc in $processes) {
        $commandLine = $proc.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            continue
        }

        if ($commandLine.IndexOf($tempBackendDir, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
            continue
        }

        Write-Info "Stopping stale temp mobile backend process (pid $($proc.ProcessId))."
        Stop-Process -Id $proc.ProcessId -ErrorAction SilentlyContinue
        $stoppedAny = $true
    }

    if ($stoppedAny) {
        Start-Sleep -Milliseconds 750
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

function Get-AdbDeviceLines([string]$adbPath) {
    return & $adbPath devices | Select-String "`tdevice$"
}

function Get-EmulatorPath([string]$adbPath) {
    $platformToolsDir = Split-Path -Parent $adbPath
    $sdkDir = Split-Path -Parent $platformToolsDir
    $candidates = @(
        (Join-Path $sdkDir 'emulator\emulator.exe'),
        (Join-Path $env:LOCALAPPDATA 'Android\Sdk\emulator\emulator.exe')
    )

    return $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

function Get-AvdName([string]$emulatorPath, [string]$preferredName) {
    $avds = & $emulatorPath -list-avds | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    if ($avds -contains $preferredName) {
        return $preferredName
    }

    return $avds | Select-Object -First 1
}

function Get-DeviceSerialFromLine($line) {
    if ($line -is [string]) {
        return ($line -split "`t")[0].Trim()
    }

    return ($line.Line -split "`t")[0].Trim()
}

function Test-AndroidDeviceReady([string]$adbPath, [string]$serial) {
    if ([string]::IsNullOrWhiteSpace($serial)) {
        return $false
    }

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

    return $sysBootCompleted -eq '1' -and
        $devBootCompleted -eq '1' -and
        $bootAnimation -eq 'stopped' -and
        -not [string]::IsNullOrWhiteSpace($packageManagerReady)
}

function Prepare-AndroidDeviceForLaunch([string]$adbPath, [string]$serial) {
    & $adbPath -s $serial shell input keyevent KEYCODE_WAKEUP 2>$null | Out-Null
    & $adbPath -s $serial shell wm dismiss-keyguard 2>$null | Out-Null
}

function Wait-ForAndroidDevice([string]$adbPath, [int]$timeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $deviceLines = Get-AdbDeviceLines -adbPath $adbPath
        $readyDeviceLines = @()

        foreach ($line in $deviceLines) {
            $serial = Get-DeviceSerialFromLine $line
            if (Test-AndroidDeviceReady -adbPath $adbPath -serial $serial) {
                Prepare-AndroidDeviceForLaunch -adbPath $adbPath -serial $serial
                $readyDeviceLines += $line
            }
        }

        if ($readyDeviceLines.Count -gt 0) {
            return $readyDeviceLines
        }

        Start-Sleep -Seconds 3
    }

    return @()
}

function Ensure-AndroidDevice([string]$adbPath, [string]$preferredAvdName) {
    & $adbPath start-server | Out-Null

    $deviceLines = Get-AdbDeviceLines -adbPath $adbPath
    if ($deviceLines) {
        Write-Info 'ADB device already connected.'
        $readyDeviceLines = Wait-ForAndroidDevice -adbPath $adbPath -timeoutSeconds 60
        if ($readyDeviceLines) {
            Write-Info 'Connected Android device is ready.'
            return $readyDeviceLines
        }

        Write-Info 'Connected Android device did not finish booting within timeout.'
        return @()
    }

    $emulatorPath = Get-EmulatorPath -adbPath $adbPath
    if (-not $emulatorPath) {
        Write-Info 'emulator.exe not found. Start an Android emulator from Visual Studio and run again.'
        return @()
    }

    $avdName = Get-AvdName -emulatorPath $emulatorPath -preferredName $preferredAvdName
    if ([string]::IsNullOrWhiteSpace($avdName)) {
        Write-Info 'No Android Virtual Device found. Create an emulator in Visual Studio and run again.'
        return @()
    }

    Write-Info "Starting Android emulator $avdName."
    Start-Process -FilePath $emulatorPath -ArgumentList @('-avd', $avdName, '-no-snapshot-load') | Out-Null

    $deviceLines = Wait-ForAndroidDevice -adbPath $adbPath -timeoutSeconds 180
    if ($deviceLines) {
        Write-Info "Android emulator $avdName is ready."
        return $deviceLines
    }

    Write-Info "Android emulator $avdName did not become ready within timeout."
    return @()
}

Stop-TempMobileBackendProcesses
Stop-KnownMobileBackendIfListening -port $backendPort

# If backend is running from project output on a different port, avoid publishing over locked files.
$backendIsListening = Is-PortListening -port $backendPort
if (-not $backendIsListening -and $autoStartBackend) {
    $projectBackendProcess = Get-BackendProcessFromProjectOutput -projectPath $backendProject
    if ($projectBackendProcess) {
        $processPorts = @(Get-ListeningPortsForProcessId -processId $projectBackendProcess.Id)
        if ($processPorts.Count -gt 0) {
            $portsText = ($processPorts | Sort-Object | ForEach-Object { $_.ToString() }) -join ', '
            Write-Info "Backend process is already running from project output (pid $($projectBackendProcess.Id), ports: $portsText). Skipping publish to avoid file-lock conflicts."

            $backendIsListening = Is-PortListening -port $backendPort
        }
    }
}

if (-not $backendIsListening -and $autoStartBackend) {
    Write-Info "Publishing backend dev copy to $backendRunDir"
    & dotnet publish $backendProject -c Debug -o $backendRunDir --nologo --disable-build-servers | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Write-Info 'Backend publish failed; trying source-project fallback startup.'

        $backendProjectDir = Split-Path -Parent $backendProject
        $previousAspnetcoreEnvironment = $env:ASPNETCORE_ENVIRONMENT
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        Start-Process -FilePath 'dotnet' `
            -ArgumentList @('run', '--project', $backendProject, '--no-build', '--urls', $backendUrl) `
            -WorkingDirectory $backendProjectDir `
            -WindowStyle Hidden | Out-Null
        $env:ASPNETCORE_ENVIRONMENT = $previousAspnetcoreEnvironment

        if (Wait-ForBackend -url $backendProbeUrl -timeoutSeconds 25) {
            Write-Info 'Backend fallback is reachable.'
        }
        else {
            Write-Info 'Backend fallback did not become reachable within timeout.'
        }
    }
    else {
        Write-Info "Starting backend on $backendUrl"
        $backendDll = Join-Path $backendRunDir 'gadifff.dll'
        $previousAspnetcoreEnvironment = $env:ASPNETCORE_ENVIRONMENT
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        Start-Process -FilePath 'dotnet' -ArgumentList ('"{0}" --urls "{1}"' -f $backendDll, $backendUrl) -WorkingDirectory $backendRunDir -WindowStyle Hidden | Out-Null
        $env:ASPNETCORE_ENVIRONMENT = $previousAspnetcoreEnvironment

        if (Wait-ForBackend -url $backendProbeUrl -timeoutSeconds 25) {
            Write-Info 'Backend is reachable.'
        }
        else {
            Write-Info 'Backend process started but did not become reachable within timeout.'
        }
    }
}
else {
    if ($backendIsListening) {
        Write-Info "Backend already listening on port $backendPort"
    }
    else {
        Write-Info "Backend is not listening on port $backendPort and auto-start is disabled. Start gadifff web first."
    }
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

$deviceLines = Ensure-AndroidDevice -adbPath $adbPath -preferredAvdName $preferredAvd
if (-not $deviceLines) {
    Write-Info 'No adb device in "device" state; skipping adb reverse.'
    exit 0
}

foreach ($line in $deviceLines) {
    $serial = ($line.Line -split "`t")[0].Trim()
    if ([string]::IsNullOrWhiteSpace($serial)) {
        continue
    }

    & $adbPath -s $serial reverse "tcp:$backendPort" "tcp:$backendPort" | Out-Null
    Write-Info "Configured adb reverse for $serial (tcp:$backendPort -> tcp:$backendPort)"
}

exit 0
