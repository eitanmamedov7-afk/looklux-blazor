# מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
# למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
# לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
# איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

param(
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    [int[]]$Ports = @(7166, 7167),
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
)

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$ErrorActionPreference = 'Stop'

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Write-Info([string]$message) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Host "[web-setup] $message"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Get-ListenerProcessIds([int]$port) {
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    try {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction Stop |
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Select-Object -ExpandProperty OwningProcess -Unique
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    catch {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return @()
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
function Test-IsLookLuxWebProcess([int]$processId, [string]$repoRoot) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not $process) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $false
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $normalizedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\')
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (-not [string]::IsNullOrWhiteSpace($process.Path)) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $processPath = [System.IO.Path]::GetFullPath($process.Path)
        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if ($processPath.StartsWith((Join-Path $normalizedRepoRoot 'gadifff\bin'), [StringComparison]::OrdinalIgnoreCase)) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            return $true
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $cimProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$processId" -ErrorAction SilentlyContinue
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $commandLine = if ($cimProcess) { $cimProcess.CommandLine } else { '' }
    # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if ([string]::IsNullOrWhiteSpace($commandLine)) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        return $false
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return $commandLine.IndexOf((Join-Path $normalizedRepoRoot 'gadifff\gadifff.csproj'), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $commandLine.IndexOf((Join-Path $normalizedRepoRoot 'gadifff\bin'), [StringComparison]::OrdinalIgnoreCase) -ge 0
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
foreach ($port in $Ports) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $processIds = Get-ListenerProcessIds -port $port
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    foreach ($processId in $processIds) {
        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if ($processId -eq $PID) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            continue
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }

        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (Test-IsLookLuxWebProcess -processId $processId -repoRoot $RepoRoot) {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info "Stopping stale web process on port $port (pid $processId)."
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Stop-Process -Id $processId -ErrorAction SilentlyContinue
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Start-Sleep -Milliseconds 500
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        else {
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            Write-Info "Port $port is used by another process (pid $processId); leaving it alone."
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}
