---
ozet: İki tarafın sözleşmesi. İnsan kısmı (Toprak) kumsalı sadece bu metotlarla değiştirir; kaplumbağa bölümleri (Umut) kumsalı buradan kurar. Her kart, görev ve olayın hangi metodu çağırdığı. Sahibi Umut + Toprak; değişiklik ikisinin onayıyla.
---

# KumsalDurumu sözleşmesi

Kod: `Assets/_Project/Scripts/Core/KumsalDurumu.cs`. Asset: `ScriptableObjects/Settings/KumsalDurumu.asset` (kullanan her script `[SerializeField]` ile bağlanır). Her değişiklikten sonra `Degisti` event'i gelir. Yeni oyunda kumsal `KumsalBaslangici` asset'inden kurulur.

> [!soru] Toprak'ın onayı bekleniyor. Eksik ya da fazla bir şey varsa bu dosyada PR ile değiştirin, `docs/kararlar.md`'ye bir satır.

## Okunanlar

| Ne | Nasıl | Not |
|---|---|---|
| Bölge sayısı | `BolgeSayisi` | Soldan sağa 0, 1, ... |
| Bölgenin katmanı (0–3) | `BolgeAl(i).Seviye(BolgeKatmani.Cop / Isik / Sezlong / Insan)` | |
| Bölgede yapı var mı | `BolgeAl(i).Var(BolgeYapisi.Golgelik / KumulOnarimi / TilkiCiti / AracIzi / Devriye)` | |
| Yuvalar | `Yuvalar[i]`: `Bolge`, `Konum`, `Bant` (farklar dahil), `KazildigiBant`, `DalgaAltinda`, `Yumurta`, `CikisinaHafta`, `Kafesli` | Sırası Bölüm 2'deki geceler; kaçış gecesi n, n. yuvadan çıkar |
| Kayıp izleri | `KayipIzleri[i]`: `Bolge`, `Konum`, `Neden`, `Kanit` (Isi/Isik/Cop/Avci/Su), `KanitAcildi` | Haritada simge |
| Denizdeki ağ seviyesi | `DenizdekiAg` | Bölüm 5'te ağ ve misina sayısı |
| Bütün kumun ısınması | `KumIsinmasi` | Bant olarak |
| Işık yasağı, Temiz Enerji | `IsikYasagi`, `TemizEnerji` | |
| Denize ulaşan yavru | `DenizeUlasanYavru` | Her 10'u +1 gönüllü (insan kısmı hesaplar) |

## Değiştirenler

### Kalıcı
| Metot | Ne yapar |
|---|---|
| `KatmanDegistir(bolge, katman, fark)` | Katmanı 0–3 arasında değiştirir |
| `YapiKoy(bolge, yapi)`, `YapiKaldir(bolge, yapi)` | Yapı koyar / kaldırır |
| `DenizdekiAgDegistir(fark)` | |
| `KumIsinmasiDegistir(fark)` | |
| `IsikYasagiAyarla(var)`, `TemizEnerjiAyarla(var)` | |
| `YuvaKafesle(yuva)`, `YuvaTasi(yuva, bolge, konum, dalgaAltinda)` | |
| `YuvaYumurtaKaybet(yuva, adet)` | Fırtına, tilki |
| `YuvaBantDegistir(yuva, fark)` | Yuvanın sıcaklığını kalıcı değiştirir (ör. taşınınca) |
| `KanitAc(iz)` | İzi açılmış yapar, `KanitAcildi(iz)` event'ini atar |
| `BirHaftaGecti()` | **Tur sonunda insan kısmı çağırır.** Yuvaların çıkışına bir hafta yaklaşır; süreli etkiler azalır, biten geri alınır |

### Süreli ("o tur", "bir gece")
Süre tur sayısıdır. `1` = sıradaki `BirHaftaGecti()`'de geri alınır; yani bu turun ardından gelen gece boyunca sürer.

| Metot | Ne yapar |
|---|---|
| `GeciciKatman(bolge, katman, fark, tur)` | Katmanı süreli değiştirir |
| `GeciciYapi(bolge, yapi, tur)` | Yapıyı süreli koyar (zaten varsa dokunmaz) |
| `GeciciKumIsinmasi(fark, tur)` | Bütün kumu süreli ısıtır |
| `GeciciYuvaBant(yuva, fark, tur)` | Yuvanın sıcaklığını süreli değiştirir |

## Kartlar → metot

| Kart | Çağrı |
|---|---|
| Gölgelik | `YapiKoy(bolge, Golgelik)` (kum 1 bant serin, kaçışta lekeler yarı boy) |
| Temizlik Kampanyası | 3 bölge için `KatmanDegistir(bolge, Cop, -1)` |
| Kumul Onarımı | `YapiKoy(bolge, KumulOnarimi)` (dalga çizgisi 4 m denize çekilir) |
| Yuva Taşıma | `YuvaTasi(...)`; yeni yerin bandı farklıysa `YuvaBantDegistir` |
| Işık Yasağı | `IsikYasagiAyarla(true)` (otel ve yol ışıkları söner) |
| Araç Yasağı | Bütün bölgeler için `YapiKaldir(bolge, AracIzi)` |
| Şezlong Düzenlemesi | Bölgelerde `KatmanDegistir(bolge, Sezlong, -n)` (yarıya) |
| Tilki Çiti | `YapiKoy(bolge, TilkiCiti)` (kenardaki bir bölge yeter; tilki ihtimali yarıya iner) |
| Hayalet Ağ Temizliği | `DenizdekiAgDegistir(-1)` |
| Temiz Enerji | `TemizEnerjiAyarla(true)` (final sahnesi değişir) |
| Gönüllü Çağrısı, Saha Araştırması | Kumsala dokunmaz (insan kısmının kendi verisi) |

## Gönüllü görevleri → metot

| Görev | Çağrı |
|---|---|
| Temizlik | `KatmanDegistir(bolge, Cop, -1)` |
| Gece devriyesi | `GeciciYapi(bolge, Devriye, 1)` (bölgede insan ve fener olmaz) |
| Yuva kafesi | `YuvaKafesle(yuva)` |
| Gölge ve sulama | `GeciciYuvaBant(yuva, -1, 1)` |
| Bilgilendirme | Kumsala dokunmaz (halk desteği) |

## Olaylar → metot

| Olay | Çağrı |
|---|---|
| Otel şezlongu (kabul) | `KatmanDegistir(bolge, Sezlong, +3)` |
| Festival (kabul) | `GeciciKatman(bolge, Isik, +2, 1)` ve `GeciciKatman(bolge, Insan, +2, 1)` (bir gece) |
| Balıkçılara destek | `DenizdekiAgDegistir(-1)` |
| Sıcak dalgası | Uyarıdan sonraki tur `GeciciKumIsinmasi(+1, 1)`; Temiz Enerji varsa etki zayıf (insan kısmı karar verir) |
| Fırtına | Dalga altındaki her yuva için `YuvaYumurtaKaybet(yuva, Yumurta / 2)` ve bir Su izi: `KayipIziEkle(new KayipIzi(bolge, konum, KayipNedeni.Dalga))` |
| Okul ziyareti | Kumsala dokunmaz |

## Tur sonu simülasyonu (insan kısmı)
GDD: "çöp birikir, sıcaklık hava durumuna göre değişir, korunmayan yuvalara avcı gelebilir." Önerilen:
- Çöp birikimi: rastgele bölgelerde `KatmanDegistir(bolge, Cop, +1)`.
- Korunmayan yuva: `Kafesli` değilse ve bölgede `TilkiCiti` yoksa ihtimalle `YuvaYumurtaKaybet` ve bir Avcı izi (`KayipNedeni.Tilki`).
- En sonda `BirHaftaGecti()`.

## Kanıtlar
İz simgesine tıklanınca `KanitAc(iz)` çağrılır ve `KanitAcildi(iz)` event'i gelir; `iz.Kanit` hangi kartın açılacağını ya da ucuzlayacağını söyler (GDD tablosu). Şimdilik simgeleri geçici insan turu gösteriyor (`KayipIziHaritasi`); yönetim ekranı aynı event'i dinler.
