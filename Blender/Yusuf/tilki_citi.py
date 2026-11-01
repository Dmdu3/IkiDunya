"""Tilki çiti (Toprak'ın "Tilki Çiti" kartı): kumulun önünde, kumsalın kenarı boyunca uzanan koruma çiti.

    Tilki_Citi       4 m'lik düz parça (X boyunca, x = -2 .. +2), kök tabanın ortasında; kod her 4 m'de bir dizer.
                     Direk sadece x = -2'de ve ortada (x = 0); x = +2'deki direk bir sonraki parçanınkidir.
    Tilki_Citi_Kose      hattın SONU: kalın direk, çapraz destek -x yönüne (çitin içine) iner, önde (-y) bilgi levhası.
                         Kök direğin dibinde; son parçanın x = +2 ucuna, dönüşsüz (0°) ve ölçeksiz konur.
    Tilki_Citi_Kose_Bas  hattın BAŞI: aynı direk ve levha, destek +x yönüne iner. İlk parçanın x = -2 ucuna,
                         dönüşsüz (0°) ve ölçeksiz konur; o parçanın eğimsiz x = -2 direği kalın direğin içinde kalır.
                         İki uç da yalnız yer değiştirmeyle konur: aynalama (-1 ölçek) ya da 180° döndürme yok.

Tel örgü ve kum sırtı x = ±2'de aynı biçimde biter, yan yana dizilen parçalar dikişsiz birleşir.
Telin kumdaki eteği z = -0,05'ten başlar (tilki altından kazamasın).
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k

# palet.md'de olup ana betikte olmayanlar
k.PALET.setdefault("P_Tas_Harabe", "#C9B6A3")
k.PALET.setdefault("P_Kabuk_Teal", "#2BA6A0")

AHSAP = "P_Kaya_Pas"
KOYU = "P_Kaya_Koyu"
TEL = "P_Tas_Harabe"
KUM = "P_Kum_Parlak"
KUM_ARKA = "P_Kum_Sicak"

BOY = 4.0          # parça uzunluğu (dizme adımı)
YARI = BOY / 2
TEL_Y = -0.06      # tel örgü direklerin ön yüzüne değer (-y)
RAY_ARKA = 0.066   # rayların arka yüzü (+y): köşe direğinin (r = 0,085) içinde kalır, uçları dışarı taşmaz


def direk(p, x, yaricap, boy, r, egim=0.015, egim_y=0.005, kapak=1.12):
    """Yıpranmış ahşap direk: kumda gömülü dip, ıslak koyu bant, hafif eğik gövde, koyu tepe kapağı.
    y eğimi küçük tutulur ki öndeki tel ve arkadaki raylar direğe hep değsin. Eğimi (ex, ey) döndürür."""
    ex, ey = r.uniform(-egim, egim), r.uniform(-egim_y, egim_y)
    tepe = (x + ex, ey, boy)
    p.silindir((x, 0, -0.1), tepe, yaricap, yaricap * 0.9, AHSAP, kenar=6)
    p.silindir((x, 0, -0.02), (x + ex * 0.15, ey * 0.15, 0.16), yaricap * 1.08, yaricap * 1.05, KOYU, kenar=6)  # ıslak dip
    p.silindir((tepe[0], tepe[1], boy - 0.01), (tepe[0], tepe[1], boy + 0.035), yaricap * kapak, yaricap * 0.55, KOYU, kenar=6)
    return ex, ey


def kum_sirti(p, x0, x1, r, adim=0.25):
    """Çitin dibine savrulmuş alçak kum sırtı (x boyunca). Yükseklik 4 m'de bir tekrar eder; uçlar kapalı.
    Denize bakan yamaç parlak kum, tepeden arkaya inen yamaç sıcak (koyu) kum: haritada çit bir şerit olarak okunur."""
    satirlar = []
    n = int(round((x1 - x0) / adim))
    for i in range(n + 1):
        x = x0 + i * adim
        h = 0.075 + 0.025 * math.sin(math.pi * x / 2 + 0.7) + 0.012 * math.sin(math.pi * x * 1.5)
        on = 0.3 + 0.05 * math.sin(math.pi * x + 1.1)
        arka = 0.24 + 0.03 * math.sin(math.pi * x * 0.5 + 2.0)
        satirlar.append([(x, -on, 0.0), (x, -on * 0.62, h * 0.55), (x, -0.13, h * 0.92), (x, -0.04, h),
                         (x, 0.08, h * 0.8), (x, arka * 0.65, h * 0.35), (x, arka, 0.0)])
    on_v = p.yuz([s[:4] for s in satirlar], KUM, dongu=False)
    arka_v = p.yuz([s[3:] for s in satirlar], KUM_ARKA, dongu=False)
    p.kapak(on_v[0] + arka_v[0][1:], KUM)
    p.kapak(on_v[-1] + arka_v[-1][1:], KUM, ters=True)


def tilki_citi(r):
    """4 m'lik tilki çiti parçası (x = -2 .. +2), boy ~1,1 m: iki ahşap direk (x = -2 ve 0), iki tahta ray,
    sarkık baklava tel örgü (eteği kumda), üst ve alt gergi teli, dipte kum sırtı. Kök tabanın ortasında."""
    r = random.Random(4101)
    p = k.Parca("Tilki_Citi")
    boy = 1.12

    # x = -2 direği eğimsiz: hat başında aynalanmış köşe direğinin içinde tam kalsın
    egimler = {-YARI: direk(p, -YARI, 0.06, boy, r, egim=0.0, egim_y=0.0, kapak=1.25),
               0.0: direk(p, 0.0, 0.06, boy, r, kapak=1.25)}

    # Tahta raylar: direklerin arkasında, uçtan uca (x = +2'de bir sonraki parçanın direğine biner).
    # Üstteki ray geniş tahta: yönetim haritasında yukarıdan sürekli koyu bir çizgi olarak okunur.
    for z, derin in ((0.98, 0.054), (0.5, 0.036)):
        y = RAY_ARKA - derin / 2
        p.kutu((0, y, z), (BOY, derin, 0.085), AHSAP)
        p.kutu((0, y + 0.004, z - 0.036), (BOY, derin - 0.005, 0.015), KOYU)  # rayın altındaki gölge çıtası

    # Tel örgü: üst kenar direkler arasında hafifçe sarkar, ortası biraz dışa şişer
    alt = -0.05

    def ust(x):
        u = ((x + YARI) % 2.0) / 2.0
        return 0.93 - 0.05 * math.sin(math.pi * u)

    def nokta(x, v):
        u = ((x + YARI) % 2.0) / 2.0
        z = alt + v * (ust(x) - alt)
        y = TEL_Y - 0.025 * math.sin(math.pi * u) * math.sin(math.pi * v)
        return (x, y, z)

    goz = 0.25          # baklava genişliği; 4 m'ye tam bölünür, parçalar dikişsiz birleşir
    h = 1.0             # v ekseninde tam yükseklik (~örgü boyu): çizgiler 45°, gözler 0,25 m kare baklava
    adim_v = 0.1
    tel = 0.008
    c = -YARI - h
    while c <= YARI + 1e-6:
        for yon in (1, -1):
            # x = c + yon * v * h, x ∈ [-2, 2], v ∈ [0, 1]
            va = (-YARI - c) / (yon * h)
            vb = (YARI - c) / (yon * h)
            v0, v1 = max(0.0, min(va, vb)), min(1.0, max(va, vb))
            if v1 - v0 < 1e-3:
                continue
            vler = [v0] + [i * adim_v for i in range(11) if v0 + 1e-3 < i * adim_v < v1 - 1e-3] + [v1]
            p.tup([nokta(c + yon * v * h, v) for v in vler], tel, TEL, kenar=3)
        c += goz

    # Gergi telleri: üst kenarda ve kum sırtının hemen üstünde (örgünün çerçevesi; iç gözler uzakta kaybolsa da okunur)
    xs = [-YARI + i * 0.25 for i in range(17)]
    p.tup([nokta(x, 1.0) for x in xs], 0.014, TEL, kenar=4)
    p.tup([nokta(x, (0.15 - alt) / (ust(x) - alt)) for x in xs], 0.013, TEL, kenar=4)

    # Teli direğe tutturan bağlar: direği saran koyu kelepçe bantları (direğin eğimini izler)
    for x in (-YARI, 0.0):
        ex, ey = egimler[x]
        for v in (0.25, 0.6, 0.92):
            z = alt + v * (ust(x) - alt)
            t = (z + 0.1) / (boy + 0.1)
            rr = 0.06 * (1 - 0.1 * t) * 1.16
            p.silindir((x + ex * t, ey * t, z - 0.018), (x + ex * t, ey * t, z + 0.018), rr, rr, KOYU, kenar=6)

    kum_sirti(p, -YARI, YARI, r)
    return p.nesne()


def levha(p, kontur, y, kalinlik, malz):
    """X-Z düzleminde düz bir şekli (kontur: (x, z) noktaları) y'den -y yönüne kalınlık kadar kalıplar."""
    on = [(x, y - kalinlik, z) for x, z in kontur]
    arka = [(x, y, z) for x, z in kontur]
    v = p.yuz([arka, on], malz)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[1], malz)


def elips(cx, cz, rx, rz, aci=0.0, n=12):
    """(cx, cz) merkezli, aci kadar dönmüş elips konturu."""
    ca, sa = math.cos(aci), math.sin(aci)
    out = []
    for i in range(n):
        t = 2 * math.pi * i / n
        x, z = math.cos(t) * rx, math.sin(t) * rz
        out.append((cx + x * ca - z * sa, cz + x * sa + z * ca))
    return out


def kose(ad, yon, tohum):
    """Çit hattını kapatan uç direk (boy ~1,4 m): kalın direk, yon (-1 / +1) x yönüne inen çapraz destek ve kazığı,
    önde (-y) ara takozlarla direkten öne alınmış beyaz bilgi levhası (teal kaplumbağa, turuncu şerit),
    dipte kum sırtının ucunu örten kum yığıntısı. Kök direğin dibinde."""
    r = random.Random(tohum)
    p = k.Parca(ad)
    yaricap = 0.085
    boy = 1.38
    direk(p, 0.0, yaricap, boy, r, egim=0.0)

    # Çapraz destek: direğin arka yüzünden çitin içine doğru kuma iner (rayların arkasından geçer), ucunda kısa kazık
    dy = 0.105
    p.tup([(yon * 0.02, dy, 0.85), (yon * 1.02, dy, 0.03)], 0.035, AHSAP, kenar=4)
    p.silindir((yon * 1.07, dy, -0.05), (yon * 1.07, dy, 0.14), 0.045, 0.035, KOYU, kenar=5)
    p.kutu((yon * 0.01, 0.075, 0.85), (0.06, 0.07, 0.06), KOYU)  # bağlantı takozu

    # Bilgi levhası: tel örgünün ve gergi telinin önünde kalsın diye direkten öne alınır (iki ara takoz)
    on_yuz = -yaricap * 0.866
    y0 = -0.10
    gen, alt, ust = 0.56, 0.78, 1.18
    for z in (alt + 0.08, ust - 0.08):
        p.kutu((0, (on_yuz + y0) / 2 + 0.005, z), (0.07, on_yuz - y0 + 0.01, 0.06), KOYU)
    p.kutu((0, y0 - 0.012, (alt + ust) / 2), (gen + 0.04, 0.024, ust - alt + 0.04), KOYU)
    yuz_y = y0 - 0.024
    levha(p, [(-gen / 2, alt), (gen / 2, alt), (gen / 2, ust), (-gen / 2, ust)], yuz_y, 0.006, "P_Kopuk")
    on_y = yuz_y - 0.006
    levha(p, [(-gen / 2, alt), (gen / 2, alt), (gen / 2, alt + 0.065), (-gen / 2, alt + 0.065)], on_y, 0.004, "P_Turuncu")

    # Kaplumbağa silüeti (yukarı yüzer): kabuk, baş, dört yüzgeç
    ikon = "P_Kabuk_Teal"
    cz = 0.985
    # (üst üste binen parçaların ön yüzleri ayrı düzlemde: kabuk en önde, yüzgeçler arkada)
    levha(p, elips(0, cz, 0.062, 0.078, n=14), on_y, 0.007, ikon)
    levha(p, elips(0, cz + 0.105, 0.026, 0.03, n=10), on_y, 0.005, ikon)
    for s in (-1, 1):
        levha(p, elips(s * 0.075, cz + 0.035, 0.058, 0.02, aci=s * 0.45), on_y, 0.003, ikon)
        levha(p, elips(s * 0.045, cz - 0.06, 0.034, 0.015, aci=-s * 0.6, n=10), on_y, 0.003, ikon)

    # Dipte kum yığıntısı: çit parçasının kum sırtı ucundaki kesiti örter
    k.kum_tumsek(p, 0.1, 0.42, 0.125, 0.06, KUM, KUM, r, seg=12, bozulma=0.15)
    return p.nesne()


def tilki_citi_kose(r):
    """Hattın sonu (son parçanın x = +2 ucu): destek -x'e iner. Boy ~1,4 m, kök direğin dibinde."""
    return kose("Tilki_Citi_Kose", -1, 4102)


def tilki_citi_kose_bas(r):
    """Hattın başı (ilk parçanın x = -2 ucu): destek +x'e iner. Boy ~1,4 m, kök direğin dibinde."""
    return kose("Tilki_Citi_Kose_Bas", 1, 4102)
