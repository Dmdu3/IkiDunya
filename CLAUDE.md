# Yapay zekâ kuralları (ekipteki herkesin aracı için)

Bu repo 4 kişilik bir oyun ekibinin ortak reposu. Herkes kendi yapay zekâ aracını kullanabilir; hepsi bu kurallara uyar. Bu dosya ortaktır, kişisel not yazılmaz.

## Önce oku
- `README.md` (kurallar, klasörler, layer'lar, tuşlar), `docs/GDD.md` (tasarımın tek doğru kaynağı), `docs/klasorler.md` (kim neyin sahibi), `docs/DURUM.md` (hangi sistem nerede, ne durumda).
- GDD uzun; sadece ilgili bölümü aç. Başlıklar: Oyun akışı, Kaplumbağa kısmı: ortak temel, Bölüm 1 Dönüş, Bölüm 2 Yuva geceleri, Bölüm 4 Kaçış, Bölüm 5 İlk yüzüş, İnsan kısmı: yönetim, İnsan kısmı: kartlar/kanıtlar/olaylar, İki tarafın bağlantısı.

## Yapma
- `main`'e doğrudan commit atma. Dal adı `isim/kisa-is`; iş bitince kullanıcıya PR açmasını söyle.
- `.unity`, `.prefab`, `.asset`, `.meta` dosyalarını metin olarak düzenleme; bunları Unity editöründe (ya da MCP for Unity ile) yapılacak adımlar olarak yap/yaz.
- `Assets/_Project/Scenes/Levels` sahnelerini değiştirme önerme (sahibi Umut); denemeler `Assets/Sandbox/<ad>/` içinde.
- Kullanıcının sahibi olmadığı klasördeki dosyayı değiştirme; gerekiyorsa sahibine ne söyleneceğini tek mesaj olarak yaz.
- Sayıları koda gömme; ScriptableObject (`ScriptableObjects/`) ya da ayar asset'i kullan.
- Oyuncunun göreceği metni koda yazma; önce `docs/metinler.md`'ye ekle, kodda kimlik (`id`) ile çağır.
- Tuşu doğrudan okuma (`Keyboard.current`, `Mouse.current`); `Scripts/Core/GameInput.cs` üzerinden.
- `docs/GDD.md`'yi kullanıcı açıkça istemeden değiştirme. Değişirse `docs/kararlar.md`'ye bir satır ekle.

## Kod
- Unity 6000.3.25f1, URP, C#, Input System, Cinemachine. Unity projesi reponun kökü.
- Kod `Assets/_Project/Scripts/<Alan>/` altında: `Core`, `Camera`, `Turtle`, `Hazards`, `Management`, `UI`, `Editor`. Namespace `IkiDunya.<Alan>` (ör. `IkiDunya.Management`).
- `[SerializeField] private` alanlar; public alan yok.
- Alanlar birbirini doğrudan aramaz (`FindObjectOfType` yok). Ortak durum `Core/KumsalDurumu` üzerinden okunur ve değiştirilir; değişiklik C# event'iyle bildirilir.
- İnsan kısmı (`Management`, Toprak) kumsalı sadece `KumsalDurumu`'nun açık metotlarıyla değiştirir; kaplumbağa bölümleri (`Turtle`, `Hazards`, Umut) bölüm başında kumsalı bu durumdan kurar. Yeni bir alan ya da metot gerekirse `docs/kararlar.md`'ye bir satır.
- Sahneye konan her obje doğru layer'da olmalı (README → Layer'lar).
- Yeni bir sistem ya da önemli değişiklik `docs/DURUM.md`'ye işlenir.
- Basit ve okunur kod; gereksiz soyutlama yok. Paket eklemeden önce ekibe sorulur.
