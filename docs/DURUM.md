---
ozet: Mekanik tarafın durumu ve sistemlerin haritası. Bir sistem eklendikçe ya da değiştikçe güncellenir. Sahibi Umut; Toprak kendi sistemlerini işler.
---

# Durum

Son güncelleme: 2026-11-01 — Alpha main'de, deneme listesi tamam (editörde 300+ FPS). Dallarda (`umut/oynanis-eksikleri`, üstünde `umut/ipucu-ses`): tilki yuvayı bozar, öncü de takılır, kumul onarımında dalga çizgisi kayar, denizde tekne gölgesi/köpük ve parlayan ağ; ipuçları, sonuç panelinde yazılar, ses yuvaları (SesAyarlari). Unity'de henüz denenmedi.

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
| Adıma göre sahne parçaları, geçici insan turu | `Core` | `AdimNesneleri`, `GeciciInsanTuru` | Umut | Yapılıyor (insan turu geçici) |
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
| Kumsalı durumdan kurma (çöp, şezlong, ışık, insan, araç izi, gölgelik, kayıp izleri) | `Hazards` | `KumsalYapilari` | Umut | Yapılıyor |
| Kumsalın ilk hali | `Core` | `KumsalBaslangici`, `DenemeKumsali` | Umut | Yapılıyor |
| Kaçışta çöp (ağ, halka, tekerlek izi) | `Hazards` | `TrashTrap` | Umut | Yapılıyor |
| Avcılar | `Hazards` | `Yengec`, `Marti`, `Tilki`, `AvciAyarlari`, `Cover`; `Core/IAvlanabilir` | Umut | Yapılıyor |
| Işık kaynakları | `Hazards` | `IsikKaynagi` | Umut | Yapılıyor |
| Kayıp izleri | `Turtle`, `Hazards` | `KacisGecesi` yazar, `KumsalYapilari` kumda gösterir, `KayipIziHaritasi` insan turunda simge + kanıt | Umut | Yapılıyor |
| İpuçları (ilk karşılaşmada bir kez) | `Core`, `UI/Night` | `Ipucu`, `IpucuPaneli` | Umut | Yapılıyor (yazılar Yusuf'ta) |
| Ses: olaylar, ortam sesi, müzik | `Core` | `Ses`, `SesYoneticisi`, `SesAyarlari` | Umut | Yapılıyor (klipler Abdü'de) |
| Kumsal haritası ve tur akışı | `Management` | — | Toprak | Planlandı |
| Gönüllüler ve görevler | `Management` | — | Toprak | Planlandı |
| Kartlar | `Management` | `CardDefinition` | Toprak | Planlandı |
| Kanıtlar ve olaylar | `Management` | — | Toprak | Planlandı |

Durum değerleri: Planlandı · Yapılıyor · Oynanabilir · Bitti.

## `KumsalDurumu` sözleşmesi

> Tam sözleşme, kart/görev/olay → metot eşlemesiyle: [`docs/kumsal-durumu.md`](kumsal-durumu.md). Aşağıdaki tablo özet.

**Taslak (Umut, Toprak'ın onayı bekleniyor).** `KumsalDurumu` bir ScriptableObject asset'i; kullanan her script ona `[SerializeField]` ile bağlanır. Yeni oyunda `OyunAkisi` sıfırlar. Her değişiklikten sonra `Degisti` event'i gelir.

| Ne | Okuma | Değiştirme (açık metot) | Kim çağırır |
| --- | --- | --- | --- |
| Bölgeler | `BolgeSayisi`, `BolgeAl(i).Seviye(katman)`, `BolgeAl(i).Var(yapi)` | `KatmanDegistir(bolge, katman, fark)`, `YapiKoy`, `YapiKaldir` | İnsan kısmı (kartlar, olaylar, simülasyon) |
| Katmanlar (0–`katmanEnFazla`) | `Cop`, `Isik`, `Sezlong`, `Insan` | | |
| Yapılar (var/yok) | `Golgelik`, `KumulOnarimi`, `TilkiCiti`, `AracIzi` | | |
| Denizdeki ağ | `DenizdekiAg` | `DenizdekiAgDegistir(fark)` | İnsan kısmı |
| Işık yasağı (otel ve yol ışıkları kapalı) | `IsikYasagi` | `IsikYasagiAyarla(var)` | İnsan kısmı (Işık Yasağı kartı) |
| Temiz Enerji oynandı mı (final değişir) | `TemizEnerji` | `TemizEnerjiAyarla(var)` | İnsan kısmı (Temiz Enerji kartı) |
| Kum ısınması (bütün kum kaç bant ısındı) | `KumIsinmasi` | `KumIsinmasiDegistir(fark)` | İnsan kısmı (sıcak dalgası olayı) |
| Yuvalar | `Yuvalar` (bölge, konum, bant, dalga altında mı, yumurta, çıkışa kalan hafta, kafesli) | `YuvaEkle` (yuva gecesi), `YuvaKafesle`, `YuvaTasi`, `YuvaYumurtaKaybet`, `BirHaftaGecti` | Ekleme kaplumbağa, gerisi insan kısmı |
| Kayıp izleri | `KayipIzleri` (bölge, konum, neden, kanıt açıldı mı) | `KayipIziEkle` (gece), `KanitAc` (insan turu) | İkisi |
| Denize ulaşan yavru | `DenizeUlasanYavru` | `DenizeUlasti(adet)` | Kaplumbağa |

Bütçe, gönüllü ve halk desteği `KumsalDurumu`'nda değil; insan kısmının kendi verisi.

## Araçlar

- **İki Dünya → Layer'ları kur:** README'deki layer'ları boş yuvalara ekler.
- **İki Dünya → Gri kutu kumsalı kur (yuva gecesi):** `Scenes/Prototypes/GriKumsal.unity` — kumsal, termal görünüm, şezlong, çöp yığını, 2 insan (biri fenerli), anne (hareket, kazma), yuva gecesi, enerji ve kazma çubuğu.
- **İki Dünya → Kumsal sahnesini kur (oyun):** `Scenes/Levels/Kumsal.unity` — iki gecenin hepsi ve geçici insan turu; her grup (`YuvaGecesiNesneleri`, `KacisGecesiNesneleri`, `InsanTuruNesneleri`) sadece kendi adımında açılır. `Sistemler` prefab'ı da konur.
- **İki Dünya → Bölüm 1 sahnesini kur (deniz)** ve **Bölüm 5 sahnesini kur (deniz):** `Scenes/Levels/Bolum1_Donus` ve `Bolum5_IlkYuzus` sahnelerini gri kutu olarak baştan kurar (deniz, taban basamakları, kontrol noktaları, tehlikeler, göstergeler, `Sistemler`).
- **İki Dünya → Menü ve duraklatmayı kur:** `Menu` sahnesini (başlık, Yeni oyun, Devam et, Ayarlar, Çıkış) kurar; `Sistemler` kalıbına EventSystem ve duraklatma menüsü ekler, deneme panelini (`AkisDeneme`) çıkarır; önce metinleri aktarır.
- **İki Dünya → Metinleri aktar:** `docs/metinler.md`'deki tablolardan `MetinTablosu` asset'ini doldurur. Metinler.md değişince yeniden çalıştırılır. Eksik metin oyunda `[id]` olarak görünür.
- **İki Dünya → Gri kutu kaçışı kur:** `Scenes/Prototypes/GriKacis.unity` — kumsal, sıcak kum lekeleri, dalga, gün doğumu, sürü, kaçış gecesi, kumdan çıkış paneli ve gün doğumu çubuğu.
- İki araç da eksik ayar asset'lerini `ScriptableObjects/Settings`'te, `Yavru`, `Golgelik`, `KayipIzi` kalıplarını `Prefabs/` altında oluşturur. Deneme sahnelerinde akış yoktur; gece kendiliğinden başlar.

## Kum sıcaklığı

`KumSicakligi.Bant(konum)` bir noktanın bandını verir: denizden uzaklığa göre Mavi → Yeşil → Turuncu → Kırmızı (sınırlar `TermalAyarlari`'nda: 8, 18, 30 m), gölgelikli bölgede 1 bant serin, `KumsalDurumu.KumIsinmasi` kadar sıcak. Termal görünüm (T) her bölge ve bant için kumun üstüne renkli şerit serer; kumsal değişince yeniden renklenir. Kazma bu bandı yuvaya yazacak.

> Bantların denizden uzaklığa göre dizilmesi bir başlangıç varsayımı (serin bant dalga çizgisine yakın: serin ama fırtına riski). GDD yerleşimi söylemiyor; denemede değişebilir.

## Kumsal geceleri

**Yuva gecesi (`YuvaGecesi`).** Anne denizden çıkar; W/S ile yürür, fareyle döner. Yerde dururken A ve D'ye sırayla basınca kazma başlar (20 vuruş), kısa yumurtlama, sonra örtme (10 vuruş). Yuvanın bölgesi, bandı, dalga çizgisine göre yeri ve yumurta sayısı (kırmızı bantta 6) `KumsalDurumu.YuvaEkle` ile yazılır; anne denize dönünce gece biter. İnsanın görüş konisine girmek ya da enerjinin bitmesi denemeyi bitirir; 2 deneme de biterse anne bulunduğu bölgede kıyıya yakın kendiliğinden yuva kazar. Çöp yığını anneyi 2 sn durdurur, 5 enerji alır.

**Kaçış gecesi (`KacisGecesi`).** Gecenin yuvası Bölüm 2'deki aynı sıradaki yuvadır (yoksa deneme yuvası). Kumdan çıkış: W ile 5 kat; yüzey zamanla serinler, ölümcül sıcaklığın üstünde çıkılırsa yavruların o oranı ölür; kum altında enerji biterse çıkış baştan. Koşu: öncüyü oyuncu sürer, diğerleri izler, Shift dağıtır. Sıcak lekeler gün doğdukça büyür (gölgelikli bölgede yarı boy), içindeki yavru enerji kaybeder. Dalga son 5 m'de gelip gider: çekilirken suya giren kurtulur, vururken giren geri savrulur. Öncü kurtulur ya da ölürse sıradaki yavru öncü olur. Gün doğunca kumda kalanlar kaybedilir. Her kayıp `KayipIziEkle`, her kurtulan `DenizeUlasti(1)`.

**Kumsalı durumdan kurma (`KumsalYapilari`).** Her bölgede: çöp seviyesi kadar çöp yığını (anneyi takar), ağ (yavruyu takar) ve şişe (labirent); şezlong seviyesi kadar şezlong; ışık seviyesi kadar otel ışığı (kumulun önünde); insan seviyesi kadar gezen insan (sadece yuva gecesinde); araç izi varsa tekerlek izi; gölgelik varsa gölgelik. Yerleşim bölge başına sabit tohumla her seferinde aynı. Yapılar adım başında ve insan turunda kurulur, gece ortasında kurulmaz (içindeki yavru yeniden takılmasın diye); kayıp izleri her değişiklikte güncellenir. Yeni oyunda kumsal `KumsalBaslangici` asset'inden kurulur (`OyunAyarlari`'na bağlı); deneme sahnelerinde bunu `DenemeKumsali` yapar. Ay ışığı denizin üstünde soluk bir yol olarak doğru yönü gösterir. Kumul onarımı olan bölgede dalga çizgisi 4 m denize çekilir (`KumSicakligi.DalgaCizgisi(bolge)`); kumdaki çizgi de o bölgede kayar (`DalgaCizgisi`).

**Kaçışta çöp, avcılar, ışık.** Ağ ve halka izleyen yavruyu takar, tekerlek izi ters çevirir; öncü yanına gidip E'yi basılı tutar (2 / 1,5 sn), bu sırada sürü bekler, sayaç işler. Space bütün sürüyü zıplatır (kum kalesi atlanır, şişe dolaşılır). Öncü de takılır ve ters döner; E'yi basılı tutup kendini kurtarır (bu sırada sürü bekler), ama yönünü kaybetmez. Yengeç deliği 2 sn titrer, sonra yanındaki bir yavruyu alır (zıplayan kurtulur). Martının gölgesi 1,5 sn önce düşer; dalış alanında 3+ yavru varsa 3, yoksa 1 yavru alır. Tilki her gece ihtimalle gelir (çit varsa yarı ihtimal), kumsalın kenarında gezer, konisinde siperde olmayan bir yavru görürse koşar, sürü 3 sn panikle dağılır. Geldiği gece 25 sn gezdikten sonra sonraki gecelerin kafessiz bir yuvasına gidip 4 sn eşeler: yumurtaların yarısı gider, yuvada Avcı izi kalır (kafesli yuvaya dokunmaz). Işık alanındaki yavru ışığa çekilir; 5 sn kalırsa yönünü kaybedip karaya yürür, öncü E ile geri çağırır. Gün doğunca kumda kalanlar o an başlarına gelen nedenle kaybedilir (takılı → Ağ, ters → Çöp, yön kaybı → Işık, diğer → Isı).

**Gecenin sonu.** Yuva gecesi yuvanın bandını (renk ve adı) ve yumurta sayısını, kaçış kurtulan, kaybedilen, dişi ve erkek sayısını gösterir (dişi oranı yuvanın bandından). Başlık, satır adları ve "devam" satırı metin id'leriyle (`sonuc_*`, `bant_*`). Tıklayınca ya da Space'le akış ilerler.

**Geçici insan turu.** Kumsala tepeden bakılır; tıklayınca bir hafta geçer (`BirHaftaGecti`) ve tur biter. Toprak'ın yönetim ekranı gelince kaldırılacak.

Henüz yok: yok (animasyonlar modeller gelince).

## Deniz bölümleri

**Yüzme (`DenizHareketi`).** Birinci şahıs: W/S/A/D baktığın yöne (yukarı-aşağı dahil), Shift hızlı (3 kat enerji). Space: yüzeydeyken kısa dalış; anne su altında Space basılı tutunca yüzeye çıkar, yavrunun Space'i sadece daldırır. Akıntı sürükler, akıntıya karşı yüzmek 2 kat enerji. Anne: 60 sn nefes, biterse yavaşlar, yüzeyde dolar. Yavru: nefes yok, enerji sürekli azalır (çılgın yüzüş), sadece kontrol noktalarında dolar. E: önündeki besini yer (denizanası +15 enerji, poşet 10 sn yarı hız); ağa ya da misinaya takılınca E'ye art arda basılır (anne ağ 8, yavru ağ 10, misina 6), sonra 5 sn yavaş. Parlayan plastik ve ışıklı tekne çeker, plastiğe değmek 5 sn yavaşlatır. Sıcak su lekesi saniyede 3 enerji alır. Tekne yüzeydekine çarpar (−25). Avcı balık gizli olmayanı kovalar; deniz çayırı ve kaya arası (`Cover`) korur.

**Bölüm (`DenizBolumu`).** Kontrol noktasına girince enerji dolar ve oradan devam edilir; enerji biterse ya da avcı yakalarsa son noktaya dönülür. Bölüm 1 kıyıda biter; Bölüm 5 yosun adasında, kamera uzaklaşarak biter. Bölüm 5'in dalga hattında dalgalar ara ara kıyıya gelir; yüzeye yakın yakalanan yavru savrulur, altından dalan geçer. Bölüm 5'teki ağ ve misina sayısı `KumsalDurumu.DenizdekiAg` seviyesinden (Hayalet Ağ Temizliği azaltır).

**Uyarılar.** Teknenin suyun hemen altına düşen gölgesi teknenin 6 m önünde gider, arkasında pervane köpüğü titrer; su altındaki oyuncu tekneyi gelmeden görür. Hayalet ağın kenarları ve misina yavaşça parıldar (`Parilti`), uzaktan seçilir.

Henüz yok: pervane sesi klibi (Abdü).

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

