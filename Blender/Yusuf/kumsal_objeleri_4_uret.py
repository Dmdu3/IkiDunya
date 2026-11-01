"""Kumsal objeleri 4. parti: yosun adası, parlayan plastikler, yuva kafesi, tilki çiti, tekerlek izi (yeni hali).

Her model kendi modülünde (aynı klasörde); mesh yardımcıları, palet, dışa verme ve önizleme
kumsal_objeleri_uret.py'den gelir, düzen aynı (kök tabanda ya da suda asılıysa ortada, metre, Y-up FBX).
    yosun_adasi.py       Yosun_Adasi                    Bölüm 5 finali, 12 m yüzen sargassum; alttan sarkan dallar, yavrunun saklandığı oyuk
    parlayan_plastik.py  Parlayan_Plastik_1..3          Bölüm 5 tehlikesi (gri küre 0,5 x 0,2 x 0,4); kenarları parlar
                         Plastik_Yigini                 çöp girdabının ortasındaki parlayan yığın (gri kutu 2,5 x 1,5 x 2,5)
    yuva_kafesi.py       Yuva_Kafesi                    Kafes görevi; 1,6 m, Yuva_Cukuru'nun tümseğinin dışından geçer
    tilki_citi.py        Tilki_Citi                     Tilki Çiti kartı; 4 m'lik parça, x = -2..+2, 4 m arayla dizilir
                         Tilki_Citi_Kose_Bas / _Kose    hattın başını (x = -2) ve sonunu (x = +2) kapatan levhalı direk
    tekerlek_izi.py      Tekerlek_Izi                   eski şerit yerine: iki yanı kum sırtlı oluk (gri kutu 6 x 0,02 x 0,5)

Her model kendi tohumunu kullanır: sıra değişse de sonuç aynı.

    blender -b -P kumsal_objeleri_4_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]
"""

import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

import kumsal_objeleri_uret as k
import parlayan_plastik
import tekerlek_izi
import tilki_citi
import yosun_adasi
import yuva_kafesi

MODELLER = [
    yosun_adasi.yosun_adasi,
    parlayan_plastik.parlayan_plastik_1, parlayan_plastik.parlayan_plastik_2, parlayan_plastik.parlayan_plastik_3,
    parlayan_plastik.plastik_yigini,
    yuva_kafesi.yuva_kafesi,
    tilki_citi.tilki_citi, tilki_citi.tilki_citi_kose_bas, tilki_citi.tilki_citi_kose,
    tekerlek_izi.tekerlek_izi,
]


def main():
    arg = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    cikti = os.path.abspath(arg[0] if arg else ".")
    blend = arg[1] if len(arg) > 1 else None
    resim = arg[2] if len(arg) > 2 else None
    bpy.ops.wm.read_factory_settings(use_empty=True)
    sahne = bpy.context.scene
    sahne.unit_settings.system = "METRIC"
    sahne.unit_settings.scale_length = 1.0

    r = random.Random(k.TOHUM)
    nesneler = []
    for uret in MODELLER:
        o = uret(r)
        k.unity_duzeni(o)
        nesneler.append(o)
        print(f"{o.name}: {k.ucgen(o)} üçgen, boyut {tuple(round(v, 3) for v in o.dimensions)}")

    os.makedirs(cikti, exist_ok=True)
    for o in nesneler:
        k.disa_ver(o, cikti)

    # Blend dosyasında yan yana dursunlar (ada büyük: arası geniş)
    x = 0.0
    for o in nesneler:
        o.location.x = x + o.dimensions.x / 2
        x += o.dimensions.x + 2.0
    if blend:
        bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath(blend), compress=True)
        print("Kaydedildi:", blend)
    if resim:
        os.makedirs(resim, exist_ok=True)
        for o in nesneler:
            o.location.x = 0
            k.onizleme(o, resim)


if __name__ == "__main__":
    main()
