"""Plaj bilgilendirme tabelası (Toprak'ın "Plaj Tabelaları" kartı): kumsala girişte "burada kaplumbağa yuvalıyor" panosu.

    Plaj_Tabelasi        iki direkli büyük pano, boy ~2,05 m; pano 1,4 x 0,9 m (z = 0,95 .. 1,85), önde (Blender -y).
                         Kök iki direğin ortasında, tabanda; direkler x = ±0,62.
    Plaj_Tabelasi_Kucuk  tek direkli küçük levha, boy ~1,3 m; levha 0,5 x 0,42 m. Kök direğin dibinde.

Modelde okunacak yazı yok (metin docs/metinler.md'den gelir, modele gömülmez): pano resimlerle konuşur.
Büyük panonun yüzü kendi malzemesinde (Tabela_Yuzu, beyaz): Unity'de yazılı/dokulu bir malzemeyle değiştirilebilir.
Yüzün önündeki resimler: mavi başlık şeridi, büyük teal kaplumbağa, üstü çizili lamba (ışık yasağı),
üstü çizili ayak izi (yuvaya basma), yazıyı andıran koyu çizgiler, turuncu alt şerit.
"""

import math
import random

import kumsal_objeleri_uret as k
import tilki_citi as t

k.PALET.setdefault("Tabela_Yuzu", "#F5F3EF")  # P_Kopuk ile aynı renk, ayrı malzeme dilimi

AHSAP = "P_Kaya_Pas"
KOYU = "P_Kaya_Koyu"
KUM = "P_Kum_Parlak"
TEAL = "P_Kabuk_Teal"
YASAK = "P_Turuncu"


def dikdortgen(x0, z0, x1, z1):
    return [(x0, z0), (x1, z0), (x1, z1), (x0, z1)]


def kaplumbaga(p, cx, cz, y, s):
    """Yukarı yüzen kaplumbağa silüeti (tilki çiti levhasındakinin s katı): kabuk, baş, dört yüzgeç."""
    t.levha(p, t.elips(cx, cz, 0.062 * s, 0.078 * s, n=16), y, 0.007, TEAL)
    t.levha(p, t.elips(cx, cz + 0.105 * s, 0.026 * s, 0.03 * s, n=10), y, 0.005, TEAL)
    for d in (-1, 1):
        t.levha(p, t.elips(cx + d * 0.075 * s, cz + 0.035 * s, 0.058 * s, 0.02 * s, aci=d * 0.45), y, 0.003, TEAL)
        t.levha(p, t.elips(cx + d * 0.045 * s, cz - 0.06 * s, 0.034 * s, 0.015 * s, aci=-d * 0.6, n=10), y, 0.003, TEAL)
    # kabuk deseni: kabuğun üstünde daha koyu üç pul
    for dz in (-0.03, 0.0, 0.03):
        t.levha(p, t.elips(cx, cz + dz * s, 0.022 * s, 0.012 * s, n=8), y - 0.007, 0.002, "P_Su_Derin")


def yasak_halkasi(p, cx, cz, y, yaricap):
    """Turuncu halka ve çapraz çizgi (yüzün önünde, X-Z düzleminde)."""
    n = 20
    halka = [(cx + math.cos(2 * math.pi * i / n) * yaricap, y - 0.012, cz + math.sin(2 * math.pi * i / n) * yaricap) for i in range(n)]
    p.tup(halka, 0.011, YASAK, kenar=4, kapali=True)
    a = math.radians(135)
    p.tup([(cx + math.cos(a) * yaricap, y - 0.014, cz + math.sin(a) * yaricap),
           (cx - math.cos(a) * yaricap, y - 0.014, cz - math.sin(a) * yaricap)], 0.011, YASAK, kenar=4)


def lamba_ikonu(p, cx, cz, y):
    """Ampul: kehribar yuvarlak, koyu duy."""
    t.levha(p, t.elips(cx, cz + 0.015, 0.042, 0.05, n=12), y, 0.003, "P_Kehribar")
    t.levha(p, dikdortgen(cx - 0.02, cz - 0.06, cx + 0.02, cz - 0.03), y, 0.003, KOYU)


def ayak_izi_ikonu(p, cx, cz, y):
    """İnsan ayak izi: taban ve beş parmak."""
    t.levha(p, t.elips(cx, cz - 0.01, 0.024, 0.05, aci=0.15, n=12), y, 0.003, KOYU)
    for i, (dx, dz, rr) in enumerate(((-0.022, 0.05, 0.011), (-0.006, 0.058, 0.009), (0.008, 0.058, 0.008),
                                      (0.02, 0.053, 0.007), (0.03, 0.045, 0.006))):
        t.levha(p, t.elips(cx + dx, cz + dz, rr, rr, n=8), y, 0.003, KOYU)


def kum_yigintisi(p, x, y, yaricap=0.24):
    p.silindir((x, y, -0.03), (x, y, 0.07), yaricap, 0.09, KUM, kenar=8)


def plaj_tabelasi(r):
    r = random.Random(5301)
    p = k.Parca("Plaj_Tabelasi")
    dx = 0.62
    boy = 2.0
    for x in (-dx, dx):
        t.direk(p, x, 0.06, boy, r, egim=0.0, egim_y=0.0)
        kum_yigintisi(p, x, 0)

    gen, alt, ust = 1.4, 0.95, 1.85
    # Arka tahtalar (direklerin önünde), koyu çerçeve
    tahta = 4
    for i in range(tahta):
        z0 = alt + (ust - alt) * i / tahta
        p.kutu((0, -0.075, z0 + (ust - alt) / tahta / 2), (gen, 0.03, (ust - alt) / tahta - 0.01), AHSAP)
    yuz_y = -0.095
    for x0, z0, x1, z1 in ((-gen / 2 - 0.05, alt - 0.05, gen / 2 + 0.05, alt), (-gen / 2 - 0.05, ust, gen / 2 + 0.05, ust + 0.05),
                           (-gen / 2 - 0.05, alt, -gen / 2, ust), (gen / 2, alt, gen / 2 + 0.05, ust)):
        t.levha(p, dikdortgen(x0, z0, x1, z1), -0.09, 0.03, KOYU)
    # Çatı tahtası (yağmur ve güneş için) ve iki dirsek
    p.kutu((0, -0.07, boy + 0.06), (gen + 0.3, 0.26, 0.04), KOYU)
    for x in (-dx, dx):
        p.kutu((x, -0.1, boy - 0.03), (0.04, 0.12, 0.14), AHSAP)

    # Yüz
    t.levha(p, dikdortgen(-gen / 2, alt, gen / 2, ust), yuz_y, 0.006, "Tabela_Yuzu")
    on = yuz_y - 0.006
    t.levha(p, dikdortgen(-gen / 2, ust - 0.16, gen / 2, ust), on, 0.004, "P_Su_Derin")     # başlık şeridi
    t.levha(p, dikdortgen(-0.36, ust - 0.1, 0.36, ust - 0.06), on - 0.004, 0.002, "P_Kopuk")  # başlık "yazısı"
    t.levha(p, dikdortgen(-gen / 2, alt, gen / 2, alt + 0.07), on, 0.004, YASAK)          # alt şerit

    kaplumbaga(p, -0.37, 1.34, on, 2.4)   # yüzgeç uçları x = -0,69 .. -0,05: çerçevenin içinde
    # Sağda iki yasak ikonu yan yana, altında yazı çizgileri
    lamba_ikonu(p, 0.14, 1.5, on)
    yasak_halkasi(p, 0.14, 1.5, on, 0.095)
    ayak_izi_ikonu(p, 0.44, 1.5, on)
    yasak_halkasi(p, 0.44, 1.5, on, 0.095)
    for i, uzun in enumerate((0.5, 0.42, 0.5, 0.3)):
        z = 1.3 - i * 0.075
        t.levha(p, dikdortgen(0.04, z, 0.04 + uzun, z + 0.03), on, 0.002, "P_Gece_Mavi")
    return p.nesne()


def plaj_tabelasi_kucuk(r):
    r = random.Random(5302)
    p = k.Parca("Plaj_Tabelasi_Kucuk")
    boy = 1.25
    yaricap = 0.055
    t.direk(p, 0.0, yaricap, boy, r, egim=0.0, egim_y=0.0)
    k.kum_tumsek(p, 0.08, 0.32, 0.09, 0.05, KUM, KUM, r, seg=10, bozulma=0.15)

    gen, alt, ust = 0.5, 0.8, 1.22
    p.kutu((0, -yaricap - 0.015, (alt + ust) / 2), (gen + 0.04, 0.03, ust - alt + 0.04), KOYU)
    yuz_y = -yaricap - 0.03
    t.levha(p, dikdortgen(-gen / 2, alt, gen / 2, ust), yuz_y, 0.006, "P_Kopuk")
    on = yuz_y - 0.006
    t.levha(p, dikdortgen(-gen / 2, alt, gen / 2, alt + 0.06), on, 0.004, YASAK)
    t.levha(p, dikdortgen(-gen / 2, ust - 0.06, gen / 2, ust), on, 0.004, "P_Su_Derin")
    kaplumbaga(p, 0.0, 1.0, on, 1.35)
    return p.nesne()
