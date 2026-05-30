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
$backendProject = Join-Path $repoRoot 'gadifff\gadifff.csproj'
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendRunDir = Join-Path $env:TEMP 'looklux-mobile-backend'
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendUrl = 'http://127.0.0.1:7164'
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$backendPort = 7164
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
    $buildOutputDir = Join-Path $repoRoot 'gadifff\bin'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $tempBackendDir = [System.IO.Path]::GetFullPath($backendRunDir).TrimEnd('\')
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $processPath = [System.IO.Path]::GetFullPath($process.Path)

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $isRepoBuildOutput = $processPath.StartsWith($buildOutputDir, [StringComparison]::OrdinalIgnoreCase)
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $cimProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$listenerPid" -ErrorAction SilentlyContinue
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $commandLine = if ($cimProcess) { $cimProcess.CommandLine } else { '' }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $isTempBackend = -not [string]::IsNullOrWhiteSpace($commandLine) -and
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $commandLine.IndexOf($tempBackendDir, [StringComparison]::OrdinalIgnoreCase) -ge 0

    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($isRepoBuildOutput -or $isTempBackend) {
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
function Wait-ForAndroidDevice([string]$adbPath, [int]$timeoutSeconds) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    while ((Get-Date) -lt $deadline) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $deviceLines = Get-AdbDeviceLines -adbPath $adbPath
        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        foreach ($line in $deviceLines) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $serial = ($line.Line -split "`t")[0].Trim()
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $bootCompleted = (& $adbPath -s $serial shell getprop sys.boot_completed 2>$null).Trim()
            # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if ($bootCompleted -eq '1') {
                # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
                return $deviceLines
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
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
        return $deviceLines
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $emulatorPath = Get-EmulatorPath -adbPath $adbPath
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not $emulatorPath) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'emulator.exe not found; cannot start an Android emulator.'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return @()
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $avdName = Get-AvdName -emulatorPath $emulatorPath -preferredName $preferredAvdName
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not $avdName) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'No Android virtual devices found; create one in Android Device Manager.'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return @()
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Starting Android emulator '$avdName'."
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Start-Process -FilePath $emulatorPath -ArgumentList @('-avd', $avdName, '-netdelay', 'none', '-netspeed', 'full') | Out-Null

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info 'Waiting for Android emulator to finish booting.'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return Wait-ForAndroidDevice -adbPath $adbPath -timeoutSeconds 180
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
Stop-KnownMobileBackendIfListening -port $backendPort

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not (Is-PortListening -port $backendPort)) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Publishing backend dev copy to $backendRunDir"
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    & dotnet publish $backendProject -c Debug -o $backendRunDir --nologo | Out-Host
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ($LASTEXITCODE -ne 0) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'Backend publish failed; skipping backend startup.'
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        exit 0
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

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
    if (Wait-ForBackend -url $backendUrl -timeoutSeconds 25) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'Backend is reachable.'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    else {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Write-Info 'Backend process started but did not become reachable within timeout.'
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
else {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Backend already listening on port $backendPort"
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
    & $adbPath -s $serial reverse tcp:7164 tcp:7164 | Out-Null
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Info "Configured adb reverse for $serial (tcp:7164 -> tcp:7164)"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
exit 0
