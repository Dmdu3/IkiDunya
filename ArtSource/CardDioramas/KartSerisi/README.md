# Kart serisi — düzenlenebilir tasarım örnekleri

`GALERI.html` kaydedilmiş kartları tek yerde gösterir; bir karta tıklamak detaylı sunumunu açar. Her `kart_...` klasöründe `SUNUM.png` kart ve açıklamayı birlikte gösterir; `kart.png` yalnızca karttır. İlgili `.blend` düzenlenebilir model, `kart_..._master.png` saydam 3D render ve `bilgi.json` kartın tasarım önizlemesi verisidir. Yeni model üretilen kartlarda `qa.json` render boyutu, alfa ve kadraj değerlerini kaydeder.

## Metin ve sayıları görseli yeniden üretmeden değiştirmek

1. İlgili kart klasöründeki `bilgi.json` dosyasının `ad`, `tur`, `butce`, `bakim`, `aciklama` veya `ek` alanını değiştirin.
2. `yenile.py` betiğini kart kimliğiyle çalıştırın. Bu işlem SVG, JSON ve PNG önizlemelerini yeniden oluşturur; Blender renderını ve modeli değiştirmez.

Blender'ın Python yorumlayıcısıyla tasarım yenileme örneği (komutu KartSerisi klasöründe çalıştırın):

    "C:\Program Files\Blender Foundation\Blender 5.2\5.2\python\bin\python.exe" yenile.py kart_kumul

PowerShell betik politikası .ps1 dosyalarının doğrudan çalıştırılmasını engelleyebilir. PNG önizlemeleri paylaşılır; SVG çıktıları betik çalıştırıldığında yeniden oluşturulur. JSON tasarımları görseli kendi kart klasörüne göre bulur.

`butce` burada **temel bedeldir**; indirimler ve oyundaki anlık maliyet hesapları Unity'nin kendi verileri ve mantığına aittir. Bu klasördeki veri bir tasarım prototipidir, Unity kartlarını değiştirmez veya hesaplamayı kendiliğinden bağlamaz. Unity entegrasyonunda görselin, kategori ve metnin ayrı UI katmanları; temel bedelin ve dinamik hesabın mevcut oyun verilerinden okunması gerekir.

Ana liste `kart_listesi.json`. Kart başına `bilgi.json` ilk kez oluşturulurken listeden kopyalanır; sonraki düzenlemelerde korunur. Model üretimi `uretim.py` ile yürütülür. `seri_uret.ps1` listedeki kimlikleri sırayla işler; her kartı ve 3D kaynağını sonraki karta başlamadan kaydeder. Kamera ve ışık ilk iki kartla aynı yön ve yumuşaklıkta tutulur. Diğer kategorilerin simgeleri taslak yer tutuculardır.

Bağış Kampanyası için mevcut `docs/metinler.md` ile GDD ve oyun kodu arasında bağış miktarı farklıdır: metinler halk desteği kadar derken GDD ve kod halk desteğinin yarısını belirtir. Kart önizlemesinin ana açıklaması, istek üzerine oyunun mevcut metniyle aynıdır; fark metin sahibinin incelemesini bekliyor.
