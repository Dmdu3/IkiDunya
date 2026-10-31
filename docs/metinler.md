---
ozet: Oyunda görünen bütün yazılar. Sahibi Yusuf. Koda girmeden önce burada yazılır; id koddaki anahtardır.
---

# Metinler

Kısa yaz: kart açıklaması en fazla 2 satır, kanıt notu en fazla 3 satır. Rakam içeren her cümle `kaynaklar.md`'de doğrulanmış olmalı (oyun kuralı olan sayılar hariç: bedel, kaç kez basılacağı).

> 2026-11-01: ilk taslak Umut'tan (Yusuf'un gözden geçirmesi bekleniyor). Değiştirilen yazı Unity'de **İki Dünya → Metinleri aktar** ile oyuna girer.

## Kartlar

| id | Ad | Açıklama |
|---|---|---|
| kart_golgelik | Gölgelik | Seçtiğin bölgeye gölgelik kur. Kum bir bant serinler, kalıcı. |
| kart_temizlik | Temizlik Kampanyası | En kirli üç bölgede çöp bir kademe azalır. |
| kart_kumul | Kumul Onarımı | Seçtiğin bölgede kumulu onar; dalga çizgisi denize çekilir. |
| kart_yuva_tasima | Yuva Taşıma | Bir yuvayı güvenli ve serin bir yere taşı. |
| kart_isik | Işık Yasağı | Otel ve yol ışıkları geceleri kapanır, kalıcı. |
| kart_arac | Araç Yasağı | Kumsala araç girmez; tekerlek izleri kaybolur. |
| kart_sezlong | Şezlong Düzenlemesi | Şezlonglar her akşam toplanır; engeller yarıya iner. |
| kart_tilki_citi | Tilki Çiti | Kumsalın kenarına çit; tilki daha seyrek gelir. |
| kart_ag_temizligi | Hayalet Ağ Temizliği | Denizdeki sahipsiz ağlar ve misinalar toplanır. |
| kart_gonullu_cagrisi | Gönüllü Çağrısı | Gelecek hafta iki gönüllü daha gelir. |
| kart_saha_arastirmasi | Saha Araştırması | Henüz bulunmamış bir kanıtı ortaya çıkarır. |
| kart_temiz_enerji | Temiz Enerji | Bugün bir şey değiştirmez. Yıllar sonra fark yaratır. |
| kart_aydinlatma | Kaplumbağa Dostu Aydınlatma | Seçilen bölgenin lambaları kısık, kırmızı ışıklıya değişir. |
| kart_gozcu_kulubesi | Gözcü Kulübesi | Seçilen bölgede her gece devriye var. |
| kart_kulucka | Kuluçkahane | Bütün yuvalar seçilen bölgeye taşınır ve kafeslenir. Hepsi tek yerde olur. |
| kart_plaj_tabela | Plaj Tabelaları | Bu gece kumsalda daha az insan olur. |
| kart_bagis | Bağış Kampanyası | Halk desteği kadar bağış toplanır; halk biraz yorulur. |
| kart_basin | Basın Bülteni | Bulunan kanıtlar halka anlatılır. Otel bundan hoşlanmaz. |
| kart_lobi | Belediye Lobisi | Belediyeyle görüşülür, ilişki iyileşir. |
| kart_koruma_alani | Koruma Alanı | Seçilen bölge korunur: şezlong, insan, ışık ve araç kalkar, bir daha giremez. |
| kart_otel_ortakligi | Otel Ortaklığı | Otel ışıklarını kısar; karşılığında bir bölgeye şezlong koyar. |
| kart_kooperatif | Balıkçı Kooperatifi | Balıkçılarla ağ değişimine ortak olunur. |
| kart_kum_torbasi | Kum Torbası Seddi | Seçilen bölgede fırtına yuvaları basmaz. |
| kart_tahliye | Yuva Tahliyesi | Dalga çizgisinin altındaki bütün yuvalar güvenli yere taşınır. |
| kart_acil_sulama | Acil Sulama | Bütün yuvalar bu tur ve bu gece serinletilir. |
| kart_dron | Dron Taraması | Haritadaki bütün kayıp izleri incelenir, kanıtları açılır. |
| kart_universite | Üniversite Ortaklığı | Bundan sonra her tur bir kart fazla gelir. |

## Gönüllü görevleri

| id | Ad | Açıklama |
|---|---|---|
| gorev_temizlik | Temizlik | Bir bölgenin çöpü bir kademe azalır. |
| gorev_devriye | Gece devriyesi | Bu gece o bölgede insan ve fener olmaz. |
| gorev_kafes | Yuva kafesi | Yuvaya tilki giremez, kalıcı. |
| gorev_golge | Gölge ve sulama | Yuvanın kumu bu tur ve gece serin kalır. |
| gorev_bilgilendirme | Bilgilendirme | Halkı bilgilendir; halk desteği artar. |

## Kanıtlar

| id | Ad | Not |
|---|---|---|
| kanit_isi | Kanıt: Isı | Yavrular sıcak kumda tükendi. Gölge, kumu serinletir. |
| kanit_isik | Kanıt: Işık | Yavrular ışığa döndü, denizi kaybetti. Işıklar söndürülebilir. |
| kanit_cop | Kanıt: Çöp | Yavrular ağa, halkaya, tekerlek izine takıldı. Araçlar ve şezlonglar düzenlenebilir. |
| kanit_avci | Kanıt: Avcı | Bir tilki yuvayı ya da sürüyü buldu. Çit onu uzak tutar. |
| kanit_su | Kanıt: Su | Deniz yuvayı bastı. Yuvalar taşınabilir. |

## Olaylar

| id | Metin | Seçenek A | Seçenek B |
|---|---|---|---|
| olay_otel | Otel kumsala yeni bir şezlong sırası koymak istiyor. | Kabul et (bütçe artar) | Reddet (otel küser) |
| olay_festival | Belediye kumsalda bir gece festivali düzenlemek istiyor. | Kabul et (destek artar) | Reddet (belediye küser) |
| olay_balikci | Balıkçılar, ağlarını kaplumbağa dostu ağlarla değiştirmek için destek istiyor. | Destekle (bütçe azalır) | Şimdi değil |
| olay_sicak_dalga | Hava tahmini: gelecek haftanın sonunda sıcak dalgası. Kum ısınacak; hazırlanmak için bir haftan var. | — | — |
| olay_firtina | Hava tahmini: gelecek haftanın sonunda fırtına. Dalga çizgisinin altındaki yuvalar su alabilir; hazırlanmak için bir haftan var. | — | — |
| olay_okul | Bir okul, kumsala ziyaret düzenlemek istiyor. | Kabul et (bir gönüllü meşgul) | Reddet |
| olay_tamam | Tamam | | |
| olay_otel_bar | Otel kumsala bir bar açmak istiyor. | Kabul et | Reddet |
| olay_festival_cop | Festivalden sonra kumsal çöp dolu. | Gönüllüler temizlesin | Bırak |
| olay_balikci_ag_getirdi | Balıkçılar denizden bir hayalet ağ çıkardı. | Sergile | Geri dönüşüme sat |
| olay_gazeteci | Bir gazeteci kaybolan yuvaları soruyor. | Açık konuş | Sessiz kal |
| olay_kopek | Turistler kumsala köpek getiriyor. | Yasak tabelası as | İzin ver |
| olay_bagisci | Bir bağışçı, adının kumsala yazılması şartıyla para veriyor. | Kabul et | Reddet |
| olay_universite | Üniversite stajyer göndermek istiyor. | Kabul et | Reddet |
| olay_otel_isik | Otel inadına bütün ışıklarını açtı. | Şikâyet et | Bekle |
| olay_belediye_kesinti | Belediye ödeneği kesmek istiyor. | Toplantıya git | Kabul et |
| olay_balikci_ag_atti | Balıkçılar eski ağlarını denize attı. | Temizlik ekibi gönder | Bırak |

## Bölüm girişleri, ipuçları, final

| id | Metin |
|---|---|
| bolum1_giris | Dönüş. Doğduğu kumsal onu çağırıyor. |
| bolum2_giris | Yuva geceleri. Kumu iyi seç. |
| bolum3_giris | Bekleyiş. Kumun altında yeni hayatlar büyüyor. |
| bolum4_giris | Kaçış. Gün doğmadan denize. |
| bolum5_giris | İlk yüzüş. Uçsuz bucaksız mavi. |
| ipucu_poset | Denizanası kasılarak yüzer, poşet düz süzülür. Yakından bak, E ile ye. |
| ipucu_nefes | Nefesin azalıyor. Space'i basılı tutup yüzeye çık; yüzeyde teknelere dikkat et. |
| ipucu_ag | Ağa takıldın! E'ye art arda bas. |
| ipucu_dalga | Dalga gelirken Space ile altından dal. |
| ipucu_termal | T: termal görünüm. Kumun sıcaklığını gör; çok sıcak ya da çok serin yerden kaçın. |
| ipucu_kazma | Dur ve A ile D'ye sırayla basarak kaz. Yumurtladıktan sonra aynı şekilde ört. |
| ipucu_kumdan_cikis | W ile yukarı kaz. Kum serinlemeden çıkma, ama çok da bekleme. |
| ipucu_suru | Öncü sensin; sürü seni izler. Shift ile dağıl, Space ile zıpla. |
| ipucu_takili | Bir yavru takıldı ya da ters döndü. Yanına git, E'yi basılı tut. |
| ipucu_tilki | Tilki kumsalın kenarında. Önündeki görüş alanından uzak dur, kumul gölgesinde ilerle. |
| ipucu_kanit | Haritadaki renkli izlere tıkla: her biri bir kanıt. |
| ipucu_gonullu | Bir görev seç, sonra haritada bölgeye ya da yuvaya tıkla. |
| ipucu_kart | Bütçen yettiğince kart oyna. Bölge ya da yuva isteyen kartta haritaya tıkla. |
| ipucu_poset_surusu | Akıntı bir sürü taşıyor. Denizanalarını ye, poşetlerden kaç. Yeterince yiyince önündeki akıntı açılır. |
| ipucu_girdap | Girdaba kapıldın! Halkanın boşluğunu yakala; parlak şamandıralar yolu gösterir. |
| ipucu_cop_seli | Ufukta köpük görürsen dalga geliyor: kum tepesine çık. Su çekilince denize koş. |
| final_30_yil | 30 yıl sonra |
| final_kurtarilan | Denize ulaşan yavru |

## Menü ve ayarlar

| id | Metin |
|---|---|
| oyun_adi | İki Dünya, Tek Yaşam |
| menu_yeni_oyun | Yeni oyun |
| menu_devam_et | Devam et |
| menu_parkurlar | Parkurlar |
| menu_ayarlar | Ayarlar |
| menu_cikis | Çıkış |
| menu_oyuna_don | Oyuna dön |
| menu_ana_menu | Ana menü |
| menu_geri | Geri |
| menu_varsayilan | Varsayılan tuşlar |
| ayar_ses | Ses |
| ayar_fare | Fare hassasiyeti |
| ayar_tuslar | Tuşlar |
| tus_ileri | İleri |
| tus_geri | Geri |
| tus_sol | Sol |
| tus_sag | Sağ |
| tus_hizli | Hızlı / dağıl |
| tus_zipla | Zıpla / dal |
| tus_etkiles | Etkileş / kurtar |
| tus_termal | Termal görünüm |
| tus_kaz_sol | Kaz (sol) |
| tus_kaz_sag | Kaz (sağ) |
| tus_kaz_yukari | Yukarı kaz |

## Gece sonu

| id | Metin |
|---|---|
| sonuc_yuva_baslik | Yuva kazıldı |
| sonuc_kacis_baslik | Gece bitti |
| sonuc_bant | Kum |
| sonuc_yumurta | Yumurta |
| sonuc_kurtulan | Denize ulaşan |
| sonuc_kaybedilen | Kaybedilen |
| sonuc_disi | Dişi |
| sonuc_erkek | Erkek |
| sonuc_devam | Devam etmek için tıkla ya da Space'e bas |
| bant_mavi | Serin |
| bant_yesil | Ilık |
| bant_turuncu | Sıcak |
| bant_kirmizi | Çok sıcak |

## Yönetim ekranı

| id | Metin |
|---|---|
| yonetim_tur | Hafta |
| yonetim_butce | Bütçe |
| yonetim_gonullu | Gönüllü |
| yonetim_destek | Halk desteği |
| yonetim_ileri | İleri |
| asama_rapor | Rapor |
| asama_gonulluler | Gönüllüler |
| asama_kartlar | Kartlar |
| asama_olay | Olay |
| rapor_gelir | Gelen bütçe |
| rapor_denize_ulasan | Denize ulaşan yavru |
| rapor_cop_birikti | Çöpü artan bölge |
| rapor_yuva_bozuldu | Avcının bozduğu yuva |
| rapor_sicak_dalga | Sıcak dalgası geldi; kum ısındı |
| rapor_sicak_dalga_zayif | Sıcak dalgası beklenenden zayıf geçti |
| rapor_firtina | Fırtınada su alan yuva |
| rapor_sakin | Sakin bir hafta geçti. |
| paydas_otel | Otel |
| paydas_belediye | Belediye |
| paydas_balikci | Balıkçılar |
| kart_turu_altyapi | Altyapı |
| kart_turu_kampanya | Kampanya |
| kart_turu_politika | Politika |
| kart_turu_acil | Acil |
| kart_turu_arastirma | Araştırma |
| kart_bakim | bakım |
| kart_ister | ister |
| kart_guclu | güçlü |
| kart_halk | Halk |
| uyari_sicakdalga | Bu haftanın sonunda sıcak dalgası |
| uyari_firtina | Bu haftanın sonunda fırtına |
| yuva_avci_riski | avcı riski |
| bolge_korunuyor | Koruma alanı |
| rapor_bakim | Bakım |
| rapor_sponsor | Otel sponsorluğu |
| rapor_kesinti | Belediye kesintisi |
| hedef_bolge_sec | Haritada bir bölge seç |
| hedef_yuva_sec | Haritada bir yuva seç |
| yuva | Yuva |
| yuva_yumurta | yumurta |
| yuva_hafta | çıkmaya hafta |
| yuva_kafesli | kafesli |
| bolge | Bölge |
| katman_cop | Çöp |
| katman_isik | Işık |
| katman_sezlong | Şezlong |
| katman_insan | İnsan |
| yapi_golgelik | Gölgelik var |
| yapi_kumulonarimi | Kumul onarıldı |
| yapi_tilkiciti | Tilki çiti var |
| yapi_aracizi | Tekerlek izi var |
| yapi_devriye | Gece devriyesi var |

## Parkurlar

| id | Metin |
|---|---|
| parkur_poset_surusu | Poşet sürüsü |
| parkur_cop_seli | Çöp seli |
| parkur_cop_girdabi | Çöp girdabı |
| parkur_sure | Süre |
| parkur_rekor | Rekor |
| parkur_bitti | Bitti |
| parkur_yeni_rekor | Yeni rekor! |
| parkur_kilitli | (kilitli) |
| parkur_poset_asama_1 | Sakin su: denizanası |
| parkur_poset_asama_2 | Yoğun su: denizanası |
| parkur_poset_asama_3 | Karanlık su: denizanası |
| parkur_poset_kiyiya | Akıntı açıldı, kıyıya yüz! |
| parkur_girdap_halka | Halka |
| parkur_sel_kurtulan | Denize ulaşan |
