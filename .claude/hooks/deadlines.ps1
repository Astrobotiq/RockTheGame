#Requires -Version 5.1
# SessionStart hook: TASKS.md'nin Active bolumunu okur ve oturum acilisinda
# o aksamin (ya da sonraki Pazartesi/Cuma oturumunun) task'larini ozetler.
#
# - Bugun Pazartesi/Cuma ise: "Bu aksamin isleri" = due tarihi bugun olanlar.
# - Diger gunlerde: sonraki Pazartesi/Cuma tarihi + o gunun task'lari.
# - Her durumda: due tarihi gecmis task'lar "Gecikenler" basligiyla ayrica listelenir.
# - Due tarihi Pazartesi/Cuma olmayan task'lar "Kural disi due tarihi" altinda uyarilir.
# - TASKS.md yoksa hata vermeden cikar.
#
# Not: string literalleri bilerek ASCII; PowerShell 5.1 BOM'suz .ps1 dosyalarini
# ANSI varsayar, Turkce karakterler bozulur. Task basliklari TASKS.md'den UTF-8
# okundugu icin onlarda bu sorun yok.

$ErrorActionPreference = 'Stop'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { Split-Path (Split-Path $PSScriptRoot) }
$tasksPath = Join-Path $root 'TASKS.md'
if (-not (Test-Path -LiteralPath $tasksPath)) { exit 0 }

# --- TASKS.md > "## Active" bolumu ---
$inActive = $false
$activeLines = @()
foreach ($line in (Get-Content -LiteralPath $tasksPath -Encoding UTF8)) {
    if ($line -match '^##\s+') {
        $inActive = ($line -match '^##\s+Active\s*$')
        continue
    }
    if ($inActive) { $activeLines += $line }
}

# --- Tamamlanmamis task'lari ayikla ---
$tasks = @()
foreach ($line in $activeLines) {
    if ($line -notmatch '^\s*-\s*\[\s*\]\s*(\S.*)$') { continue }
    $body = $Matches[1]

    $title = if ($body -match '\*\*(.+?)\*\*') { $Matches[1] } else { ($body -split ' - ')[0].Trim() }
    $cat = if ($body -match '(#(?:art|test|level|code))\b') { $Matches[1] } else { '' }

    $due = $null
    if ($body -match 'due\s+(\d{4}-\d{2}-\d{2})') {
        try { $due = [datetime]::ParseExact($Matches[1], 'yyyy-MM-dd', $null) } catch { $due = $null }
    }

    $tasks += [pscustomobject]@{ Title = $title; Cat = $cat; Due = $due }
}

# --- Tarih baglami ---
$trDays = @{
    'Monday' = 'Pazartesi'; 'Tuesday' = 'Sali'; 'Wednesday' = 'Carsamba'
    'Thursday' = 'Persembe'; 'Friday' = 'Cuma'; 'Saturday' = 'Cumartesi'; 'Sunday' = 'Pazar'
}
$sessionFocus = @{ 'Monday' = 'Art & Test'; 'Friday' = 'Code & Level' }

$today = (Get-Date).Date
$isSessionDay = $sessionFocus.ContainsKey([string]$today.DayOfWeek)

$nextSession = $today
do { $nextSession = $nextSession.AddDays(1) } until ($sessionFocus.ContainsKey([string]$nextSession.DayOfWeek))

function Format-Line($t) {
    $cat = if ($t.Cat) { " $($t.Cat)" } else { '' }
    "  - $($t.Title)$cat"
}

# --- Cikti ---
$out = New-Object System.Collections.Generic.List[string]
$todayLabel = "$($today.ToString('yyyy-MM-dd')) $($trDays[[string]$today.DayOfWeek])"
$out.Add("== RockTheGame planlama == ($todayLabel)")

if ($isSessionDay) {
    $focus = $sessionFocus[[string]$today.DayOfWeek]
    $out.Add("Bu aksamin isleri -- $($trDays[[string]$today.DayOfWeek]) 20:00, $focus" + ':')
    $todays = @($tasks | Where-Object { $_.Due -eq $today })
    if ($todays.Count -eq 0) { $out.Add('  (bu tarihe atanmis task yok)') }
    else { foreach ($t in $todays) { $out.Add((Format-Line $t)) } }
}
else {
    $focus = $sessionFocus[[string]$nextSession.DayOfWeek]
    $out.Add("Sonraki oturum: $($nextSession.ToString('yyyy-MM-dd')) $($trDays[[string]$nextSession.DayOfWeek]) 20:00, $focus" + ':')
    $upcoming = @($tasks | Where-Object { $_.Due -eq $nextSession })
    if ($upcoming.Count -eq 0) { $out.Add('  (bu tarihe atanmis task yok)') }
    else { foreach ($t in $upcoming) { $out.Add((Format-Line $t)) } }
}

$overdue = @($tasks | Where-Object { $_.Due -ne $null -and $_.Due -lt $today } | Sort-Object Due)
if ($overdue.Count -gt 0) {
    $out.Add('Gecikenler:')
    foreach ($t in $overdue) {
        $days = [int]($today - $t.Due).TotalDays
        $out.Add((Format-Line $t) + " (due $($t.Due.ToString('yyyy-MM-dd')), $days gun gecikme)")
    }
}

$offSchedule = @($tasks | Where-Object { $_.Due -ne $null -and -not $sessionFocus.ContainsKey([string]$_.Due.DayOfWeek) } | Sort-Object Due)
if ($offSchedule.Count -gt 0) {
    $out.Add('Kural disi due tarihi (yalnizca Pazartesi/Cuma olmali):')
    foreach ($t in $offSchedule) {
        $out.Add((Format-Line $t) + " (due $($t.Due.ToString('yyyy-MM-dd')) $($trDays[[string]$t.Due.DayOfWeek]))")
    }
}

$out -join "`n"
exit 0
