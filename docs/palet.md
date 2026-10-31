---
ozet: Oyunun renk paleti (Sword of the Sea'den esinlenen). Modellere ve malzemelere renk verirken buradaki kodları kullanın. Hazırlayan Yusuf.
---

# Renk paleti

Tarz: sıcak kum ve okra kayalar, yeşim-camgöbeği deniz, açık mavi gökyüzü; canlı vurgular az ve küçük alanlarda. Gölgelendirme yumuşak (keskin çizgi film bantları yok).

Kaynak sütunu: **ölçüldü** = oyunun Steam'deki 12 resmi ekran görüntüsünden piksel gruplamasıyla çıkarıldı; **tahmin** = ilk denemede tarzdan yola çıkarak seçildi. Ölçülen renkler oyuna daha yakın; gün batımı tonları (pembe, gece mavisi) ekran görüntülerinde pek yok, gerekirse gün batımı ve gece sahnelerinde kullanılır.

Renkler sRGB hex. Blender'da görüntü dönüşümü **Standard** olmalı (AgX renkleri soluklaştırıyor). Malzemeler Blender'da `P_` adıyla bir kütüphane dosyasında duruyor; şimdilik repo dışında, Yusuf'ta.

## Su

| Ad | Renk | Kullanım | Kaynak |
|---|---|---|---|
| P_Su_Yesim | `#09AB81` | Deniz, ana su rengi | ölçüldü |
| P_Su_Turkuaz | `#19BEC4` | Sığ, aydınlık su | ölçüldü |
| P_Su_Derin | `#167FA1` | Derin su | ölçüldü |
| P_Su_Gece | `#0F2A3C` | Su altı gölgeleri, en koyu ton | ölçüldü |
| P_Kopuk | `#F5F3EF` | Dalga köpüğü, bulut | ölçüldü |
| P_Deniz_Sig | `#3FD0C9` | Sığ su (alternatif) | tahmin |
| P_Deniz_Derin | `#11707F` | Derin su (alternatif) | tahmin |

## Gökyüzü

| Ad | Renk | Kullanım | Kaynak |
|---|---|---|---|
| P_Gok_Mavi | `#7CCEDD` | Gündüz gökyüzü | ölçüldü |
| P_Gok_Pus | `#C8D8DE` | Uzaktaki pus, ufuk | ölçüldü |
| P_Gok_Altin | `#FFD98E` | Gün batımı ufku, güneş ışığı | tahmin |
| P_Gok_Pembe | `#F4A3A8` | Gün batımı | tahmin |
| P_Gece_Mavi | `#1E2A5A` | Gece gökyüzü | tahmin |

## Kum ve kaya

| Ad | Renk | Kullanım | Kaynak |
|---|---|---|---|
| P_Kum_Bej | `#EFCC92` | Kum, kumul | ölçüldü |
| P_Kum_Parlak | `#F0D4AB` | Güneş alan kum | ölçüldü |
| P_Kum_Acik | `#F2C98A` | Düz kum (sıcak) | tahmin |
| P_Kum_Sicak | `#E39A5B` | Kumul yamacı | tahmin |
| P_Kum_Golge | `#B8656E` | Gün batımında gölgedeki kum | tahmin |
| P_Kaya_Okra | `#DFAC6A` | Kayalık, harabe | ölçüldü |
| P_Kaya_Pismis | `#D47C34` | Pişmiş toprak yapılar | ölçüldü |
| P_Kaya_Pas | `#99471E` | Kaya gölgesi | ölçüldü |
| P_Kaya_Koyu | `#632D16` | En koyu kaya tonu | ölçüldü |
| P_Tas_Harabe | `#C9B6A3` | Soluk taş, deniz kayası | tahmin |

## Bitki

| Ad | Renk | Kullanım | Kaynak |
|---|---|---|---|
| P_Yosun | `#829D1C` | Sarmaşık, canlı yosun | ölçüldü |
| P_Yaprak_Zeytin | `#7C8456` | Soluk bitki örtüsü, deniz çayırı | ölçüldü |
| P_Yaprak_Koyu | `#2F472E` | Gölgedeki bitkiler | ölçüldü |
| P_Denizanasi | `#78B998` | Denizanası; hafif ışık yayar | ölçüldü |

## Kaplumbağa ve vurgular

| Ad | Renk | Kullanım | Kaynak |
|---|---|---|---|
| P_Kabuk_Teal | `#2BA6A0` | Kaplumbağa kabuğu | tahmin |
| P_Isik_Parilti | `#7FFFF0` | Parlayan desen, ışık yayar | tahmin |
| P_Altin | `#F2D20F` | Küçük canlı vurgu (balık) | ölçüldü |
| P_Kehribar | `#F2A94C` | Sıcak ışık, fener | ölçüldü |
| P_Turuncu | `#ED7208` | Sancak, uyarı | ölçüldü |
| P_Mercan | `#FF6F61` | Mercan, vurgu | tahmin |
