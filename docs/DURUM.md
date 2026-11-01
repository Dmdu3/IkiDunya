---
ozet: Mekanik tarafın durumu ve sistemlerin haritası. Bir sistem eklendikçe ya da değiştikçe güncellenir. Sahibi Umut; Toprak kendi sistemlerini işler.
---

# Durum

Son güncelleme: 2026-11-01 — Dalda (`umut/cila`): Hepsini kur, PC build, F6 paneli, geri bildirim efektleri, kamera hissi, gece gökyüzü ve izler, ara sahneler, final jeneriği, yeni ayarlar, oyun kolu, performans, Kendini dene. Alpha main'de, deneme listesi tamam (editörde 300+ FPS). Main'de: insan kısmı (yönetim ekranı, Faz 0–4). Main'de: Yusuf'un kumsalı (#40). Main'de: minigame sahneleri ve Yusuf'un canlıları (#43, #45). Dalda (`umut/metin-animasyon-kamera`): bütün metinlerin ilk taslağı, kodla animasyon, birinci şahısta kendi gövdesi gizli. Unity'de henüz denenmedi.

## Sistem haritası

| Sistem | Klasör | Ana script(ler) | Sahibi | Durum |
| --- | --- | --- | --- | --- |
| Oyun akışı ve bölüm geçişi | `Core` | `OyunAkisi`, `OyunAyarlari` | Umut | Yapılıyor |
| Kumsal durumu (ortak durum) | `Core` | `KumsalDurumu` | Umut | Yapılıyor (taslak) |
| Tuşlar | `Core` | `GameInput` | Umut | Yapılıyor |
| Kayıt ve devam etme | `Core` | `Kayit` | Umut | Yapılıyor |
| Geçiş ve bölüm başlığı | `Core` | `Gecis` (OyunAkisi kurar) | Umut | Yapılıyor |
| Duraklatma ve oyuncu ayarları | `Core` | `Duraklatma`, `OyunTercihleri` | Umut | Yapılıyor |
| Oyuncu metinleri | `Core` | `MetinTablosu` (docs/metinler.md'den) | Umut | Yapılıyor |
| Ana menü, ayarlar, duraklatma menüsü | `UI/Menu` | `AnaMenu`, `AyarlarPaneli`, `DuraklatmaMenusu`, `MetinYazisi` | Umut | Yapılıyor |
| Final: "30 yıl sonra" | `Core` | `FinalSahnesi` | Umut | Yapılıyor |
| Kameralar (hayvan kısmı birinci şahıs, insan kısmı yönetim ekranı) | `Camera` | `BirinciSahisKamera`, `KameraAyarlari` | Umut | Yapılıyor |
| Anne (kara): hareket, enerji, kazma | `Turtle` | `AnneKara`, `AnneKazma`, `AnneAyarlari`, `Enerji` | Umut | Yapılıyor |
| Yuva gecesi: denemeler, yuva sonucu | `Turtle` | `YuvaGecesi` | Umut | Yapılıyor |
| Kaçış gecesi: kumdan çıkış, koşu, kurtarma | `Turtle` | `KacisGecesi`, `Suru`, `Yavru`, `YavruAyarlari` | Umut | Yapılıyor |
| Adıma göre sahne parçaları | `Core` | `AdimNesneleri` | Umut | Yapılıyor |
| Gece göstergeleri (enerji, kazma, kumdan çıkış, gün doğumu, kurtarma) ve sonuç paneli | `UI/Night` | `EnerjiCubugu`, `Cubuk`, `KazmaGostergesi`, `KacisGostergesi`, `GeceSonucuPaneli` | Umut | Yapılıyor (yazısız) |
| Gri kutu kumsal ve layer kurulumu | `Editor` | `GriKumsalKur`, `KatmanKurulumu` | Umut | Yapılıyor |
| Deniz hareketi, nefes, besin, ağdan kurtulma | `Turtle` | `DenizHareketi`, `DenizAyarlari` (anne, yavru) | Umut | Yapılıyor |
| Deniz bölümü: kontrol noktaları, final | `Turtle` | `DenizBolumu`, `KontrolNoktasi` | Umut | Yapılıyor |
| Deniz tehlikeleri | `Hazards` | `Akinti`, `Yiyecek`, `Tekne`, `AvciBalik`, `DenizDalgasi`, `DenizYapilari`, `HazardZone` (sıcak su), `DenizTehlikeAyarlari`; `Core/IYuzucu` | Umut | Yapılıyor |
| Deniz göstergeleri | `UI/Night` | `DenizGostergesi` | Umut | Yapılıyor |
| Kum sıcaklığı ve termal görünüm (T) | `Hazards` | `KumSicakligi`, `TermalGorunum`, `TermalAyarlari` | Umut | Yapılıyor |
| Çöp yığını (anne takılır) | `Hazards` | `TrashTrap` | Umut | Yapılıyor |
| İnsanlar ve görüş konisi | `Hazards` | `Insan`, `InsanAyarlari` | Umut | Yapılıyor |
| Sıcak kum lekesi, gün doğumu, dalga | `Hazards` | `HazardZone`, `GunDogumu`, `Dalga`, `KacisAyarlari` | Umut | Yapılıyor |
| Kumsalı durumdan kurma (çöp, halka, şezlong, şemsiye, ışık, insan, araç izi, gölgelik, çit, gözcü kulübesi, kum torbası seddi, plaj tabelası, kayıp izleri) | `Hazards` | `KumsalYapilari` | Umut | Yapılıyor |
| Kumsalın ilk hali | `Core` | `KumsalBaslangici`, `DenemeKumsali` | Umut | Yapılıyor |
| Kaçışta çöp (ağ, halka, tekerlek izi) | `Hazards` | `TrashTrap` | Umut | Yapılıyor |
| Avcılar | `Hazards` | `Yengec`, `Marti`, `Tilki`, `AvciAyarlari`, `Cover`; `Core/IAvlanabilir` | Umut | Yapılıyor |
| Işık kaynakları | `Hazards` | `IsikKaynagi` | Umut | Yapılıyor |
| Kayıp izleri | `Turtle`, `Hazards` | `KacisGecesi` yazar, `KumsalYapilari` kumda gösterir, `KayipIziHaritasi` insan turunda simge + kanıt | Umut | Yapılıyor |
| İpuçları (ilk karşılaşmada bir kez) | `Core`, `UI/Night` | `Ipucu`, `IpucuPaneli` | Umut | Yapılıyor (yazılar Yusuf'ta) |
| Ses: olaylar, ortam sesi, müzik | `Core` | `Ses`, `SesYoneticisi`, `SesAyarlari` | Umut | Yapılıyor (ilk klipler bağlı, kalanlar Yusuf'ta) |
| Kumsal haritası ve tur akışı | `Management`, `UI/Management` | `InsanTuru`, `HaritaSecici`, `YuvaHaritasi`, `YonetimArayuzu` | Toprak | Yapılıyor |
| Yönetim ekranı görünümü | `UI/Management`, `Art/UI/Resources/KartDioramalari` | `YonetimArayuzu` | Toprak / Abdü | Kart ve gönüllü dioramaları, kompakt paneller ve hedef seçerken gizlenen alt panel eklendi. "Belgesel" temasında: koyu paneller, serif başlıklar, kum vurgusu. Yerleşim tuval biriminde (yazı boyutuyla büyür, 1.5'te çakışmasız denendi); hedef seçerken istem ve Geri alttaki şeritte; kart bilgisi sağda kart panelinin üstünde. |
| Kaynaklar (bütçe, gönüllü, halk desteği, bakım), rapor, kayıt | `Management` | `YonetimDurumu`, `YonetimAyarlari`; `Core/IOyunVerisi` | Toprak | Yapılıyor |
| Paydaşlar (Otel, Belediye, Balıkçılar) | `Management` | `Paydas`, `YonetimDurumu`, `YonetimAyarlari` | Toprak | Yapılıyor (Unity'de bir turda denendi, 2026-11-01) |
| Gönüllüler ve görevler | `Management` | `GorevTanimi`, `Etkiler` | Toprak | Yapılıyor |
| Kartlar (5 tür, 27 kart; Acil kartlar uyarıyla ele gelir) | `Management` | `KartTanimi`, `Etkiler`, `InsanTuru` | Toprak | Yapılıyor (Unity'de bir turda denendi: hedefsiz, bölge hedefli, vazgeçme, koşullu, 2026-11-01) |
| Kanıtlar (ikinci kanıt güçlendirir), olaylar (16, zincir, paydaş tepkisi), uyarılar, tur sonu simülasyonu, avcı riski | `Management` | `OlayTanimi`, `Etkiler` | Toprak | Yapılıyor (bir olay ve tur sonu denendi; zincir, uyarı, güçlenme denenmedi) |
| İnsan kısmı içeriği (kart, görev, olay asset'leri) | `Management/Editor` | `InsanIcerigiKur` (İki Dünya → İnsan kısmı içeriğini kur) | Toprak | Yapılıyor |

Durum değerleri: Planlandı · Yapılıyor · Oynanabilir · Bitti.

## `KumsalDurumu` sözleşmesi

> Tam sözleşme, kart/görev/olay → metot eşlemesiyle: [`docs/kumsal-durumu.md`](kumsal-durumu.md). Aşağıdaki tablo özet.

**Taslak (Umut, Toprak'ın onayı bekleniyor).** `KumsalDurumu` bir ScriptableObject asset'i; kullanan her script ona `[SerializeField]` ile bağlanır. Yeni oyunda `OyunAkisi` sıfırlar. Her değişiklikten sonra `Degisti` event'i gelir.

| Ne | Okuma | Değiştirme (açık metot) | Kim çağırır |
| --- | --- | --- | --- |
| Bölgeler | `BolgeSayisi`, `BolgeAl(i).Seviye(katman)`, `BolgeAl(i).Var(yapi)` | `KatmanDegistir(bolge, katman, fark)`, `KatmanSifirla`, `YapiKoy`, `YapiKaldir` | İnsan kısmı (kartlar, olaylar, simülasyon) |
| Katmanlar (0–`katmanEnFazla`) | `Cop`, `Isik`, `Sezlong`, `Insan` | | |
| Yapılar (var/yok) | `Golgelik`, `KumulOnarimi`, `TilkiCiti`, `AracIzi` | | |
| Denizdeki ağ | `DenizdekiAg` | `DenizdekiAgDegistir(fark)` | İnsan kısmı |
| Işık yasağı (otel ve yol ışıkları kapalı) | `IsikYasagi` | `IsikYasagiAyarla(var)` | İnsan kısmı (Işık Yasağı kartı) |
| Temiz Enerji oynandı mı (final değişir) | `TemizEnerji` | `TemizEnerjiAyarla(var)` | İnsan kısmı (Temiz Enerji kartı) |
| Kum ısınması (bütün kum kaç bant ısındı) | `KumIsinmasi` | `KumIsinmasiDegistir(fark)` | İnsan kısmı (sıcak dalgası olayı) |
| Yuvalar | `Yuvalar` (bölge, konum, bant, dalga altında mı, yumurta, çıkışa kalan hafta, kafesli) | `YuvaEkle` (yuva gecesi), `YuvaKafesle`, `YuvaTasi`, `YuvaYumurtaKaybet`, `BirHaftaGecti` | Ekleme kaplumbağa, gerisi insan kısmı |
| Kayıp izleri | `KayipIzleri` (bölge, konum, neden, kanıt açıldı mı) | `KayipIziEkle` (gece), `KanitAc` (insan turu) | İkisi |
| Denize ulaşan yavru | `DenizeUlasanYavru` | `DenizeUlasti(adet)` | Kaplumbağa |

Bütçe, gönüllü ve halk desteği `KumsalDurumu`'nda değil; insan kısmının kendi verisi (`YonetimDurumu`, kayda `IOyunVerisi` olarak girer).

## Araçlar

- **İki Dünya → Hepsini kur:** sorusuz, sırayla Menü, Kumsal, Bölüm 1, Bölüm 5 ve üç parkur sahnesini kurar, Build Settings'teki sahne listesini düzeltir, Menü'yü açar. Model ya da metin değişince tek tık.
- **İki Dünya → PC build al:** `Build/Windows/IkiDunya.exe` (Windows 64-bit). `Build/` repoya girmez.
- **İki Dünya → Kendini dene:** oyunu açmadan kontroller (akış sırası, süreli etkilerin geri alınması, kayıp izi → kanıt, metinler.md'de tekrar eden id, kodda olup metinler.md'de olmayan id). Sonuç Console'da.
- **F6 (oyunda, geliştirici paneli):** akıştaki herhangi bir adıma ya da parkura atlar, menüye döner. Sadece editörde ve development build'de.
- **İki Dünya → Layer'ları kur:** README'deki layer'ları boş yuvalara ekler.
- **İki Dünya → Gri kutu kumsalı kur (yuva gecesi):** `Scenes/Prototypes/GriKumsal.unity` — kumsal, termal görünüm, şezlong, çöp yığını, 2 insan (biri fenerli), anne (hareket, kazma), yuva gecesi, enerji ve kazma çubuğu.
- **İki Dünya → Kumsal sahnesini kur (oyun):** `Scenes/Levels/Kumsal.unity` — iki gecenin hepsi ve geçici insan turu; her grup (`YuvaGecesiNesneleri`, `KacisGecesiNesneleri`, `InsanTuruNesneleri`) sadece kendi adımında açılır. `Sistemler` prefab'ı da konur.
- **İki Dünya → Bölüm 1 sahnesini kur (deniz)** ve **Bölüm 5 sahnesini kur (deniz):** `Scenes/Levels/Bolum1_Donus` ve `Bolum5_IlkYuzus` sahnelerini gri kutu olarak baştan kurar (deniz, taban basamakları, kontrol noktaları, tehlikeler, göstergeler, `Sistemler`).
- **İki Dünya → Menü ve duraklatmayı kur:** `Menu` sahnesini (başlık, Yeni oyun, Devam et, Ayarlar, Çıkış) kurar; `Sistemler` kalıbına EventSystem ve duraklatma menüsü ekler, deneme panelini (`AkisDeneme`) çıkarır; önce metinleri aktarır.
- **İki Dünya → İnsan kısmı içeriğini kur:** 27 kart, 5 gönüllü görevi ve 16 olay asset'ini GDD'deki başlangıç değerleriyle yazar (varsa üstüne; asset'lerde yapılan dengeyi siler) ve `YonetimAyarlari`'na bağlar; elde 5 kart. Toprak'ın aracı (`Management/Editor/InsanIcerigiKur`).
- **İki Dünya → Metinleri aktar:** `docs/metinler.md`'deki tablolardan `MetinTablosu` asset'ini doldurur. Metinler.md değişince yeniden çalıştırılır. Eksik metin oyunda `[id]` olarak görünür.
- **İki Dünya → Gri kutu kaçışı kur:** `Scenes/Prototypes/GriKacis.unity` — kumsal, sıcak kum lekeleri, dalga, gün doğumu, sürü, kaçış gecesi, kumdan çıkış paneli ve gün doğumu çubuğu.
- İki araç da eksik ayar asset'lerini `ScriptableObjects/Settings`'te, `Yavru`, `Golgelik`, `KayipIzi` kalıplarını `Prefabs/` altında oluşturur. Deneme sahnelerinde akış yoktur; gece kendiliğinden başlar.

## Kum sıcaklığı

`KumSicakligi.Bant(konum)` bir noktanın bandını verir: denizden uzaklığa göre Mavi → Yeşil → Turuncu → Kırmızı (sınırlar `TermalAyarlari`'nda: 8, 18, 30 m), gölgelikli bölgede 1 bant serin, `KumsalDurumu.KumIsinmasi` kadar sıcak. Termal görünüm (T) her bölge ve bant için kumun üstüne renkli şerit serer; kumsal değişince yeniden renklenir. Yuvaya `KumSicakligi.ZeminBandi` (ısınma hariç) yazılır; güncel bandı `KumsalDurumu.YuvaBandi(i)` verir (ısınma dahil), kaçış gecesi ve harita bunu okur.

> Bantların denizden uzaklığa göre dizilmesi bir başlangıç varsayımı (serin bant dalga çizgisine yakın: serin ama fırtına riski). GDD yerleşimi söylemiyor; denemede değişebilir.

## Kumsal geceleri

**Yuva gecesi (`YuvaGecesi`).** Anne denizden çıkar; W/S ile yürür, fareyle döner. Yerde dururken A ve D'ye sırayla basınca kazma başlar (20 vuruş), kısa yumurtlama, sonra örtme (10 vuruş). Yuvanın bölgesi, bandı, dalga çizgisine göre yeri ve yumurta sayısı (kırmızı bantta 6) `KumsalDurumu.YuvaEkle` ile yazılır; anne denize dönünce gece biter. İnsanın görüş konisine girmek ya da enerjinin bitmesi denemeyi bitirir; 2 deneme de biterse anne bulunduğu bölgede kıyıya yakın kendiliğinden yuva kazar. Çöp yığını anneyi 2 sn durdurur, 5 enerji alır.

**Kaçış gecesi (`KacisGecesi`).** Gecenin yuvası Bölüm 2'deki aynı sıradaki yuvadır (yoksa deneme yuvası). Kumdan çıkış: W ile 5 kat; yüzey zamanla serinler, ölümcül sıcaklığın üstünde çıkılırsa yavruların o oranı ölür; kum altında enerji biterse çıkış baştan. Koşu: öncüyü oyuncu sürer, diğerleri izler, Shift dağıtır. Sıcak lekeler gün doğdukça büyür (gölgelikli bölgede yarı boy), içindeki yavru enerji kaybeder. Dalga son 5 m'de gelip gider: çekilirken suya giren kurtulur, vururken giren geri savrulur. Öncü kurtulur ya da ölürse sıradaki yavru öncü olur. Gün doğunca kumda kalanlar kaybedilir. Her kayıp `KayipIziEkle`, her kurtulan `DenizeUlasti(1)`.

**Kumsalı durumdan kurma (`KumsalYapilari`).** Her bölgede: çöp seviyesi kadar çöp yığını (anneyi takar), ağ (yavruyu takar) ve şişe (labirent); şezlong seviyesi kadar şezlong; ışık seviyesi kadar otel ışığı (kumulun önünde); insan seviyesi kadar gezen insan (sadece yuva gecesinde); araç izi varsa tekerlek izi; gölgelik varsa gölgelik. Yerleşim bölge başına sabit tohumla her seferinde aynı. Yapılar adım başında ve insan turunda kurulur, gece ortasında kurulmaz (içindeki yavru yeniden takılmasın diye); kayıp izleri her değişiklikte güncellenir. Yeni oyunda kumsal `KumsalBaslangici` asset'inden kurulur (`OyunAyarlari`'na bağlı); deneme sahnelerinde bunu `DenemeKumsali` yapar. Ay ışığı denizin üstünde soluk bir yol olarak doğru yönü gösterir. Kumul onarımı olan bölgede dalga çizgisi 4 m denize çekilir (`KumSicakligi.DalgaCizgisi(bolge)`); kumdaki çizgi de o bölgede kayar (`DalgaCizgisi`).

**Kaçışta çöp, avcılar, ışık.** Ağ ve halka izleyen yavruyu takar, tekerlek izi ters çevirir; öncü yanına gidip E'yi basılı tutar (2 / 1,5 sn), bu sırada sürü bekler, sayaç işler. Space bütün sürüyü zıplatır (kum kalesi atlanır, şişe dolaşılır). Öncü de takılır ve ters döner; E'yi basılı tutup kendini kurtarır (bu sırada sürü bekler), ama yönünü kaybetmez. Yengeç deliği 2 sn titrer, sonra yanındaki bir yavruyu alır (zıplayan kurtulur). Martının gölgesi 1,5 sn önce düşer; dalış alanında 3+ yavru varsa 3, yoksa 1 yavru alır. Tilki her gece ihtimalle gelir (çit varsa yarı ihtimal), kumsalın kenarında gezer, konisinde siperde olmayan bir yavru görürse koşar, sürü 3 sn panikle dağılır. Geldiği gece 25 sn gezdikten sonra sonraki gecelerin kafessiz bir yuvasına gidip 4 sn eşeler: yumurtaların yarısı gider, yuvada Avcı izi kalır (kafesli yuvaya dokunmaz). Işık alanındaki yavru ışığa çekilir; 5 sn kalırsa yönünü kaybedip karaya yürür, öncü E ile geri çağırır. Gün doğunca kumda kalanlar o an başlarına gelen nedenle kaybedilir (takılı → Ağ, ters → Çöp, yön kaybı → Işık, diğer → Isı).

**Gecenin sonu.** Yuva gecesi yuvanın bandını (renk ve adı) ve yumurta sayısını, kaçış kurtulan, kaybedilen, dişi ve erkek sayısını gösterir (dişi oranı yuvanın bandından). Başlık, satır adları ve "devam" satırı metin id'leriyle (`sonuc_*`, `bant_*`). Tıklayınca ya da Space'le akış ilerler.

**İnsan turu.** Geçici tur kalktı; yerine yönetim ekranı geldi (aşağıda "İnsan kısmı").

Henüz yok: yok (animasyonlar modeller gelince).

## Deniz bölümleri

**Yüzme (`DenizHareketi`).** Birinci şahıs: W/S/A/D baktığın yöne (yukarı-aşağı dahil), Shift hızlı (3 kat enerji). Space: yüzeydeyken kısa dalış; anne su altında Space basılı tutunca yüzeye çıkar, yavrunun Space'i sadece daldırır. Akıntı sürükler, akıntıya karşı yüzmek 2 kat enerji. Anne: 60 sn nefes, biterse yavaşlar, yüzeyde dolar. Yavru: nefes yok, enerji sürekli azalır (çılgın yüzüş), sadece kontrol noktalarında dolar. E: önündeki besini yer (denizanası +15 enerji, poşet 10 sn yarı hız); ağa ya da misinaya takılınca E'ye art arda basılır (anne ağ 8, yavru ağ 10, misina 6), sonra 5 sn yavaş. Parlayan plastik ve ışıklı tekne çeker, plastiğe değmek 5 sn yavaşlatır. Sıcak su lekesi saniyede 3 enerji alır. Tekne yüzeydekine çarpar (−25). Avcı balık gizli olmayanı kovalar; deniz çayırı ve kaya arası (`Cover`) korur.

**Bölüm (`DenizBolumu`).** Kontrol noktasına girince enerji dolar ve oradan devam edilir; enerji biterse ya da avcı yakalarsa son noktaya dönülür. Bölüm 1 kıyıda biter; Bölüm 5 yosun adasında, kamera uzaklaşarak biter. Bölüm 5'in dalga hattında dalgalar ara ara kıyıya gelir; yüzeye yakın yakalanan yavru savrulur, altından dalan geçer. Bölüm 5'teki ağ ve misina sayısı `KumsalDurumu.DenizdekiAg` seviyesinden (Hayalet Ağ Temizliği azaltır).

**Uyarılar.** Teknenin suyun hemen altına düşen gölgesi teknenin 6 m önünde gider, arkasında pervane köpüğü titrer; su altındaki oyuncu tekneyi gelmeden görür. Hayalet ağın kenarları ve misina yavaşça parıldar (`Parilti`), uzaktan seçilir.

Pervane sesi klibi bağlı (`Audio/SFX/Pervane.wav`).

## Menü, kayıt, final (Faz 7)

**Akış.** Adımlar arasında ekran kararır; yeni bölüme girilince karanlıkta `bolumN_giris` metni görünür (Bölüm 3 dahil). Her adımın başında kayıt yazılır (`Kayit`: adım sırası + `KumsalDurumu`'nun JSON'u, `persistentDataPath/kayit.json`). Ana menüde "Devam et" kayıtlı adımdan sürer; "Yeni oyun" kaydı siler. Son adım (final) bitince kayıt silinir, menüye dönülür.

**Duraklatma.** Esc oyun sırasında duraklatma menüsünü açar (zaman durur, imleç serbest). Duraklatma açıkken ve kapandığı karede `GameInput` oyun girdilerini vermez, böylece menüye tıklamak oyuna geçmez. Ayarlar (ses, fare hassasiyeti) bu bilgisayarda `PlayerPrefs`'te.

**Final.** Bölüm 5'ten sonra `Final` adımı (Kumsal sahnesi, `FinalNesneleri`): büyümüş dişi kaplumbağa denizden kumsala yürür; Temiz Enerji oynandıysa güneş serin renkte, oynanmadıysa kızgın ve kumda sıcak izler. Ekranda `final_30_yil` ve denize ulaşan yavru sayısı. Yürüyüş bitince tıklayınca oyun biter.

## Oyun akışı

`OyunAkisi` her oyun sahnesinde durur (ilki sahneler arasında yaşar). Adımlar `OyunAyarlari.AkisiKur()` ile GDD'deki sıradan kurulur: Bölüm 1 → 3 yuva gecesi (aralarında 1'er tur) → 6 bekleyiş turu → 3 kaçış gecesi (her birinden sonra 1 tur) → Bölüm 5 → final; toplam 11 tur. Sistemler `OyunAkisi.AdimBasladi` event'ini dinler, adımları bitince `OyunAkisi.AdimBitti()` der. Kumsal sahnesinde gece ve tur arasında sahne yeniden yüklenmez. Bir oyun sahnesinde doğrudan Play'e basılırsa akış o sahnenin ilk adımından başlar. F1–F4 sadece geliştirici sürümünde çalışır.

## Görünüm ve erişilebilirlik

- **Gece ve gündüz (`OrtamIsigi`, `OrtamAyarlari`):** kumsalda yuva ve kaçış gecelerinde ay ışığı, koyu gökyüzü ve sis; insan turunda gündüz; finalde güneşin rengini `FinalSahnesi` verir. Ay ışığı denizin üstünde soluk bir yol.
- **Su (`SuDalgalanmasi`, `SuAltiGorunumu`):** su yüzeyi dalga dalga oynar (iki yüzlü malzeme, alttan da görünür); denizde göz suya girince sis koyu ve yoğun olur.
- **Işık halkası:** otel ışığının kumda çekim alanı kadar sarı bir halkası var; Işık Yasağı ile söner.
- **Termal görünüm:** şeritler yarı saydam; her bandın ayrı deseni var (Mavi yatay çizgi, Yeşil nokta, Turuncu çapraz, Kırmızı dama), renk ayırt edemeyenler için.
- **Tuş atama:** Ayarlar'da 11 eylemin tuşu değiştirilir (`TusSatiri`), `PlayerPrefs`'te saklanır, "varsayılan" ile geri döner.
- **Geliştirici:** F5 kare hızı (FPS) göstergesi.
- **Editör:** `KumsalDurumu` asset'i Play bitince Play öncesi haline döner; Play'deki yuvalar ve izler git'e değişiklik olarak düşmez.

## Bilinen sorunlar

- `IkiDunya.Camera` namespace'i yüzünden `IkiDunya.*` içindeki kodda `Camera` yazınca Unity'nin kamera sınıfı değil bu namespace bulunur. Kamera sınıfı için `UnityEngine.Camera` yaz.

## İpuçları ve ses

**İpuçları (`Ipucu`, `IpucuPaneli`).** Oyuncu bir şeyle ilk karşılaştığında ekranın altında kısa bir yazı (6 sn, `OyunAyarlari.IpucuSuresi`); birden çok istenirse sırayla. Her ipucu bir oyunda bir kez çıkar (bu bilgisayarda saklanır, Yeni oyun sıfırlar); geçişte ve duraklatmada bekler. `OyunTercihleri.Ipuclari` kapatır (ayarlarda düğmesi henüz yok). Nerede çıkar:

| id | Ne zaman |
| --- | --- |
| `ipucu_poset`, `ipucu_nefes` | Bölüm 1 başında |
| `ipucu_ag` | Denizde ilk kez ağa ya da misinaya takılınca |
| `ipucu_dalga` | Bölüm 5 başında |
| `ipucu_termal`, `ipucu_kazma` | Yuva gecesi başında |
| `ipucu_kumdan_cikis` | Kaçış gecesinde kumdan çıkış başlarken |
| `ipucu_suru` | Yavrular yüzeye çıkınca |
| `ipucu_takili` | Bir yavru ilk kez takılınca ya da ters dönünce |
| `ipucu_tilki` | Tilkinin geldiği ilk gece |

**Ses (`Ses`, `SesYoneticisi`, `SesAyarlari`).** Sistemler bir kez `Ses.Cal(SesTuru, konum?)` der; klibi `ScriptableObjects/Settings/SesAyarlari`'ndaki yuvadan çalınır (birden çok klip varsa rastgele biri). Klip yoksa sessiz geçer. Konumlu sesler (yengeç, martı, tilki, kazma, yavru) 3B, geri kalanı ekrandan. Her adımın ortam sesi ve müziği aynı asset'te; adım değişince yumuşak geçişle değişir, menüde menü müziği. Teknenin pervanesi kendi AudioSource'unda döngüyle (`SesTuru.Pervane`). Ana ses düzeyi ayarlardan. **Menü ve duraklatmayı kur** her ses türü ve adım için boş yuva açar; Abdü klipleri `Audio/` altına koyup yuvalara sürükler.

| Ses türü | Nerede |
| --- | --- |
| Kazma, YuvaTamam | Anne A/D ile kazarken, yuva bitince |
| KumKazma | Yavrular W ile bir kat çıkınca |
| Takilma, Kurtarma | Yavru ya da yüzücü takılınca; öncü kurtarınca |
| YavruKurtuldu, YavruKaybedildi | Yavru denize girince, kaybedilince |
| YengecUyari, MartiUyari, TilkiUyari, TilkiEseliyor | Avcıların uyarıları (titreyen kum, gölge, koşu, yuva eşeleme) |
| Yeme, AgdanKurtulma, KontrolNoktasi, DalgaVurdu | Denizde |
| Pervane | Tekne (döngü) |
| Ipucu | İpucu kutusu açılınca |
| DugmeTikla | Menü ve yönetim düğmeleri (`UI/DugmeSesi`), tuş atama (`TusSatiri`), İleri ve Space (`InsanTuru`), hedefli kart seçimi, haritada görev verme |
| KartOyna, KartDagit | Kart oynanınca, el dağıtılınca (`InsanTuru`) |
| KanitBulundu | Haritada kayıp izine tıklanıp kanıt açılınca (`KayipIziHaritasi`) |

**Klipler (2026-11-01, Yusuf).** Bütün efektler (26) ve bütün adımların ortam sesi bağlı (kaynak ve lisans `docs/varliklar.md`'de). Ortam: `Donus`, `IlkYuzus` ve `Parkur` su altı, `YuvaGecesi` ve `KacisGecesi` kumsal gecesi, `InsanTuru` gündüz kumsal, `Final` sakin deniz. Çöp Seli gece kumsalda geçtiği için `SesAyarlari`'ndaki kendi yuvasından (`copSeliOrtami`) kumsal gecesi sesini çalar. Henüz yok: müzikler.

## İnsan kısmı (yönetim ekranı)

Toprak'ın onayıyla Umut yazdı (2026-11-01); kodu artık Toprak sürdürür. Sayılar GDD'den; kâğıt prototip atlandı, denge `ScriptableObjects/Settings/YonetimAyarlari`'nda ayarlanır. **Kumsal sahnesini kur (oyun)** 12 kart (`ScriptableObjects/Cards`), 5 görev (`VolunteerTasks`), 6 olay (`Events`) asset'ini GDD değerleriyle yazar ve ekranı kurar; **Menü ve duraklatmayı kur** `YonetimDurumu`'nu kayda bağlar.

**Genişleme (2026-11-01, Toprak).** Paydaşlar (`Paydas`: Otel, Belediye, Balıkçılar; −3..+3, dost +2, küs −2) `YonetimDurumu`'nda; kartlar ve olay seçenekleri `PaydasFarki` taşır. Kart türleri (`KartTuru`), bakım (gelirden düşer), paydaş koşulu, ikinci kanıtla güçlenme (`Guclu`, `Deger`). Acil kartlar havuza değil, uyarı aktifken ele girer. Uyarılar iki aşamalı: olay `bekleyenUyari`'yı kurar, sıradaki `TurBasla` onu `aktifUyari` yapar, `Etkiler.TurSonu` gerçekleştirir. Olaylarda koşul (`OlayKosulu`) ve zincir (`aSonraki`/`bSonraki`; kayıtta liste sırası). Gönüllü Çağrısı ve olayların gönüllü farkı gelecek tura işler (`SonrakiTurGonullu`). Haritadaki yuva listesinde avcı riski (`Etkiler.AvciRiski`).

**Ekran.** Kumsala tepeden bakılır (kumsalın kendisi harita: çöp, şezlong, ışık, gölgelik, araç izi sahnede görünür; T termal bantları açar). İmlecin altındaki bölge vurgulanır, solda katmanları ve yapıları yazar; yuvalar haritada bandının renginde simge (kafesliyse halkalı), solda listede bant, yumurta ve çıkışa kalan hafta. Kayıp izleri kanıt renginde; tıklanınca kanıt açılır. Üstte tur, bütçe, boş/toplam gönüllü, halk desteği ve aşama. Sol panel boşken (yuva yok, imleç haritada değil) gizlenir. Kartlarda yazı üstten başlar (başlıklar aynı hizada); İleri düğmesinde tuşu da yazar (ayarlardan değişirse güncellenir).

**Bir tur (`InsanTuru`).** Tur başında: bir hafta geçer (`BirHaftaGecti`), gelir gelir (ilk tur hariç +3, destek 7+ ise +5), geçen turun uyarısı gerçekleşir (sıcak dalgası: bu tur ve gelen gece kum +1 bant, Temiz Enerji varsa %50 ihtimalle hiç; fırtına: dalga altındaki her yuva yumurtasının yarısını kaybeder, Su izi bırakır). Sonra İleri düğmesi ya da Space ile:
1. **Rapor:** gelir, denize ulaşan yavru, geçen turun sonunda olanlar (çöp birikti, yuva bozuldu), uyarının sonucu, bu tur açılan kanıtlar.
2. **Gönüllüler:** görev seç, haritada bölge ya da yuva seç (her atama bir gönüllü). Temizlik çöp −1, devriye o tur ve gece insan yok, kafes yuvaya tilki giremez, gölge ve sulama yuva o tur ve gece 1 bant serin, bilgilendirme destek +1. Gönüllü: 4 + her 10 yavruya 1 (en fazla 10) + çağrı − okul.
3. **Kartlar:** havuzdan 4 kart; bütçe yettiğince oynanır, hedefli kartta haritada seçilir (aynı karta tekrar tıklamak vazgeçer). Oynanmayanlar havuza döner; kalıcı yasaklar ve Temiz Enerji bir kez oynanır.
4. **Olay:** rastgele (art arda aynısı gelmez); iki seçenek ya da uyarı (Tamam). Seçenek düğmesinde kaynak etkisi yazar (bütçe, halk, paydaş, gelecek turun gönüllüsü). Destek 3'ün altındaysa otel ve festivalde kabul kapalı; bütçe yetmeyen seçenek kapalı.
5. **Tur sonu:** her bölgede %30 çöp +1; kafessiz ve çıkmamış her yuvaya %20 (Tilki Çiti varsa %10) avcı: yumurtanın %30'u gider, Avcı izi. Sonuçlar gelecek turun raporunda. Sonra akış ilerler.

**Kanıtlar.** Isı → Gölgelik 1 ucuz; Işık → Işık Yasağı; Çöp → Araç Yasağı, Şezlong Düzenlemesi; Avcı → Tilki Çiti; Su → Yuva Taşıma. Saha Araştırması açılmamış bir kanıtı rastgele açar.

**Kartların etkisi** `docs/kumsal-durumu.md`'deki eşlemeyle; ayrıntı: Temizlik Kampanyası en kirli 3 bölgede çöp −1; Kumul Onarımı seçilen bölgede; Yuva Taşıma yuvayı gölgelikli bir bölgeye (yoksa aynı bölgede) 12 m'ye, dalga çizgisinin üstüne taşır, bandı yeni yere göre değişir; Tilki Çiti kenardaki bölgeye; Gönüllü Çağrısı o tur +2.

**Metinler.** Bütün yazılar id ile (kart ve görev adları metinler.md'de var; açıklamalar `Açıklama` sütunundan `id_aciklama`, olay seçenekleri `Seçenek A/B` sütunlarından `id_a`, `id_b` olarak aktarılır). Ekranın kendi yazıları için yeni id'ler Yusuf'ta.

Henüz yok: kartların ve olay penceresinin görsel hali (Faz 9), gönüllü atamasını geri alma.

## Kumsal arazisi (Yusuf'un modelleri)

**Kumsal sahnesini kur** (ve gri kumsal/kaçış komutları) `KumsalArazi` kalıbını bulursa gri kum, kumul, deniz tabanı ve su yüzeyi yerine onu koyar (önce `Prefabs/Environment/KumsalArazi.prefab`, yoksa `Sandbox/Yusuf/KumsalArazi/`); bulamazsa gri kutu kurar. Arazi kumsalın düzeniyle aynı: kıyı z≈0 (±4,5 m kıvrımlı), kara +z, deniz yüzeyi y = −0,55, kumullar z≈37–45, 300 m genişlik (oyun ortadaki 80 m'yi kullanır).
- **Zemine oturtma (`Core/Zemin`):** Ground layer'a yukarıdan bakıp yüksekliği verir. `KumSicakligi.BolgeNoktasi` artık zemine oturmuş nokta verir (çöp, şezlong, ışık, gölgelik, araç izi, insan yolu, yuva taşıma); insanlar ve tilki yürürken, finaldeki kaplumbağa kumu izler. Kurulum, düz kuma göre konmuş kaçış ve final parçalarını (sıcak lekeler, kum kalesi, siperler, avcılar, sürü, anne başlangıcı, final izleri) arazinin yüksekliğine taşır.
- **Kıvrımlı kıyı:** kurulum kıyıyı araziden metre metre ölçer (`KumSicakligi.kiyiCizgisi`); kıyıdan uzaklık (sıcaklık bantları, dalga çizgisi, kaçıştaki dalga, bölge noktaları) o x'teki kıyıdan ölçülür.
- **Termal görünüm:** şeritler kumu izleyen ızgara (1 m), kıyıyı izler; ilk T'de kurulur.
- **Harita (insan turu):** imleç zemine çarpar; bölge vurgusu kumsalın 6 m üstünde.
- **Objeler:** çöp yığını (`Cop_Yigini_1`), şişe, şezlong ve kaçıştaki kum kalesi Yusuf'un modelleriyle görünür; gri kutunun collider'ı ve layer'ı kalır (oynanış aynı). Model bulunmazsa gri kutu.
- Arazi varken kaçıştaki düz su şeridi görünmez (dalga logic'i aynı; görüntüyü Yusuf'un denizi veriyor).

Henüz yok: Bölüm 1 ve 5 için deniz tabanı modeli; ağ, tekerlek izi, ışık direği, gölgelik, insan, tilki modelleri; arazi dosyalarının kalıcı klasöre (`Prefabs/Environment`, `Models/Kumsal`) taşınması (Yusuf, Unity içinden).

## Minigame'ler (parkurlar)

GDD: "Parkurlar" ve bölümlerdeki "Minigame" başlıkları. Sayılar `ScriptableObjects/Settings/ParkurAyarlari`'nda. Her parkur kendi sahnesinde (`Scenes/Levels/Parkur_*`) ve akışta bir adım (`AdimTuru.Parkur`, Sira = `ParkurTuru`):

`Donus` → **Parkur: Poşet sürüsü** → yuva geceleri … kaçış gecesi 1 → **Parkur: Çöp seli** (2. gece yerine) → … → `IlkYuzus` (Sira 0, girdap kapısına kadar) → **Parkur: Çöp girdabı** → `IlkYuzus` (Sira 1, devam noktasından) → final.

- **Kapılar:** `KontrolNoktasi.Kapi` — Bölüm 1'de z −30'da akıntı perdesi (bölüm biter), Bölüm 5'te z −110'da girdap kapısı (ilk parça biter; devam parçasında sıradan kontrol noktası). `DenizBolumu.devamBaslangici` Sira 1'in başladığı yer.
- **Ortak (`Core/Parkur`):** süre, rekor, açılma, sayaç (`Bilgi`). `UI/Night/ParkurGostergesi` sağ üstte süre/rekor/sayaç; bitince sonuç, ardından akış ilerler (hikâyede sıradaki adım, menüden oynanıyorsa menü). Ana menüde **Parkurlar** (`OyunAkisi.ParkurOyna`; kayıt yazılmaz).
- **Poşet sürüsü (`Turtle/PosetSurusu`):** 3 aşama (açık deniz, sığ sular, kumsalın önü); her aşamanın alanında sürü yandan akar, hedef kadar denizanası yenince sonundaki akıntı duvarı (Obstacle) kalkar. Enerji sürekli azalır; enerji biterse aşamanın başından. Karanlık aşamada güneş ve ortam ışığı kısılır (denizanası ışıksız malzemeyle parlar, poşet ışıklı malzemeyle görünmez olur). Yoğun aşamada misina, karanlıkta hayalet ağ.
- **Çöp girdabı (`Turtle/CopGirdabi`):** yavru merkezde; 5/10/15 m halkalar ters yönlerde döner, boşluk (45°) halkanın ileri yönünde, parlak şamandıralarla işaretli. Şu anki halka boşluğu dışında duvar gibi; girdap döndürür ve hafif çeker; `AgirCop*` çöpler çarpar (savrulma, −8 enerji). Enerji biterse merkezden.
- **Çöp seli (`Turtle/CopSeli`):** 40×48 m kumsal parçası; yuva z 40, deniz z < 0. Dalga döngüsü uyarı (köpük ufuktan gelir, `SelGeliyor` sesi) → yükselme → bekleme → çekilme → sakin; her dalga daha uzağa. Kum tepeleri (0,15 m, Space ile çıkılır) güvenli; sudaki yavru itilir, 2 sn'de kaybedilir (Su). Denize sadece su çekilirken/durgunken girilir. Kasalar suyla gelir ve iter; ağlar takar. 150 sn'de gün doğar. Hikâyede o gecenin yuvasının yumurtası kadar yavru; kurtulan `DenizeUlasti`, kaybedilen yuvanın yerinde iz.
- **Kurulum:** İki Dünya → **Parkur: Poşet sürüsü / Çöp girdabı / Çöp seli sahnesini kur**. Kurulan Levels sahneleri build listesine kendiliğinden eklenir (Menu en başa).
- İpuçları: `ipucu_poset_surusu`, `ipucu_girdap`, `ipucu_cop_seli`. Sesler: `Kombo`, `ParkurBitti`, `ParkurAsama`, `SelGeliyor`.

## Canlılar (Yusuf'un modelleri)

`Sandbox/Yusuf/Canlilar` (ya da `Models/Canlilar`): kurulum komutları modeli bulursa gri kutunun yerine koyar; boyu oyundaki gövdeye göre ölçeklenir (en uzun kenar), tabanı yere oturur, collider ve layer oynanış kalıbında kalır. Anne (kara ve deniz, final), yavru (kumsal ve deniz; ters dönen yavruda model çevrilir), yengeç (deliğin yanında), martı, tilki (eşelerken sallanan gövde), avcı balık, insan.

## Animasyon ve birinci şahıs

- **Kodla animasyon (`Turtle/Salinim`, `AnimasyonAyarlari`):** modeller iskeletsiz; kök hareket ettikçe görünüş (model ya da gri gövde) türüne göre salınır: **Kara** (anne, yavru, tilki, insan, final kaplumbağası: her adımda kalkar, iki yana yatar), **Deniz** (yüzücüler: yavaş dalgalanma, dururken de), **Kuş** (martı: süzülme), **Balık** (avcı balık: sağa-sola kıvrılma). Anne kazarken geriye yatar, her A/D vuruşunda sarsılır. Sayılar `ScriptableObjects/Settings/AnimasyonAyarlari`'nda. Kurulum komutları ekler.
- **Birinci şahıs:** `BirinciSahisKamera` kendi gövdesinin modelini kameraya görünmez yapar, gölgesi kalır (kafanın içinden bakma, göze giren kabuk olmaz). Öncü yavru değişince eski öncü yeniden görünür.
- **Metinler:** `docs/metinler.md`'de kodun kullandığı bütün id'lerin ilk taslağı (Umut; Yusuf gözden geçirecek). **Metinleri aktar** (ya da Menü ve duraklatmayı kur) ile oyuna girer.

## Modeller (2026-11-01 eklemeleri) ve uyarılar

Kurulum komutları şunları bulursa gri kutunun yerine koyar (önce `Assets/_Project/Models/...`, yoksa Sandbox):
- **Deniz tabanı** (`Sandbox/Yusuf/DenizTabani/DenizTabani.prefab`): Bölüm 1, Bölüm 5 ve deniz parkurlarında üç gri basamağın yerine (ölçüler aynı, collider'lı). Bölüm 5'teki kayalar `Kaya_1..3`, deniz çayırı görüntüsü `DenizCayiri_6x10/6x8/8x8` (saklanmayı yine `Cover` kutusu yapar). *Yusuf'un dalı (`claude/admiring-meitner-b9crj2`) birleşince devreye girer.*
- **İnsanlar:** `Insan`, `Insan_02`, `Insan_03` üç ayrı kalıp; `KumsalYapilari.insanKaliplari`'ndan bölgeye göre biri seçilir.
- **Tekne:** `Sandbox/Tekne/BalikciTeknesi_Gorsel` (Abdü), gri tekne kutusunun yerine. Pervane animasyonu kalıpta değil, sonra bağlanacak.
- **Yiyecek:** kök yeme alanı (tetikleyici küre), görünüş çocukta: denizanası `Sandbox/Denizanasi/Denizanasi` (Abdü; `Denizanasi.controller` ile kasılır), poşet `KumsalObjeleri/Poset`.

Uyarılar: kameralardaki kulak `Core/TekKulak` ile tek (açılan kamera alır); menü kamerasında da kulak var. Sahnenin kendi `Sistemler` kopyası Awake'te hemen kapanır (iki EventSystem uyarısı).
- **Akıntılar** artık görünmez bir alan ve içinde akış yönünde süzülen ince çizgiler (eskiden koca saydam beyaz kutulardı).
- **Süsler:** Bölüm 1, Poşet sürüsü ve Çöp girdabı sahnelerinde oyun alanının kenarlarına Yusuf'un kayaları ve deniz çayırı (tabanın yüksekliğine oturur).
- **Model dönüşü:** kalıbın kendi dönüşü korunur (Blender eksen düzeltmesi; tekne dik duruyordu). Denizanası artık sürekli yükselmez, aynı derinlik çevresinde kasılır.
- **Poşet:** −10 enerji (`DenizTehlikeAyarlari.posetEnerjiKaybi`), yarı hız, `PosetYuttu` sesi, `ipucu_poset_yuttu`, deniz göstergesinde yavaşlık çubuğu.

## Cila (#64)

- **Geri bildirim (`Core/Geribildirim`, `GeribildirimAyarlari`, `UI/Night/EkranEfektleri`):** çarpma ve yakalanmada kamera sarsıntısı ve kırmızı flaş; poşette kahverengi bulanma; yavru kurtulunca yeşil parıltı; enerji azalınca (`Turtle/EnerjiUyarisi`) ekran kenarı kararır ve kalp atışı sesi (`SesTuru.KalpAtisi`, ses yuvası boş; `SesTuru`'nda Yusuf'un seslerinden sonra, en sonda).
- **Kamera:** yürürken/yüzerken hafif baş sallanması, Shift'te görüş açısı genişler, oyun kolunun sağ çubuğuyla bakış. Sayılar `KameraAyarlari` ve `GeribildirimAyarlari`'nda.
- **Gece:** yıldızlar ve ay (`Core/GeceGokyuzu`, shader `IkiDunya/Gokyuzu`; `OrtamIsigi` gece açar); anne ve yavrular kumda iz bırakır (`Turtle/IzBirakici`, 25 sn'de silinir).
- **Su altı ışık hüzmeleri (`Core/SuIsiklari`):** deniz sahnelerinde yüzücünün çevresinde; suyun üstündeyken görünmez.
- **Ara sahneler (`Core/AraSahne`):** Bölüm 1, yuva gecesi, kaçış gecesi ve Bölüm 5 başında kamera geniş bir açıdan oyuncunun gözüne süzülür; bu sırada tuşlar kilitli (`GameInput.Kilitli`).
- **Final:** kamera yürüyüş boyunca kayar, yazılar belirir, sonra jenerik (`jenerik` metni) akar; tıklayınca ya da jenerik bitince oyun biter.
- **Ayarlar:** yazı boyutu (bütün arayüz, `Core/ArayuzOlcegi`), ipuçları aç/kapa, tam ekran, çözünürlük, grafik kalitesi (düşük/orta/yüksek). Ayarlar paneli tam ekran koyu zeminde açılır; tuşları sıfırlama düğmesi tuş sütununda. Açılır listeler 40 birimlik satırlarla, açılınca seçili satıra kayar (`UI/Menu/AcilirListeKaydirma`). Panel açılınca ve Geri ile dönünce ilk basılabilir seçenek seçilir (`MenuSecenegi.IlkiniSec`), oyun koluyla gezinme kopmaz.
- **Yazı boyutu ve taşma (`Core/ArayuzOlcegi`):** yazı boyutu 0.75–1.5 tuvali ölçekler (1.5'te tuval 1280x720 gibi davranır). Tuval her en-boy oranında en az referans kadar kalır (`Expand`). Tuvaldeki her yazı kutusuna sığmazsa kendi boyutunun %60'ına kadar küçülür (best fit); kodla kurulan yazılar `ArayuzOlcegi.Boyut(yazi, boyut)` ile boyut alır. **Kural:** yazıyla büyümesi gereken paneller ekran oranıyla (anchor yüzdesi) değil tuval birimiyle (sabit boy + köşe çapası) kurulur ve 1280x720'ye sığacak boyda seçilir; yoksa yazı büyür, kutu büyümez. Kurulum kodunda oranla verilen kutu için `GriKumsalKur.Sabitle(rect, enAz, enCok)`: kutu 1920x1080'deki yerinde kalır ama en yakın kenara (ya da ortaya) çapalanıp sabit boy alır, büyürken ekrandan taşmaz. Yönetim ekranı, ipucu kutusu, ana menü, duraklatma, parkurlar, gece/deniz çubukları, parkur süre/sonuç kutuları, gece sonuç paneli (boyu satır sayısına göre) ve bölüm geçiş yazısı bu kurala göre; 1.5'te çakışmasız denendi. Sonuç paneli açıkken ipucu kutusu gizlenir ve bekler (`GeceSonucuPaneli.Acik`).
- **Arayüz teması "Belgesel" (`Core/ArayuzTemasi`, `ScriptableObjects/Settings/ArayuzTemasi`):** bütün arayüzün yazı tipleri ve renkleri tek asset'te; kurulum kodu ve `YonetimArayuzu` renkleri buradan alır, koda renk yazılmaz. Başlıklar serif (Lora), gövde sans (Source Sans 3 Regular, kalın yerlerde SemiBold); dosyalar ve OFL lisansları `Art/UI/Fonts`. Koyu yarı saydam paneller, krem yazı, kum rengi vurgu (seçili seçenek, başlık altı çizgi, aşama). Menüler kutusuz: sol üstte serif başlık + kısa vurgu çizgisi, altında sola yaslı seçenekler (`UI/Menu/MenuSecenegi`: seçiliyken vurgu rengi ve solunda çizgi, fare üstüne gelince seçer). Menü ve ayarların zemini alacakaranlık ufku, duraklatmada oyunun üstünde soldan koyulaşan perde (`UI/Menu/UfukZemini`, kodla çizilir). Renk değişikliği asset'ten yapılır; kurulumla gelen renkler (çubuk zemini, panel) sahneyi yeniden kurunca güncellenir.
- **İpucu kutusu:** üstte ortada (yönetimin üst çubuğunun altında), boyu yazıya göre; iki yanda 440 birim boş bırakır ki yönetimin sol paneli ve kart bilgisiyle çakışmasın.
- **Oyun kolu:** sol çubuk hareket, sağ çubuk bakış, LT hızlı, A zıpla/dal, X etkileş, LB/RB kazma sol/sağ, Y ya da d-pad yukarı kazmada yukarı, sağ çubuğa basış termal, Start duraklat. Menülerde ilk düğme seçili gelir. İnsan kısmının harita ekranı (Toprak) oyun koluyla denenmedi.
- **Performans:** deniz kamerasında uzak düzlem 150 m (sis zaten 100 m'de örter); insanlar, kayalar, deniz çayırı ve deniz tabanının süsleri ekranda çok küçülünce çizilmez (LODGroup), küçük süsler gölge atmaz.

## Kart yapıları: kum torbası, gözcü kulübesi, plaj tabelası (Yusuf'un modelleri, 5. parti)

Kaynak betikler `Blender/Yusuf/` (`kum_torbasi.py`, `gozcu_kulubesi.py`, `plaj_tabelasi.py`, toplu: `kumsal_objeleri_5_uret.py`). FBX'ler `Models/Kumsal/` altında (`blender -b -P Blender/Yusuf/kumsal_objeleri_5_uret.py -- Assets/_Project/Models/Kumsal Blender/Yusuf/kumsal_objeleri_5.blend`), malzemeleri öteki kumsal objeleri gibi FBX'in içinde. Düzen öteki kumsal objeleriyle aynı (kök tabanın ortasında, metre, önü Blender'da −y).
- **`Kum_Torbasi_Seddi`** (Kum Torbası Seddi kartı): 2 m'lik parça (x = −1..+1), boy ~0,35 m, derinlik ~0,8 m; 2 m arayla dönüşsüz dizilince dikişsiz. Tek çuval: `Kum_Torbasi` (0,6 × 0,36 × 0,13 m).
- **`Gozcu_Kulubesi`** (Gözcü Kulübesi kartı): ayak tabanı 2,3 × 2,3 m, döşeme 2,2 m, mahya ~4,6 m, bayrak ~5,35 m; önde ~0,9 m taşan merdiven. Ön saçakta yanan fener (`P_Kehribar`).
- **`Plaj_Tabelasi`** (Plaj Tabelaları kartı): iki direkli pano (1,4 × 0,9 m, boy ~2,1 m); üzerinde yazı yok, resimler var (kaplumbağa, ışık yasağı, ayak izi yasağı). Pano yüzü ayrı malzeme (`Tabela_Yuzu`), Unity'de dokulu malzemeyle değiştirilebilir. Küçüğü `Plaj_Tabelasi_Kucuk` (tek direk, ~1,3 m).

**Kumsala bağlandı (2026-11-01, Toprak).** Kalıplar `Prefabs/Structures/` altında, `KumsalYapilari` kurar:
- Gece devriyesi olan bölge (Gözcü Kulübesi kartı, devriye görevi) → `GozcuKulubesi`, bölgenin sağında 31 m içeride, merdiveni denize dönük.
- Kumul onarımı olan bölge (Kumul Onarımı ve Kum Torbası kartları) → `KumTorbasiSeddi` parçaları deniz kenarından 5 m'de, her parça kıyının o noktadaki yönüne döner; iki ucunda tek `KumTorbasi`.
- İnsanı süreli azaltan etki süren bölge (Plaj Tabelaları) → `PlajTabelasi` / `PlajTabelasiKucuk` sırayla, 12 m'de. Durum `KumsalDurumu.InsanSureliAzaldi(bolge)`'dan okunur; etki sınırda kırpılıp hiç insan azaltmadıysa tabela çıkmaz.

**Kullanılmayan öteki modeller de bağlandı (2026-11-01):**
- `Ag`, `HayaletAg`, `Misina`, `OtelIsigi` kalıpları artık `Ag`, `Hayalet_Ag`, `Misina`, `Isik_Diregi` modelini gösterir; eski küpler collider olarak kaldı, renderer'ları kapalı. Hayalet ağ ve misinada `Parilti` modelin parıltı malzemesini (`malzemeSirasi`) yakar.
- Bölüm 1 ve Poşet sürüsündeki elle kopyalanmış hayalet ağ/misina kopyaları kalıp örneğine çevrildi (kalıp değişince sahne de değişir).
- Bölüm 1 ve Bölüm 5'teki şamandıra silindirleri `Samandira` modeli; Bölüm 5'in ikinci parlayan plastiği `Parlayan_Plastik_3`.
- Çöp katmanında her ikinci ağ yerine `PlastikHalka` (aynı tuzak, daha küçük alan). Her ikinci şezlongun yanına `Semsiye` (sadece direği engel).
- Tilki çitinin bittiği uçlara `TilkiCitiSolUc` (`Tilki_Citi_Kose`) ve `TilkiCitiSagUc` (`Tilki_Citi_Kose_Bas`); komşu bölgede de çit varsa uç konmaz.
- Yuva haritasında yumurtalı yuvanın ortasında üç küre yerine `Yumurta_Kumesi` (`Prefabs/UI/YumurtaKumesiSimgesi`). Tek `Yumurta` modeli kullanılmıyor (kümenin içinde zaten var).
