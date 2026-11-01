# Final ve gönüllü ataması — ekip değerlendirmesi

Bu belge bir uygulama ve karar önerisidir. Oyun mekaniği veya gerçek oyun sahnesi değiştirilmemiştir.

## Öncelik 1: Final

**Sorun:** `FinalSahnesi` Temiz Enerji'ye göre kumsalın görünümünü değiştiriyor ve denize ulaşanların toplamını gösteriyor. Başka koruma kararlarına ve denize ulaşanlardan hikâyedeki yavrunun nasıl seçildiğine açık bir karşılık vermiyor.

**Önerilen oyuncu akışı:**

1. Önce gece sonuçlarından bir özet: denize ulaşan yavru toplamı ve gerçekten kaydedilmiş diğer koruma sonuçları. Toplamın bir oyuncu kararı sayesinde kurtarıldığı iddia edilmez.
2. Daha sonra 30 yıl sonrası: Temiz Enerji'nin mevcut görsel etkisi korunur. Koruma alanı gibi saklanan kararlar için ayrı, kısa ve doğrulanabilir bir karşılık düşünülür. Bölgenin o andaki durumu 30 yıl sonrasına kesin olarak taşınmış gibi sunulmaz.
3. Başka kartlar için kalıcı geçmiş kaydı bulunup bulunmadığı tek tek kontrol edilmeden “bunu yaptın” yazılmaz. Temizlik gibi geçici etkilerden uzun vadeli sonuç çıkarılmaz.
4. **Kullanıcının ekip adına onayladığı karar:** Son kaçış gecesinde yavru kurtulmasa bile önceki gecelerden denize ulaşan varsa İlk Yüzüş o yavrulardan biriyle devam eder. Hiçbir gecede yavru denize ulaşmadığında ise yetişkinin aynı yavru olduğu söylenmemeli; sıfır yavru için duygusal sonuç ekranı gösterilir. Bu durumda İlk Yüzüş'ün atlanıp atlanmayacağı henüz kesinleşmedi. Toplam sayı, son geceden yavru çıkıp çıkmadığını tek başına göstermez.
5. Final metinleri `docs/metinler.md`'de kimliklerle hazırlanıp Yusuf tarafından metin tablosuna aktarılır; ekran ve oyun akışı Umut tarafından uygulanır. Nihai görsel yerleşim Abdü'nün onayına sunulur.

**Ekipten karar:** Koruma eylemlerinin hangileri 30 yıl sonrasını etkiler? Hangi kararlar için geçmiş kaydı tutulur? Toplam sıfırken duygusal sonuç ekranından sonra İlk Yüzüş atlanacak mı? Bu sorular yanıtlanmadan yeni final sonucu kesinleştirilmemeli.

## Öncelik 2: Gönüllü atamasını geri alma

**Doğrulanan durum:** `InsanTuru` hedef seçilince `Etkiler.GorevVer` çağırır. Bu işlem gönüllüyü kullanır ve temizlik, kafes, devriye, sulama veya bilgilendirmeyi hemen uygular. Hedef seçerken vazgeçmek ile uygulanmış görevi geri almak farklıdır; ikincisi için işlem bulunmuyor.

**Öneri:** Gönüllü aşamasında atamaları hedef ve görev türüyle listele; tur ilerlemeden bir atamanın yanındaki “Geri al” ile yalnız o atamayı kaldır ve gönüllüyü geri ver. Görev etkisi de geri alınmalı. Kart veya olaya geçince atamaların kesinleşmesi önerilir; bunun onayı ekipten alınmalı.

**Uygulamada dikkat:** Etkiyi genel olarak ters yönde uygulamak yeterli değildir. Örneğin çöp seviyesi sınırlara kırpılabilir, geçici devriye veya sulamanın kendi kayıtları vardır, kafes önceki durumdan gelmiş olabilir ve halk desteği sınıra dayanabilir. Her atamada gerçekten yapılan değişiklik ve ilgili geçici etkinin kimliği saklanmalı; diğer atamalar ve önceden var olan etkiler korunmalı. Aynı hedefe birden fazla görev verildiğinde sıralama açıkça tanımlanmalı. Sahne, kayıt/yükleme ve tur geçişi de test edilmeli.

**Örnek kontrol senaryosu:** Bölge 3'te çöp 2 iken bir görev sonucu 1 olur, boş gönüllü 4'ten 3'e iner. Görev aşamasında bu atama geri alınınca çöp tekrar 2, boş gönüllü 4 olur. Bu sayılar test örneğidir.

## Kanıt akışı: hazır olan ve eksik olan

`kanit_karar_onizleme.html` tıklanabilir bir tasarım denemesidir: örnek gece sonucu, ağ izi, temizlik ve ikinci kanıt anlatımını gösterir. Çalışan Unity arayüzüne bağlanmamıştır. Oyunda kayıp izi ve kanıt türü kaydedilir, ancak kanıt notunun, hangi kartı nasıl etkilediğinin ve ikinci kanıtın farkının ilgili noktada gösterilmesi ayrıca yapılmalıdır. Geceye özel neden özeti için geceye bağlı kayıt da ayrıca gerekir. Taslaktaki örnek sayılar gerçek sonuç verisi değildir.

## Güncel metin denetimi

Güncel `docs/metinler.md` içeriğinden aktarılan 255 satır ile Unity'deki `MetinTablosu.asset` karşılaştırıldı: beklenen 255 satır mevcut ve değerleri aynı. Sıcak dalgası ve fırtınanın A/B hücrelerindeki “—” bilinçli boş hücredir; eksik aktarım sayılmaz. Daha önce söylenen “10 eksik kimlik” bu sürüm için doğrulanamadı; eski rapor güncel bulgu sayılmamalı.

Hâlâ eksik olan, yeni biyoloji bilgi metinlerinin kimlikleri ve gerçek bilgi ekranındaki kullanımıdır; mevcut `ipucu_termal` de eski yanlış yönlendirmeyi içerir. Önerilen metinler `biyoloji_termal_metin_notlari.md` içinde hazırdır.
