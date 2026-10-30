---
ozet: Mekanik tarafın durumu ve sistemlerin haritası. Bir sistem eklendikçe ya da değiştikçe güncellenir. Sahibi Umut; Toprak kendi sistemlerini işler.
---

# Durum

Son güncelleme: 2026-10-30 — Faz 2 (teknik kurulum). Henüz oyun kodu yok.

## Sistem haritası

| Sistem | Klasör | Ana script(ler) | Sahibi | Durum |
| --- | --- | --- | --- | --- |
| Oyun akışı ve bölüm geçişi | `Core` | — | Umut | Planlandı |
| Kumsal durumu (ortak durum) | `Core` | `KumsalDurumu` | Umut | Planlandı |
| Tuşlar | `Core` | `GameInput` | Umut | Planlandı |
| Kayıt | `Core` | — | Umut | Planlandı |
| Kameralar | `Camera` | — | Umut | Planlandı |
| Anne (kara): hareket, enerji, kazma | `Turtle` | — | Umut | Planlandı |
| Yavru sürüsü | `Turtle` | — | Umut | Planlandı |
| Deniz hareketi, nefes | `Turtle` | — | Umut | Planlandı |
| Tehlike bölgeleri (sıcak leke, ışık, ağ) | `Hazards` | `HazardZone` | Umut | Planlandı |
| Avcılar | `Hazards` | — | Umut | Planlandı |
| Kayıp izleri | `Hazards` | — | Umut | Planlandı |
| Kumsal haritası ve tur akışı | `Management` | — | Toprak | Planlandı |
| Gönüllüler ve görevler | `Management` | — | Toprak | Planlandı |
| Kartlar | `Management` | `CardDefinition` | Toprak | Planlandı |
| Kanıtlar ve olaylar | `Management` | — | Toprak | Planlandı |

Durum değerleri: Planlandı · Yapılıyor · Oynanabilir · Bitti.

## `KumsalDurumu` sözleşmesi

> Faz 0'da Umut ve Toprak birlikte doldurur: alanlar, açık metotlar, event'ler.

## Bilinen sorunlar

—
