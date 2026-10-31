# Deniz tabanı (Bölüm 1, Bölüm 5, Poşet sürüsü, Çöp girdabı)

Akdeniz tabanı: kum, kayalık şev, oyun alanının dışında kayalık yamaçlar, mercan kayalıkları (gorgonya, kırmızı mercan, taş mercan, sarı sünger, pembe kabuklu algler) ve Posidonia çayırı. Blender betiğiyle üretildi. Unity'de malzemeler ve prefab'lar kuruldu; henüz bir sahnede denenmedi.

![önizleme](Kaynak~/onizleme.png)

## Dosyalar

| Dosya | İçinde | Not |
| --- | --- | --- |
| `DenizTabani.fbx` | `DenizTabani_Zemin`, `Dekor_Kayalar`, `Dekor_Mercanlar` | Sahne koordinatında; kökü (0,0,0)'a koy |
| `Kayalar.fbx` | `Kaya_1`, `Kaya_2`, `Kaya_3` | ~3 m; Bölüm 5'teki gri kaya küplerinin yerine. Pivot tabanın ortası |
| `DenizCayiri.fbx` | `DenizCayiri_6x10`, `_6x8`, `_8x8` | `GriDenizKur`'daki üç saklanma alanının boyunda (en x, uzunluk z). Pivot tabanın ortası, boy ~1,9 m |
| `DenizTabani_Renk.png` | Taban rengi (1024×2048) | Bütün tabana bir kez yayılır |
| `DenizObjeleri_Renk.png` | Renk atlası (256×256, 8 sütun) | Kaya, kabuk, gorgonya, kırmızı mercan, taş mercan, sünger, posidonia, çakıl |
| `Kaynak~/` | Üretim betiği, `.blend`, önizleme | Unity bu klasörü görmez |
| `DenizTabani.prefab` | Bütün taban | Sahneye (0,0,0)'a konur |
| `Kaya_1..3.prefab`, `DenizCayiri_6x10/6x8/8x8.prefab` | Tek parçalar | Pivot tabanın ortası |
| `DenizTabani_Zemin.mat`, `DenizObjeleri.mat`, `DenizCayiri.mat` | Malzemeler | Aşağıdaki kuruluma göre |

Üçgen: zemin 282 bin, süs kayaları 46 bin, mercanlar 266 bin, çayır alanı başına 15–20 bin.

## Ölçüler (gri kutuyla aynı)

Kıyı z = 0, deniz −z, su yüzeyi y = 0. Taban: z −50'ye kadar kıyıdan inen eğim, sığ düzlük y −6 (z −112..−66), kayalık şev (z −114..−128), açık deniz y −15 (z −240'a kadar; arka duvarın ötesi de dolu). Genişlik x −70..70; oyun alanı (x ±30) dışında yamaçlar yükselir, y −1,3'ü geçmez.

- Ortadaki oyun şeridinde (|x| < 18) süsler derinde y −10,97'nin altında kalır (oyun nesneleri −12'nin üstünde); sığ düzlükte ortada süs yok.
- Kıyı ucu `KumsalArazi` deniz sahnesine y +0,55'e konursa onun kumunun 0,12–0,35 m altında kalır; arazi konmazsa taban kıyıya kadar kendi başına uzanır.

## Unity'de kurulum

Aşağıdakiler yapıldı (2026-11-01); modeller yeniden üretilirse içe aktarma ayarları korunur, prefab'lar kendiliğinden güncellenir.

1. **Malzemeler** (URP/Lit, bu klasörde):
   - `DenizTabani_Zemin`: Base Map `DenizTabani_Renk`, Smoothness 0,08. Detail Normal Map `KumsalArazi/Kum_Dalgacik_N`, ölçek 0,25, Detail tiling **47 × 84** (Kumsal_Zemin gibi 3 m'de bir dalgacık).
   - `DenizObjeleri`: Base Map `DenizObjeleri_Renk`, Smoothness 0,1.
   - `DenizCayiri`: Base Map `DenizObjeleri_Renk`, **Render Face: Both** (yapraklar tek katman).
   - `DenizObjeleri_Renk` dokusunda **Generate Mip Maps kapalı** ve Wrap Mode Clamp (yoksa uzaktan renk sütunları karışır).
2. **FBX içe aktarma:** Scale Factor 1, Generate Colliders kapalı; Materials sekmesinde FBX'teki malzeme adlarını yukarıdakilere eşle.
3. **Prefab `DenizTabani`:** boş kök, **Y ekseninde 180° döndür** (KumsalArazi.prefab gibi; Blender'ın varsayılan FBX eksenleri yüzünden), altına `DenizTabani.fbx`.
   - `DenizTabani_Zemin`, `Dekor_Kayalar`: layer **Ground**, MeshCollider.
   - `Dekor_Mercanlar`: layer Default, collider yok (içinden geçilir).
4. **Kayalar ve çayır:** her biri için prefab (kök yine Y 180°). Kayalar Ground + MeshCollider. Çayır sadece görüntü: layer Default, collider yok; saklanmayı `GriDenizKur`'un kurduğu `Cover` kutusu yapmaya devam eder.

Deniz kurulumu (`Editor/GriDenizKur.cs`, sahibi Umut) bu modelleri henüz aramıyor; sahneye girmesi için orada ekleme gerekiyor.

## Yeniden üretme

```bash
python "Kaynak~/deniz_tabani_uret.py" -- . ../KumsalArazi/Kumsal_Zemin.fbx
```

Blender 5.2 ile (`blender -b -P Kaynak~/deniz_tabani_uret.py -- . ../KumsalArazi/Kumsal_Zemin.fbx`) ya da pip'teki `bpy` modülüyle çalışır; bu klasörde çalıştırılır. Ölçüler, renkler ve yoğunluk betiğin başında. Aynı tohumla aynı sonuç çıkar.
