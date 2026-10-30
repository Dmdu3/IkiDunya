---
ozet: Oyunun tek doğru tasarım kaynağı. Değişiklik PR ile girer ve docs/kararlar.md'ye bir satır yazılır.
---

# İki Dünya, Tek Yaşam — Oynanış Mekanikleri

## Oyun akışı

Oyun tek bir anne kaplumbağayla başlar, onun yavrusuyla biter. Kaplumbağa bölümlerinin arasına insan turları girer; 3. bölüm tamamen insan kısmıdır. Toplam süre yaklaşık 2 saat.

1. **Dönüş** — anne, denizde (~20 dk)
2. **Yuva geceleri** — anne, kumsalda; 3 gece, aralarında insan turları (~25 dk)
3. **Bekleyiş** — insan; kuluçkanın 6 haftası (~25 dk)
4. **Kaçış** — yavrular, kumsalda; 3 gece, aralarında insan turları (~30 dk)
5. **İlk yüzüş** — tek yavru, denizde (~20 dk)

Bu belgedeki bütün rakamlar başlangıç değeridir; testle ayarlanır.

## Kaplumbağa kısmı: ortak temel

Bütün kaplumbağa bölümleri aynı kamerada, aynı tuşlarla ve aynı enerji çubuğuyla oynanır. Bölümleri tehditler ve hedef ayırır.

- **Kamera:** birinci şahıs, kaplumbağanın gözünden (karada ve denizde, anne ve yavru). İnsan kısmı yönetim ekranıdır.
- **Enerji:** 0–100. Hareket ve tehditler azaltır. Biterse son kontrol noktasından devam edilir; oyun bitmez.
- **Kontrol noktaları:** her bölümde 2–4 tane.
- **Uyarı kuralı:** her tehdit vurmadan önce görünür ya da duyulur bir işaret verir (titreyen kum, düşen gölge, yaklaşan pervane sesi). Oyuncu hiçbir tehditten habersiz yakalanmaz.

### Kontroller

| Tuş | Anne | Yavru |
| --- | --- | --- |
| WASD | Hareket | Hareket (sürüde öncü yavru) |
| Shift | Hızlı hareket, 3 kat enerji harcar | Kumsalda sürüyü dağıt, bırakınca toplan |
| Space | Denizde yüzeye çık / dal | Kumsalda atla, denizde dal |
| E (basılı tut) | Ye, ağdan kurtul | Takılan ya da ters dönen yavruyu kurtar |
| A / D sırayla | Kazma ve örtme | — |
| W (kum altında) | — | Bir kat yukarı kaz |
| T | Termal görünüm (kum sıcaklığı) | Termal görünüm |

## Bölüm 1 — Dönüş (anne, denizde)

**Hedef:** Açık denizden doğduğu kumsala ulaşmak. Harita üç bölgeden oluşur: açık deniz, sığ kıyı suları, kumsalın önü. Her bölgenin sonunda kontrol noktası vardır.

- **Akıntılar:** Haritada soluk oklarla görünür. Akıntıyla yüzmek hızlandırır, akıntıya karşı yüzmek 2 kat enerji harcar. Oyuncu rotayı akıntılara göre seçer.
- **Nefes:** 60 saniyelik nefes çubuğu. Biterse kaplumbağa yüzeye çıkana kadar yavaşlar. Yüzeye çıkmak (Space) çubuğu doldurur ama yüzeyde tekneler vardır.
- **Beslenme:** Denizanası +15 enerji verir. Poşet denizanasına benzer: yenirse enerji vermez ve kaplumbağa 10 saniye yarı hızda kalır. Ayırt etmek için yakından bakılır: denizanası kasılarak ilerler, poşet akıntıyla düz süzülür.

### Tehditler

| Tehdit | Uyarı | Etki | Kaçış |
| --- | --- | --- | --- |
| Hayalet ağ | Ağın kenarları hafif parlar | Takılır, durur | E'ye 8 kez bas; sonra 5 sn yavaş |
| Tekne | Pervane sesi, suyun üstünde gölge | Yüzeyde çarparsa −25 enerji | Tekne geçene kadar derinde kal |
| Sıcak su lekesi | Su titrer, renk koyulaşır | İçinde saniyede −3 enerji | Etrafından dolaş |

**Bölüm sonu:** Su sığlaşır, kumsalın ışıkları görünür. Anne kıyıya ulaşınca Bölüm 2'nin ilk gecesi başlar.

## Bölüm 2 — Yuva geceleri (anne, kumsalda)

**Hedef:** Üç ayrı gecede, enerji bitmeden güvenli bir yere yuva kazıp denize dönmek. Gerçekte de dişi caretta bir sezonda yaklaşık iki haftada bir, birkaç yuva kazar. Her gecenin arasında bir insan turu oynanır, bu yüzden üç gece birbirinden farklı bir kumsalda geçer.

- **Karada yavaş:** Enerji saniyede 1 azalır. Bir deneme en fazla yaklaşık 100 saniye sürer.
- **Termal görünüm (T):** Kum renk bantlarıyla görünür.

| Bant | Sonuç |
| --- | --- |
| Mavi (serin) | 12 yumurta, çoğu erkek |
| Yeşil (dengeli) | 12 yumurta, dengeli |
| Turuncu (sıcak) | 12 yumurta, çoğu dişi |
| Kırmızı (ölümcül) | 6 yumurta yaşar, hepsi dişi |

- **Dalga çizgisi:** Kıyıya paralel kesik çizgi. Altına kazılan yuvayı fırtına basabilir (insan kısmında fırtına gelirse yumurtaların yarısı gider).
- **Kazma:** Yerde A ve D'ye sırayla basılır. Doğru sıra çubuğu doldurur, aynı tuşa iki kez basmak kumu geri döktürür. 20 vuruş yuvayı açar, kısa yumurtlama animasyonu oynar, 10 vuruş yuvayı örter. Kazarken de enerji azalır.
- **Engeller:** Şezlong ve şemsiyelerin etrafından dolaşılır. Çöp yığınına takılan anne 2 saniye durur ve 5 enerji kaybeder.
- **İnsanlar:** Kumsalda gezen insanların önünde görüş konisi vardır. Anne bu koniye girerse ürker ve denize döner; deneme biter. Telefon feneri koniyi uzatır.
- **Deneme hakkı:** Her gece 2 deneme. İkisi de biterse anne kumsalın en yakın noktasına kendiliğinden yuva kazar; genelde kötü bir yer olur.

**Gecenin sonucu:** Yuvanın yeri, sıcaklık bandı, dalga çizgisine göre konumu ve yumurta sayısı kaydedilir. Bölüm 4'teki üç kaçış gecesi bu üç yuvadan çıkar. Yuvalar haritada insan kısmında da görünür.

## Bölüm 4 — Kaçış (yavrular, kumsalda)

**Hedef:** Her yuvanın yavrularını gün doğmadan denize sokmak. Bölüm, Bölüm 2'deki üç yuvanın sırasıyla çıktığı üç geceden oluşur. Her gecenin kumsalı, insan kısmında alınan kararlara göre kurulur. Oyunun aksiyon kalbi burasıdır.

### 1. Kumdan çıkış (~30 sn)

Yavrular kumun 5 kat altındadır. Ekranda kumun kesiti ve yüzey sıcaklığı göstergesi görünür. W ile bir kat yukarı kazılır. Yüzey sıcakken çıkan yavrular ölür; çok beklenirse enerji biter. Doğru an gecenin serinlediği andır.

### 2. Kumsal koşusu (~2,5 dk)

- **Sürü:** Oyuncu öncü yavruyu yönetir, diğerleri kısa bir gecikmeyle izler. Shift sürüyü dağıtır, bırakınca toplanırlar.
- **Gün doğumu sayacı:** 150 saniye. Azaldıkça sıcak kum lekeleri büyür; içinde kalan yavrunun enerjisi hızla düşer, biterse durur.

### Çöp

| Engel | Etki | Çözüm |
| --- | --- | --- |
| Şişe, kum kalesi, çukur | Labirent; sürü dar geçitte sıkışır | Rota seç, Space ile küçük engelden atla |
| Ağ parçası, plastik halka | Takılan yavru durur | Yanına git, E'yi 2 sn basılı tut |
| Tekerlek izi | Düşen yavru ters döner | Yanına git, E'yi 1,5 sn basılı tut |

Kurtarma sırasında sürü bekler ve sayaç işlemeye devam eder.

### Avcılar (doğal, kaçılan tehditler)

| Avcı | Uyarı | Etki | Kaçış |
| --- | --- | --- | --- |
| Yengeç | Deliğin kumu 2 sn titrer | 1 yavru | Delikten uzak geç ya da atla |
| Martı | Gölgesi 1,5 sn önce kuma düşer | Dağınık sürüden 1, toplu sürüden 3 yavru | Shift ile dağıl |
| Tilki | Bazı gecelerde kumsalın kenarında gezer, önünde görüş konisi | Görürse sürüye koşar, sürü panikle dağılır | Koninin dışından, kumul gölgesinden sessiz geç |

Dağınık sürü martıya karşı güvenli ama yengeçlere ve kayba açıktır; oyuncu sürekli toplanma ile dağılma arasında seçim yapar.

### Işık

Otel, yol lambası ve telefon feneri sürüyü yan rüzgâr gibi kendine çeker. Oyuncu karşı yöne bastırır. Ay ışığı denizin üstünde doğru yönü gösterir. Işığın içinde 5 saniyeden uzun kalan yavru yönünü kaybeder ve karaya doğru yürür; öncü yavru yanına gidip onu sürüye geri çağırabilir (E).

### Dalga

Son 5 metrede dalgalar gelip gider. Dalga çekilirken suya giren yavru kurtulur, dalga vururken giren geri savrulur.

**Gecenin sonucu:** Kurtulan yavru sayısı ve dişi-erkek oranı gösterilir. Kaybedilen her yavru, kaybedildiği yerde nedenini taşıyan bir iz bırakır: Isı, Işık, Çöp, Avcı, Su. İnsan kısmı bu izleri okur.

## Bölüm 5 — İlk yüzüş (yavru, denizde)

**Hedef:** Kıyı sularını geçip açık denizdeki yüzen yosun adasına ulaşmak. Bu bölümde tek bir yavru oynanır: son kaçış gecesinde denize ulaşan yavrulardan biri, hikâyenin yavrusu. Harita üç bölgedir: dalga hattı, sığ kıyı suları (kayalık ve deniz çayırı), açık deniz.

- **Çılgın yüzüş:** Yavrular denize girince ilk günlerde neredeyse hiç durmadan yüzer ve yumurtadan kalan besinle yaşar. Oyunda bu, sürekli azalan bir enerji çubuğudur (saniyede 0,5). Yemek yoktur; enerji sadece kontrol noktalarında dolar. Oyuncuyu ileri iten, durdurmayan bir baskıdır.
- **Dalgaları aşmak:** Dalga gelirken Space ile altından dalınır. Zamanlama kaçarsa yavru kıyıya geri savrulur.

### Tehditler

| Tehdit | Uyarı | Etki | Kaçış |
| --- | --- | --- | --- |
| Avcı balık | Gölgesi yaklaşır | Yakalarsa kontrol noktasına dönülür | Kayaların ya da deniz çayırının arasına saklan, gölge geçene kadar bekle |
| Misina | İnce parlak çizgi | Takılır, durur | E'ye 6 kez bas |
| Plastik parçaları | Parıldar | Yavruyu kendine çeker; değerse 5 sn yavaş | Karşı yöne bastır |
| Işıklı tekne | Suya vuran ışık | Kumsaldaki ışık gibi yavruyu çeker | Karşı yöne bastır, ışık alanından çık |
| Hayalet ağ | Kenarları hafif parlar | Takılır, durur | E'ye 10 kez bas |

**Final:** Yavru yosun adasına ulaşınca yosunun arasına saklanır ve kamera uzaklaşır. Kısa bir "30 yıl sonra" sahnesiyle oyun biter.

## İnsan kısmı: yönetim

Oyuncu kumsaldaki gönüllü ekibinin koordinatörüdür. İnsan kısmı turlarla oynanır; bir tur yaklaşık bir haftayı temsil eder ve 1–3 dakika sürer.

| Ne zaman | Tur sayısı |
| --- | --- |
| Bölüm 2: yuva geceleri arasında | 2 |
| Bölüm 3: Bekleyiş (kuluçka) | 6 |
| Bölüm 4: kaçış geceleri arasında | 2 |
| Bölüm 5'ten önce | 1 |
| **Toplam** | **11** |

### Kumsal haritası

Kumsal yukarıdan, bölgelere ayrılmış halde görünür (örneğin 8 bölge). Açılıp kapanan katmanlar:

- Yuvalar ve her yuvanın durumu (sıcaklık, korunma, çıkmaya kaç hafta kaldığı)
- Sıcaklık bantları
- Çöp yoğunluğu (bölge başına 0–3)
- Işık kaynakları (otel, lamba, yol)
- Avcı izleri (tilki, yengeç delikleri)
- Dalga çizgisi
- Gecelerden kalan kayıp izleri

### Bir turun akışı

1. **Rapor:** Geçen turun sonuçları. Hayvan bölümünden geliniyorsa kayıp izleri haritada simge olarak görünür; tıklanan iz bir kanıt açar.
2. **Gönüllüleri ata:** Her gönüllü haritada bir bölgeye ve bir göreve sürüklenir.
3. **Kart oyna:** Bütçe yettiği kadar kart oynanır.
4. **Olay:** Bir olay gelir, iki seçenekten biri seçilir.
5. **Tur biter:** Simülasyon ilerler: çöp birikir, sıcaklık hava durumuna göre değişir, korunmayan yuvalara avcı gelebilir.

### Gönüllü görevleri

Oyun 4 gönüllüyle başlar. Denize ulaşan her 10 yavru için 1 gönüllü daha katılır (en fazla 10).

| Görev | Etki | Süre |
| --- | --- | --- |
| Temizlik | Bölgenin çöpü 1 kademe azalır | Kalıcı (çöp yeniden birikir) |
| Gece devriyesi | Bölgede insan ve telefon feneri olmaz | O tur ve sıradaki gece |
| Yuva kafesi | Yuvaya tilki giremez | Kalıcı |
| Gölge ve sulama | Yuvanın sıcaklığı 1 bant düşer | O tur |
| Bilgilendirme | Halk desteği +1 | Kalıcı |

### Kaynaklar

| Kaynak | Başlangıç | Nasıl değişir |
| --- | --- | --- |
| Bütçe | 6 | Her tur +3; halk desteği 7 ve üstüyse +5. Kartlara harcanır |
| Gönüllü | 4 | Kurtarılan yavrularla artar |
| Halk desteği | 4 (0–10) | Bilgilendirme ve olay seçimleriyle değişir. 3'ün altına düşerse otel ve belediye olaylarında olumlu seçenek kapanır |

## İnsan kısmı: kartlar, kanıtlar, olaylar

Gönüllüler günlük işi yapar; kalıcı ve büyük değişiklikler kartlarla yapılır. Her tur havuzdan 4 kart gösterilir, oyuncu bütçesi yettiğince oynar. Oynanmayan kartlar havuza döner.

### Kartlar

| Kart | Bedel | Etki | Açılma |
| --- | --- | --- | --- |
| Gölgelik | 2 | Seçilen bölgede kum 1 bant serinler, kalıcı | Baştan |
| Temizlik Kampanyası | 2 | Bir turda 3 bölgenin çöpü 1 kademe azalır | Baştan |
| Kumul Onarımı | 3 | Dalga çizgisi 4 m geri çekilir | Baştan |
| Yuva Taşıma | 2 | Bir yuva güvenli ve serin bir bölgeye taşınır | Su kanıtı |
| Işık Yasağı | 3 | Otel ve yol ışıkları geceleri kapalı, kalıcı | Işık kanıtı |
| Araç Yasağı | 1 | Tekerlek izleri oluşmaz | Çöp kanıtı |
| Şezlong Düzenlemesi | 2 | Şezlonglar her akşam toplanır; engeller yarıya iner | Çöp kanıtı |
| Tilki Çiti | 3 | Kumsalın kenarına çit; tilki gelme ihtimali yarıya iner | Avcı kanıtı |
| Hayalet Ağ Temizliği | 3 | Bölüm 5'te denizdeki ağ ve misina azalır | Baştan |
| Gönüllü Çağrısı | 1 | O tur +2 gönüllü | Baştan |
| Saha Araştırması | 1 | Henüz bulunmamış bir kanıtı açar | Baştan |
| Temiz Enerji | 4 | Kısa vadede etki yok; sonraki sıcak dalgaları daha zayıf gelir ve final sahnesi değişir | Baştan |

### Kanıtlar

Kanıtlar kaplumbağa bölümlerindeki kayıplardan doğar. Kayıp izine tıklanınca kanıt açılır; kanıt ilgili kartı havuza ekler ya da 1 ucuzlatır. Kanıt eksikliği oyunu kilitlemez; "Baştan" kartlar her zaman havuzdadır.

| Kanıt | Hangi kayıptan | Ne açar |
| --- | --- | --- |
| Isı | Sıcak kumda ya da kırmızı yuvada kayıp | Gölgelik 1 ucuzlar |
| Işık | Işığa kapılıp yönünü kaybeden yavru | Işık Yasağı |
| Çöp | Ağa, halkaya, tekerlek izine takılan yavru | Araç Yasağı, Şezlong Düzenlemesi |
| Avcı | Tilkinin bozduğu yuva, tilkiye yakalanan sürü | Tilki Çiti |
| Su | Fırtınada basan yuva | Yuva Taşıma |

### Olaylar

Her tur bir olay gelir. Bazılarının bir tur önceden uyarısı vardır.

| Olay | Seçenek A | Seçenek B |
| --- | --- | --- |
| Otel yeni şezlong sırası istiyor | Kabul: bütçe +3, bir bölge engelle dolar | Red: bütçe değişmez, halk desteği −1 |
| Belediye kumsalda festival yapmak istiyor | Kabul: halk desteği +2, bir gece kumsal ışık ve insanla dolar | Red: halk desteği −1 |
| Balıkçılar ağ değişimi için destek istiyor | Destekle: bütçe −2, denizde ağ azalır | Reddet: değişiklik yok |
| Sıcak dalgası uyarısı | Bilgi: gelecek tur bütün kum 1 bant ısınacak | — |
| Fırtına uyarısı | Bilgi: gelecek tur dalga çizgisi altındaki yuvaların yarısı su alacak | — |
| Okul ziyareti | Kabul: bir gönüllü bir tur meşgul, halk desteği +2 | Red: değişiklik yok |

## İki tarafın bağlantısı

Kaplumbağa kısmında kaybedilen her yavru insan kısmında bir kanıta, insan kısmında alınan her karar bir sonraki gecenin haritasına dönüşür. Kaplumbağa bölümleri başlarken kumsal, insan kısmındaki harita durumundan kurulur; ayrı bir "etki ekranı" yoktur, oyuncu farkı sahnede görür.

| İnsan kısmında | Kaplumbağa kısmında |
| --- | --- |
| Temizlik, Temizlik Kampanyası | Kaçış labirenti seyrek, ağ ve halka az |
| Araç Yasağı | Tekerlek izi yok |
| Şezlong Düzenlemesi | Yuva gecesinde ve kaçışta engeller yarı yarıya |
| Işık Yasağı, gece devriyesi | Işık çekimi zayıf ya da yok; anneyi ürküten insan yok |
| Yuva kafesi, Tilki Çiti | Tilki o yuvaya giremez ya da hiç gelmez |
| Gölgelik, gölge ve sulama | Yuvanın kumu serin; kaçışta sıcak lekeler küçük |
| Kumul Onarımı, Yuva Taşıma | Fırtına yuvayı basmaz; yuva gecesinde kazılabilecek alan büyük |
| Hayalet Ağ Temizliği, balıkçılara destek | Bölüm 5'te denizde daha az ağ ve misina |
| Otel şezlongunu kabul, festival | Sonraki gecede daha çok engel, ışık ve insan |
| Temiz Enerji | Kısa vadede hiçbir şey; sonraki sıcak dalgaları zayıf, final sahnesi farklı |

### Ters yön: kaplumbağadan insana

| Kaplumbağa kısmında | İnsan kısmında |
| --- | --- |
| Kaybedilen yavrular | Haritada kayıp izi → kanıt → yeni ya da ucuz kart |
| Denize ulaşan yavrular | Her 10 yavru +1 gönüllü |
| Yuvanın yeri ve sıcaklığı | Haritada korunacak yuva; sıcak yuva sulama ister |
| Dalga çizgisi altındaki yuva | Fırtına uyarısında taşıma kararı |
