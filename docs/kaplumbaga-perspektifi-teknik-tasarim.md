# İki Dünya, Tek Yaşam
## Kaplumbağa Perspektifi – Unity Teknik Tasarım ve Değişiklik Planı

**Kararlar**
- Kamera: Birinci şahıs (First Person)
- Görsel yaklaşım: Sinematik gerçekçilik
- Hareket: Doğal / biyolojik
- Canlı: Caretta caretta
- Kapsam: Anne kaplumbağanın deniz ve kara bölümleri ile yavrunun deniz ve kara bölümleri

**Proje tabanı:** Unity 6 (`6000.3.25f1`), URP, Input System 1.20.0

---

## 1. Tasarım hedefi

Oyuncu, insan biçiminde hareket eden bir karakteri değil, bir Caretta caretta'nın çevresinde ilerleyen canlı bir bedeni kontrol ettiğini hissetmelidir. Gerçekçilik; kamerayı bulanıklaştırmak veya ekrana sürekli efekt koymakla değil, hareketin, çevresel direncin, duyusal ipuçlarının ve animasyonun tutarlı çalışmasıyla sağlanacaktır.

Sinematik anlatım, biyolojik gerçekliği bozmayacaktır. Örneğin otomobilin gerçek bir canavara dönüşmesi yerine, yaklaşan gölgesi, motor titreşimi, büyüyen silueti ve ses tasarımı oyuncuda tehdit hissi oluşturacaktır.

### Başarı ölçütleri
1. Deniz hareketi anında tam hıza çıkmamalı; ivmelenmeli ve bırakıldığında kontrollü biçimde süzülmelidir.
2. Yüzgeç animasyonu hareket hızı ve yönüyle senkronize olmalıdır.
3. Kamera göz hizasında sabit bir insan kamerası gibi durmamalı; düşük genlikli, biyolojik baş/gövde hareketi göstermelidir.
4. Su altı ve su üstü geçişleri görsel ve işitsel olarak hissedilmelidir.
5. Kara hareketi deniz hareketinden belirgin biçimde farklı, ağır ve sürtünmeli olmalıdır.
6. İnsan bölümündeki çevre kararları, sonraki kaplumbağa bölümünde aynı çevre parçalarını görünür biçimde değiştirmelidir.
7. Oyuncu için gerekli kontroller erişilebilir kalmalı; gerçekçilik gereksiz kontrol zorluğuna dönüşmemelidir.

---

## 2. Mevcut Unity projesinin tespiti

Projede birinci şahıs yaklaşımının temeli zaten bulunuyor. Bu nedenle mevcut sistemler silinip sıfırdan yazılmamalı; küçük, kontrollü adımlarla genişletilmelidir.

| Mevcut dosya | Bugünkü rolü | Planlanan değişiklik |
|---|---|---|
| `Assets/_Project/Scripts/Camera/BirinciSahisKamera.cs` | Birinci şahıs bakış, gövdeyi gizleme, sallanma, FOV | Göz pivotu, düşük frekanslı biyolojik kamera hareketi, kara/deniz kamera profilleri |
| `Assets/_Project/Scripts/Camera/SuAltiGorunumu.cs` | Su seviyesine göre sis rengi/yoğunluğu | Su geçişi, renk soğurumu, partikül/tortu ve post-process entegrasyonu |
| `Assets/_Project/Scripts/Camera/KameraAyarlari.cs` | Hassasiyet ve bakış sınırları | Profil bazlı kamera ayarları |
| `Assets/_Project/Scripts/Turtle/DenizHareketi.cs` | `CharacterController` ile yüzme, dalma, nefes, akıntı, tehlike | İvme, sürüklenme, pasif süzülme, dikey denge, dönüş hızı |
| `Assets/_Project/Scripts/Turtle/DenizAyarlari.cs` | Yüzme/enerji/nefes parametreleri | Yeni yüzme fiziği parametreleri |
| `Assets/_Project/Scripts/Turtle/AnneKara.cs` | Karada ileri/geri hareket | Yüzgeç itişi, ağır ivmelenme, zemin sürtünmesi ve gövde salınımı |
| `Assets/_Project/Scripts/Turtle/AnneAyarlari.cs` | Kara hareketi ve yuva ayarları | Kara hareket fiziği parametreleri |
| `Assets/_Project/Scripts/Turtle/Yavru.cs` | Yavru durumları ve tehlikeler | Yavruya özel daha kısa adım/yüzgeç döngüsü ve panik tepkisi |
| `Assets/_Project/Scripts/Turtle/AnimasyonAyarlari.cs` ve `Salinim.cs` | Kodla salınım | Gerçek yüzgeç animasyonlarıyla birlikte çalışan hız/yön tabanlı animasyon |
| `Assets/_Project/Scripts/Core/GameInput.cs` | Ortak hareket/bakış girdileri | Tuş eşlemesi korunacak; hareket sistemi girdiyi doğrudan hız olarak değil hedef yön olarak kullanacak |

### Mevcut sınırlamalar
- `DenizHareketi` içinde hareket vektörü doğrudan gözün `forward/right` yönlerinden hesaplanıp `CharacterController.Move` ile uygulanıyor. Bu, anlık ve mekanik bir kontrol hissi verebilir.
- `BirinciSahisKamera` yatay bakışta gövdeyi, dikey bakışta kamerayı döndürüyor. Bu temel korunabilir; ancak baş/göz hareketi ve kamera profilleri ayrıştırılmalı.
- `SuAltiGorunumu` yalnızca sis rengi/yoğunluğu değiştiriyor. Tek başına sinematik su altı hissi için yeterli değil.
- `AnneKara` ileri/geri hareketi doğrudan hızla uyguluyor. Yüzgeç itişi ve zemin sürtünmesi hissi bulunmuyor.
- Projede `AnimasyonAyarlari` ve `Salinim` altyapısı var; ancak bunlar tek başına ayrıntılı yüzgeç animasyonu yerine geçmez.
- `BirinciSahisKamera` gövde render'larını `ShadowsOnly` yapıyor. Bu nedenle oyuncu birinci şahısta kendi kabuğunu/yüzgeçlerini görmeyecek. Bu bilinçli olabilir; ancak kamera alt kenarında çok az yüzgeç/kabuk ipucu göstermek için ayrı bir first-person görünüm modeli düşünülmeli.
- `Assets/_Project/Scenes/Levels` içindeki deniz ve kumsal sahnelerinde kamera bileşenleri kullanılıyor. Değişiklikler tüm ilgili sahnelere uygulanmalı; yalnızca tek sahnede bırakılmamalı.

---

## 3. Önerilen yeni bileşen mimarisi

Yeni sistemi tek bir büyük script içinde toplamayın. Mevcut sınıfların sorumluluklarını koruyup aşağıdaki bileşenleri ekleyin.

### 3.1 Yeni scriptler

| Yeni dosya | Sorumluluk |
|---|---|
| `Turtle/SwimmingMotor.cs` | İvme, hız sınırı, sürüklenme, süzülme ve dikey denge |
| `Turtle/FlipperAnimator.cs` | Hız/yön girdisine göre dört yüzgecin animasyon parametrelerini sürmek |
| `Camera/TurtleCameraMotion.cs` | Çok hafif baş/gövde salınımı ve kamera sarsıntısı |
| `Camera/EnvironmentCameraProfile.cs` | Kara, su altı ve su üstü kamera profili |
| `Camera/WaterTransitionEffect.cs` | Su yüzeyini geçerken kısa görsel/işitsel geçiş |
| `Perception/TurtlePerception.cs` | Tehdit ve çevresel ipuçlarının oyuncuya sunulması |
| `Perception/PerceptionCue.cs` | Ses, gölge, görüş ve titreşim ipuçları için veri tanımı |
| `Audio/TurtleAudioController.cs` | Su altı, su üstü, nefes, yüzgeç ve çevresel ses katmanları |

`Perception` ve `Audio` klasörleri mevcut klasör düzenine uygun biçimde eklenebilir. İlk prototipte tüm yeni scriptleri yazmak zorunlu değil; önce `SwimmingMotor`, `FlipperAnimator` ve kamera profili uygulanmalıdır.

### 3.2 Bileşen sorumlulukları

- `DenizHareketi`: Oyun kuralları, nefes, enerji, tuzak, besin ve tehlike olayları.
- `SwimmingMotor`: Fiziksel hareket hesabı.
- `BirinciSahisKamera`: Fare/kol bakışı ve gövde yönü.
- `TurtleCameraMotion`: Bakışa eklenen küçük biyolojik hareket.
- `FlipperAnimator`: Hareket hızından türetilen animasyon.
- `TurtlePerception`: Tehlikeyi doğrudan HUD işaretiyle değil, sahne içi ipuçlarıyla hissettirme.
- `SuAltiGorunumu`: Ortamın su altı/su üstü görsel geçişi.
- `TurtleAudioController`: Aynı geçişin ses katmanları.

---

## 4. Deniz hareket sistemi

### 4.1 Kontrol felsefesi

Klavye/kol girdisi doğrudan hız olmamalı; oyuncunun istediği hareket yönünü belirtmelidir. Motor bu hedefe zaman içinde yaklaşmalıdır.

- W/S: ileri/geri yüzme isteği.
- A/D: yana hareket isteği; ileri harekete göre daha düşük verim/hız.
- Fare/sağ analog: başın bakış yönü; gövde yönü yumuşakça bu yöne uyum sağlar.
- Space: mevcut tasarımdaki dalış/yüzeye çıkış davranışı korunur; dikey hareket ani değil, ivmeli olmalıdır.
- Shift: kısa süreli güçlü yüzgeç vuruşu/sprint; enerji maliyeti yüksek olmalıdır.
- Tuş bırakıldığında: karakter anında durmamalı; bir miktar süzülmeli, sonra su direnciyle yavaşlamalıdır.

### 4.2 Önerilen hareket modeli

`CharacterController` ilk aşamada korunmalıdır. Projenin tehlike alanları, kontrol noktaları, trigger etkileşimleri ve `IYuzucu` sözleşmesi bu yapıyla çalışıyor. İlk prototipte Rigidbody'ye geçmek gereksiz kapsam ve regresyon riski yaratır.

Motorun her karede hesaplayacağı değerler:

1. Girdi vektörünü normalize et.
2. Kamera/göz yönünden hedef yüzme yönünü oluştur.
3. İleri, yana ve dikey hız hedeflerini ayrı değerlendir.
4. Mevcut hızı hedef hıza doğru `MoveTowards` ile yaklaştır.
5. Girdi bırakıldığında su direncine göre hızı azalt.
6. Akıntı hızını ayrı bir çevresel kuvvet gibi ekle.
7. Su yüzeyini geçmeyi engelleyen mevcut sınır mantığını koru.
8. `CharacterController.Move` çağrısını tek bir noktada yap.

Örnek başlangıç parametreleri (oyun testiyle ayarlanacak; biyolojik ölçüm iddiası değildir):

| Parametre | İlk test aralığı |
|---|---:|
| İleri azami hız | 2.0–3.0 m/sn |
| Geri hız çarpanı | 0.35–0.55 |
| Yana hız çarpanı | 0.45–0.65 |
| İvmelenme | 2.0–3.5 m/sn² |
| Girdi bırakınca yavaşlama | 0.6–1.2 m/sn² |
| Gövde dönüş hızı | 70–110 derece/sn |
| Dikey denge kuvveti | Sahne ölçeğine göre test |
| Sprint çarpanı | 1.25–1.5 |

Bu değerler gerçek Caretta yüzme hızlarının birebir simülasyonu değil, oynanış için başlangıç aralığıdır.

### 4.3 Pseudocode

```csharp
Vector3 input = Vector3.ClampMagnitude(
    new Vector3(GameInput.Hareket.x, verticalInput, GameInput.Hareket.y), 1f);

Vector3 desiredDirection =
    (goz.forward * input.z + goz.right * input.x + Vector3.up * input.y).normalized;

Vector3 desiredVelocity = CalculateTargetVelocity(desiredDirection);
velocity = Vector3.MoveTowards(
    velocity,
    desiredVelocity,
    acceleration * Time.deltaTime);

if (input.sqrMagnitude < 0.01f)
    velocity = Vector3.MoveTowards(
        velocity, Vector3.zero, waterDrag * Time.deltaTime);

Vector3 finalVelocity = velocity + currentVelocity;
controller.Move(finalVelocity * Time.deltaTime);
```

Bu örnek doğrudan mevcut scriptin yerine yapıştırılacak tamamlanmış kod değildir. `Nefes`, `Enerji`, `Takili`, `Yavaslik`, `Akinti`, `IsikKaynagi`, `TrashTrap`, `Yakala` ve kontrol noktası davranışları mevcut `DenizHareketi` içinde korunarak entegre edilmelidir.

### 4.4 Enerji ve nefes

- Enerji, yalnızca hareket mesafesine değil, yüzgeç vuruşu yoğunluğuna göre harcanmalı.
- Pasif süzülme, sürekli güçlü yüzmeye göre daha az enerji tüketmeli.
- Akıntıya karşı yüzmek daha maliyetli olmalı.
- Nefes göstergesi küçük ve sade kalmalı; oyuncuyu sürekli sayısal HUD'a boğmamalı.
- Yüzeye çıkıldığında su altı sesleri boğuklaşmalı, hava sesi ve nefes alma duyulmalı.
- Nefessiz kalınca hareket ve ses değişmeli; ekranı tamamen karartmak gibi aşırı efekt kullanılmamalı.

---

## 5. Yüzgeç ve gövde animasyonu

### 5.1 Animasyon yaklaşımı

Dört yüzgeç aynı anda aynı açıyla sallanmamalıdır. Ön yüzgeçler ana itişi üretirken arka yüzgeçler yön ve dengeye katkı vermelidir. Animasyon, hareket motorunun gerçek hızına bağlanmalıdır.

Önerilen durumlar:

- `IdleFloat`: Su içinde neredeyse hareketsiz süzülme; çok hafif yüzgeç düzeltmeleri.
- `Cruise`: Düzenli ve orta frekanslı yüzgeç vuruşu.
- `FastSwim`: Daha güçlü ve sık ön yüzgeç vuruşu.
- `TurnLeft/TurnRight`: İç ve dış yüzgeçte farklı genlik.
- `Dive`: Ön yüzgeç ve gövde eğimiyle dalış.
- `Surface`: Yüzeye yaklaşırken hareketin yumuşaması.
- `Struggle`: Ağa takılınca düzensiz ve sınırlı yüzgeç hareketi.
- `Exhausted`: Daha küçük hareket genliği ve uzun toparlanma aralığı.

### 5.2 Model gereksinimi

Animasyon ekibinden istenecek model:
- Gövde/kabuk,
- Baş ve boyun,
- Sol/sağ ön yüzgeç,
- Sol/sağ arka yüzgeç,
- Kuyruk.

Model rig'li değilse Blender'da kemik sistemi kurulmalı veya yüzgeçler ayrı mesh/transform olarak hazırlanmalıdır. Yalnızca tüm modeli sallayan `Salinim` sistemi gerçek yüzgeç hareketi için yeterli değildir.

Birinci şahıs kamera nedeniyle oyuncunun kendi modeli `ShadowsOnly` yapılmaya devam edebilir. Ancak kameranın alt kenarında çok az görünen, yalnızca birinci şahıs için hazırlanmış ayrı bir ön yüzgeç/kabuk parçası kullanılabilir. Bu parça ana modelle çakışmamalı ve gölge üretmemelidir.

---

## 6. Kamera ve sinematik gerçekçilik

### 6.1 Kamera hiyerarşisi

Önerilen Unity hiyerarşisi:

```text
TurtleRoot
├── VisualRoot
│   ├── Shell
│   ├── Head
│   ├── FrontFlipper_L
│   ├── FrontFlipper_R
│   ├── RearFlipper_L
│   └── RearFlipper_R
├── CameraYawPivot
│   └── CameraPitchPivot
│       └── Main Camera
└── SwimMotor
```

- `TurtleRoot`: Fiziksel gövde ve hareket.
- `CameraYawPivot`: Yatay bakış/gövde yönü.
- `CameraPitchPivot`: Dikey bakış.
- `Main Camera`: Göz hizasında.
- Kamera, başın içine veya kabuğun merkezine değil, gözlerin arasındaki gerçekçi noktaya yerleştirilmeli.

Mevcut sahne hiyerarşisi farklıysa doğrudan yeniden kurmak yerine önce mevcut referanslar ve prefab bağlantıları kontrol edilmelidir.

### 6.2 Kamera davranışı

- Kamera salınımı çok düşük genlikte olmalı.
- Yüzme sırasında dikey bobbing sınırlı olmalı; denizde insan yürüyüşü head-bob'u kullanılmamalı.
- Hızlanınca FOV artışı varsayılan olarak kapatılmalı veya çok az tutulmalı.
- Dönüşlerde kamera gövdeden bağımsız, aşırı serbest hareket etmemeli.
- Darbe sarsıntısı kısa ve kontrollü olmalı.
- Oyuncunun görüşü sürekli bulanıklaştırılmamalı.
- Su altı görüşünde mavi/yeşil renk soğurumu, uzak mesafede kontrast kaybı ve parçacıklar kullanılmalı.

### 6.3 Su altı ve su üstü geçişi

`SuAltiGorunumu.cs` şu anda su seviyesine göre sis rengini ve yoğunluğunu değiştiriyor. Buna aşamalı olarak şunlar eklenmeli:

1. Kamera suya girerken kısa ses filtresi geçişi.
2. Su yüzeyinden geçerken kısa kabarcık/ışık kırılması efekti.
3. Su altında uzaktaki objelerde kontrast ve renk doygunluğunun azalması.
4. Deniz tabanına yakınken çok hafif kum/tortu partikülleri.
5. Yüzeye yakınken dalga altı ışık desenleri.
6. Su üstünde rüzgâr, dalga ve kıyı seslerinin daha net duyulması.

Efektler URP Volume üzerinden yönetilmeli. `RenderSettings` ve global Volume ayarları sahneler arası çakışmayacak biçimde tek bir ortam yöneticisi tarafından kontrol edilmeli.

---

## 7. Kaplumbağanın duyusal perspektifi

Oyuncuya insan HUD'ı üzerinden “tehlike burada” demek yerine çevrenin kendisi tehlikeyi anlatmalı.

| Tehdit | Oyuncuya verilecek ipucu | Kaçınılması gereken |
|---|---|---|
| Tekne | Düşük frekanslı motor sesi, yaklaşan gölge, su hareketi | Ekranda kırmızı hedef işareti |
| Hayalet ağ | Görüşte ince ağ iplikleri, takılma riski, hareketin kısıtlanması | Ağın parlayan oyun objesi olması |
| Plastik poşet | Akıntıyla sürüklenen, ışığı yansıtan yarı saydam şekil | Poşetin otomatik olarak yiyecek gibi işaretlenmesi |
| Avcı balık | Çevresel hareket, siluet ve yaklaşma sesi | Duvar arkası düşman göstergesi |
| Kıyı aracı | Kum titreşimi, motor sesi, büyüyen gölge | Aracın gerçek bir canavara dönüşmesi |
| Yapay ışık | Gece görüşünde parlama ve yön duygusunu bozan çevresel kontrast | Termal/sonar gibi gerçek dışı özel görüş |

Not: Caretta caretta'nın duyuları insan duyularıyla aynı değildir. Her duyusal efekt bilimsel dayanakla ve ölçülü kullanılmalı. Özellikle sonar/radar benzeri bir algı modu eklenmemeli. Oyunun mevcut termal görünümü gerçek kaplumbağa görüşü olarak sunulmamalı; bu özellik yalnızca oyun içi özel bir mekanikse açıkça ayrıştırılmalı.

---

## 8. Kara hareketi

Denizdeki hareket kodu karaya taşınmamalıdır. `AnneKara.cs` ayrı bir kara motoruna dönüştürülmeli.

- İleri hareket yüzgeçlerin kuma basmasıyla ivmelenmeli.
- Geri hareket çok daha yavaş olmalı.
- Dönüşler ağır olmalı; gövde yönü bir anda değişmemeli.
- Kum üzerinde sürtünme ve küçük gövde salınımı olmalı.
- Yokuş ve yumuşak kumda hareket maliyeti artabilir.
- Yuva kazma sırasında hareket kilidi korunmalı.
- Yumurta bırakma ve yuva örtme animasyonları hareket motoruyla çakışmamalı.

Karada kamera su altı profiline geçmemeli; yalnızca kara profili ve uygun ses ortamı kullanılmalı.

---

## 9. İnsan kararlarının kaplumbağa sahnesine etkisi

Bu sistem oyunun ana farklılaştırıcısıdır. İnsan yönetimi yalnızca sayısal bir puan değiştirmemeli.

Örnek bağlantılar:
- `kart_ag_temizligi`: İlgili bölgedeki hayalet ağ prefabı kaldırılır veya güvenli dekoratif duruma geçer.
- `kart_aydinlatma` / ışık azaltma: Gece sahnesindeki ışık kaynaklarının yoğunluğu azalır.
- `kart_temizlik`: Çöp ve plastik yoğunluğu azalır.
- `kart_golgelik`: Yuva bölgesinin gölge ve sıcaklık durumu değişir.
- `kart_koruma_alani`: İnsan/araç tehlikeleri belirli bölgeye giremez.

Bu etkiler mevcut `KumsalDurumu` ve yönetim durumuyla veri üzerinden bağlanmalı. Sahne nesneleri kart scriptlerinden doğrudan kontrol edilmemeli; bölüm başında ortak çevre durumunu okuyup kendi görsel durumunu kurmalıdır.

---

## 10. Uygulama sırası

### Faz 0 – Güvenli başlangıç
1. Yeni dal aç: `feature/turtle-perspective`.
2. `Bolum1_Donus`, `Bolum5_IlkYuzus`, `Kumsal` sahnelerinin yedeğini al.
3. Mevcut hareket ve kamera davranışını kısa video ile kaydet.
4. Inspector'daki ScriptableObject değerlerini dışa not et.
5. Sahne ve prefab referanslarını değiştirmeden önce meta dosyalarını koru.

### Faz 1 – Deniz prototipi
1. `SwimmingMotor` ekle.
2. `DenizHareketi` içindeki hareket hesabını motora devret.
3. Nefes, enerji, akıntı, tuzak, yiyecek ve checkpoint olaylarını koru.
4. `FlipperAnimator` ile hız/ivme animasyonunu bağla.
5. İlk test yalnızca `Bolum1_Donus` sahnesinde yapılmalı.

**Faz 1 tamamlanma koşulu:** Oyuncu hızlanmayı, süzülmeyi, yavaşlamayı ve dönüş ağırlığını hissedebilmeli; mevcut nefes ve tuzak mekanikleri bozulmamalı.

### Faz 2 – Kamera ve atmosfer
1. Kamera pivotlarını doğrula.
2. Kara/deniz kamera profilleri ekle.
3. Su geçiş efektlerini ekle.
4. Ses katmanlarını ekle.
5. Aşırı motion blur, vignette ve kamera sallantısından kaçın.

### Faz 3 – Kara hareketi
1. `AnneKara` için ivmelenme ve sürtünme.
2. Yüzgeç itiş animasyonu.
3. Kum/zemin etkileşimi.
4. Kazma ve yumurtlama akışlarının regresyon testi.

### Faz 4 – Yavru ve algı
1. Yavru için daha kısa/çevik hareket profili.
2. Sürüdeki yavruların animasyon ve panik davranışları.
3. Tehditlerin ses/gölge/çevre ipuçları.
4. Oyuncuya yalnızca gerekli minimum HUD.

### Faz 5 – İnsan-karakter çevre bağlantısı
1. Kart etkilerini ortak çevre verisine bağla.
2. Sonraki kaplumbağa sahnesinde çevresel değişiklikleri göster.
3. Aynı bölgeyi karar öncesi/sonrası karşılaştırmalı test et.

---

## 11. Test senaryoları ve kabul kriterleri

| Test | Beklenen sonuç |
|---|---|
| W tuşuna basma | Kaplumbağa anında tam hıza sıçramaz; ivmelenir |
| W bırakma | Bir miktar süzülür, su direnciyle yavaşlar |
| A/D ile dönüş | Gövde ve kamera yumuşak tepki verir |
| Space ile dalış | Dikey hareket kontrollü ve yumuşaktır |
| Su yüzeyini geçme | Görüntü ve ses katmanları doğru değişir |
| Akıntıya girme | Hareket yönü ve hızı etkilenir |
| Ağ tuzağına takılma | Hareket kısıtlanır, kurtulma sistemi çalışır |
| Nefesin bitmesi | Mevcut nefes/enerji kuralı korunur |
| Kontrol noktasında yeniden doğma | Motor hızı ve kamera salınımı sıfırlanır |
| İnsan bölümünde ağ temizleme | Sonraki ilgili deniz sahnesinde ağ görünmez/etkisiz olur |
| Kara sahnesinde hareket | Deniz fiziği veya su efektleri devreye girmez |
| Pause ve resume | İmleç, kamera ve hareket girdileri doğru kilitlenir |

Ek olarak düşük ve yüksek FPS koşullarında hareket aynı hissedilmeli; fizik ve animasyon `Time.deltaTime` ile ölçeklenmelidir.

---

## 12. Ekip görev dağılımı

| Rol | Görev |
|---|---|
| Unity gameplay geliştirici | `SwimmingMotor`, `DenizHareketi` entegrasyonu, kara motoru |
| Teknik animasyon / karakter sanatçısı | Rig, yüzgeç kemikleri, yüzme ve kara animasyonları |
| Çevre sanatçısı | Su altı partikülleri, ışık, tortu, kıyı atmosferi |
| Ses tasarımcısı | Su altı filtresi, motor titreşimi, yüzgeç, nefes ve çevre sesleri |
| Oyun tasarımcısı | Hareket hissi, enerji dengesi, tehdit okunabilirliği ve test kayıtları |

Sahne dosyalarına aynı anda birden fazla kişinin doğrudan müdahale etmesi önlenmeli. Yeni sistemler önce kişisel sandbox sahnesinde test edilip prefab veya kontrollü PR ile ana sahneye alınmalı.

---

## 13. Riskler

1. **Birinci şahısta beden hissinin kaybolması:** Oyuncu kaplumbağa olduğunu hissetmeyebilir. Çözüm: yüzgeç sesleri, gövde ivmesi, düşük genlikli kamera hareketi ve gerektiğinde alt görüşte yüzgeç ipucu.
2. **Aşırı kamera efekti:** Baş dönmesi ve rahatsızlık oluşturabilir. Çözüm: tüm kamera hareketleri ayarlanabilir ve düşük genlikli olmalı.
3. **Hareket fiziğinin mevcut görevleri bozması:** Çözüm: önce yalnızca deniz bölümünde prototip, ardından checkpoint/nefes/ağ testleri.
4. **Bilimsel gerçeklikten uzaklaşma:** Çözüm: gerçek biyolojik özelliklerle sinematik sunum ayrı belgelenmeli.
5. **Sahne tutarsızlığı:** Birden çok deniz sahnesinde farklı inspector değerleri olabilir. Çözüm: ortak ScriptableObject profilleri ve sahne kontrol listesi.
6. **Rigidbody'ye erken geçiş:** Mevcut `CharacterController` ve trigger altyapısını bozabilir. Çözüm: ilk sürümde `CharacterController` korunmalı; Rigidbody yalnızca prototip karşılaştırması başarılı olursa değerlendirilmelidir.

---

## 14. İlk sprint için net görev listesi

- [ ] `feature/turtle-perspective` dalı aç.
- [ ] `Bolum1_Donus` için hareket referans videosu al.
- [ ] `SwimmingMotor.cs` oluştur.
- [ ] `DenizHareketi.cs` hareket hesabını motora taşı.
- [ ] Hızlanma / süzülme / dönüş değerlerini ScriptableObject'e ekle.
- [ ] Kontrol noktası yeniden başlatmasında motor hızını sıfırla.
- [ ] `FlipperAnimator.cs` prototipi oluştur.
- [ ] Dört yüzgeç için animasyon rig'i hazırla.
- [ ] Aynı sahnede klavye ve gamepad testi yap.
- [ ] Nefes, ağ, akıntı ve enerji regresyon testlerini tamamla.
- [ ] Ekipten en az 3 kişiyle kısa oynanış testi yap.

**İlk sprintin dışında bırakılacaklar:** Yeni hayvan türleri, yeni kartlar, yeni bölüm tasarımı, gelişmiş sonar/termal algı, Rigidbody'ye tam geçiş ve kapsamlı post-processing. Önce temel kaplumbağa hissi doğrulanmalıdır.

---

## Son karar

Bu projede hedeflenen deneyim; birinci şahıs kamera, sinematik çevre sunumu ve doğal biyolojik hareketin birleşimidir. Mevcut birinci şahıs kamera ve deniz hareket sistemi korunarak geliştirilecek; ilk oynanabilir prototip `Bolum1_Donus` sahnesinde hazırlanacaktır.

**Öncelik sırası:** Önce hareket hissi, sonra animasyon, sonra kamera/atmosfer, en son duyusal ipuçları ve insan kararlarının çevreye etkisi.
