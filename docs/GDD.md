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

### Minigame: Poşet sürüsü (zorunlu, kendi sahnesi)

Kıyıya yaklaşırken anne denizi boydan boya kesen bir akıntı perdesine kapılır; ekran kararır ve poşet sürüsü parkuru açılır. Anne kıyıya doğru bir akıntı koridorundan geçer; akıntı yandan denizanası ve poşet karışık bir sürü taşır. Üç aşama:

1. **Sakin su** (açık deniz): seyrek sürü, 5 denizanası.
2. **Yoğun su** (sığ sular): poşet çoğalır, sürü hızlanır, misinalar gerilidir; 6 denizanası.
3. **Karanlık su** (kumsalın önü): ışık azalır; denizanası hafifçe parlar, poşet görünmez olur; hayalet ağlar; 7 denizanası.

Her aşamanın sonunda bir akıntı duvarı vardır; aşamanın denizanası yenince açılır. Enerji sürekli azalır ve sadece denizanası doldurur. Art arda 3 denizanası kombo yapar (+12 enerji); poşet komboyu bozar ve yavaşlatır. Enerji biterse aşamanın başından devam edilir. Son duvardan sonra kıyıya ulaşınca parkur biter ve Bölüm 2 başlar.

**Bölüm sonu:** Su sığlaşır, kumsalın ışıkları görünür; poşet sürüsünü geçen anne kumsala çıkar ve Bölüm 2'nin ilk gecesi başlar.

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

### Minigame: Çöp seli (ikinci kaçış gecesi, kendi sahnesi)

İkinci kaçış gecesi fırtınadan sonradır ve bütünüyle çöp seli parkurunda oynanır: o gecenin yuvasından çıkan yavrular, selin çöple doldurduğu dar bir kumsal parçasında denize koşar. Deniz dalga dalga kumsala basar:

1. **Uyarı:** ufukta beyaz bir köpük çizgisi belirir, uğultu gelir (3 sn).
2. **Su basar:** su kumsala yürür, bir süre durur, sonra çekilir.
3. **Sakin:** kısa bir süre deniz durgundur.

Su gelirken alçak kum tepelerinin (Space ile çıkılır) üstündeki yavrular güvendedir; açıkta yakalanan yavru karaya itilir ve 2 sn'den uzun kalırsa sürüklenir (Su izi). Su çekilirken ya da durgunken denize giren yavru kurtulur, dalga gelirken giren geri itilir. Her dalga bir öncekinden daha uzağa ulaşır. Selin getirdiği şezlong, kaya ve çöp yığınları yolları daraltır, yüzen kasalar suyla gelip yavruları iter, ağlara takılan yavru öncüyle kurtarılır. 2,5 dakika sonra gün doğar; kumda kalanlar kaybedilir. Sonunda kurtulan yavru sayısı ve süre gösterilir.

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

### Minigame: Çöp girdabı (zorunlu, kendi sahnesi)

Sığ suların sonunda yavru dönen bir akıntıya kapılır; ekran kararır ve çöp girdabı parkuru açılır. Yavru girdabın merkezinde, parlayan plastik yığınının yanında başlar. Çevresinde iç içe üç çöp halkası ters yönlerde döner; her halkanın bir yerinde boşluk vardır (iki yanında parlak şamandıralar). Halkadan sadece boşluğundan geçilir.

- Girdap yavruyu döndürür ve hafifçe merkeze çeker; boşluğa yetişmek için akıntıya karşı yüzmek gerekir.
- Halkalardaki ağır çöpler (varil, kasa) dönerken çarparsa yavruyu savurur ve enerji alır.
- Enerji biterse merkezden baştan. Üçüncü halkadan çıkınca parkur biter; yavru açık denizden yolculuğuna devam eder.

**Final:** Yavru yosun adasına ulaşınca yosunun arasına saklanır ve kamera uzaklaşır. Kısa bir "30 yıl sonra" sahnesiyle oyun biter.

## Parkurlar (minigame'ler)

Bölüm 1, 4 ve 5'teki minigame'ler hikâyenin zorunlu birer adımıdır; ayrıca süreli birer parkurdur.

- Parkur başlayınca ekranın köşesinde süre ve rekor görünür; bitince süre ve "yeni rekor" gösterilir.
- Parkurlar hikâyede zorunludur ve her biri kendi sahnesinde oynanır: anne ya da yavru kapıya gelince ekran kararır, parkur açılır, bitince hikâye kaldığı yerden sürer.
- Hikâyede bitirilen parkur ana menüde **Parkurlar** altında açılır ve oradan tek başına, istendiği kadar oynanır (kayıt değişmez; bitince menüye dönülür).
- Rekorlar oyuncunun bilgisayarında saklanır.
- İki dünyanın bağı aynıdır: insan kısmındaki kararlar (Temizlik, Hayalet Ağ Temizliği) kumsalın ve denizin çöp yoğunluğunu belirler; parkurlardaki kayıplar da kanıt bırakır.

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
5. **Tur biter:** Simülasyon ilerler: geçen turun uyarısı gerçekleşir (sıcak dalgası, fırtına), çöp birikir (şezlong ve insan çoksa daha hızlı), korunmayan yuvalara avcı gelebilir. Haritada her yuvanın avcı riski görünür.

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
| Bütçe | 6 | Her tur +3; halk desteği 7 ve üstüyse +5. Otel dostsa +1, Belediye küsse −1. Bakımlı kartların bakımı gelirden düşer. Kartlara harcanır |
| Gönüllü | 4 | Kurtarılan yavrularla artar; Gönüllü Çağrısı ve bazı olaylar gelecek tur için ekler ya da meşgul eder |
| Halk desteği | 4 (0–10) | Bilgilendirme, kartlar ve olay seçimleriyle değişir. 3'ün altına düşerse otel ve belediye olaylarında olumlu seçenek kapanır |

### Paydaşlar

Kumsalın üç paydaşı var: **Otel**, **Belediye**, **Balıkçılar**. Her biriyle ilişki −3 ile +3 arasıdır, başta 0. Kartlar ve olay seçimleri ilişkiyi değiştirir; kumsalı korumak çoğu zaman birini kızdırır. +2 ve üstü **dost**, −2 ve altı **küs** sayılır.

| Paydaş | Dostsa | Küsse |
| --- | --- | --- |
| Otel | Her tur +1 bütçe (sponsorluk) | "Otel ışıkları" olayı havuza girer |
| Belediye | Politika kartları 1 ucuz | Her tur gelir −1; "Ödenek kesintisi" olayı havuza girer |
| Balıkçılar | Hayalet Ağ Temizliği iki kat temizler | "Denize atılan ağlar" olayı havuza girer |

Bazı kartlar bir paydaşla en az belli bir ilişki ister (ör. Koruma Alanı: Belediye +2); ilişki yetmezse kart elde görünür ama oynanamaz.

## İnsan kısmı: kartlar, kanıtlar, olaylar

Gönüllüler günlük işi yapar; kalıcı ve büyük değişiklikler kartlarla yapılır. Her tur havuzdan 5 kart gösterilir, oyuncu bütçesi yettiğince oynar. Oynanmayan kartlar havuza döner. Bir uyarı gelmişse, o uyarının Acil kartları bu 5 karta ek olarak ele gelir.

Kart oynamak üç şeyi tartmaktır: bütçe (bugün ve bakım olarak her tur), paydaş ilişkisi ve zamanlama (uyarı, gece, kanıt).

### Kart türleri

| Tür | Kural |
| --- | --- |
| Altyapı | Haritaya konur, kalıcıdır. Bazılarının her tur bakımı var |
| Kampanya | Anında ya da o gece etki eder; tekrar oynanabilir |
| Politika | Kalıcı karar, bir kez oynanır; çoğu bir paydaşı kızdırır ya da bir paydaşın desteğini ister |
| Acil | Sadece uyarı gelmişken ele gelir; uyarı tur sonunda gerçekleşmeden önce oynanır |
| Araştırma | Bilgi ve kanıt; başka kartları açar, ucuzlatır ve güçlendirir |

### Kartlar

| Kart | Tür | Bedel | Etki | Paydaş | Açılma |
| --- | --- | --- | --- | --- | --- |
| Gölgelik | Altyapı | 2 | Seçilen bölgede kum 1 bant serinler, kalıcı | — | Baştan (Isı kanıtı 1 ucuzlatır) |
| Kumul Onarımı | Altyapı | 3 | Seçilen bölgede dalga çizgisi geri çekilir | — | Baştan |
| Tilki Çiti | Altyapı | 3 | Kumsalın kenarına çit; tilki gelme ihtimali yarıya iner | — | Avcı kanıtı |
| Kaplumbağa Dostu Aydınlatma | Altyapı | 2 | Seçilen bölgede ışık 2 kademe azalır, kalıcı | — | Baştan (Işık kanıtı 1 ucuzlatır) |
| Gözcü Kulübesi | Altyapı | 3, bakım 1 | Seçilen bölgede her gece devriye var | — | Baştan |
| Kuluçkahane | Altyapı | 5 | Bütün yuvalar seçilen bölgeye taşınır ve kafeslenir; bir kez. Bütün yumurtalar tek yerde: o bölgenin sıcaklığı ve dalgası hepsini etkiler | Halk +1 | Baştan |
| Temizlik Kampanyası | Kampanya | 2 | En kirli 3 bölgenin çöpü 1 kademe azalır (ikinci Çöp kanıtıyla 4 bölge) | — | Baştan |
| Yuva Taşıma | Kampanya | 2 | Bir yuva güvenli ve serin bir bölgeye taşınır | — | Su kanıtı |
| Hayalet Ağ Temizliği | Kampanya | 3 | Bölüm 5'te denizdeki ağ ve misina azalır (Balıkçılar dostsa iki kat) | Balıkçılar +1 | Baştan |
| Gönüllü Çağrısı | Kampanya | 1 | Gelecek tur +2 gönüllü | — | Baştan |
| Plaj Tabelaları | Kampanya | 1 | Bu gece bütün bölgelerde insan 1 kademe azalır (ikinci Işık kanıtıyla 2) | — | Baştan |
| Bağış Kampanyası | Kampanya | 0 | Bütçe + halk desteğinin yarısı; halk desteği −1 | — | Baştan |
| Basın Bülteni | Kampanya | 1 | Halk desteği + açılmış kanıt türü sayısı (en az 1) | Otel −1 | Baştan |
| Belediye Lobisi | Kampanya | 2 | Belediyeyle görüşme | Belediye +1 | Baştan |
| Işık Yasağı | Politika | 3 | Otel ve yol ışıkları geceleri kapalı | Otel −2; Belediye 0 ister | Işık kanıtı |
| Araç Yasağı | Politika | 1 | Tekerlek izleri oluşmaz | Balıkçılar −1 | Çöp kanıtı |
| Şezlong Düzenlemesi | Politika | 2 | Şezlonglar her akşam toplanır; engeller yarıya iner | Otel −1 | Çöp kanıtı |
| Koruma Alanı | Politika | 4, bakım 1 | Seçilen bölgede şezlong, insan, ışık ve araç izi kalkar; otel ve festival oraya bir daha giremez | Otel −2; Belediye +2 ister | Baştan |
| Otel Ortaklığı | Politika | 2 | Otel ışıklarını kaplumbağa dostu yapar: bütün bölgelerde ışık 1 azalır; karşılığında bir bölgeye 1 şezlong sırası | Otel +2 | Baştan |
| Balıkçı Kooperatifi | Politika | 3 | Ağ değişimine ortak olunur; denizdeki ağ azalır | Balıkçılar +2 | Baştan |
| Temiz Enerji | Politika | 4 | Kısa vadede etki yok; sonraki sıcak dalgaları daha zayıf gelir ve final sahnesi değişir | Halk +1 | Baştan |
| Kum Torbası Seddi | Acil | 2 | Seçilen bölgede fırtına yuvaları basmaz (o gece de sürer) | — | Fırtına uyarısında |
| Yuva Tahliyesi | Acil | 3 | Dalga çizgisi altındaki bütün yuvalar serin ve güvenli yere taşınır | — | Fırtına uyarısında |
| Acil Sulama | Acil | 2 | Bütün yuvalar bu tur ve o gece 1 bant serinler (ikinci Isı kanıtıyla 2) | — | Sıcak dalgası uyarısında |
| Saha Araştırması | Araştırma | 1 | Henüz bulunmamış bir kanıtı açar | — | Baştan |
| Dron Taraması | Araştırma | 2 | Haritadaki açılmamış bütün kayıp izlerinin kanıtı açılır | — | Baştan |
| Üniversite Ortaklığı | Araştırma | 3 | Bundan sonra her tur 1 kart fazla gelir; bir kez | — | Baştan |

### Kanıtlar

Kanıtlar kaplumbağa bölümlerindeki kayıplardan doğar. Kayıp izine tıklanınca kanıt açılır; kanıt ilgili kartı havuza ekler ya da 1 ucuzlatır. Aynı türden ikinci kanıt, o kanıta bağlı kartı güçlendirir. Kanıt eksikliği oyunu kilitlemez; "Baştan" kartlar her zaman havuzdadır.

| Kanıt | Hangi kayıptan | Ne açar | İkinci kanıt |
| --- | --- | --- | --- |
| Isı | Sıcak kumda ya da kırmızı yuvada kayıp | Gölgelik 1 ucuzlar | Acil Sulama 2 bant |
| Işık | Işığa kapılıp yönünü kaybeden yavru | Işık Yasağı; Kaplumbağa Dostu Aydınlatma 1 ucuzlar | Plaj Tabelaları 2 kademe |
| Çöp | Ağa, halkaya, tekerlek izine takılan yavru | Araç Yasağı, Şezlong Düzenlemesi | Temizlik Kampanyası 4 bölge |
| Avcı | Tilkinin bozduğu yuva, tilkiye yakalanan sürü | Tilki Çiti | — |
| Su | Fırtınada basan yuva | Yuva Taşıma | — |

### Olaylar

Her tur bir olay gelir. Seçimler bütçeyi, halk desteğini ve paydaşları değiştirir; bazı seçimler sıradaki turun olayını belirler (zincir). Paydaş küsse onun tepki olayı havuza girer.

**Uyarılar:** Sıcak dalgası ve fırtına bir tur önceden haber verilir ve *sonraki turun sonunda* gerçekleşir. Arada bir tam tur hazırlık vardır: gönüllüler (gölge ve sulama, kafes), kartlar ve uyarının Acil kartları. Ekranın üstünde bekleyen uyarı yazar.

| Olay | Seçenek A | Seçenek B | Ne zaman |
| --- | --- | --- | --- |
| Otel yeni şezlong sırası istiyor | Kabul: bütçe +3, Otel +1, bir bölge şezlongla dolar | Red: Otel −1 | Her zaman (A halk desteği ister) |
| Otel kumsala bar açmak istiyor | Kabul: bütçe +4, Otel +1, bir bölgede ışık ve insan kalıcı +1 | Red: Otel −2 | Zincir: şezlongu kabul ettiysen |
| Belediye kumsalda festival yapmak istiyor | Kabul: halk desteği +2, Belediye +1, bir gece bir bölge ışık ve insanla dolar | Red: Belediye −1 | Her zaman (A halk desteği ister) |
| Festivalden sonra kumsal çöp dolu | Gönüllüler temizler: gelecek tur 2 gönüllü meşgul | Bırak: en kalabalık 3 bölgeye çöp +1 | Zincir: festivali kabul ettiysen |
| Balıkçılar ağ değişimi için destek istiyor | Destekle: bütçe −2, Balıkçılar +2, denizde ağ azalır | Reddet: Balıkçılar −1 | Her zaman |
| Balıkçılar denizden hayalet ağ getirdi | Sergile: halk desteği +2 | Geri dönüşüme sat: bütçe +2 | Zincir: balıkçıları desteklediysen |
| Sıcak dalgası uyarısı | Bilgi: gelecek turun sonunda bütün kum 1 bant ısınır | — | Her zaman |
| Fırtına uyarısı | Bilgi: gelecek turun sonunda dalga çizgisi altındaki yuvaların yarısı su alır | — | Her zaman |
| Okul ziyareti | Kabul: gelecek tur bir gönüllü meşgul, halk desteği +2 | Red: değişiklik yok | Her zaman |
| Gazeteci kayıp yuvaları soruyor | Açık konuş: halk desteği +2, Otel −1, Belediye −1 | Sessiz kal: Belediye +1, halk desteği −1 | Her zaman |
| Turistler kumsala köpek getiriyor | Yasak tabelası: bütçe −1, Otel −1 | İzin ver: bu tur yuvalara avcı gelme ihtimali iki kat | Her zaman |
| Bir bağışçı adının kumsala yazılmasını istiyor | Kabul: bütçe +5, Otel +1, halk desteği −1 | Red: değişiklik yok | Her zaman |
| Üniversite stajyer göndermek istiyor | Kabul: bütçe −1, gelecek tur +2 gönüllü | Red: değişiklik yok | 4. turdan sonra |
| Otel inadına bütün ışıklarını açtı | Şikâyet et: bütçe −2, Belediye +1 | Bekle: iki tur bütün bölgelerde ışık +1 | Otel küsken |
| Belediye ödeneği kesmek istiyor | Toplantıya git: gelecek tur bir gönüllü meşgul, Belediye +1 | Kabul et: bütçe −3 | Belediye küsken |
| Balıkçılar eski ağları denize attı | Temizlik ekibi: bütçe −2 | Bırak: denizdeki ağ artar, Balıkçılar +1 | Balıkçılar küsken |

## İki tarafın bağlantısı

Kaplumbağa kısmında kaybedilen her yavru insan kısmında bir kanıta, insan kısmında alınan her karar bir sonraki gecenin haritasına dönüşür. Kaplumbağa bölümleri başlarken kumsal, insan kısmındaki harita durumundan kurulur; ayrı bir "etki ekranı" yoktur, oyuncu farkı sahnede görür.

| İnsan kısmında | Kaplumbağa kısmında |
| --- | --- |
| Temizlik, Temizlik Kampanyası | Kaçış labirenti seyrek, ağ ve halka az |
| Araç Yasağı | Tekerlek izi yok |
| Şezlong Düzenlemesi | Yuva gecesinde ve kaçışta engeller yarı yarıya |
| Işık Yasağı, Kaplumbağa Dostu Aydınlatma, Otel Ortaklığı, gece devriyesi, Gözcü Kulübesi, Plaj Tabelaları | Işık çekimi zayıf ya da yok; anneyi ürküten insan az ya da yok |
| Yuva kafesi, Tilki Çiti, Kuluçkahane | Tilki o yuvaya giremez ya da hiç gelmez |
| Gölgelik, gölge ve sulama | Yuvanın kumu serin; kaçışta sıcak lekeler küçük |
| Kumul Onarımı, Yuva Taşıma, Kum Torbası Seddi, Yuva Tahliyesi | Fırtına yuvayı basmaz; yuva gecesinde kazılabilecek alan büyük |
| Hayalet Ağ Temizliği, Balıkçı Kooperatifi, balıkçılara destek | Bölüm 5'te denizde daha az ağ ve misina; denize atılan ağlar artırır |
| Otel şezlongunu ve barını kabul, festival, otelin ışıkları | Sonraki gecede daha çok engel, ışık ve insan |
| Koruma Alanı | O bölgede engel, ışık, insan ve araç izi yok |
| Temiz Enerji | Kısa vadede hiçbir şey; sonraki sıcak dalgaları zayıf, final sahnesi farklı |

### Ters yön: kaplumbağadan insana

| Kaplumbağa kısmında | İnsan kısmında |
| --- | --- |
| Kaybedilen yavrular | Haritada kayıp izi → kanıt → yeni ya da ucuz kart |
| Denize ulaşan yavrular | Her 10 yavru +1 gönüllü |
| Yuvanın yeri ve sıcaklığı | Haritada korunacak yuva; sıcak yuva sulama ister |
| Dalga çizgisi altındaki yuva | Fırtına uyarısında taşıma kararı |
