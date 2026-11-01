"""Kum torbası (Toprak'ın "Kum Torbası Seddi" kartı): fırtınada yuvaları sudan koruyan çuval seddi.

    Kum_Torbasi         tek dolu çuval, ~0,6 x 0,36 x 0,13 m; bir ucu iple boğulmuş. Kök tabanın ortasında, uzun kenar X'te.
    Kum_Torbasi_Seddi   2 m'lik sed parçası (x = -1 .. +1), üç sıra şaşırtmalı çuval, boy ~0,35 m, derinlik ~0,8 m.
                        Kod her 2 m'de bir dizer; dönüşsüz ve ölçeksiz konur. Kök tabanın ortasında, sed X boyunca.

Dizme: alt ve üst sıranın çuvalları x = -0,75, -0,25, 0,25, 0,75'te; orta sıra yarım adım kaymış (x = -1, -0,5, 0, 0,5).
x = -1'deki orta çuval bir önceki parçaya taşar, x = +1'deki ise bir sonraki parçanın x = -1 çuvalıdır: yan yana dizilen
parçalar dikişsiz birleşir. Hattın en sonunda orta sırada yarım çuvallık boşluk kalır (sed ucu basamaklı görünür).
Çuvallar 0,5 m arayla dizilir ama 0,6 m uzunluğundadır: birbirine basmış, ezilmiş durur.
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k

CUVAL = "P_Kaya_Pismis"    # çuval bezi (kumun önünde koyu okunur)
CUVAL_SOLUK = "P_Kaya_Okra"  # güneşte solmuş çuval
IP = "P_Kaya_Pas"

ADIM = 0.5   # çuval dizme adımı
BOY = 2.0    # sed parçası uzunluğu


def torba(p, merkez, aci, r, boy=0.6, en=0.36, yukseklik=0.13, malz=CUVAL, bagli=True):
    """Dolu çuval: yastık biçimli, üstü şişkin, altı ezilip yassılmış. aci: Z çevresinde dönüş.
    bagli ise +x ucu iple boğulmuş (ip bilezik ve düğüm). Tabanı merkez'in z'sinde."""
    cx, cy, cz = merkez
    ca, sa = math.cos(aci), math.sin(aci)
    ust = yukseklik / 1.35          # orta çizgiden tepeye
    alt = ust * 0.35                 # orta çizgiden tabana (yassı)
    seg_u, seg_v = 9, 10
    sis = [r.uniform(-0.004, 0.004) for _ in range(seg_u * seg_v)]

    def yer(x, y, z):
        return Vector((cx + x * ca - y * sa, cy + x * sa + y * ca, cz + alt + z))

    def kesit(u, olcek=1.0, kayma=0.0):
        """u = -1 .. 1 boyunca çuvalın enine kesiti (halka)."""
        f = (1 - abs(u) ** 3) ** (1 / 3)
        if u > 0.6 and bagli:                  # boğum: bağlı uca doğru daha çok daralır
            f *= 1 - 0.45 * (u - 0.6) / 0.4
        halka = []
        for j in range(seg_v):
            a = 2 * math.pi * j / seg_v
            y = math.cos(a) * en / 2 * f * olcek
            z = math.sin(a) * f * olcek
            z = z * ust if z >= 0 else z * alt
            i = int((u + 1) / 2 * (seg_u - 1)) * seg_v + j
            halka.append(yer(u * boy / 2, y, z + sis[min(i, len(sis) - 1)] + kayma))
        return halka

    halkalar = [kesit(-1 + 2 * i / seg_u) for i in range(1, seg_u)]
    v = p.yuz(halkalar, malz)
    # Uçlar: tek noktaya kapanan yelpaze
    m = p.dilim(malz)
    for uc, u in ((v[0], -1.0), (v[-1], 1.0)):
        o = p.bm.verts.new(yer(u * boy / 2 * (0.97 if bagli and u > 0 else 1.0), 0, (ust - alt) * 0.15))
        for j in range(seg_v):
            f = p.bm.faces.new((uc[j], uc[(j + 1) % seg_v], o))
            f.material_index = m

    if bagli:
        # İp bilezik: boğumu saran kapalı halka, ve ucunda küçük düğüm ile sarkan ip
        u = 1 - 2 / seg_u              # son halka: gövde buradan uca daralır, bilezik boğuma oturur
        bilezik = [Vector(q) for q in kesit(u, olcek=1.08)]
        p.tup(bilezik, 0.012, IP, kenar=4, kapali=True)
        uc = yer(boy / 2 * 0.97, 0, (ust - alt) * 0.15)
        p.tup([uc, uc + Vector((ca * 0.05, sa * 0.05, -0.01)), uc + Vector((ca * 0.08 - sa * 0.03, sa * 0.08 + ca * 0.03, -0.04))],
              0.01, IP, kenar=4)


def kum_torbasi(r):
    """Tek çuval (kumda tek başına ya da yığın halinde kullanılabilir). Kök tabanın ortasında."""
    r = random.Random(5101)
    p = k.Parca("Kum_Torbasi")
    torba(p, (0, 0, 0), 0.0, r)
    return p.nesne()


def kum_torbasi_seddi(r):
    """2 m'lik çuval seddi (x = -1 .. +1): altta iki sıra (önde ve arkada), ortada iki sıra yarım adım kaymış,
    tepede tek sıra. Bağlı uçlar dışarıdan görünmez diye sadece öndeki ve tepedeki çuvallar bağlıdır."""
    r = random.Random(5102)
    p = k.Parca("Kum_Torbasi_Seddi")
    h = 0.13
    oturma = 0.8   # üstteki sıra alttakinin arasına çöker
    siralar = [
        # (z, y'ler, x başlangıcı)
        (0.0, (-0.2, 0.2), -0.75),
        (h * oturma, (-0.11, 0.13), -1.0),
        (2 * h * oturma, (0.01,), -0.75),
    ]
    for sira, (z, yler, x0) in enumerate(siralar):
        for y in yler:
            for i in range(4):
                x = x0 + i * ADIM
                malz = CUVAL_SOLUK if r.random() < 0.3 else CUVAL
                bagli = sira == 2 or y < 0
                torba(p, (x + r.uniform(-0.02, 0.02), y + r.uniform(-0.02, 0.02), z + r.uniform(-0.01, 0.005)),
                      r.uniform(-0.07, 0.07) + (math.pi if r.random() < 0.5 and not bagli else 0.0), r,
                      boy=0.6 * r.uniform(0.96, 1.04), en=0.36 * r.uniform(0.95, 1.05), yukseklik=h * r.uniform(0.93, 1.07),
                      malz=malz, bagli=bagli)
    return p.nesne()
