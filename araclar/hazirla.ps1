$ErrorActionPreference = 'Stop'
$Repo = Split-Path $PSScriptRoot -Parent
$RepoAdi = 'IkiDunya'
$UnitySurum = '6000.3.25f1'

function Baslik($t) { Write-Host ""; Write-Host "== $t ==" -ForegroundColor Cyan }
function Bitir($mesaj, $renk = 'Green') { Write-Host ""; Write-Host $mesaj -ForegroundColor $renk; Write-Host ""; Read-Host "Kapatmak için Enter" | Out-Null; exit }
function Var($komut) { [bool](Get-Command $komut -ErrorAction SilentlyContinue) }
function Sor($soru) { ("$(Read-Host $soru)").Trim().Trim('"') }

Write-Host "İki Dünya, Tek Yaşam — ekip reposu hazırlığı" -ForegroundColor Cyan
Write-Host "Repo: $Repo"
if (-not (Var 'git')) { Bitir "Git bulunamadı. https://git-scm.com adresinden kur ve tekrar çalıştır." Red }

# 1) Unity projesi
Baslik "1/5 Unity projesi"
$Unity = Join-Path $Repo 'Unity'
if (-not (Test-Path (Join-Path $Unity 'Assets'))) {
  $Bulunan = @(Get-ChildItem -LiteralPath $Repo -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'ProjectSettings\ProjectVersion.txt') })
  if ($Bulunan.Count -eq 1) {
    Write-Host "Unity projesi '$($Bulunan[0].Name)' adıyla bulundu; adı 'Unity' yapılıyor." -ForegroundColor Yellow
    try {
      Rename-Item -LiteralPath $Bulunan[0].FullName -NewName 'Unity' -ErrorAction Stop
      Write-Host "Tamam. Unity Hub'da eski kaydı kaldır (Remove from list), sonra Add > $Unity" -ForegroundColor Yellow
    } catch {
      Bitir "Klasörün adı değiştirilemedi. Unity'yi ve bu klasörü açan her şeyi (VS Code, Gezgin penceresi) kapatıp tekrar çalıştır." Red
    }
  } elseif ($Bulunan.Count -gt 1) {
    Bitir "Birden fazla Unity projesi bulundu: $($Bulunan.Name -join ', '). Fazlasını sil ve tekrar çalıştır." Red
  }
}
if (-not (Test-Path (Join-Path $Unity 'Assets'))) {
  Write-Host "Unity projesi henüz yok. Unity Hub'da şunu yap:" -ForegroundColor Yellow
  Write-Host "  New project > Editor: $UnitySurum > şablon: Universal 3D"
  Write-Host "  Project name: Unity"
  Write-Host "  Location: $Repo"
  Write-Host "Proje açılınca Unity'yi KAPAT ve bu dosyayı tekrar çalıştır."
  Bitir "Unity projesi oluşturulunca tekrar çalıştır." Yellow
}
Write-Host "Unity projesi bulundu." -ForegroundColor Green

# 2) Klasörler
Baslik "2/5 Klasörler"
$Proje = Join-Path $Unity 'Assets\_Project'
$Klasorler = @(
  'Scripts\Cekirdek', 'Scripts\Kaplumbaga', 'Scripts\Tehditler', 'Scripts\Yonetim',
  'Scripts\Arayuz\Gece', 'Scripts\Arayuz\Yonetim', 'Scripts\Editor',
  'Veri\Kartlar', 'Veri\Olaylar', 'Veri\Gorevler', 'Veri\Ayarlar',
  'Sahneler\Deneme', 'Prefabs', 'Modeller', 'Ses', 'Arayuz\Gorseller'
)
foreach ($k in $Klasorler) {
  $yol = Join-Path $Proje $k
  New-Item -ItemType Directory -Force -Path $yol | Out-Null
  $tut = Join-Path $yol '.gitkeep'
  if (-not (Test-Path $tut)) { New-Item -ItemType File -Path $tut | Out-Null }
}
$Tp = Join-Path $Unity 'Assets\ThirdParty'
New-Item -ItemType Directory -Force -Path $Tp | Out-Null
if (-not (Test-Path (Join-Path $Tp '.gitkeep'))) { New-Item -ItemType File -Path (Join-Path $Tp '.gitkeep') | Out-Null }
Copy-Item -LiteralPath (Join-Path $Repo 'docs\klasorler.md') -Destination (Join-Path $Proje 'BENI-OKU.md') -Force
$Gh1 = Join-Path $Repo '.github'
New-Item -ItemType Directory -Force -Path $Gh1 | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'pr-sablonu.md') -Destination (Join-Path $Gh1 'pull_request_template.md') -Force
Write-Host "Klasörler hazır: Assets\_Project" -ForegroundColor Green

# 3) Git
Baslik "3/5 Git"
if (-not (Test-Path (Join-Path $Repo '.git'))) { git -C $Repo init -b main | Out-Null }
if (-not (git -C $Repo config user.email)) {
  git -C $Repo config user.name (Sor "Git için adın")
  git -C $Repo config user.email (Sor "GitHub e-postan")
}
git -C $Repo add -A
git -C $Repo diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
  git -C $Repo commit -q -m "Başlangıç: Unity projesi, klasörler, ekip kuralları ve GDD"
  if ($LASTEXITCODE -ne 0) { Bitir "Commit atılamadı. Yukarıdaki hatayı Claude'a gönder." Red }
  Write-Host "İlk commit atıldı." -ForegroundColor Green
} else { Write-Host "Commit'lenecek değişiklik yok." }

# 4) GitHub
Baslik "4/5 GitHub"
$Sahip = $null
$Gh = (Var 'gh')
if ($Gh) { gh auth status 2>$null | Out-Null; $Gh = ($LASTEXITCODE -eq 0) }
if ((git -C $Repo remote) -contains 'origin') {
  $Url = git -C $Repo remote get-url origin
  Write-Host "Zaten bağlı: $Url" -ForegroundColor Yellow
  git -C $Repo push -u origin main
} elseif ($Gh) {
  gh repo create $RepoAdi --private --source $Repo --remote origin --push
  if ($LASTEXITCODE -ne 0) { Bitir "GitHub'da repo açılamadı (aynı adla repo var olabilir). Hata mesajını Claude'a gönder." Red }
  $Url = git -C $Repo remote get-url origin
} else {
  Write-Host "Tarayıcıda github.com/new açılıyor:" -ForegroundColor Yellow
  Write-Host "  Repository name: $RepoAdi  |  Private  |  README, .gitignore, license EKLEME"
  try { Start-Process "https://github.com/new" } catch { }
  $Url = Sor "Repo açılınca adresini yapıştır (ör. https://github.com/Dmdu3/$RepoAdi.git)"
  if (-not $Url) { Bitir "Adres girilmedi. Repo açınca bu dosyayı tekrar çalıştır." Yellow }
  git -C $Repo remote add origin $Url
  git -C $Repo push -u origin main
  if ($LASTEXITCODE -ne 0) { Bitir "Gönderilemedi. Repo boş mu, adres doğru mu kontrol et." Red }
}
if ($Url -match 'github\.com[/:]([^/]+)/') { $Sahip = $Matches[1] }
Write-Host "GitHub: https://github.com/$Sahip/$RepoAdi" -ForegroundColor Green

# 5) Ekip ve main kuralı
Baslik "5/5 Ekip ve main kuralı"
$Ekip = [ordered]@{ 'Toprak' = 'torpakx'; 'Yusuf' = ''; 'Abdü' = '' }
foreach ($ad in @($Ekip.Keys)) {
  if (-not $Ekip[$ad]) { $Ekip[$ad] = (Sor "$ad için GitHub kullanıcı adı (Enter = atla)").TrimStart('@') }
}
if ($Gh -and $Sahip) {
  foreach ($ad in $Ekip.Keys) {
    $k = $Ekip[$ad]
    if ($k) {
      gh api -X PUT "repos/$Sahip/$RepoAdi/collaborators/$k" -f permission=push 2>$null | Out-Null
      if ($LASTEXITCODE -eq 0) { Write-Host "Davet gönderildi: $ad ($k)" -ForegroundColor Green }
      else { Write-Host "Davet gönderilemedi: $k — elle ekle." -ForegroundColor Yellow }
    }
  }
  $Kural = Join-Path $env:TEMP 'ikidunya-kural.json'
  @'
{"name":"main-koruma","target":"branch","enforcement":"active",
 "conditions":{"ref_name":{"include":["~DEFAULT_BRANCH"],"exclude":[]}},
 "rules":[{"type":"deletion"},{"type":"non_fast_forward"},
  {"type":"pull_request","parameters":{"required_approving_review_count":0,"dismiss_stale_reviews_on_push":false,"require_code_owner_review":false,"require_last_push_approval":false,"required_review_thread_resolution":false}}]}
'@ | Set-Content -LiteralPath $Kural -Encoding ascii
  gh api -X POST "repos/$Sahip/$RepoAdi/rulesets" --input $Kural 2>$null | Out-Null
  if ($LASTEXITCODE -eq 0) { Write-Host "main kuralı açıldı: PR zorunlu, onay zorunlu değil." -ForegroundColor Green }
  else { Write-Host "main kuralı açılamadı (private repoda GitHub Pro gerekebilir). Settings > Rules'tan elle aç." -ForegroundColor Yellow }
} else {
  Write-Host "Elle yap: GitHub'da repo > Settings > Collaborators > Add people (rol: Write)" -ForegroundColor Yellow
  foreach ($ad in $Ekip.Keys) { if ($Ekip[$ad]) { Write-Host "  $ad`: $($Ekip[$ad])" } }
  Write-Host "Elle yap: Settings > Rules > New ruleset > main-koruma: Default branch, Restrict deletions, Block force pushes, Require a pull request (onay 0)" -ForegroundColor Yellow
}

# İkinci beyin bağlantısı
$Wiki = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'IkinciBeyin\IkiDunya'
if (Test-Path $Wiki) {
  $C = Join-Path $Wiki '.claude'
  New-Item -ItemType Directory -Force -Path $C | Out-Null
  [IO.File]::WriteAllText((Join-Path $C 'repo-yolu.txt'), ($Repo -replace '\\', '/'), (New-Object Text.UTF8Encoding $false))
  Write-Host ""
  Write-Host "Wiki bağlandı: $Wiki" -ForegroundColor Green
}

Baslik "Bitti"
Write-Host "Ekibe gönder: https://github.com/$Sahip/$RepoAdi ve docs\EKIP-REHBERI.md"
Write-Host "Son adım: IkinciBeyin\_sistem\KURULUM.bat'ı bir kez çalıştır; IkiDunya için repo yolu sorarsa: $Repo"
Bitir "Hazır." Green
