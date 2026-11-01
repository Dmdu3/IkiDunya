# Kaplumbağanın algısı (yanılgılar)

Kaplumbağa insanın bıraktığı şeyi doğanın bir parçası sanır. Oyuncu bunu kaplumbağanın gözünden yaşar: aldanır, bir şeylerin ters olduğunu ancak dikkat edince sezer, gerçeği çoğu zaman bedelini ödediği an görür. Yanılgıyı tamamen ortadan kaldıran şey kaplumbağanın dikkati değil, insan kısmındaki kararlardır.

## İlke: gerçekten kandır

- **Önceden ilan etme.** Kural (denizanası kasılır, yosun parlamaz, gerçek ay denizdedir) aldanmadan önce değil, ilk aldanıştan sonra ipucu olarak gelir.
- **Yanılgı tek ve inandırıcı olsun.** Gökte aynı anda tek sahte ay olur; gerçek ay o sırada bulutun ardındadır. Sahte denizanası uzaktan gerçeği gibi kasılır.
- **Kaplumbağanın gözü kusurlu.** Uzak bulanık ve soluk (kaplumbağa görüşü), parlak ışık taşar: uzaktaki şey ancak yaklaşınca belli olur.
- **Gerçeği görmenin bir bedeli var.** Poşet ve plastik yakınlıkla kendiliğinden ele vermez: ya yavaşlayıp yakından bakarsın (zaman ve enerji) ya da yutar/değersin ve gerçeği o an görürsün.
- **Dürüst bir kanal bırak.** Göz yanılsa da kulak doğruyu söyler: kıyının dalga sesi hep denizden gelir.

## Akış: dört adım

1. **Çekim:** Yanılgı gerçeğinden çekicidir. Sahte ay sodyum sarısı, gerçek aydan büyük ve parlak; kumda ona giden bir ışık yolu var (gerçek ayın denizdeki yansıması gibi).
2. **Şüphe:** Ele verenler inceliklidir ve ancak dikkatle görülür: sahte denizanası son metrelerde kasılmayı bırakıp düz süzülür; sahte ay elektrikle titrer, yansıması denizde değil kumdadır, dalga sesi başka yönden gelir; sahte yosun parlar. Çok yakında yanılgı arada bir "göz kırpar".
3. **Kırılma:** Gerçeğin ortaya çıktığı an: denizanası/yosun içine çöker ve poşet/plastik belirir; ay gökten lambanın ampulüne düşer, kumdaki yol söner, direk görünür; yutulan poşet bir an ağızda, gözün önünde görünür. Soğuk bir parıltı, sarsıntı ve ses; ilk seferde neyin ele verdiğini söyleyen ipucu. Kırılma o nesne için kalıcıdır (her gecenin lambaları yeniden yanıltır: kaplumbağa öğrenemez).
4. **İki dünya:** İnsan kısmında Işık Yasağı varsa lambalar söner: sahte ay yok, bulut açılır, gerçek ay ve denizdeki yolu görünür. Oyuncu farkı sahnede yaşar (GDD "İki tarafın bağlantısı").

## Yanılgılar

| Bölüm | Gerçek | Sanılan | İnce ele veren | Gerçeği görmek |
| --- | --- | --- | --- | --- |
| 1 Dönüş (anne) | Poşet | Denizanası | Son ~6 m'de kasılma tekler, ~2,5 m'de durur; düz süzülür | Yavaşlayıp (≤1,5 m/sn) ~4 m içinde 1 sn bakmak ya da yutmak (ağızda görünür) |
| 4 Kaçış (yavrular, gece) | Sokak/otel lambası | Ay (karada) | Titrer; yansıması kumda; dalga sesi denizden | ~8 m'ye girmek (ay lambaya düşer) ya da Işık Yasağı (hiç görünmez) |
| 5 İlk yüzüş (yavru) | Parlayan plastik | Yosun parçası | Yosun parlamaz | Yavaşlayıp ~4 m içinde 1 sn bakmak ya da değmek |

## İçgüdü (Bölüm 4)

Yavru gece en parlak ayı izler. Oyuncu bakışını ~1 sn oynatmazsa bakış yavaşça en çekici aya döner; yavru ileri yürüdükçe o yöne gider. Sahte ay gerçeğinden 2 kat çekicidir, bulutun ardındaki gerçek ay daha da az çeker. Oyuncu bakışını oynattığı an içgüdü bırakır. Lambaya bakmak yanılgıyı kırmaz: yavru içgüdüsünü dikkatle yenemez.

## Bilerek yok

- **Bölüm 2 (anne, kumsal):** Yuvalayan dişi ışıktan kaçar, ona aldanmaz.
- **Parkurlar:** Kendi kuralları var (poşet sürüsünün karanlık aşaması poşetin görünmez olmasına dayanır).
- **Termal görünüm:** Kum sıcaklığı aracıdır, algıyla ilgisi yoktur.

## Ölçü

İlk kez oynayan (GDD'yi okumamış) biri her bölümde en az bir kez aldanmalı: Bölüm 1'de en az bir poşet yutmalı, kaçış gecesinde en az bir kez sahte aya yürümeli. Playtest'te sayılır (`docs/playtest.md`).

## Teknik

- `Turtle/Algi`: `AlgiNesnesi` (nesnede; tek ay seçimi, kasılma, kırılma, ağızda gösterme), `KaplumbagaAlgisi` (kamerada: güncelleme, göz hızı, bulut, içgüdü), `AlgiAyarlari` (bütün sayılar, tür başına profil), `AlgiTuru`, `AgizdakiCop`.
- `Core`: `GeceGokyuzu.Bulut` ve bulut parçaları, `AyYansimasi` (denizdeki ay yolu bulutla söner), `KiyiSesi` (dinleyici hizasında su çizgisinden dalga sesi), `Geribildirim.Uyanis`, `TekKulak.Etkin`.
- Görüş: `Settings/KaplumbagaGorusu` post-process profili (uzak bulanıklık, renk solması, kenar kararması, parıltı), sadece kaplumbağa kameralarında.
- Sesler `SesTuru.YanilgiPoset`, `YanilgiIsik`, `Kiyi` (kodla sentezlenmiş geçici klipler). Görünüşleri kurulum komutları hazırlar (`GriKumsalKur.IsikKur`, `KameraKur`; `GriDenizKur.PosetYanilgisiKur`, `PlastikKur`).
