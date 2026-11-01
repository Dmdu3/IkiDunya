# Kaplumbağanın algısı (yanılgılar)

Kaplumbağa insanın bıraktığı şeyi doğanın bir parçası sanır. Oyuncu bunu kaplumbağanın gözünden yaşar: önce aldanır, sonra bir şeylerin ters olduğunu sezer, en sonunda gerçeği görür. Yanılgıyı tamamen ortadan kaldıran şey kaplumbağanın dikkati değil, insan kısmındaki kararlardır.

## His: dört adım

1. **Çekim:** Yanılgı güzeldir, gerçeğinden daha çekicidir. Sahte ay gerçek aydan büyük ve sıcaktır; sahte denizanası gerçeğiyle aynı modeldir.
2. **Şüphe:** Dikkatli göz bir terslik sezer. Her yanılgının uzaktan da görülen bir ipucu vardır (aşağıdaki tablo). Yaklaştıkça yanılgı kısa anlarda "göz kırpar": gerçek görünüş bir an görünür. Doğrudan bakınca bu kırpmalar sıklaşır.
3. **Kırılma:** Gerçeğin ortaya çıktığı an kısa ama hissedilir: yanılgı söner (denizanası içine çöker, ay gökten lambanın ampulüne düşer), gerçek görünüş belirir, soluk bir ekran parıltısı ve kendi sesi gelir. İlk seferde bir ipucu neyin ele verdiğini söyler. Kırılma kalıcıdır: o nesne bir daha yanıltmaz.
4. **İki dünya:** İnsan kısmında Işık Yasağı varsa lambalar söner ve gece tek bir ay kalır; temizlik az poşet ve plastik demektir. Oyuncu farkı sahnede görür (GDD "İki tarafın bağlantısı").

## Yanılgılar

| Bölüm | Gerçek | Sanılan | Uzaktan ipucu | Kırılma |
| --- | --- | --- | --- | --- |
| 1 Dönüş (anne) | Poşet | Denizanası | Kasılmaz, akıntıyla düz süzülür | ~2,4 m'ye girmek ya da ~6 m içinde 1,5 sn doğrudan bakmak |
| 4 Kaçış (yavrular, gece) | Sokak/otel lambası | Ay (kara tarafında) | Denizde yansıması yok, elektrikle titrer | ~8 m'ye girmek (ay lambaya düşer) ya da Işık Yasağı (hiç görünmez) |
| 5 İlk yüzüş (yavru) | Parlayan plastik | Yosun parçası | Yosun parlamaz; bu parlıyor | ~3 m'ye girmek ya da ~7 m içinde 1,5 sn doğrudan bakmak |

## İçgüdü (Bölüm 4)

Yavru gece en parlak ayı izler. Oyuncu fareyi/sağ çubuğu bir süre oynatmazsa bakış yavaşça en çekici "aya" döner; yavru ileri yürüdükçe o yöne gider. Işık varsa bu sahte aydır (kara, tehlike); Işık Yasağı varsa gerçek aydır (deniz, kurtuluş). Oyuncu bakışını oynattığı an içgüdü bırakır. Lambaya doğrudan bakmak yanılgıyı kırmaz: yavru içgüdüsünü dikkatle yenemez, ancak çok yaklaşınca ya da insan ışığı söndürünce görür.

## Bilerek yok

- **Bölüm 2 (anne, kumsal):** Yuvalayan dişi ışıktan kaçar, ona aldanmaz; yanılgı gerçekle çelişir.
- **Parkurlar:** Kendi kuralları var (poşet sürüsünün karanlık aşaması poşetin görünmez olmasına dayanır).
- **Termal görünüm:** Kum sıcaklığı aracıdır, algıyla ilgisi yoktur.

## Teknik

`Turtle/Algi`: `AlgiNesnesi` (nesnede), `KaplumbagaAlgisi` (kamerada: güncelleme ve içgüdü), `AlgiAyarlari` (bütün sayılar, tür başına profil), `AlgiTuru`. Görünüşleri kurulum komutları hazırlar (`GriKumsalKur.IsikKur`, `GriDenizKur.PosetYanilgisiKur`, `PlastikKur`). Sesler `SesTuru.YanilgiPoset`, `SesTuru.YanilgiIsik`; ipuçları `docs/metinler.md`.
