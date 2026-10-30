# Repo düzenini Anadolu Ajan'la aynı yapar (bir kere çalıştırılır; tekrar çalıştırmak güvenlidir).
$ErrorActionPreference = 'Continue'
try { [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding $false } catch {}
$Repo = Split-Path $PSScriptRoot -Parent
$Utf8 = New-Object System.Text.UTF8Encoding $false

function Baslik($t) { Write-Host ""; Write-Host "== $t ==" -ForegroundColor Cyan }
function Bitir($mesaj, $renk = 'Green') { Write-Host ""; Write-Host $mesaj -ForegroundColor $renk; Write-Host ""; Read-Host "Kapatmak için Enter" | Out-Null; exit }
function Var($komut) { [bool](Get-Command $komut -ErrorAction SilentlyContinue) }
function Klasor($yol) {
  New-Item -ItemType Directory -Force -Path $yol | Out-Null
  $tut = Join-Path $yol '.gitkeep'
  if (-not (Test-Path $tut)) { New-Item -ItemType File -Path $tut | Out-Null }
}
function BosMu($yol) {
  # İçinde .gitkeep ve .meta dışında dosya yoksa boştur
  -not (Get-ChildItem -LiteralPath $yol -Recurse -File -Force | Where-Object { $_.Name -ne '.gitkeep' -and $_.Extension -ne '.meta' })
}
function Sil($yol) {
  if (Test-Path -LiteralPath $yol) { Remove-Item -LiteralPath $yol -Recurse -Force }
  if (Test-Path -LiteralPath "$yol.meta") { Remove-Item -LiteralPath "$yol.meta" -Force }
}

Write-Host "İki Dünya, Tek Yaşam — repo düzeni (Anadolu ile aynı)" -ForegroundColor Cyan
Write-Host "Repo: $Repo"
if (-not (Var 'git')) { Bitir "Git bulunamadı." Red }

# 1) Unity kapalı mı
Baslik "1/6 Kontrol"
$Eski = Join-Path $Repo 'Unity'
if (Test-Path $Eski) {
  $deneme = Join-Path $Repo 'Unity_kontrol'
  try { Rename-Item -LiteralPath $Eski -NewName 'Unity_kontrol' -ErrorAction Stop; Rename-Item -LiteralPath $deneme -NewName 'Unity' -ErrorAction Stop }
  catch { Bitir "Unity klasörü kullanımda. Unity'yi, VS Code'u ve bu klasörü açan Gezgin pencerelerini kapatıp tekrar çalıştır." Red }
}
Write-Host "Tamam."

# 2) Dal
Baslik "2/6 Dal"
git -C $Repo fetch -q origin 2>$null
$Dal = 'umut/anadolu-duzeni'
$suan = (git -C $Repo branch --show-current)
if ($suan -ne $Dal) {
  git -C $Repo rev-parse -q --verify "refs/heads/$Dal" 2>$null | Out-Null
  if ($LASTEXITCODE -eq 0) { $Dal = $Dal + '-' + (Get-Date -Format 'HHmm') }
  git -C $Repo switch -q -c $Dal
}
Write-Host "Dal: $Dal"

# 3) Unity projesini köke taşı
Baslik "3/6 Unity projesi köke"
if (Test-Path $Eski) {
  foreach ($d in 'Assets', 'Packages', 'ProjectSettings', 'Library', 'UserSettings') {
    $k = Join-Path $Eski $d
    $h = Join-Path $Repo $d
    if ((Test-Path $k) -and -not (Test-Path $h)) { Move-Item -LiteralPath $k -Destination $h; Write-Host "Taşındı: $d" }
  }
  Remove-Item -LiteralPath $Eski -Recurse -Force -ErrorAction SilentlyContinue
  if (Test-Path $Eski) { Write-Host "Unity klasöründe kalanlar silinemedi; sonra elle sil: $Eski" -ForegroundColor Yellow }
} else { Write-Host "Unity projesi zaten kökte." }
if (-not (Test-Path (Join-Path $Repo 'Assets'))) { Bitir "Kökte Assets klasörü yok; bir şeyler ters gitti. Ekrandakini Claude'a gönder." Red }
foreach ($e in 'araclar', 'HAZIRLA.bat') { $y = Join-Path $Repo $e; if (Test-Path $y) { Remove-Item -LiteralPath $y -Recurse -Force } }

# 4) _Project klasör ağacı
Baslik "4/6 Klasör ağacı"
$P = Join-Path $Repo 'Assets\_Project'
New-Item -ItemType Directory -Force -Path $P | Out-Null
# Eski Türkçe klasörler (boşsa silinir)
foreach ($e in 'Scripts\Cekirdek', 'Scripts\Kaplumbaga', 'Scripts\Tehditler', 'Scripts\Yonetim', 'Scripts\Arayuz', 'Veri', 'Sahneler', 'Modeller', 'Ses', 'Arayuz') {
  $y = Join-Path $P $e
  if (Test-Path $y) {
    if (BosMu $y) { Sil $y; Write-Host "Kaldırıldı (boş): $e" }
    else { Write-Host "İçinde dosya var, dokunulmadı: $e — Unity'de yeni yerine taşı." -ForegroundColor Yellow }
  }
}
Sil (Join-Path $P 'BENI-OKU.md')
$Yeni = @(
  'Scripts\Core', 'Scripts\Camera', 'Scripts\Turtle', 'Scripts\Hazards', 'Scripts\Management',
  'Scripts\UI\Night', 'Scripts\UI\Management', 'Scripts\Editor',
  'ScriptableObjects\Cards', 'ScriptableObjects\Events', 'ScriptableObjects\VolunteerTasks', 'ScriptableObjects\Settings',
  'Scenes\Levels', 'Scenes\Prototypes',
  'Prefabs\Turtles', 'Prefabs\Environment', 'Prefabs\Hazards', 'Prefabs\Structures', 'Prefabs\UI',
  'Art\Characters', 'Art\Environment', 'Art\Props', 'Art\UI',
  'Models', 'Materials', 'Animations', 'Shaders',
  'Audio\Ambience', 'Audio\Music', 'Audio\SFX'
)
foreach ($k in $Yeni) { Klasor (Join-Path $P $k) }
foreach ($ad in 'Umut', 'Toprak', 'Yusuf', 'Abdu') { Klasor (Join-Path $Repo "Assets\Sandbox\$ad") }
Klasor (Join-Path $Repo 'Assets\ThirdParty')
Write-Host "Klasörler hazır." -ForegroundColor Green

# 5) Paketler ve LFS
Baslik "5/6 Paketler ve Git LFS"
$Man = Join-Path $Repo 'Packages\manifest.json'
if (Test-Path $Man) {
  $m = [IO.File]::ReadAllText($Man, $Utf8)
  if ($m -notmatch 'com\.coplaydev\.unity-mcp') {
    $m = $m -replace '("dependencies"\s*:\s*\{)', ("`$1`n    `"com.coplaydev.unity-mcp`": `"https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity`",")
    [IO.File]::WriteAllText($Man, $m, $Utf8)
    Write-Host "MCP for Unity paketi eklendi (Unity açılınca iner)." -ForegroundColor Green
  } else { Write-Host "MCP for Unity zaten var." }
}
git lfs version 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) { git -C $Repo lfs install --local 2>$null | Out-Null; Write-Host "Git LFS açık." -ForegroundColor Green }
else { Write-Host "Git LFS bulunamadı. GitHub Desktop'ı kur ya da https://git-lfs.com" -ForegroundColor Yellow }

# 6) Commit, gönder, PR
Baslik "6/6 GitHub"
git -C $Repo add -A
git -C $Repo diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
  git -C $Repo commit -q -m "Repo düzeni Anadolu ile aynı: Unity projesi kökte, yeni klasör ağacı, Sandbox, LFS, MCP for Unity"
  Write-Host "Commit atıldı."
}
git -C $Repo push -q -u origin $Dal
if ($LASTEXITCODE -ne 0) { Bitir "Gönderilemedi (internet ya da giriş). Sonra GitHub Desktop'ta Push origin'e bas." Yellow }
$Adres = ((git -C $Repo remote get-url origin) -replace '\.git$', '')
$Pr = $null
if (Var 'gh') {
  gh auth status 2>$null | Out-Null
  if ($LASTEXITCODE -eq 0) {
    $Pr = gh pr create --repo ($Adres -replace '^https://github.com/', '') --base main --head $Dal --title "Repo düzeni: Anadolu ile aynı" --body "Unity projesi köke taşındı; Assets/_Project yeni klasör ağacı; Assets/Sandbox; Git LFS; MCP for Unity; README, CLAUDE.md ve docs güncellendi." 2>$null
  }
}
if (-not $Pr) { $Pr = "$Adres/compare/main...$($Dal)?expand=1" }
Write-Host "PR: $Pr" -ForegroundColor Green
try { Start-Process $Pr } catch { }

Baslik "Bitti"
Write-Host "1) PR'ı GitHub'da birleştir (Merge)."
Write-Host "2) GitHub Desktop: Current branch > main > Fetch > Pull."
Write-Host "3) Unity Hub: eski 'Unity' kaydını kaldır (Remove from list), Add > $Repo"
Write-Host "4) Unity ilk açılışta Library'yi ve MCP paketini hazırlar; birkaç dakika sürebilir."
Bitir "Hazır." Green
