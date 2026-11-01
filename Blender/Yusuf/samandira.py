"""Şamandıralar: girdap boşluğunun parlak işaret şamandırası ve deniz bölümlerinin kontrol noktası şamandırası.
(Kumsaldaki çöp şamandırası Samandira.fbx ayrı; ona dokunulmaz.)

    Samandira_Isaret   Çöp girdabı (GriDenizKur.Halka): boşluğun iki yanında, her yükseklikte duran "BoslukIsareti"
                       (gri küre 0,5 m) yerine. ~0,5 m yuvarlak balıkçı şamandırası; ekvatorunda ışıyan kuşak
                       (P_Isik_Parilti), tepede ve dipte ip kulağı, sarkan ip, yosun püskülleri. Suda asılı: KÖK ORTADA.
    Samandira_Kontrol  Bölüm 1 ve 5'in kontrol noktası (GriDenizKur.Kontrol: gri sarı silindir 0,6 x 0,4) yerine.
                       Sarı işaret şamandırası: gövde çapı ~0,64 m, üç ayaklı kafes direk, tepede yanan fener (P_Kehribar),
                       dipte kısa zincir. KÖK SU HİZASINDA (z = 0): gövdenin ~0,3 m'si ve zincir suyun altında, boy ~1,25 m üstte.
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k

k.PALET.setdefault("P_Altin", "#F2D20F")

GOVDE = "P_Turuncu"
PARILTI = "P_Isik_Parilti"
SARI = "P_Altin"
KOYU = "P_Kaya_Koyu"
METAL = "P_Gece_Mavi"
IP = "P_Kopuk"


def kure_bantli(p, merkez, yaricap, bantlar, boylam=14, enlem=10, basik=1.0):
    """Enlem halkalarından küre; bantlar: (alt_enlem, ust_enlem, malzeme) listesi (0 = dip, enlem = tepe).
    Kutuplar tek noktaya kapanır. basik: z ekseninde ölçek."""
    cx, cy, cz = merkez

    def halka(e):
        a = math.pi * e / enlem
        rr = math.sin(a) * yaricap
        z = -math.cos(a) * yaricap * basik
        return [(cx + math.cos(2 * math.pi * b / boylam) * rr, cy + math.sin(2 * math.pi * b / boylam) * rr, cz + z)
                for b in range(boylam)]

    for e0, e1, malz in bantlar:
        lo, hi = max(e0, 1), min(e1, enlem - 1)
        if hi > lo:
            p.yuz([halka(e) for e in range(lo, hi + 1)], malz)
        i = p.dilim(malz)
        if e0 == 0:  # dip kutbu
            uc = [p.bm.verts.new(q) for q in halka(1)]
            o = p.bm.verts.new((cx, cy, cz - yaricap * basik))
            for b in range(boylam):
                p.bm.faces.new((uc[(b + 1) % boylam], uc[b], o)).material_index = i
        if e1 == enlem:  # tepe kutbu
            uc = [p.bm.verts.new(q) for q in halka(enlem - 1)]
            o = p.bm.verts.new((cx, cy, cz + yaricap * basik))
            for b in range(boylam):
                p.bm.faces.new((uc[b], uc[(b + 1) % boylam], o)).material_index = i


def kulak(p, z, yon, malz=KOYU):
    """Şamandıranın tepesinde/dibinde ip kulağı: kısa boyun ve halka (x-z düzleminde)."""
    p.silindir((0, 0, z), (0, 0, z + yon * 0.04), 0.035, 0.03, malz, kenar=8)
    mz = z + yon * 0.075
    halka = [(math.cos(2 * math.pi * s / 10) * 0.035, 0, mz + math.sin(2 * math.pi * s / 10) * 0.035) for s in range(10)]
    p.tup(halka, 0.01, malz, kenar=4, kapali=True)


def samandira_isaret(r):
    r = random.Random(6101)
    p = k.Parca("Samandira_Isaret")
    R = 0.23
    enlem = 10
    # Turuncu gövde, ekvatorda ışıyan kuşak, ince koyu dikiş çizgileri (kalıp birleşimi)
    kure_bantli(p, (0, 0, 0), R, [(0, 4, GOVDE), (4, 6, PARILTI), (6, enlem, GOVDE)], enlem=enlem, basik=0.96)
    for z in (-0.105, 0.105):  # kuşağın kenarlarındaki çıkıntılı sırtlar
        rr = math.sqrt(R * R - (z / 0.96) ** 2) + 0.006
        p.tup([(math.cos(2 * math.pi * s / 16) * rr, math.sin(2 * math.pi * s / 16) * rr, z) for s in range(16)],
              0.008, KOYU, kenar=4, kapali=True)
    kulak(p, R * 0.96 - 0.005, 1)
    kulak(p, -R * 0.96 + 0.005, -1)

    # Dip kulağından sarkan kopmuş ip (akıntıda hafif kıvrık) ve tepe kulağından kısa ip ucu
    alt = -R * 0.96 - 0.11
    # (kısa tutulur: kurulum modeli en uzun kenarına göre ölçekler, uzun ip küreyi küçültür)
    ip = [(0.015 * math.sin(i * 0.9), 0.02 * i * 0.3, alt - 0.035 * i) for i in range(5)]
    ip = [(x + 0.03 * math.sin(i * 0.7), y, z) for i, (x, y, z) in enumerate(ip)]
    p.tup(ip, 0.012, IP, kenar=4)
    ust = R * 0.96 + 0.11
    p.tup([(0, 0, ust), (0.06, 0.01, ust + 0.03), (0.11, 0.03, ust + 0.02)], 0.011, IP, kenar=4)

    # Yosun püskülleri: alt yarıda gövdeye tutunmuş
    for _ in range(7):
        a = r.uniform(0, 2 * math.pi)
        e = r.uniform(-0.85, -0.25)  # alt yarı (sin enlem)
        cz = R * 0.96 * e
        rr = math.sqrt(max(R * R - (cz / 0.96) ** 2, 0)) + 0.005
        bas = Vector((math.cos(a) * rr, math.sin(a) * rr, cz))
        disari = Vector((math.cos(a), math.sin(a), 0))
        boy = r.uniform(0.08, 0.16)
        p.tup([bas, bas + disari * 0.03 + Vector((0, 0, -boy * 0.5)), bas + disari * 0.02 + Vector((r.uniform(-0.03, 0.03), 0, -boy))],
              0.012, "P_Yosun", kenar=3)
    return p.nesne()


def samandira_kontrol(r):
    r = random.Random(6102)
    p = k.Parca("Samandira_Kontrol")
    # Gövde: sudan aşağı daralan alt kısım, su hizasında koyu lastik usturmaça, üstte sarı kubbe-güverte
    p.silindir((0, 0, -0.32), (0, 0, -0.1), 0.14, 0.3, SARI, kenar=12)
    p.silindir((0, 0, -0.1), (0, 0, 0.08), 0.3, 0.32, SARI, kenar=12)
    p.tup([(math.cos(2 * math.pi * s / 16) * 0.335, math.sin(2 * math.pi * s / 16) * 0.335, 0.02) for s in range(16)],
          0.035, KOYU, kenar=5, kapali=True)  # usturmaça
    p.silindir((0, 0, 0.08), (0, 0, 0.16), 0.32, 0.28, GOVDE, kenar=12)  # turuncu kuşak
    p.silindir((0, 0, 0.16), (0, 0, 0.22), 0.28, 0.2, SARI, kenar=12)     # güverte
    p.silindir((0, 0, -0.36), (0, 0, -0.32), 0.07, 0.14, METAL, kenar=8)  # dip ağırlığı

    # Su çizgisinin hemen üstünde ve altında yosun lekesi (gövdeye yapışmış şerit)
    p.silindir((0, 0, -0.1), (0, 0, -0.04), 0.302, 0.306, "P_Yosun", kenar=12)

    # Üç ayaklı kafes direk: güverteden tepeye daralır, iki ara halka
    tepe = 1.0
    ayaklar = []
    for s in range(3):
        a = 2 * math.pi * s / 3 + math.pi / 2
        alt = Vector((math.cos(a) * 0.16, math.sin(a) * 0.16, 0.2))
        ust = Vector((math.cos(a) * 0.06, math.sin(a) * 0.06, tepe))
        ayaklar.append((alt, ust))
        p.tup([alt, ust], 0.018, SARI, kenar=4)
    for z in (0.45, 0.75):
        f = (z - 0.2) / (tepe - 0.2)
        nokta = [a + (u - a) * f for a, u in ayaklar]
        p.tup(nokta, 0.012, SARI, kenar=4, kapali=True)
    # Yansıtıcı bant: direğin ortasında, iki kafes halkası arasında üçgen levha (beyaz, gece ışıkta parlar)
    for s in range(3):
        a0, u0 = ayaklar[s]
        a1, u1 = ayaklar[(s + 1) % 3]
        q = [a0 + (u0 - a0) * 0.42, a1 + (u1 - a1) * 0.42, a1 + (u1 - a1) * 0.6, a0 + (u0 - a0) * 0.6]
        i = p.dilim("P_Kopuk")
        p.bm.faces.new([p.bm.verts.new(v) for v in q]).material_index = i
        p.bm.faces.new([p.bm.verts.new(v) for v in reversed(q)]).material_index = i

    # Fener: tabla, kehribar cam, kafes çubukları, şapka
    p.silindir((0, 0, tepe - 0.02), (0, 0, tepe + 0.02), 0.09, 0.09, METAL, kenar=8)
    p.silindir((0, 0, tepe + 0.02), (0, 0, tepe + 0.16), 0.06, 0.055, "P_Kehribar", kenar=8)
    for s in range(4):
        a = 2 * math.pi * s / 4 + math.pi / 4
        p.tup([(math.cos(a) * 0.068, math.sin(a) * 0.068, tepe + 0.02), (math.cos(a) * 0.063, math.sin(a) * 0.063, tepe + 0.16)],
              0.007, METAL, kenar=4)
    p.silindir((0, 0, tepe + 0.16), (0, 0, tepe + 0.2), 0.08, 0.08, METAL, kenar=8)
    p.silindir((0, 0, tepe + 0.2), (0, 0, tepe + 0.25), 0.08, 0.01, METAL, kenar=8)

    # Dipte kısa zincir: halkalar sırayla 90° döner (çapa hattının başı)
    z = -0.4
    for i in range(6):
        a = 0 if i % 2 == 0 else math.pi / 2
        ca, sa = math.cos(a), math.sin(a)
        n = 8
        h = [(math.cos(2 * math.pi * s / n) * 0.022 * ca, math.cos(2 * math.pi * s / n) * 0.022 * sa,
              z + math.sin(2 * math.pi * s / n) * 0.035) for s in range(n)]
        p.tup(h, 0.007, METAL, kenar=4, kapali=True)
        z -= 0.055
    return p.nesne()
