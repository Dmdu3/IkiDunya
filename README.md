# İki Dünya, Tek Yaşam

Bir anne caretta caretta ile başlayıp onun yavrusuyla biten, iklim ve çevre temalı oyun. Kaplumbağa bölümlerinde denizde ve kumsalda hayatta kalırsın; aralardaki insan turlarında gönüllü ekibini yönetirsin ve kararların bir sonraki gecenin kumsalını değiştirir.
Unity 6 · URP · 3D · PC

## Önce bunu oku

- Tasarım (tek doğru kaynak): [`docs/GDD.md`](docs/GDD.md)
- İş planı, faz faz ve kişi kişi: **İki Dünya, Tek Yaşam — Yapılacaklar** (Umut'tan bağlantıyı iste); kısa hali [`docs/yol-haritasi.md`](docs/yol-haritasi.md)
- Mekanik tarafın durumu ve sistemlerin haritası: [`docs/DURUM.md`](docs/DURUM.md)
- Kurulum ve günlük çalışma, adım adım: [`docs/EKIP-REHBERI.md`](docs/EKIP-REHBERI.md)

## Kurulum (kısa)

1. **Unity 6000.3.25f1 (LTS)**. Herkes tam olarak bu sürümü kullanır.
2. **GitHub Desktop** ile bu repoyu klonla. OneDrive, Masaüstü ya da Drive klasörü kullanma (öneri: `C:\Projeler\IkiDunya`).
3. Unity Hub → *Add project from disk* → klonladığın klasör.

## Kurallar

- `main`'e doğrudan gönderilmez. Herkes kendi dalında çalışır: `isim/kisa-aciklama`
  (örnek: `umut/yavru-suru`, `toprak/gonullu-atama`, `yusuf/kart-metinleri`, `abdu/dalga-sesleri`).
- Her değişiklik pull request ile girer. Kendi alanındaki PR'ı herkes kendisi birleştirir; `Scenes/Levels`, `Scripts/Core`, `ProjectSettings` ve `Packages`'a dokunan PR'ları Umut birleştirir.
- Oyun sahnelerinin (`Scenes/Levels`) sahibi Umut. Herkes kendi `Assets/Sandbox/<adın>` klasöründe dener; sahneye girecek şey prefab olur.
- Dosyaları Unity'nin Project penceresinden taşı, yeniden adlandır ve sil (yoksa `.meta` kopar).
- Sayılar koda gömülmez: tasarım değeri `docs/GDD.md`'de, oyundaki değeri `ScriptableObjects/` altında; değişiklik `docs/denge-gunlugu.md`'ye.
- Oyuncunun gördüğü her yazı önce `docs/metinler.md`'ye girer.
- Büyük dosyalar (görsel, ses, model, video, font) Git LFS ile gider; GitHub Desktop bunu kendisi yapar. Kaynağını `docs/varliklar.md`'ye yaz. 100 MB üstü tek dosya girmez.

## Klasörler

| Yol | İçerik | Sahibi |
| --- | --- | --- |
| `Assets/_Project/Scripts/Core` | Oyun akışı, bölüm geçişi, kayıt, `KumsalDurumu`, `GameInput` | Umut |
| `Assets/_Project/Scripts/Camera` | Kumsal, deniz ve yönetim kameraları | Umut |
| `Assets/_Project/Scripts/Turtle` | Anne ve yavru hareketi (kara, deniz), sürü, enerji, nefes, kazma | Umut |
| `Assets/_Project/Scripts/Hazards` | Ağ, tekne, sıcak leke, çöp, avcılar, ışık, dalga | Umut |
| `Assets/_Project/Scripts/Management` | İnsan kısmı: harita, turlar, gönüllüler, kaynaklar, kartlar, kanıtlar, olaylar | Toprak |
| `Assets/_Project/Scripts/UI` | `Night/` kaplumbağa ekranı (Umut), `Management/` yönetim ekranı (Toprak) | Umut, Toprak |
| `Assets/_Project/Scripts/Editor` | Sahne kurulum araçları | Umut |
| `Assets/_Project/ScriptableObjects` | `Cards`, `Events`, `VolunteerTasks` (Toprak); `Settings` (Umut + Toprak) | Toprak |
| `Assets/_Project/Scenes/Levels` | `Menu`, `Bolum1_Donus`, `Kumsal` (Bölüm 2–4), `Bolum5_IlkYuzus` | Umut |
| `Assets/_Project/Scenes/Prototypes` | Gri kutu denemeler (kazma, sürü, deniz) | Umut |
| `Assets/_Project/Prefabs` | `Turtles`, `Environment`, `Hazards`, `Structures` (gündüz yapıları), `UI` | Umut (alt klasörü sahibi isteyebilir) |
| `Assets/_Project/Art` | `Characters`, `Environment`, `Props`, `UI` | Abdü |
| `Assets/_Project/Models`, `Materials`, `Animations` | Hazır modeller, malzemeler, animasyonlar | Abdü (animasyon kodu Umut) |
| `Assets/_Project/Audio` | `Ambience`, `Music`, `SFX` | Abdü |
| `Assets/_Project/Shaders` | Termal kum, su yüzeyi, gece ışığı | Umut |
| `Assets/Sandbox/<adın>` | Kişisel deneme alanı ve deneme sahnen | Sen |
| `Assets/ThirdParty/<yayıncı>` | Dışarıdan gelen paketler (ör. `ThirdParty/Quaternius`) | Abdü; eklemeden önce ekibe sor |
| `docs` | Ekibin ortak belgeleri | Herkes (dosya sahipleri `docs/klasorler.md`'de) |
| `Tools` | Unity dışı yardımcı betikler | Umut |

Ayrıntı ve iki tarafın buluştuğu yer: [`docs/klasorler.md`](docs/klasorler.md)

## Layer'lar

Sahneye bir şey koyarken layer'ını doğru seç; yanlış layer en sık karşılaşılan hatadır.

| Layer | Ne için | Kaplumbağa üstünde yürür mü | Not |
| --- | --- | --- | --- |
| `Ground` | Kum, kumul, deniz tabanı | Evet | Termal bantlar bu zeminin bölgelerinden okunur |
| `Water` | Deniz yüzeyi, dalga alanı | Hayır (yüzülür) | Dalga çizgisi `KumsalDurumu`'ndan gelir |
| `Obstacle` | Şezlong, şemsiye, kaya, duvar | Hayır (etrafından dolaşılır) | Yavru üstünden atlayamaz |
| `Trash` | Şişe, kum kalesi, ağ parçası, halka | Kısmen | Küçükler atlanır; ağ ve halka takar (`TrashTrap` bileşeni) |
| `Hazard` | Sıcak leke, ışık alanı, sıcak su, hayalet ağ | – | Collider **Is Trigger** olmalı ve `HazardZone` bileşeni taşımalı |
| `Cover` | Kumul gölgesi, deniz çayırı, kaya arası | – | **Is Trigger** + `Cover` bileşeni; avcıdan saklanma |
| `Structure` | Gündüz kurulan yapılar: gölgelik, kafes, çit | Hayır | Kartların sahnedeki hali |
| `Turtle` | Anne ve yavrular | – | |
| `Predator` | Yengeç, martı, tilki, avcı balık | – | |
| `Default` | Hiçbirine uymayan süs objeleri | Evet | |

## Tuşlar

Bütün tuşlar tek bir dosyada: `Assets/_Project/Scripts/Core/GameInput.cs`. Yeni tuş eklerken önce oradaki listeye bak, çakışma olmasın. Scriptlerde `Keyboard.current` ya da `Mouse.current` ile doğrudan tuş okunmaz.

| Tuş | Anne | Yavru |
| --- | --- | --- |
| WASD | Hareket | Hareket (sürüde öncü yavru) |
| Shift | Hızlı hareket (3 kat enerji) | Kumsalda sürüyü dağıt, bırakınca toplan |
| Space | Denizde yüzeye çık / dal | Kumsalda atla, denizde dal |
| E (basılı) | Ye, ağdan kurtul | Takılan ya da ters dönen yavruyu kurtar |
| A / D sırayla | Kazma ve örtme | – |
| W (kum altında) | – | Bir kat yukarı kaz |
| T | Termal görünüm | Termal görünüm |
| Esc | Duraklatma menüsü | Duraklatma menüsü |
| Fare | İnsan turunda harita, kart ve gönüllü seçimi | |
| F1–F4 | (Geliştirici) Bölüm 1, Kumsal, Bölüm 5, Menü'ye geç | |

## Kullanılan paketler

- Universal Render Pipeline
- Input System
- Cinemachine (kurulacak, Faz 2)
- MCP for Unity (Claude Code'un Unity editörünü kullanabilmesi için; oyuna girmez)
