# Kanıttan karara: arayüz denemesi

Bu çalışma, `kanit_karar_onizleme.html` dosyasında tarayıcıdan incelenen bir tasarım ve etkileşim örneğidir. Oyun içi Unity uygulaması değildir. Ekrandaki gece, bölge ve sayılar örnektir. Güncel oyun: `9abb453`.

## İncelenecek akış

1. **Gece sonucu:** Örnek toplamlar ve kayıp nedeni dağılımı, oyuncuya neyin araştırılacağını söyler. Geceye özel neden toplamları mevcut sonuç panelinde bulunmadığı için bu dağılımın veri kaynağı ve gösterimi ekip kararından sonra hazırlanmalıdır.
2. **Kanıt:** Bölgedeki ağ izi, çöp kanıtı ve ilgili temizlik seçeneği arasında bağlantı kurulur. İkinci kanıt düğmesi mevcut Temizlik Kampanyası kuralını gösterir: 3 yerine 4 bölge. Bu gösterimin ve ikinci kanıtın oyun içindeki yeri ekipçe değerlendirilecektir.
3. **Görev:** Tek bir bölgede çöp seviyesi örnek olarak 2'den 1'e düşer; gönüllü sayısı 4/4'ten 3/4'e iner. Bu yalnızca önizleme durumudur. Çöp tur sonunda yeniden birikebileceği için sonraki geceye kesin düzey garantisi verilmez.
4. **Bilgi:** Oyun kuralı ile doğaya ilişkin açıklama ayrı gösterilir. Kaynaklı biyoloji metinlerinin kapsamı ve son dili Yusuf ve ekip tarafından gözden geçirilmelidir.

## Veriye ilişkin sınırlar

- `KacisGecesi` kayıp izine neden ve konum yazar; mevcut sonuç paneli kurtulan, kaybedilen ve cinsiyet sayılarını gösterir. Kayıp izi modelinde gece kimliği yoktur. Gecelere göre güvenilir döküm için kayıt tasarımı gerekir; birikmiş izler otomatik olarak son gecenin sonucu sayılmamalıdır.
- `CopSeli` kaybı, örnekteki gibi gerçek kayıp konumuna değil ilgili yuvanın konumuna yazılır. Bu parkur için haritada kesin kayıp yeri vaat edilmemelidir.
- `InsanTuru.KanitAcildi` kanıt türünü ekler ve ilk yeni türü rapora yazar. Mevcut ekranda `kanit_*_not` açıklamalarının gösterimi bulunmamıştır.
- Gönüllü temizlik görevi seçilen bölgenin çöpünü 1 azaltır. Temizlik Kampanyası en kirli 3 bölgeyi, mevcut ikinci çöp kanıtı kuralıyla 4 bölgeyi etkiler. Seviye zaten sıfırsa kayda değer düşüş olmaz.
- `FinalSahnesi` görünümü şu anda Temiz Enerji durumuna göre ayarlar ve denize ulaşan yavru toplamını yazar. Diğer kararların finale etkisi bu önizlemede kararlaştırılmamıştır.

## Ekipte netleştirilmesi gerekenler

1. Hangi kayıp nedenleri oyuncuya hangi ayrıntıda gösterilecek? Gece ve parkur için kayıt nasıl tutulacak?
2. İlk ve ikinci kanıt ne açacak, ucuzlatacak veya güçlendirecek? Oyuncuya önceki ve yeni değer nasıl sunulacak?
3. Temizliğin sonucu, tur sonu yeni çöp birikmesi de hesaba katılarak hangi raporda gösterilecek?
4. Koşuda kalan, kurtulan ve kaybedilen yavru sayılarından hangileri sürekli görünmeli?
5. Finalde kurtarılanların ve diğer koruma kararlarının karşılığı ne olmalı? Hiç yavru kurtulmazsa akış ne yapmalı?
6. Biyolojik bilgilerin doğrulanması, metin uzunluğu ve oyun içinde erişim biçimi nasıl olacak?

## Kaynak örneği

- NOAA Fisheries, [Entanglement of Marine Life: Risks and Response](https://www.fisheries.noaa.gov/insight/entanglement-marine-life-risks-and-response): deniz kaplumbağalarının balıkçılık malzemeleri ve deniz çöplerine dolanabilmesi.
- U.S. Fish & Wildlife Service, [Loggerhead Sea Turtle](https://www.fws.gov/species/loggerhead-sea-turtle-caretta-caretta): kuluçka ve yuvalama dönemleri için örnek kaynak. Bölgesel farklılıklar ve metnin doğruluğu yayınlanmadan önce kontrol edilmelidir.

## Olası uygulama sahipliği

- **Toprak:** yönetim arayüzü, kanıt ve görev sunumu.
- **Umut:** gece/parkur sonuç verisi ve ilgili oyun akışı.
- **Yusuf:** oyuncu metinleri ve biyolojik kaynakların doğrulanması.
- **Abdü:** görsel tasarım, simgeler ve sunumun değerlendirilmesi.

Bu dağılım depo sahiplik belgesinin yorumudur; değişiklikleri ilgili kişiler koordine etmelidir. Görsel deneme, dosya sorumlularının Unity entegrasyonunun yerine geçmez.
