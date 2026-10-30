# Yapay zekâ kuralları (ekipteki herkesin aracı için)

Bu repo 4 kişilik bir oyun ekibinin ortak reposu. Herkes kendi yapay zekâ aracını kullanabilir; hepsi bu kurallara uyar. Bu dosya ortaktır, kişisel not yazılmaz.

## Önce oku
- `README.md` (çalışma kuralları), `docs/GDD.md` (tasarımın tek doğru kaynağı), `docs/klasorler.md` (kim neyin sahibi).
- GDD uzun; sadece ilgili bölümü aç. Başlıklar: Oyun akışı, Kaplumbağa kısmı: ortak temel, Bölüm 1 Dönüş, Bölüm 2 Yuva geceleri, Bölüm 4 Kaçış, Bölüm 5 İlk yüzüş, İnsan kısmı: yönetim, İnsan kısmı: kartlar/kanıtlar/olaylar, İki tarafın bağlantısı.

## Yapma
- `main`'e doğrudan commit atma. Dal adı `isim/kisa-is`; iş bitince kullanıcıya PR açmasını söyle.
- `.unity`, `.prefab`, `.asset`, `.meta` dosyalarını metin olarak düzenleme; bunları Unity editöründe yapılacak adımlar olarak yaz.
- `Sahneler/` altındaki oyun sahnelerini değiştirme önerme (sahibi Umut); `Sahneler/Deneme/<Ad>.unity` kullan.
- Kullanıcının sahibi olmadığı klasördeki dosyayı değiştirme; gerekiyorsa sahibine ne söyleneceğini tek mesaj olarak yaz.
- Sayıları koda gömme; ScriptableObject ya da ayar asset'i kullan.
- Oyuncunun göreceği metni koda yazma; önce `docs/metinler.md`'ye ekle, kodda kimlik (`id`) ile çağır.
- `docs/GDD.md`'yi kullanıcı açıkça istemeden değiştirme. Değişirse `docs/kararlar.md`'ye bir satır ekle.

## Kod
- Unity 6000.3.25f1, Universal 3D, C#, Input System, Cinemachine.
- Kod `Unity/Assets/_Project/Scripts/<Alan>/` altında; namespace `IkiDunya.<Alan>` (ör. `IkiDunya.Yonetim`).
- `[SerializeField] private` alanlar; public alan yok.
- Alanlar birbirini doğrudan aramaz (`FindObjectOfType` yok). Ortak durum `Cekirdek` içindeki `KumsalDurumu` üzerinden okunur ve değiştirilir; değişiklik C# event'iyle bildirilir.
- İnsan kısmı (Toprak) kumsalı sadece `KumsalDurumu`'nun açık metotlarıyla değiştirir; kaplumbağa kısmı (Umut) bölüm başında bu durumdan kumsalı kurar. Yeni bir alana ihtiyaç varsa iki taraf da bilsin diye `docs/kararlar.md`'ye yazılır.
- Basit ve okunur kod; gereksiz soyutlama, yeni paket ekleme yok (paketleri Abdü ekler, önce ekibe sorulur).
