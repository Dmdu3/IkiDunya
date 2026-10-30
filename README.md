# İki Dünya, Tek Yaşam

Bir anne caretta caretta ile başlayıp onun yavrusuyla biten, iklim ve çevre temalı bir oyun. Oyuncu kaplumbağa bölümlerinde denizde ve kumsalda hayatta kalır, aralardaki insan turlarında gönüllü ekibini yönetir; insanın kararları bir sonraki gecenin kumsalını değiştirir.

Unity 6000.3.25f1 · Universal 3D · PC

**Tasarımın tamamı:** [`docs/GDD.md`](docs/GDD.md) — oyunun tek doğru kaynağı.

## Ekip

| Kişi | Rol | Alanı |
|---|---|---|
| Umut | Ana kodcu | Çekirdek, kaplumbağa ve yavru kontrolü, tehditler, kamera, sahneler, build |
| Toprak | Sistem tasarımı + ikinci kodcu | İnsan kısmı (yönetim), kartlar, olaylar, denge |
| Yusuf | İçerik ve sunum | Metinler, bilgi doğrulama, model ve ses seçimi, tanıtım |
| Abdü | Ses, model yerleştirme, test | Ses, modeller, paket ekleme, playtest |

Kim hangi klasörün sahibi: [`docs/klasorler.md`](docs/klasorler.md)  
Kim ne yapıyor: [`docs/yol-haritasi.md`](docs/yol-haritasi.md)  
Repoyu ilk kez kuracaksan: [`docs/EKIP-REHBERI.md`](docs/EKIP-REHBERI.md)

## Klasörler

```
docs/                 ekibin ortak belgeleri
  GDD.md              tasarım (tek doğru kaynak)
  yol-haritasi.md     fazlar ve kişi kişi işler
  klasorler.md        klasör sahipliği
  kararlar.md         alınan kararlar ve nedenleri
  metinler.md         oyundaki bütün yazılar (Yusuf)
  kaynaklar.md        bilgilerin kaynakları (Yusuf)
  varliklar.md        model, ses, font ve lisansları
  playtest.md         test notları (Abdü)
  denge-gunlugu.md    sayı değişiklikleri (Toprak)
  EKIP-REHBERI.md     kurulum ve günlük çalışma
Unity/                Unity projesi (Unity Hub'da bu klasörü aç)
  Assets/_Project/    bizim her şeyimiz (ayrıntı: docs/klasorler.md)
  Assets/ThirdParty/  indirilen paketler
```

## Çalışma kuralları

1. **main her zaman çalışır.** Doğrudan main'e gönderme yok. Her iş kendi dalında yapılır ve Pull Request ile girer.
2. **Dal adı `isim/kisa-is`.** Örnek: `umut/yavru-suru`, `toprak/gonullu-atama`, `yusuf/kart-metinleri`, `abdu/dalga-sesleri`.
3. **Küçük ve sık.** Bir dal en fazla 1–2 gün yaşar. Her çalışmaya başlarken `main`'i kendi dalına çek.
4. **Sahne kuralı.** Unity sahneleri birleştirilemez; iki kişi aynı sahneyi değiştirirse birinin işi kaybolur.
   - `Sahneler/` altındaki oyun sahnelerini **sadece Umut** değiştirir.
   - Herkes kendi deneme sahnesinde çalışır: `Sahneler/Deneme/<Ad>.unity`.
   - Sahneye girecek şeyi **prefab** yap; ana sahneye Umut koyar.
5. **Başkasının klasörüne dokunma.** Gerekiyorsa o kişiye yaz (bkz. `docs/klasorler.md`).
6. **Sayılar koda gömülmez.** Tasarım değeri `docs/GDD.md`'de, oyundaki değeri `Veri/` altındaki ScriptableObject'lerde durur. Değişiklik `docs/denge-gunlugu.md`'ye yazılır.
7. **Metinler önce `docs/metinler.md`'ye**, sonra oyuna girer.
8. **50 MB üstü dosya girmez.** Model ve sesleri eklemeden önce küçült; kaynağını `docs/varliklar.md`'ye yaz.
9. **PR'ı kim birleştirir:** kendi alanındaki PR'ı herkes kendisi birleştirebilir. Sahnelere, `Scripts/Cekirdek/`, `ProjectSettings/` ve `Packages/`'a dokunan PR'ları Umut birleştirir.

## Yapay zekâ kullananlar için

Aracına ilk iş olarak şunu söyle: *"README.md, CLAUDE.md, docs/GDD.md ve docs/klasorler.md'yi oku, bu kurallara uy."* Kurallar [`CLAUDE.md`](CLAUDE.md) içinde.
