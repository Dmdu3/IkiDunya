"""Gözcü kulübesi (Toprak'ın "Gözcü Kulübesi" kartı): seçilen bölgede her gece devriye; gönüllülerin kumsalı izlediği yüksek kule.

    Gozcu_Kulubesi   dört ayak üstünde ahşap kulübe. Ayak tabanı 2,3 x 2,3 m, döşeme y = 2,2 m, mahya ~4,55 m,
                     bayrak ucu ~5,3 m. Önde (Blender -y) kuma inen merdiven (~0,9 m öne taşar).
                     Kök tabanın ortasında (ayakların arası); merdiven önü tilki çitinin levhasıyla aynı yön.

Parçalar: eğik ayaklar ve çapraz destekler, tahta döşeme, üç yanı ve kapının iki yanı yarım duvar (korkuluk),
direkler, iki yana inen teal çatı (yönetim haritasında yukarıdan kulübe olarak okunur), ön saçakta yanan fener
(P_Kehribar, ışır; gece devriyesi), mahyada turuncu bayrak, yan duvarda can simidi, ön duvarda kaplumbağa levhası.
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k
import tilki_citi as t

AHSAP = "P_Kaya_Pas"
AHSAP_ACIK = "P_Kaya_Pismis"
KOYU = "P_Kaya_Koyu"
CATI = "P_Kabuk_Teal"
KUM = "P_Kum_Parlak"

ZEMIN = 2.2      # döşemenin üst yüzü
YARI = 1.1       # kulübe yarı genişliği (direkler)
TABAN = 1.15     # ayakların kumdaki yarı aralığı
DUVAR = 1.0      # yarım duvar boyu
CATI_ALT = 4.0   # direklerin tepesi (saçak hizası)
MAHYA = 4.55
SACAK = 1.45     # çatının yarı genişliği (saçak taşması)
KAPI = 0.42      # ön duvardaki kapı boşluğunun yarı genişliği


def kutu8(p, alt, ust, malz):
    """Dört alt, dört üst köşeden kutu (eğik levhalar için)."""
    v = p.yuz([alt, ust], malz)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[1], malz)


def merdiven(p, r):
    """Önde kumdan döşemeye çıkan ahşap merdiven; korkuluğu döşemenin üstüne kadar uzar."""
    alt_y, ust_y = -YARI - 0.9, -YARI - 0.05
    yon = Vector((0, ust_y - alt_y, ZEMIN + 0.05)).normalized()
    for x in (-0.32, 0.32):
        bas = Vector((x, alt_y, -0.08))
        son = Vector((x, ust_y, ZEMIN + 0.05))
        p.tup([bas, son, son + Vector((0, 0.05, 0.85))], 0.04, AHSAP, kenar=4)
    n = 7
    for i in range(1, n + 1):
        f = i / (n + 1)
        y = alt_y + (ust_y - alt_y) * f
        z = -0.08 + (ZEMIN + 0.13) * f
        p.kutu((0, y, z), (0.62, 0.18, 0.04), AHSAP_ACIK)


def can_simidi(p, merkez, yaricap=0.22, kalin=0.05):
    """Yan duvara asılı can simidi (y-z düzleminde): dört dilim, turuncu ve beyaz sırayla."""
    cx, cy, cz = merkez
    for d in range(4):
        noktalar = []
        for s in range(7):
            a = 2 * math.pi * (d + s / 6) / 4
            noktalar.append((cx, cy + math.cos(a) * yaricap, cz + math.sin(a) * yaricap))
        p.tup(noktalar, kalin, "P_Turuncu" if d % 2 == 0 else "P_Kopuk", kenar=6)
    # asma ipi
    p.tup([(cx - 0.01, cy, cz + yaricap + kalin), (cx - 0.03, cy, cz + yaricap + 0.17)], 0.008, KOYU, kenar=4)


def kaplumbaga_levhasi(p, cx, cz, y, gen=0.5, boy=0.38):
    """Ön duvardaki küçük beyaz levha: teal kaplumbağa silüeti, turuncu alt şerit (tilki çiti levhasıyla aynı dil)."""
    p.kutu((cx, y + 0.006, cz), (gen + 0.04, 0.012, boy + 0.04), KOYU)
    yuz = y
    t.levha(p, [(cx - gen / 2, cz - boy / 2), (cx + gen / 2, cz - boy / 2), (cx + gen / 2, cz + boy / 2), (cx - gen / 2, cz + boy / 2)],
            yuz, 0.006, "P_Kopuk")
    on = yuz - 0.006
    t.levha(p, [(cx - gen / 2, cz - boy / 2), (cx + gen / 2, cz - boy / 2), (cx + gen / 2, cz - boy / 2 + 0.06),
                (cx - gen / 2, cz - boy / 2 + 0.06)], on, 0.004, "P_Turuncu")
    ikon = "P_Kabuk_Teal"
    iz = cz + 0.03
    t.levha(p, t.elips(cx, iz, 0.06, 0.075, n=14), on, 0.007, ikon)
    t.levha(p, t.elips(cx, iz + 0.1, 0.025, 0.028, n=10), on, 0.005, ikon)
    for s in (-1, 1):
        t.levha(p, t.elips(cx + s * 0.072, iz + 0.033, 0.055, 0.019, aci=s * 0.45), on, 0.003, ikon)
        t.levha(p, t.elips(cx + s * 0.043, iz - 0.058, 0.032, 0.014, aci=-s * 0.6, n=10), on, 0.003, ikon)


def gozcu_kulubesi(r):
    r = random.Random(5201)
    p = k.Parca("Gozcu_Kulubesi")

    # Ayaklar: kumda geniş, yukarı daralır; dipte ıslak koyu bant ve kum yığıntısı
    for sx in (-1, 1):
        for sy in (-1, 1):
            bas = (sx * TABAN, sy * TABAN, -0.15)
            ust = (sx * (YARI - 0.06), sy * (YARI - 0.06), ZEMIN - 0.1)
            p.silindir(bas, ust, 0.09, 0.075, AHSAP, kenar=6)
            p.silindir((bas[0], bas[1], -0.02), (bas[0], bas[1], 0.22), 0.098, 0.096, KOYU, kenar=6)
            p.silindir((bas[0], bas[1], -0.03), (bas[0], bas[1], 0.07), 0.32, 0.13, KUM, kenar=8)

    # Çapraz destekler: her yanda X, altta yatay kiriş
    def ayak(sx, sy, z):
        f = (z + 0.15) / (ZEMIN - 0.1 + 0.15)
        return Vector((sx * (TABAN + (YARI - 0.06 - TABAN) * f), sy * (TABAN + (YARI - 0.06 - TABAN) * f), z))

    for a, b in (((-1, -1), (1, -1)), ((1, -1), (1, 1)), ((1, 1), (-1, 1)), ((-1, 1), (-1, -1))):
        p.tup([ayak(*a, 0.35), ayak(*b, 1.9)], 0.035, AHSAP_ACIK, kenar=4)
        p.tup([ayak(*b, 0.35), ayak(*a, 1.9)], 0.035, AHSAP_ACIK, kenar=4)
        p.tup([ayak(*a, 0.3), ayak(*b, 0.3)], 0.04, AHSAP, kenar=4)

    # Döşeme: iki taşıyıcı kiriş, üstünde x boyunca tahtalar (iki ton)
    for sy in (-1, 1):
        p.kutu((0, sy * (YARI - 0.06), ZEMIN - 0.12), (2 * YARI + 0.2, 0.14, 0.16), KOYU)
    tahta = 9
    for i in range(tahta):
        y = -YARI - 0.1 + (2 * YARI + 0.2) * (i + 0.5) / tahta
        p.kutu((r.uniform(-0.02, 0.02), y, ZEMIN - 0.025), (2 * YARI + 0.24, (2 * YARI + 0.2) / tahta - 0.012, 0.05),
               AHSAP if i % 2 == 0 else AHSAP_ACIK)

    # Direkler: döşemeden saçağa
    for sx in (-1, 1):
        for sy in (-1, 1):
            p.kutu((sx * YARI, sy * YARI, (ZEMIN + CATI_ALT) / 2), (0.11, 0.11, CATI_ALT - ZEMIN), AHSAP)

    # Yarım duvar: yatay tahtalar, üstte korkuluk başlığı. Ön (-y) yanda kapı boşluğu.
    sira = 3
    for i in range(sira):
        z = ZEMIN + 0.12 + (DUVAR - 0.2) * (i + 0.5) / sira
        malz = AHSAP_ACIK if i % 2 == 0 else AHSAP
        hh = (DUVAR - 0.2) / sira - 0.03
        p.kutu((YARI, 0, z), (0.05, 2 * YARI, hh), malz)
        p.kutu((-YARI, 0, z), (0.05, 2 * YARI, hh), malz)
        p.kutu((0, YARI, z), (2 * YARI, 0.05, hh), malz)
        for s in (-1, 1):
            p.kutu((s * (YARI + KAPI) / 2, -YARI, z), (YARI - KAPI, 0.05, hh), malz)
    bas_z = ZEMIN + DUVAR - 0.04
    p.kutu((YARI, 0, bas_z), (0.1, 2 * YARI + 0.1, 0.07), KOYU)
    p.kutu((-YARI, 0, bas_z), (0.1, 2 * YARI + 0.1, 0.07), KOYU)
    p.kutu((0, YARI, bas_z), (2 * YARI + 0.1, 0.1, 0.07), KOYU)
    for s in (-1, 1):
        p.kutu((s * (YARI + KAPI) / 2, -YARI, bas_z), (YARI - KAPI + 0.05, 0.1, 0.07), KOYU)
    for s in (-1, 1):  # kapı pervazı
        p.kutu((s * KAPI, -YARI, ZEMIN + DUVAR / 2), (0.07, 0.08, DUVAR), AHSAP)

    # Çatı: mahya X boyunca, iki yana (±y) iner; alt yüzde koyu kaplama, altında üçgen alınlıklar
    kal = 0.07
    for sy in (-1, 1):
        alt = [Vector((-SACAK, 0, MAHYA)), Vector((SACAK, 0, MAHYA)), Vector((SACAK, sy * SACAK, CATI_ALT - 0.06)),
               Vector((-SACAK, sy * SACAK, CATI_ALT - 0.06))]
        kutu8(p, alt, [q + Vector((0, 0, kal)) for q in alt], CATI)
        # saçak alnı (koyu şerit, haritada çatının kenarını çizer)
        kutu8(p, [Vector((-SACAK - 0.02, sy * SACAK, CATI_ALT - 0.1)), Vector((SACAK + 0.02, sy * SACAK, CATI_ALT - 0.1)),
                  Vector((SACAK + 0.02, sy * (SACAK + 0.04), CATI_ALT - 0.1)), Vector((-SACAK - 0.02, sy * (SACAK + 0.04), CATI_ALT - 0.1))],
             [Vector((-SACAK - 0.02, sy * SACAK, CATI_ALT + 0.03)), Vector((SACAK + 0.02, sy * SACAK, CATI_ALT + 0.03)),
              Vector((SACAK + 0.02, sy * (SACAK + 0.04), CATI_ALT + 0.03)), Vector((-SACAK - 0.02, sy * (SACAK + 0.04), CATI_ALT + 0.03))], KOYU)
    p.kutu((0, 0, MAHYA + kal + 0.03), (2 * SACAK + 0.1, 0.14, 0.08), KOYU)  # mahya tahtası
    kenar_z = MAHYA - (MAHYA - CATI_ALT + 0.06) * YARI / SACAK   # çatının alt yüzü y = ±YARI'da
    for sx in (-1, 1):
        x = sx * YARI
        ucgen = [(x - 0.03, -YARI, CATI_ALT), (x - 0.03, YARI, CATI_ALT), (x - 0.03, YARI, kenar_z),
                 (x - 0.03, 0, MAHYA - 0.01), (x - 0.03, -YARI, kenar_z)]
        v = p.yuz([ucgen, [(q[0] + 0.06, q[1], q[2]) for q in ucgen]], AHSAP_ACIK)
        p.kapak(v[0], AHSAP_ACIK, ters=True)
        p.kapak(v[1], AHSAP_ACIK)
    for sy in (-1, 1):  # saçak kirişi (direk tepeleri arasında)
        p.kutu((0, sy * YARI, CATI_ALT - 0.04), (2 * YARI + 0.1, 0.12, 0.1), KOYU)

    # Fener: ön saçağın ortasından sarkar (gece devriyesinin ışığı)
    fy = -YARI - 0.12
    p.tup([(0, -YARI, CATI_ALT - 0.08), (0, fy, CATI_ALT - 0.08), (0, fy, CATI_ALT - 0.22)], 0.012, KOYU, kenar=4)
    p.silindir((0, fy, CATI_ALT - 0.25), (0, fy, CATI_ALT - 0.21), 0.07, 0.04, "P_Gece_Mavi", kenar=6)
    p.silindir((0, fy, CATI_ALT - 0.44), (0, fy, CATI_ALT - 0.25), 0.055, 0.065, "P_Kehribar", kenar=6)
    p.silindir((0, fy, CATI_ALT - 0.48), (0, fy, CATI_ALT - 0.44), 0.06, 0.07, "P_Gece_Mavi", kenar=6)

    # Bayrak: mahyanın +x ucunda direk, turuncu flama (iki yüzlü)
    bx = SACAK - 0.15
    p.tup([(bx, 0, MAHYA + 0.05), (bx, 0, MAHYA + 0.8)], 0.02, KOYU, kenar=5)
    flama = [(bx, 0, MAHYA + 0.76), (bx + 0.45, 0.04, MAHYA + 0.66), (bx + 0.05, 0.02, MAHYA + 0.55), (bx, 0, MAHYA + 0.52)]
    i = p.dilim("P_Turuncu")
    for sira_ in (flama, list(reversed(flama))):
        dy = 0.004 if sira_ is flama else -0.004
        f = p.bm.faces.new([p.bm.verts.new((x, y + dy, z)) for x, y, z in sira_])
        f.material_index = i

    merdiven(p, r)
    can_simidi(p, (YARI + 0.09, 0.35, ZEMIN + 0.5))
    kaplumbaga_levhasi(p, -(YARI + KAPI) / 2, ZEMIN + 0.5, -YARI - 0.03)
    return p.nesne()
