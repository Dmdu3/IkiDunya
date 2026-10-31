---
ozet: Unity projesinde hangi klasörün ve belgenin sahibi kim; iki tarafın buluştuğu yer. Başkasının klasörüne dokunmadan önce ona yaz.
---

# Klasör sahipliği

Unity projesi reponun kökü. Bizim her şeyimiz `Assets/_Project/` altında. Kod namespace'i `IkiDunya.<Alan>`.

## Kod (`Assets/_Project/Scripts/`)

| Klasör | Sahibi | İçinde ne var |
| --- | --- | --- |
| `Core/` | Umut | Oyun akışı, bölüm geçişleri, kayıt, `KumsalDurumu` (ortak durum), `GameInput` (bütün tuşlar) |
| `Camera/` | Umut | Kumsal, deniz ve yönetim kameraları (Cinemachine) |
| `Turtle/` | Umut | Anne ve yavru hareketi (kara, deniz), sürü, enerji, nefes, kazma |
| `Hazards/` | Umut | Ağ, tekne, sıcak leke, çöp, avcılar, ışık, dalga |
| `Management/` | Toprak | Kumsal haritası, tur akışı, gönüllüler, kaynaklar, kartlar, kanıtlar, olaylar |
| `UI/Night/` | Umut | Kaplumbağa bölümlerinin ekran göstergeleri |
| `UI/Menu/` | Umut | Ana menü, ayarlar, duraklatma menüsü |
| `UI/Management/` | Toprak | Harita ekranı, kart eli, olay penceresi |
| `Editor/` | Umut | Sahne kurulum araçları |

## Veri, sahne, varlık (`Assets/_Project/`)

| Klasör | Sahibi | İçinde ne var |
| --- | --- | --- |
| `ScriptableObjects/Cards`, `Events`, `VolunteerTasks` | Toprak | Kart, olay ve gönüllü görevi asset'leri |
| `ScriptableObjects/Settings` | Umut + Toprak | Bölüm ve kumsal ayarları; değiştiren diğerine haber verir |
| `Scenes/Levels/` | Umut | `Menu`, `Bolum1_Donus`, `Kumsal` (Bölüm 2–4 ve insan turları), `Bolum5_IlkYuzus` |
| `Scenes/Prototypes/` | Umut | Gri kutu denemeler |
| `Prefabs/Turtles`, `Environment`, `Hazards`, `Structures`, `UI` | Umut | Oyun nesneleri; `Structures` (gölgelik, kafes, çit) ve `UI/Management` için Toprak isteyebilir |
| `Art/`, `Models/`, `Materials/`, `Animations/` | Abdü | Hazır görsel ve modeller; animasyon kodu Umut'ta |
| `Audio/Ambience`, `Music`, `SFX` | Abdü | Sesler ve müzik |
| `Shaders/` | Umut | Termal kum, su, gece ışığı |
| `Assets/Sandbox/<Ad>/` | Herkes kendi | Deneme alanı ve deneme sahnesi |
| `Assets/ThirdParty/<yayıncı>/` | Abdü | Paketler; eklemeden önce ekibe sor |

## Belgeler (`docs/`)

| Dosya | Sahibi |
| --- | --- |
| `GDD.md`, `kararlar.md` | Ekip (değişiklik PR ile) |
| `DURUM.md` | Umut (Toprak kendi sistemlerini işler) |
| `metinler.md`, `kaynaklar.md` | Yusuf |
| `varliklar.md`, `playtest.md` | Abdü |
| `denge-gunlugu.md` | Toprak |

## İki tarafın buluştuğu yer

İnsan kısmı (Toprak) kumsalı sadece `Core/KumsalDurumu`'nun açık metotlarıyla değiştirir. Kaplumbağa bölümleri (Umut) bölüm başında kumsalı bu durumdan kurar. Yeni bir alan ya da metot gerekirse önce birbirinize yazın, sonra `docs/kararlar.md`'ye bir satır ekleyin.
