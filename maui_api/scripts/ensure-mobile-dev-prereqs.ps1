# מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
# למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
# לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
# איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

param()

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$ErrorActionPreference = 'Stop'

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Write-Info([string]$message) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Host "[mobile-setup] $message"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$projectDir = Split-Path -Parent $scriptDir
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$repoRoot = Split-Path -Parent $projectDir
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendProject = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_BACKEND_CSPROJ)) {
    'C:\Users\Eitan\looklux-blazor\gadifff\gadifff.csproj'
}
else {
    $env:LOOKLUX_BACKEND_CSPROJ
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendRunDir = Join-Path $env:TEMP 'looklux-mobile-backend'
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$defaultBackendPort = 7166
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$configuredBackendPort = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_MOBILE_BACKEND_PORT)) {
    $defaultBackendPort
}
else {
    $env:LOOKLUX_MOBILE_BACKEND_PORT
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendPort = $defaultBackendPort
# שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
if ([int]::TryParse($configuredBackendPort, [ref]$backendPort) -and $backendPort -gt 0) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $backendPort = [int]$backendPort
}
else {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $backendPort = $defaultBackendPort
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendUrl = "http://0.0.0.0:$backendPort"
$backendProbeUrl = "http://127.0.0.1:$backendPort"
$autoStartBackend = $env:LOOKLUX_AUTO_START_BACKEND -eq '1'
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$preferredAvd = if ([string]::IsNullOrWhiteSpace($env:LOOKLUX_ANDROID_AVD)) { 'Pixel_9_API_35' } else { $env:LOOKLUX_ANDROID_AVD }

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not (Test-Path $backendProject)) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Backend project not found at $backendProject"
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    exit 0
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Is-PortListening([int]$port) {
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    try {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $null -ne $listener
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    catch {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $false
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Get-PortListenerProcessId([int]$port) {
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    try {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop |
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Select-Object -First 1 -ExpandProperty OwningProcess
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    catch {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $null
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
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

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Stop-KnownMobileBackendIfListening([int]$port) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $listenerPid = Get-PortListenerProcessId -port $port
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not $listenerPid) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $process = Get-Process -Id $listenerPid -ErrorAction SilentlyContinue
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not $process -or [string]::IsNullOrWhiteSpace($process.Path)) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $tempBackendDir = [System.IO.Path]::GetFullPath($backendRunDir).TrimEnd('\')
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $cimProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$listenerPid" -ErrorAction SilentlyContinue
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $commandLine = if ($cimProcess) { $cimProcess.CommandLine } else { '' }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $isTempBackend = -not [string]::IsNullOrWhiteSpace($commandLine) -and
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $commandLine.IndexOf($tempBackendDir, [StringComparison]::OrdinalIgnoreCase) -ge 0

    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($isTempBackend) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info "Stopping existing mobile backend on port $port (pid $listenerPid)."
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Stop-Process -Id $listenerPid -ErrorAction SilentlyContinue
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Sleep -Milliseconds 750
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Stop-TempMobileBackendProcesses() {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $tempBackendDir = [System.IO.Path]::GetFullPath($backendRunDir).TrimEnd('\')
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $stoppedAny = $false
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $processes = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue

    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    foreach ($proc in $processes) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $commandLine = $proc.CommandLine
        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            continue
        }

        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if ($commandLine.IndexOf($tempBackendDir, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            continue
        }

        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info "Stopping stale temp mobile backend process (pid $($proc.ProcessId))."
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Stop-Process -Id $proc.ProcessId -ErrorAction SilentlyContinue
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $stoppedAny = $true
    }

    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($stoppedAny) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Sleep -Milliseconds 750
    }
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Wait-ForBackend([string]$url, [int]$timeoutSeconds) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    while ((Get-Date) -lt $deadline) {
        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        try {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
            # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
                return $true
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }
        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        catch {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Start-Sleep -Milliseconds 750
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return $false
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Get-AdbDeviceLines([string]$adbPath) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return & $adbPath devices | Select-String "`tdevice$"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Get-EmulatorPath([string]$adbPath) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $platformToolsDir = Split-Path -Parent $adbPath
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $sdkDir = Split-Path -Parent $platformToolsDir
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $candidates = @(
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        (Join-Path $sdkDir 'emulator\emulator.exe'),
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        (Join-Path $env:LOCALAPPDATA 'Android\Sdk\emulator\emulator.exe')
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    )

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Get-AvdName([string]$emulatorPath, [string]$preferredName) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $avds = & $emulatorPath -list-avds | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($avds -contains $preferredName) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $preferredName
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return $avds | Select-Object -First 1
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
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
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    while ((Get-Date) -lt $deadline) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $deviceLines = Get-AdbDeviceLines -adbPath $adbPath
        $readyDeviceLines = @()

        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        foreach ($line in $deviceLines) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $serial = Get-DeviceSerialFromLine $line
            # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (Test-AndroidDeviceReady -adbPath $adbPath -serial $serial) {
                # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
                Prepare-AndroidDeviceForLaunch -adbPath $adbPath -serial $serial
                $readyDeviceLines += $line
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }

        if ($readyDeviceLines.Count -gt 0) {
            return $readyDeviceLines
        }

        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Sleep -Seconds 3
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return @()
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Ensure-AndroidDevice([string]$adbPath, [string]$preferredAvdName) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    & $adbPath start-server | Out-Null

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $deviceLines = Get-AdbDeviceLines -adbPath $adbPath
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($deviceLines) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'ADB device already connected.'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $readyDeviceLines = Wait-ForAndroidDevice -adbPath $adbPath -timeoutSeconds 60
        if ($readyDeviceLines) {
            Write-Info 'Connected Android device is ready.'
            return $readyDeviceLines
        }

        Write-Info 'Connected Android device did not finish booting within timeout.'
        return @()
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
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
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
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

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not $backendIsListening -and $autoStartBackend) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Publishing backend dev copy to $backendRunDir"
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    & dotnet publish $backendProject -c Debug -o $backendRunDir --nologo --disable-build-servers | Out-Host
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($LASTEXITCODE -ne 0) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'Backend publish failed; trying source-project fallback startup.'

        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $backendProjectDir = Split-Path -Parent $backendProject
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $previousAspnetcoreEnvironment = $env:ASPNETCORE_ENVIRONMENT
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Process -FilePath 'dotnet' `
            -ArgumentList @('run', '--project', $backendProject, '--no-build', '--urls', $backendUrl) `
            -WorkingDirectory $backendProjectDir `
            -WindowStyle Hidden | Out-Null
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $env:ASPNETCORE_ENVIRONMENT = $previousAspnetcoreEnvironment

        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (Wait-ForBackend -url $backendProbeUrl -timeoutSeconds 25) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info 'Backend fallback is reachable.'
        }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        else {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info 'Backend fallback did not become reachable within timeout.'
        }
    }
    else {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info "Starting backend on $backendUrl"
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $backendDll = Join-Path $backendRunDir 'gadifff.dll'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $previousAspnetcoreEnvironment = $env:ASPNETCORE_ENVIRONMENT
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Process -FilePath 'dotnet' -ArgumentList ('"{0}" --urls "{1}"' -f $backendDll, $backendUrl) -WorkingDirectory $backendRunDir -WindowStyle Hidden | Out-Null
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $env:ASPNETCORE_ENVIRONMENT = $previousAspnetcoreEnvironment

        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (Wait-ForBackend -url $backendProbeUrl -timeoutSeconds 25) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info 'Backend is reachable.'
        }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        else {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info 'Backend process started but did not become reachable within timeout.'
        }
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
else {
    if ($backendIsListening) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info "Backend already listening on port $backendPort"
    }
    else {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info "Backend is not listening on port $backendPort and auto-start is disabled. Start gadifff web first."
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$adbCandidates = @()
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if ($env:ANDROID_SDK_ROOT) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $adbCandidates += (Join-Path $env:ANDROID_SDK_ROOT 'platform-tools\adb.exe')
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if ($env:ANDROID_HOME) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $adbCandidates += (Join-Path $env:ANDROID_HOME 'platform-tools\adb.exe')
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$adbCandidates += (Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe')
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$programFilesX86 = [Environment]::GetEnvironmentVariable('ProgramFiles(x86)')
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $adbCandidates += (Join-Path $programFilesX86 'Android\android-sdk\platform-tools\adb.exe')
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if ($env:ProgramFiles) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $adbCandidates += (Join-Path $env:ProgramFiles 'Android\android-sdk\platform-tools\adb.exe')
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$adbCandidates += 'C:\Android\platform-tools\adb.exe'

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$adbPath = $adbCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not $adbPath) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info 'adb.exe not found; skipping adb reverse setup.'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    exit 0
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$deviceLines = Ensure-AndroidDevice -adbPath $adbPath -preferredAvdName $preferredAvd
# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not $deviceLines) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info 'No adb device in "device" state; skipping adb reverse.'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    exit 0
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
foreach ($line in $deviceLines) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $serial = ($line.Line -split "`t")[0].Trim()
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ([string]::IsNullOrWhiteSpace($serial)) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        continue
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    & $adbPath -s $serial reverse "tcp:$backendPort" "tcp:$backendPort" | Out-Null
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Configured adb reverse for $serial (tcp:$backendPort -> tcp:$backendPort)"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
exit 0
