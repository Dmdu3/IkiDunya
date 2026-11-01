# Biyoloji ve termal yönlendirme — inceleme notu

Bu belge ekip içindir. Kaynak bağlantıları oyuncu arayüzüne konmaz. Oyuncuya gösterilecek yeni veya değişen metinler için metin sorumlusu Yusuf'un `docs/metinler.md` dosyasına işlemesi ve metin tablosuna aktarması gerekir. Aşağıdakiler mevcut mekanikleri değiştirmeden hazırlanmış önerilerdir.

## Önerilen kısa metinler

| Kullanım / kimlik | Öneri |
| --- | --- |
| `ipucu_termal` | T ile kumun sıcaklık bantlarını gör. Serin bant daha çok erkek, sıcak bant daha çok dişi yavruyla ilişkilidir. Kırmızı bantta daha az yumurta yaşar; dalga çizgisine de dikkat et. |
| Yeni bilgi: sıcaklık | Yumurta gelişimi sırasında daha serin koşullar genellikle erkek, daha sıcak koşullar ise dişi yavru oranını artırır. Aşırı sıcaklık yumurtaların yaşamını tehdit edebilir. Doğada yuva sıcaklığı zamanla ve yuva içinde değişir. |
| Yeni bilgi: kuluçka | Caretta caretta yumurtaları genellikle yaklaşık iki ayda açılır. Süre sıcaklık ve diğer çevre koşullarına göre değişebilir. |
| Yeni bilgi: geri dönüş | Yetişkin dişi carettalar yuvalamak için doğdukları bölgenin kıyılarına geri dönebilir. Yıllar önce başlayan yolculuk, yeni bir yuvayla devam eder. |
| Yeni bilgi: oyundaki sadeleştirme | Oyunda sıcaklık dört bantla gösterilir; yavru dağılımı ve kuluçka için basitleştirilmiş kurallar kullanılır. |

`bolum1_giris` içindeki “Doğduğu kumsal onu çağırıyor” şiirsel bir giriş olarak kalabilir. Açıklayıcı bilgi, her bireyin tam olarak aynı noktaya döndüğünü kesin bir kural gibi söylememeli.

## İncelenen tutarsızlıklar

- **Serin bant:** `ipucu_termal` “çok serin yerden kaçın” diyor. Mevcut oyunda serin bant için ayrı bir yumurta kaybı gösterilmiyor; bant dişi oranını etkiliyor. Bu yüzden yukarıdaki öneri serinliği tek başına tehlike olarak göstermiyor. Çok düşük sıcaklığın doğada oluşturabileceği risk ayrı bir biyolojik konudur.
- **Kuluçka:** GDD'nin insan turu açıklamasında altı hafta yazıyor. `AnneAyarlari` sınıfının varsayılanı ve Unity'deki `Assets/_Project/ScriptableObjects/Settings/AnneAyarlari.asset` değeri sekiz hafta. Metinde mevcut ayara göre sekiz hafta kullanıldı; hangi sürenin tasarım niyeti olduğu ekipçe kararlaştırılmalı.
- **Cinsiyet hesabı:** Oyun, kaçış gecesinin sonunda yuvanın o andaki bandından kurtulanların dişi sayısını hesaplıyor. Doğada cinsiyet gelişim süresinin bir dönemindeki koşullarla ilişkilidir; oyundaki tek bantlık hesap bu sürecin sadeleştirmesidir. Gölge, sulama veya taşımanın zamanlaması bakımından bu fark ekipçe değerlendirilmeli.
- **Kırmızı bant:** Mevcut kodda kırmızı bantta daha az yumurta yuvanın kaydı sırasında belirleniyor. Sonradan oluşan bütün sıcaklık değişimlerinde aynı kaybın uygulandığı varsayılmamalı.
- **Ekrana aktarım:** Bilgi kartları şu anda HTML önizlemesindedir. Gerçek oyun içi bilgi arayüzü, metin kimlikleri ve gösterim noktaları ayrıca uygulanmalıdır; bu belgede oynanış veya Unity sahneleri değiştirilmemiştir.

## Kaynaklar

- NOAA Fisheries, [Loggerhead Turtle: Science](https://www.fisheries.noaa.gov/species/loggerhead-turtle/science?page=1): yaklaşık iki aylık kuluçka, sıcaklığa bağlı cinsiyet, ışık ve diğer tehditler.
- NOAA Fisheries, [Kuzeybatı Atlantik caretta değerlendirmesi](https://media.fisheries.noaa.gov/2023-03/Signed_5YrReview_NWAtlantic_Loggerhead.pdf): kuluçka süresindeki değişkenlik, gelişimin orta döneminde sıcaklık ve doğum bölgesine dönüş.
- NOAA, [How do sea turtles hatch?](https://oceanservice.noaa.gov/facts/turtle-hatch.html): yuvalama, kuluçka ve yavruların denize yolculuğu.
- NOAA Fisheries, [Loggerhead Turtle](https://www.fisheries.noaa.gov/species/loggerhead-turtle): yüksek sıcaklığın yumurta kaybı ve cinsiyet oranına etkisi.
