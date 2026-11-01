"""Caretta caretta yumurtaları: tek yumurta ve kum çanağındaki yumurta kümesi.

Gerçek boy: yumurta neredeyse küre, çapı 4-4,5 cm, deri gibi yumuşak kabuklu; birkaçı hafif göçük.
Kümede GDD'deki gibi 12 yumurta (7 + 3 + 1 üst üste, biri yana yuvarlanmış), çevresinde kazılmış
kumun yığıldığı alçak bir çanak. Renkler paletten: yumurta P_Kopuk, kum P_Kum_Bej.

Çalıştırma (Blender 5.2, arka planda):
    blender -b -P yumurta_kumesi_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]

Mesh yardımcıları, dışa verme ve önizleme kumsal_objeleri_uret.py'den gelir; düzen aynı
(kök tabanın ortasında, metre, Unity'de dik durur). Aynı tohumla her seferinde aynı sonuç çıkar.
"""

import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy
from mathutils import Euler, Matrix, Vector

from kumsal_objeleri_uret import Parca, disa_ver, onizleme, ucgen, unity_duzeni

TOHUM = 7
KABUK = "P_Kopuk"
KUM = "P_Kum_Bej"


def yumurta_ekle(p, r, merkez, yaricap, gocuk):
    """Hafif basık küre; gocuk True ise rastgele bir yanında yumuşak çöküntü."""
    import bmesh
    i = p.dilim(KABUK)
    yeni = bmesh.ops.create_uvsphere(p.bm, u_segments=14, v_segments=9, radius=yaricap)["verts"]
    yon = Vector((r.uniform(-1, 1), r.uniform(-1, 1), r.uniform(-0.3, 1))).normalized()
    for v in yeni:
        v.co.z *= 0.95
        if gocuk:
            d = v.co.normalized().dot(yon)
            if d > 0.55:
                v.co *= 1 - 0.18 * ((d - 0.55) / 0.45) ** 1.5
    don = Euler((r.uniform(0, math.pi), r.uniform(0, math.pi), r.uniform(0, math.pi))).to_matrix().to_4x4()
    bmesh.ops.transform(p.bm, matrix=Matrix.Translation(merkez) @ don, verts=yeni)
    for f in {f for v in yeni for f in v.link_faces}:
        f.material_index = i


def kum_canagi(p, r):
    """Yumurtaların etrafında kazılmış kumdan alçak, düzensiz set; dış kenar zeminde (z = 0)."""
    kenar = 28
    ic, tepe, dis = 0.10, 0.135, 0.23
    dalga = [(r.uniform(0, 2 * math.pi), r.uniform(0.15, 0.3), k) for k in (2, 3, 5)]

    def yukseklik(a):
        return 0.035 * (1 + sum(g * math.sin(k * a + f) for f, g, k in dalga))

    halkalar = []
    for t in (0.0, 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0):
        satir = []
        for s in range(kenar):
            a = 2 * math.pi * s / kenar
            if t <= 1:  # iç yamaç
                rr = ic + (tepe - ic) * t
                z = 0.005 + (yukseklik(a) - 0.005) * math.sin(t * math.pi / 2)
            else:       # dış yamaç
                u = t - 1
                rr = tepe + (dis - tepe) * u
                z = yukseklik(a) * (1 - u) ** 2
            satir.append((math.cos(a) * rr, math.sin(a) * rr, z))
        halkalar.append(satir)
    v = p.yuz(halkalar, KUM)
    # Çanak tabanı: yumurtaların altında kalan kum; zeminle çakışmasın diye biraz yukarıda
    merkez = p.bm.verts.new((0, 0, 0.005))
    i = p.dilim(KUM)
    for s in range(kenar):
        f = p.bm.faces.new((merkez, v[0][s], v[0][(s + 1) % kenar]))
        f.material_index = i


def yumurta(r):
    p = Parca("Yumurta")
    yaricap = 0.0215
    yumurta_ekle(p, r, Vector((0, 0, yaricap * 0.95)), yaricap, gocuk=False)
    return p.nesne()


def yumurta_kumesi(r):
    p = Parca("Yumurta_Kumesi")
    kum_canagi(p, r)

    yr = 0.0215
    d = 2 * yr                          # alt katta komşu yumurtaların merkez uzaklığı
    oyuk = d / math.sqrt(3)             # üç yumurtanın arasındaki oyuğun merkeze uzaklığı
    kat = math.sqrt(d * d - oyuk * oyuk)
    gom = 0.006                         # alt kat kuma biraz gömülü
    yerler = [Vector((0, 0, yr - gom))]
    yerler += [Vector((math.cos(a) * d, math.sin(a) * d, yr - gom)) for a in (math.radians(60 * k) for k in range(6))]
    yerler += [Vector((math.cos(a) * oyuk, math.sin(a) * oyuk, yr - gom + kat)) for a in (math.radians(30 + 120 * k) for k in range(3))]
    yerler.append(Vector((0, 0, yr - gom + 2 * kat)))
    yerler.append(Vector((math.cos(2.4) * 0.085, math.sin(2.4) * 0.085, yr + 0.002)))  # yana yuvarlanmış

    for k, m in enumerate(yerler):
        m = m + Vector((r.uniform(-0.002, 0.002), r.uniform(-0.002, 0.002), 0))
        yumurta_ekle(p, r, m, yr * r.uniform(0.95, 1.04), gocuk=k in (2, 5, 8, 11))
    return p.nesne()


MODELLER = [yumurta, yumurta_kumesi]


def main():
    arg = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    cikti = os.path.abspath(arg[0] if arg else ".")
    blend = arg[1] if len(arg) > 1 else None
    resim = arg[2] if len(arg) > 2 else None
    bpy.ops.wm.read_factory_settings(use_empty=True)
    sahne = bpy.context.scene
    sahne.unit_settings.system = "METRIC"
    sahne.unit_settings.scale_length = 1.0

    r = random.Random(TOHUM)
    nesneler = []
    for uret in MODELLER:
        o = uret(r)
        for f in o.data.polygons:
            f.use_smooth = True
        unity_duzeni(o)
        nesneler.append(o)
        print(f"{o.name}: {ucgen(o)} üçgen, boyut {tuple(round(v, 3) for v in o.dimensions)}")

    os.makedirs(cikti, exist_ok=True)
    for o in nesneler:
        disa_ver(o, cikti)

    for i, o in enumerate(nesneler):
        o.location.x = i * 0.5
    if blend:
        bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath(blend), compress=True)
        print("Kaydedildi:", blend)
    if resim:
        os.makedirs(resim, exist_ok=True)
        for o in nesneler:
            o.location.x = 0
            onizleme(o, resim)


if __name__ == "__main__":
    main()
