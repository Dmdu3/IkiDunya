"""Kumsal objeleri 5. parti: kum torbası, gözcü kulübesi, plaj bilgilendirme tabelası (Toprak'ın kartları için).

Her model kendi modülünde (aynı klasörde); mesh yardımcıları, palet, dışa verme ve önizleme
kumsal_objeleri_uret.py'den gelir, düzen aynı (kök tabanın ortasında, metre, Y-up FBX).
    kum_torbasi.py     Kum_Torbasi            tek çuval, 0,6 x 0,36 x 0,13 m
                       Kum_Torbasi_Seddi      Kum Torbası Seddi kartı; 2 m'lik parça, x = -1..+1, 2 m arayla dizilir, boy ~0,35 m
    gozcu_kulubesi.py  Gozcu_Kulubesi         Gözcü Kulübesi kartı; 2,3 m tabanlı, döşemesi 2,2 m'de ahşap kule, önde merdiven, fener
    plaj_tabelasi.py   Plaj_Tabelasi          Plaj Tabelaları kartı; iki direkli 1,4 x 0,9 m pano, boy ~2 m
                       Plaj_Tabelasi_Kucuk    tek direkli küçük levha, boy ~1,3 m (bölgelere serpiştirmek için)

Her model kendi tohumunu kullanır: sıra değişse de sonuç aynı. Önü (merdiven, pano yüzü) Blender'da -y.

    blender -b -P kumsal_objeleri_5_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]
örnek (repo kökünden):
    blender -b -P Blender/Yusuf/kumsal_objeleri_5_uret.py -- Assets/_Project/Models/Kumsal Blender/Yusuf/kumsal_objeleri_5.blend
"""

import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

import gozcu_kulubesi
import kum_torbasi
import kumsal_objeleri_uret as k
import plaj_tabelasi

MODELLER = [
    kum_torbasi.kum_torbasi, kum_torbasi.kum_torbasi_seddi,
    gozcu_kulubesi.gozcu_kulubesi,
    plaj_tabelasi.plaj_tabelasi, plaj_tabelasi.plaj_tabelasi_kucuk,
]


def main():
    arg = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    cikti = os.path.abspath(arg[0] if arg else ".")
    blend = arg[1] if len(arg) > 1 else None
    resim = os.path.abspath(arg[2]) if len(arg) > 2 else None
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

    # Blend dosyasında yan yana dursunlar
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
