---
ozet: Mekanik tarafın durumu ve sistemlerin haritası. Bir sistem eklendikçe ya da değiştikçe güncellenir. Sahibi Umut; Toprak kendi sistemlerini işler.
---

# Durum

Son güncelleme: 2026-10-30 — Faz 2 (teknik kurulum). `Scripts/Core` iskeleti denendi. Faz 3 başı: gri kutu kumsal, birinci şahıs kamera, anne kara hareketi ve enerji yazıldı (Unity'de henüz denenmedi).

## Sistem haritası

| Sistem | Klasör | Ana script(ler) | Sahibi | Durum |
| --- | --- | --- | --- | --- |
| Oyun akışı ve bölüm geçişi | `Core` | `OyunAkisi`, `OyunAyarlari` | Umut | Yapılıyor |
| Kumsal durumu (ortak durum) | `Core` | `KumsalDurumu` | Umut | Yapılıyor (taslak) |
| Tuşlar | `Core` | `GameInput` | Umut | Yapılıyor |
| Kayıt | `Core` | — | Umut | Planlandı |
| Kameralar (hayvan kısmı birinci şahıs, insan kısmı yönetim ekranı) | `Camera` | `BirinciSahisKamera`, `KameraAyarlari` | Umut | Yapılıyor |
| Anne (kara): hareket, enerji, kazma | `Turtle` | `AnneKara`, `AnneAyarlari`, `Enerji` | Umut | Yapılıyor (hareket ve enerji; kazma yok) |
| Enerji çubuğu | `UI/Night` | `EnerjiCubugu` | Umut | Yapılıyor |
| Gri kutu kumsal ve layer kurulumu | `Editor` | `GriKumsalKur`, `KatmanKurulumu` | Umut | Yapılıyor |
| Yavru sürüsü | `Turtle` | — | Umut | Planlandı |
| Deniz hareketi, nefes | `Turtle` | — | Umut | Planlandı |
| Kum sıcaklığı ve termal görünüm (T) | `Hazards` | `KumSicakligi`, `TermalGorunum`, `TermalAyarlari` | Umut | Yapılıyor |
| Tehlike bölgeleri (sıcak leke, ışık, ağ) | `Hazards` | `HazardZone` | Umut | Planlandı |
| Avcılar | `Hazards` | — | Umut | Planlandı |
| Kayıp izleri | `Hazards` | — | Umut | Planlandı |
| Kumsal haritası ve tur akışı | `Management` | — | Toprak | Planlandı |
| Gönüllüler ve görevler | `Management` | — | Toprak | Planlandı |
| Kartlar | `Management` | `CardDefinition` | Toprak | Planlandı |
| Kanıtlar ve olaylar | `Management` | — | Toprak | Planlandı |

Durum değerleri: Planlandı · Yapılıyor · Oynanabilir · Bitti.

## `KumsalDurumu` sözleşmesi

> Faz 0'da Umut ve Toprak birlikte doldurur: alanlar, açık metotlar, event'ler.

**Taslak (Umut, Toprak'ın onayı bekleniyor).** `KumsalDurumu` bir ScriptableObject asset'i; kullanan her script ona `[SerializeField]` ile bağlanır. Yeni oyunda `OyunAkisi` sıfırlar. Her değişiklikten sonra `Degisti` event'i gelir.

| Ne | Okuma | Değiştirme (açık metot) | Kim çağırır |
| --- | --- | --- | --- |
| Bölgeler | `BolgeSayisi`, `BolgeAl(i).Seviye(katman)`, `BolgeAl(i).Var(yapi)` | `KatmanDegistir(bolge, katman, fark)`, `YapiKoy`, `YapiKaldir` | İnsan kısmı (kartlar, olaylar, simülasyon) |
| Katmanlar (0–`katmanEnFazla`) | `Cop`, `Isik`, `Sezlong`, `Insan` | | |
| Yapılar (var/yok) | `Golgelik`, `KumulOnarimi`, `TilkiCiti`, `AracIzi` | | |
| Denizdeki ağ | `DenizdekiAg` | `DenizdekiAgDegistir(fark)` | İnsan kısmı |
| Kum ısınması (bütün kum kaç bant ısındı) | `KumIsinmasi` | `KumIsinmasiDegistir(fark)` | İnsan kısmı (sıcak dalgası olayı) |
| Yuvalar | `Yuvalar` (bölge, konum, bant, dalga altında mı, yumurta, çıkışa kalan hafta, kafesli) | `YuvaEkle` (yuva gecesi), `YuvaKafesle`, `YuvaTasi`, `YuvaYumurtaKaybet`, `BirHaftaGecti` | Ekleme kaplumbağa, gerisi insan kısmı |
| Kayıp izleri | `KayipIzleri` (bölge, konum, neden, kanıt açıldı mı) | `KayipIziEkle` (gece), `KanitAc` (insan turu) | İkisi |
| Denize ulaşan yavru | `DenizeUlasanYavru` | `DenizeUlasti(adet)` | Kaplumbağa |

Bütçe, gönüllü ve halk desteği `KumsalDurumu`'nda değil; insan kısmının kendi verisi.

## Araçlar

- **İki Dünya → Layer'ları kur:** README'deki layer'ları boş yuvalara ekler.
- **İki Dünya → Gri kutu kumsalı kur:** `Scenes/Prototypes/GriKumsal.unity` sahnesini baştan kurar (kum, deniz, kumul, dalga çizgisi, 8 bölge, şezlong ve çöp, birinci şahıs anne, enerji çubuğu). Eksik `AnneAyarlari` ve `KameraAyarlari` asset'lerini `ScriptableObjects/Settings`'te oluşturur.

## Kum sıcaklığı

`KumSicakligi.Bant(konum)` bir noktanın bandını verir: denizden uzaklığa göre Mavi → Yeşil → Turuncu → Kırmızı (sınırlar `TermalAyarlari`'nda: 8, 18, 30 m), gölgelikli bölgede 1 bant serin, `KumsalDurumu.KumIsinmasi` kadar sıcak. Termal görünüm (T) her bölge ve bant için kumun üstüne renkli şerit serer; kumsal değişince yeniden renklenir. Kazma bu bandı yuvaya yazacak.

> Bantların denizden uzaklığa göre dizilmesi bir başlangıç varsayımı (serin bant dalga çizgisine yakın: serin ama fırtına riski). GDD yerleşimi söylemiyor; denemede değişebilir.

## Oyun akışı

`OyunAkisi` her oyun sahnesinde durur (ilki sahneler arasında yaşar). Adımlar `OyunAyarlari.AkisiKur()` ile GDD'deki sıradan kurulur: Bölüm 1 → 3 yuva gecesi (aralarında 1'er tur) → 6 bekleyiş turu → 3 kaçış gecesi (her birinden sonra 1 tur) → Bölüm 5; toplam 11 tur. Sistemler `OyunAkisi.AdimBasladi` event'ini dinler, adımları bitince `OyunAkisi.AdimBitti()` der. Kumsal sahnesinde gece ve tur arasında sahne yeniden yüklenmez. Bir oyun sahnesinde doğrudan Play'e basılırsa akış o sahnenin ilk adımından başlar. F1–F4 sadece geliştirici sürümünde çalışır.

## Bilinen sorunlar

- `IkiDunya.Camera` namespace'i yüzünden `IkiDunya.*` içindeki kodda `Camera` yazınca Unity'nin kamera sınıfı değil bu namespace bulunur. Kamera sınıfı için `UnityEngine.Camera` yaz.
