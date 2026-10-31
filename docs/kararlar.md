---
ozet: Ekipçe alınan kararlar, tarih ve nedeniyle. En yeni en üstte. GDD'yi değiştiren her karar buraya bir satır.
---

# Kararlar

| Tarih | Karar | Neden | Kim |
|---|---|---|---|
| 2026-10-31 | Görsel tarz: Sword of the Sea'den esinlenen renk paleti (`docs/palet.md`), yumuşak gölgelendirme | Kırmızı Kaplumbağa, Wavetale ve ikisinin harmanı da Blender'da aynı sahneyle denendi; sıcak kum ve yeşim deniz kumsal-deniz temasına en iyi oturan | Yusuf |
| 2026-10-31 | Termal bantlar renk + desenle ayırt edilir; tuşlar ayarlardan atanabilir | Erişilebilirlik (Faz 10): renk körlüğü, farklı klavyeler | Umut |
| 2026-10-31 | Editörde `KumsalDurumu` Play bitince eski haline döner | Play'deki durum asset'e yazılıp git'te sürekli değişmiş görünüyordu (pull'ları da kilitliyordu) | Umut |
| 2026-10-31 | `KumsalDurumu`'na süreli etkiler (`GeciciKatman`, `GeciciYapi`, `GeciciKumIsinmasi`, `GeciciYuvaBant`; `BirHaftaGecti` geri alır), `YuvaBantDegistir`, `BolgeYapisi.Devriye`, `KanitTuru` ve `KanitAcildi` event'i; tam sözleşme `docs/kumsal-durumu.md` | GDD'deki "o tur" ve "bir gece" süren görev ve olaylar (devriye, gölge ve sulama, festival, sıcak dalgası) kumsala geri alınabilir biçimde yansısın; Toprak onaylayacak | Umut |
| 2026-10-31 | Geçici insan turunda tıklama kayıp izi simgelerine (kanıt), tur sonu Space'e | Kanıt toplamak tıklama ister; turu yanlışlıkla bitirmesin | Umut |
| 2026-10-31 | Oyuncu metinleri `MetinTablosu` asset'inden id ile; asset `docs/metinler.md`'den editör aracıyla doldurulur, eksik metin `[id]` görünür | Metin tek yerde (Yusuf'un dosyası), kod sadece id bilir; eksikler oyunda hemen fark edilir | Umut |
| 2026-10-31 | Kayıt her adımın başında, kendiliğinden: adım sırası + `KumsalDurumu`; tek kayıt yuvası | Gece ortası kaydı karmaşık; adım başı yeterli ve ~2 saatlik oyun için tek yuva yeter | Umut |
| 2026-10-31 | Akışa `Final` adımı (Kumsal sahnesinde); `KumsalDurumu`'na `TemizEnerji` ve `TemizEnerjiAyarla(var)` | GDD: "30 yıl sonra" sahnesi Temiz Enerji kartına göre iki hal | Umut |
| 2026-10-31 | Yeni klasör `Scripts/UI/Menu` (Umut): ana menü, ayarlar, duraklatma | Ne kaplumbağa ekranı (`UI/Night`) ne yönetim ekranı (`UI/Management`) | Umut |
| 2026-10-31 | Denizde yön bakılan yer (yukarı-aşağı dahil); anne Space basılı tutunca yüzeye çıkar, yüzeyde Space'e basınca dalar; yavrunun Space'i sadece daldırır | GDD'deki "Space: yüzeye çık / dal" (anne) ve "denizde dal" (yavru) birinci şahısta böyle oturuyor | Umut |
| 2026-10-31 | Denizde ağ ve misinadan E'ye art arda basarak kurtulunur (kumsalda E basılı tutulur) | GDD tablosu: denizde "E'ye 8/10/6 kez bas", kumsalda "E'yi 2 sn basılı tut" | Umut |
| 2026-11-01 | Minigame'ler zorunlu ve kendi sahnelerinde: akışa `Parkur` adımı olarak girer (Bölüm 1'den sonra poşet sürüsü; 2. kaçış gecesi yerine çöp seli; Bölüm 5 girdapta ikiye bölünür, parkurdan sonra `IlkYuzus` Sira 1 devam noktasından). Aşamalı, uyarılı, daha uzun tasarım | İlk deneme: alan bulunamadı, girdap 13 sn ve sıkıcı, sel anlaşılmadı. Kullanıcı kararı: girmesi zorunlu, ayrı sahnede, daha detaylı | Umut |
| 2026-11-01 | Minigame'ler GDD'ye girdi: Bölüm 1 poşet sürüsü, Bölüm 4 çöp seli (2. kaçış gecesi), Bölüm 5 çöp girdabı; hepsi süreli parkur, bitirilen menüde "Parkurlar"dan tekrar oynanır, rekor bu bilgisayarda | Kullanıcı kararı: oyuncu kaplumbağa tarafında çöple boğuşsun, parkurla kaçsın, zaman harcayacağı tekrar oynanabilir bir şey olsun | Umut |
| 2026-11-01 | Kumsal arazisi kalıp olarak kurulur (gri kutunun yerine), nesneler Ground'a oturtulur, kıyı kıvrımı araziden ölçülüp kıyıdan uzaklık oradan hesaplanır | Kurulum komutları sahneyi baştan kurduğu için arazi sahnede elle değil kalıpta durmalı; kumsalın oyun düzeni (kıyı z≈0, kara +z) değişmeden görüntü değişsin | Umut |
| 2026-11-01 | `Management/` ve `UI/Management/` Umut'a geçti; insan kısmını Umut yazar, Toprak dengeler | Toprak'ın onayıyla; oyun bir an önce uçtan uca oynansın | Umut |
| 2026-11-01 | Faz 0 kararları: kumsal 8 bölge, katmanlar çöp/ışık/şezlong/insan (0–3); insan turu ekranı kumsal sahnesinde tepeden kamera, kumsalın kendisi harita; kayıt her adımın başında | Var olan kumsal ve kayıt düzeniyle aynı; ayrı bir harita ekranı çizmeye gerek yok | Umut |
| 2026-11-01 | Kâğıt prototip (Faz 1) atlandı; sayılar GDD'den `YonetimAyarlari`'na, denge oyunda yapılır | Kullanıcı kararı: önce oynanır sürüm | Umut |
| 2026-11-01 | `BirHaftaGecti` insan turunun sonunda değil başında çağrılır | "O tur" süren etkiler (devriye, gölge ve sulama, festival, sıcak dalgası) ardından gelen gecede de sürsün; sonda çağrılınca gece gelmeden geri alınıyordu | Umut |
| 2026-11-01 | Kayda giren ek veri: `Core/IOyunVerisi` (YeniOyun, Yaz, Oku); OyunAkisi'nin `ekVeriler` listesi; insan kısmının `YonetimDurumu`'su böyle kaydedilir | Core insan kısmını tanımadan kaydedebilsin | Umut |
| 2026-11-01 | Tur sonu simülasyonu: bölge başına %30 çöp +1; kafessiz ve çıkmamış yuvaya %20 avcı (Tilki Çiti ile %10), yumurtanın %30'u gider. Temiz Enerji varken sıcak dalgası %50 ihtimalle kumu ısıtmaz | GDD sayı vermiyor; başlangıç değerleri, `YonetimAyarlari`'nda dengelenir | Umut |
| 2026-11-01 | Metinleri aktar ek sütunları da alır: Açıklama → `id_aciklama`, Not → `id_not`, Seçenek A/B → `id_a`, `id_b` | metinler.md'nin var olan tabloları yeni satır açmadan kullanılsın | Umut |
| 2026-11-01 | İpuçları ilk karşılaşmada bir kez, ekranın altında; hangi ipucunun gösterildiği bu bilgisayarda saklanır, Yeni oyun sıfırlar | Öğretici bölüm yok; oyuncu öğrenirken durdurulmasın, tekrar oynayan bıkmasın | Umut |
| 2026-11-01 | Ses tek kapıdan: `Ses.Cal(SesTuru)`; klipler ve adım başına ortam sesi/müzik `SesAyarlari` asset'inde, yeni ses türü enum'un sonuna eklenir | Kod klibe bağlı kalmaz; Abdü sesleri kod açmadan yerleştirir, klip yoksa oyun sessiz ama bozulmaz | Umut |
| 2026-11-01 | Öncü yavru da ağa, halkaya ve tekerlek izine takılır; E'yi basılı tutup kendini kurtarır (yönünü yine kaybetmez). Önceki "öncü takılmaz" kararının yerine | Öncü hiç takılmayınca çöp sadece arkadakiler için tehlikeydi; kendini kurtarmak da oyuncunun elinde | Umut |
| 2026-11-01 | Tilki geldiği gece bir süre gezdikten sonra sonraki gecelerin kafessiz bir yuvasını eşeler: yumurtaların yarısı gider, yuvada Avcı izi kalır. Bozmayı insan turunun simülasyonu da ayrıca yapabilir | GDD'deki "tilkinin bozduğu yuva" kanıtı ve yuva kafesinin anlamı kaplumbağa tarafında da görünsün | Umut |
| 2026-10-31 | Kontrol noktasına girince enerji dolar (anne ve yavru) | GDD yavru için söylüyor; anne için de bir sonraki bölgeye temiz başlangıç | Umut |
| 2026-10-31 | Kumsalın ilk hali `KumsalBaslangici` asset'inde (bölge bölge çöp, ışık, şezlong, insan, araç izi; denizdeki ağ); `KumsalDurumu.Sifirla(baslangic, katmanEnFazla)` | Kartlar (Temizlik, Şezlong Düzenlemesi, Işık Yasağı, Araç Yasağı) bir şeyi azaltabilsin diye kumsal dolu başlamalı; ilk hal sahnede değil, durumda | Umut |
| 2026-10-31 | `KumsalDurumu`'na `IsikYasagi` ve `IsikYasagiAyarla(var)` | Işık Yasağı kartı otel ve yol ışıklarını kapatır; telefon feneri yasağa uymaz | Umut |
| 2026-10-31 | Kaçışta öncü yavru takılmaz ve yönünü kaybetmez; kurtaran odur | Oyuncu öncüyü sürer; öncü takılırsa kurtaracak kimse kalmaz | Umut |
| 2026-10-31 | Martı: dalış alanında 3 ya da daha çok yavru varsa 3, yoksa 1 yavru alır | GDD'deki "toplu sürüden 3, dağınıktan 1" kuralını sürünün o anki yayılmasından ölçmek | Umut |
| 2026-10-31 | Gecenin sonuç paneli şimdilik yazısız (sayı ve renk) | Oyuncu metni önce `docs/metinler.md`'ye girer; Yusuf'un listesi gelene kadar | Umut |
| 2026-10-31 | GDD'deki kamera satırı birinci şahıs olarak güncellendi | 2026-10-30 kamera kararı (hayvan kısımları birinci şahıs, insan kısmı yönetim ekranı) | Umut |
| 2026-10-31 | `KumsalDurumu`'na `KumIsinmasi` ve `KumIsinmasiDegistir(fark)`: bütün kum kaç bant ısındı | GDD'deki sıcak dalgası olayı ("gelecek tur bütün kum 1 bant ısınacak") kumsala bu yolla yansır | Umut |
| 2026-10-30 | `KumsalDurumu` taslağı: ScriptableObject asset'i; bölge katmanları (çöp, ışık, şezlong, insan), bölge yapıları (gölgelik, kumul onarımı, tilki çiti, araç izi), yuvalar, kayıp izleri, denizdeki ağ, denize ulaşan yavru; açık metotlar `docs/DURUM.md`'de. Bütçe, gönüllü ve halk desteği insan kısmında kalır | Sahneler arasında yaşar, inspector'da görünür, iki taraf aynı asset'e bağlanır. Toprak'ın onayıyla kesinleşir | Umut |
| 2026-10-30 | Kamera: kaplumbağa bölümleri (kara ve deniz, anne ve yavru) birinci şahıs; insan kısmı yönetim ekranı | Faz 0 kamera kararı. GDD'deki "üstten ve hafif arkadan takip" satırı buna göre güncellenecek | Umut |
| 2026-10-30 | Repo düzeni Anadolu'yla aynı: Unity projesi reponun kökünde; `Assets/_Project/{Scripts,ScriptableObjects,Scenes,Prefabs,Art,Models,Materials,Animations,Audio,Shaders}`, `Assets/Sandbox/<ad>`, `Assets/ThirdParty/<yayıncı>`, `Tools/`, `docs/DURUM.md` | İki projede aynı alışkanlık; Claude Code ve ekip aynı yerleri bilir | Umut |
| 2026-10-30 | Script klasörleri İngilizce: Core, Camera, Turtle, Hazards, Management, UI, Editor | Anadolu ve Unity alışkanlığı | Umut |
| 2026-10-30 | Büyük dosyalar Git LFS ile (Anadolu'daki gibi) | Görsel, ses ve modeller geçmişi şişirmesin | Umut |
| 2026-10-30 | MCP for Unity paketi | Claude Code editörü sürebilsin (sahne kurulumu, derleme kontrolü) | Umut |
| 2026-10-30 | Oyun tek anne kaplumbağayla başlar, yavrusuyla biter; 5 bölüm, ~2 saat | Tek bir ailenin hikâyesi; sezonda birkaç yuva gerçeği bölümleri doğal çoğaltır | Umut |
| 2026-10-30 | Kaplumbağa kısmında çöp, avcı ve ışık süre veren ana engeller; sıcak kum, fırtına ve ısınan deniz de kalır | Oynanış çeşitliliği ve iklim teması birlikte | Umut |
| 2026-10-30 | İnsan kısmı tur bazlı: gönüllü atama + kart + olay; tek ortak durum `KumsalDurumu` | İki taraf aynı veriden beslenir, sonuç sahnede görünür | Umut |
| 2026-10-30 | main korumalı, PR zorunlu, onay zorunlu değil | 4 kişi için hız; herkes kendi alanını birleştirir | Umut |
| 2026-10-30 | Oyun sahneleri (`Scenes/Levels`) sadece Umut'ta; herkes `Assets/Sandbox/<ad>` | Unity sahneleri birleştirilemez | Umut |
| 2026-10-30 | Unity 6000.3.25f1, Universal 3D | Ekipte herkes aynı sürüm | Umut |
