---
ozet: Mekanik tarafın durumu ve sistemlerin haritası. Bir sistem eklendikçe ya da değiştikçe güncellenir. Sahibi Umut; Toprak kendi sistemlerini işler.
---

# Durum

Son güncelleme: 2026-10-31 — Faz 3: yuva gecesi (kazma, çöp, insanlar, deneme hakkı) ve kaçış gecesi (kumdan çıkış, sürü, sıcak leke, gün doğumu, dalga), kayıp izleri, gölgelik yazıldı (Unity'de henüz denenmedi).

## Sistem haritası

| Sistem | Klasör | Ana script(ler) | Sahibi | Durum |
| --- | --- | --- | --- | --- |
| Oyun akışı ve bölüm geçişi | `Core` | `OyunAkisi`, `OyunAyarlari` | Umut | Yapılıyor |
| Kumsal durumu (ortak durum) | `Core` | `KumsalDurumu` | Umut | Yapılıyor (taslak) |
| Tuşlar | `Core` | `GameInput` | Umut | Yapılıyor |
| Kayıt | `Core` | — | Umut | Planlandı |
| Kameralar (hayvan kısmı birinci şahıs, insan kısmı yönetim ekranı) | `Camera` | `BirinciSahisKamera`, `KameraAyarlari` | Umut | Yapılıyor |
| Anne (kara): hareket, enerji, kazma | `Turtle` | `AnneKara`, `AnneKazma`, `AnneAyarlari`, `Enerji` | Umut | Yapılıyor |
| Yuva gecesi: denemeler, yuva sonucu | `Turtle` | `YuvaGecesi` | Umut | Yapılıyor |
| Kaçış gecesi: kumdan çıkış, koşu | `Turtle` | `KacisGecesi`, `Suru`, `Yavru`, `YavruAyarlari` | Umut | Yapılıyor |
| Gece göstergeleri (enerji, kazma, kumdan çıkış, gün doğumu) | `UI/Night` | `EnerjiCubugu`, `Cubuk`, `KazmaGostergesi`, `KacisGostergesi` | Umut | Yapılıyor |
| Gri kutu kumsal ve layer kurulumu | `Editor` | `GriKumsalKur`, `KatmanKurulumu` | Umut | Yapılıyor |
| Deniz hareketi, nefes | `Turtle` | — | Umut | Planlandı |
| Kum sıcaklığı ve termal görünüm (T) | `Hazards` | `KumSicakligi`, `TermalGorunum`, `TermalAyarlari` | Umut | Yapılıyor |
| Çöp yığını (anne takılır) | `Hazards` | `TrashTrap` | Umut | Yapılıyor |
| İnsanlar ve görüş konisi | `Hazards` | `Insan`, `InsanAyarlari` | Umut | Yapılıyor |
| Sıcak kum lekesi, gün doğumu, dalga | `Hazards` | `HazardZone`, `GunDogumu`, `Dalga`, `KacisAyarlari` | Umut | Yapılıyor |
| Kumsalı durumdan kurma (gölgelik, kayıp izleri) | `Hazards` | `KumsalYapilari` | Umut | Yapılıyor |
| Tehlike bölgeleri (ışık, ağ) | `Hazards` | `HazardZone` | Umut | Planlandı |
| Avcılar | `Hazards` | — | Umut | Planlandı |
| Kayıp izleri | `Turtle`, `Hazards` | `KacisGecesi` yazar, `KumsalYapilari` gösterir | Umut | Yapılıyor |
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
- **İki Dünya → Gri kutu kumsalı kur (yuva gecesi):** `Scenes/Prototypes/GriKumsal.unity` — kumsal, termal görünüm, şezlong, çöp yığını, 2 insan (biri fenerli), anne (hareket, kazma), yuva gecesi, enerji ve kazma çubuğu.
- **İki Dünya → Gri kutu kaçışı kur:** `Scenes/Prototypes/GriKacis.unity` — kumsal, sıcak kum lekeleri, dalga, gün doğumu, sürü, kaçış gecesi, kumdan çıkış paneli ve gün doğumu çubuğu.
- İki araç da eksik ayar asset'lerini `ScriptableObjects/Settings`'te, `Yavru`, `Golgelik`, `KayipIzi` kalıplarını `Prefabs/` altında oluşturur. Deneme sahnelerinde akış yoktur; gece kendiliğinden başlar.

## Kum sıcaklığı

`KumSicakligi.Bant(konum)` bir noktanın bandını verir: denizden uzaklığa göre Mavi → Yeşil → Turuncu → Kırmızı (sınırlar `TermalAyarlari`'nda: 8, 18, 30 m), gölgelikli bölgede 1 bant serin, `KumsalDurumu.KumIsinmasi` kadar sıcak. Termal görünüm (T) her bölge ve bant için kumun üstüne renkli şerit serer; kumsal değişince yeniden renklenir. Kazma bu bandı yuvaya yazacak.

> Bantların denizden uzaklığa göre dizilmesi bir başlangıç varsayımı (serin bant dalga çizgisine yakın: serin ama fırtına riski). GDD yerleşimi söylemiyor; denemede değişebilir.

## Kumsal geceleri

**Yuva gecesi (`YuvaGecesi`).** Anne denizden çıkar; W/S ile yürür, fareyle döner. Yerde dururken A ve D'ye sırayla basınca kazma başlar (20 vuruş), kısa yumurtlama, sonra örtme (10 vuruş). Yuvanın bölgesi, bandı, dalga çizgisine göre yeri ve yumurta sayısı (kırmızı bantta 6) `KumsalDurumu.YuvaEkle` ile yazılır; anne denize dönünce gece biter. İnsanın görüş konisine girmek ya da enerjinin bitmesi denemeyi bitirir; 2 deneme de biterse anne bulunduğu bölgede kıyıya yakın kendiliğinden yuva kazar. Çöp yığını anneyi 2 sn durdurur, 5 enerji alır.

**Kaçış gecesi (`KacisGecesi`).** Gecenin yuvası Bölüm 2'deki aynı sıradaki yuvadır (yoksa deneme yuvası). Kumdan çıkış: W ile 5 kat; yüzey zamanla serinler, ölümcül sıcaklığın üstünde çıkılırsa yavruların o oranı ölür; kum altında enerji biterse çıkış baştan. Koşu: öncüyü oyuncu sürer, diğerleri izler, Shift dağıtır. Sıcak lekeler gün doğdukça büyür (gölgelikli bölgede yarı boy), içindeki yavru enerji kaybeder. Dalga son 5 m'de gelip gider: çekilirken suya giren kurtulur, vururken giren geri savrulur. Öncü kurtulur ya da ölürse sıradaki yavru öncü olur. Gün doğunca kumda kalanlar kaybedilir. Her kayıp `KayipIziEkle`, her kurtulan `DenizeUlasti(1)`.

**Kumsalı durumdan kurma (`KumsalYapilari`).** Gölgelikli bölgeye gölgelik koyar, kayıp izlerinin yerine işaret bırakır; `KumsalDurumu` değişince yeniden kurar. Kumul onarımı olan bölgede dalga çizgisi 4 m denize çekilir (`KumSicakligi.DalgaAltinda`); çizginin görüntüsü henüz kaymıyor.

Henüz yok: yavruların çöpe takılması ve E ile kurtarma, avcılar, ışık, tilki (Faz 5); gecenin sonuç ekranı (metinler gerekiyor).

## Oyun akışı

`OyunAkisi` her oyun sahnesinde durur (ilki sahneler arasında yaşar). Adımlar `OyunAyarlari.AkisiKur()` ile GDD'deki sıradan kurulur: Bölüm 1 → 3 yuva gecesi (aralarında 1'er tur) → 6 bekleyiş turu → 3 kaçış gecesi (her birinden sonra 1 tur) → Bölüm 5; toplam 11 tur. Sistemler `OyunAkisi.AdimBasladi` event'ini dinler, adımları bitince `OyunAkisi.AdimBitti()` der. Kumsal sahnesinde gece ve tur arasında sahne yeniden yüklenmez. Bir oyun sahnesinde doğrudan Play'e basılırsa akış o sahnenin ilk adımından başlar. F1–F4 sadece geliştirici sürümünde çalışır.

## Bilinen sorunlar

- `IkiDunya.Camera` namespace'i yüzünden `IkiDunya.*` içindeki kodda `Camera` yazınca Unity'nin kamera sınıfı değil bu namespace bulunur. Kamera sınıfı için `UnityEngine.Camera` yaz.
