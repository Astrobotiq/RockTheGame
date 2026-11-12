#Requires -Version 5.1
# SessionStart hook: TASKS.md'nin Active bolumunu okur ve oturum acilisinda
# bugunun ve yarinin task'larini ozetler.
#
# - Calisma duzeni: her gun 5-6 saat (2026-11-08'den beri); due tarihi herhangi bir gun olabilir.
# - "Bugunun isleri" = due tarihi bugun olanlar, "Yarin" = due tarihi yarin olanlar.
# - Due tarihi gecmis task'lar "Gecikenler" basligiyla ayrica listelenir.
# - Bir gune 4'ten fazla task atanmissa "Asiri yuklu gunler" altinda uyarilir.
# - TASKS.md yoksa hata vermeden cikar.
#
# Not: string literalleri bilerek ASCII; PowerShell 5.1 BOM'suz .ps1 dosyalarini
# ANSI varsayar, Turkce karakterler bozulur. Task basliklari TASKS.md'den UTF-8
# okundugu icin onlarda bu sorun yok.

$ErrorActionPreference = 'Stop'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$maxPerDay = 4

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

$today = (Get-Date).Date
$tomorrow = $today.AddDays(1)

function Format-Line($t) {
    $cat = if ($t.Cat) { " $($t.Cat)" } else { '' }
    "  - $($t.Title)$cat"
}

# --- Cikti ---
$out = New-Object System.Collections.Generic.List[string]
$out.Add("== RockTheGame planlama == ($($today.ToString('yyyy-MM-dd')) $($trDays[[string]$today.DayOfWeek]))")

$out.Add('Bugunun isleri (gunluk 5-6 saat):')
$todays = @($tasks | Where-Object { $_.Due -eq $today })
if ($todays.Count -eq 0) { $out.Add('  (bugune atanmis task yok)') }
else { foreach ($t in $todays) { $out.Add((Format-Line $t)) } }

$out.Add("Yarin: $($tomorrow.ToString('yyyy-MM-dd')) $($trDays[[string]$tomorrow.DayOfWeek])")
$next = @($tasks | Where-Object { $_.Due -eq $tomorrow })
if ($next.Count -eq 0) { $out.Add('  (yarina atanmis task yok)') }
else { foreach ($t in $next) { $out.Add((Format-Line $t)) } }

$overdue = @($tasks | Where-Object { $_.Due -ne $null -and $_.Due -lt $today } | Sort-Object Due)
if ($overdue.Count -gt 0) {
    $out.Add('Gecikenler:')
    foreach ($t in $overdue) {
        $days = [int]($today - $t.Due).TotalDays
        $out.Add((Format-Line $t) + " (due $($t.Due.ToString('yyyy-MM-dd')), $days gun gecikme)")
    }
}

$heavy = @($tasks | Where-Object { $_.Due -ne $null -and $_.Due -ge $today } | Group-Object { $_.Due.ToString('yyyy-MM-dd') } | Where-Object { $_.Count -gt $maxPerDay } | Sort-Object Name)
if ($heavy.Count -gt 0) {
    $out.Add("Asiri yuklu gunler (en fazla $maxPerDay task):")
    foreach ($g in $heavy) { $out.Add("  - $($g.Name): $($g.Count) task") }
}

$out -join "`n"
exit 0
