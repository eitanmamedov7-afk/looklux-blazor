# מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
# למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
# לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
# איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

[CmdletBinding()]
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
param(
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    [ValidateSet("http", "https")]
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    [string]$LaunchProfile = "http",
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    [switch]$NoRun
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
)

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$ErrorActionPreference = "Stop"

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$webProject = Join-Path $repoRoot "gadifff\gadifff.csproj"

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if (-not (Test-Path $webProject)) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    throw "Web project not found at: $webProject"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$processIds = [System.Collections.Generic.HashSet[int]]::new()

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
Get-Process -Name "gadifff" -ErrorAction SilentlyContinue |
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    ForEach-Object { [void]$processIds.Add($_.Id) }

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$dotnetHosts = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue |
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Where-Object {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $_.CommandLine -and (
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $_.CommandLine -like "*gadifff.dll*" -or
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            $_.CommandLine -like "*gadifff.csproj*"
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        )
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$dotnetHosts | ForEach-Object { [void]$processIds.Add([int]$_.ProcessId) }

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if ($processIds.Count -gt 0) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Host "Stopping stale web host process(es): $($processIds -join ', ')"
    # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
    foreach ($procId in $processIds) {
        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        try { Stop-Process -Id $procId -Force -ErrorAction Stop } catch { }
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }

    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    $timeout = [DateTime]::UtcNow.AddSeconds(8)
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    while ([DateTime]::UtcNow -lt $timeout) {
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        $alive = @()
        # שורת סקריפט שמבצעת צעד בהכנת הסביבה או בהרצת הפרויקט.
        foreach ($procId in $processIds) {
            # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (Get-Process -Id $procId -ErrorAction SilentlyContinue) {
                # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
                $alive += $procId
            # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
            }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        }

        # בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if ($alive.Count -eq 0) { break }
        # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
        Start-Sleep -Milliseconds 250
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    }
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
$baseOutputPath = "C:\maui-bin\run-web\"
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
New-Item -ItemType Directory -Path $baseOutputPath -Force | Out-Null

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
Set-Location $repoRoot
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
Write-Host "Starting web app with profile '$LaunchProfile' on isolated output path..."

# בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
if ($NoRun) {
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Host "Prepared command:"
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    Write-Host "dotnet run --project `"$webProject`" --launch-profile $LaunchProfile -p:UseAppHost=false -p:BaseOutputPath=$baseOutputPath"
    # פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
    return
# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
}

# פקודת סקריפט שמקדמת את הכנת הסביבה או את הרצת הפרויקט.
& dotnet run --project $webProject --launch-profile $LaunchProfile -p:UseAppHost=false -p:BaseOutputPath=$baseOutputPath
