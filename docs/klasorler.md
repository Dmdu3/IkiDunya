---
ozet: Unity projesinde hangi klasörün sahibi kim. Başkasının klasörüne dokunmadan önce ona yaz.
---

# Klasör sahipliği

Yol: `Unity/Assets/_Project/`. Kod namespace'i klasör adıyla aynıdır: `IkiDunya.<Alan>`.

| Klasör | Sahibi | İçinde ne var |
|---|---|---|
| `Scripts/Cekirdek/` | Umut | Oyun akışı, bölüm geçişleri, kayıt, `KumsalDurumu` (ortak durum) |
| `Scripts/Kaplumbaga/` | Umut | Anne ve yavru hareketi (kara, deniz), sürü, enerji, nefes, kazma |
| `Scripts/Tehditler/` | Umut | Ağ, tekne, sıcak leke, çöp, avcılar, ışık, dalga |
| `Scripts/Yonetim/` | Toprak | Kumsal haritası, tur akışı, gönüllüler, kaynaklar, kartlar, kanıtlar, olaylar |
| `Scripts/Arayuz/Gece/` | Umut | Kaplumbağa bölümlerinin ekran göstergeleri |
| `Scripts/Arayuz/Yonetim/` | Toprak | Harita ekranı, kart eli, olay penceresi |
| `Scripts/Editor/` | Umut | Sahne kurulum araçları |
| `Veri/Kartlar/`, `Veri/Olaylar/`, `Veri/Gorevler/` | Toprak | Kart, olay ve gönüllü görevi ScriptableObject'leri |
| `Veri/Ayarlar/` | Umut + Toprak | Bölüm ve kumsal ayarları; değiştiren diğerine haber verir |
| `Sahneler/` (oyun sahneleri) | Umut | `Bolum1_Donus`, `Kumsal` (Bölüm 2–4), `Bolum5_IlkYuzus`, `Menu` |
| `Sahneler/Deneme/<Ad>.unity` | Herkes kendi | Deneme sahnesi |
| `Prefabs/` | Umut | Oyun nesneleri (alt klasör sahibi isteyebilir) |
| `Modeller/`, `Ses/` | Abdü | Hazır model ve sesler |
| `Arayuz/Gorseller/` | Abdü | İkon, kart görseli |
| `Assets/ThirdParty/` | Abdü | Paketler; ekleme tek elden, önce ekibe sor |
| `docs/metinler.md`, `docs/kaynaklar.md` | Yusuf | Metinler ve kaynaklar |
| `docs/playtest.md` | Abdü | Test notları |
| `docs/denge-gunlugu.md` | Toprak | Sayı değişiklikleri |

## İki tarafın buluştuğu yer

İnsan kısmı (Toprak) kumsalı sadece `Cekirdek/KumsalDurumu`'nun açık metotlarıyla değiştirir. Kaplumbağa kısmı (Umut) bölüm başında kumsalı bu durumdan kurar. Yeni bir alan ya da metot gerekirse önce birbirinize yazın, sonra `docs/kararlar.md`'ye bir satır ekleyin.
