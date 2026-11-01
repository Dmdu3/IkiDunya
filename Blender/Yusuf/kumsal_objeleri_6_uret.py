"""Kumsal objeleri 6. parti: şamandıralar (çöp girdabının işaret şamandırası, deniz bölümlerinin kontrol şamandırası).

Mesh yardımcıları, palet, dışa verme ve önizleme kumsal_objeleri_uret.py'den gelir (metre, Y-up FBX).
    samandira.py   Samandira_Isaret    girdap boşluğunun parlak şamandırası (gri küre 0,5 m yerine); KÖK ORTADA
                   Samandira_Kontrol   kontrol noktası şamandırası (gri silindir 0,6 x 0,4 yerine); KÖK SU HİZASINDA

    blender -b -P kumsal_objeleri_6_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]
örnek (repo kökünden):
    blender -b -P Blender/Yusuf/kumsal_objeleri_6_uret.py -- Assets/Sandbox/Yusuf/KumsalObjeleri Blender/Yusuf/kumsal_objeleri_6.blend
"""

import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

import kumsal_objeleri_uret as k
import samandira

MODELLER = [samandira.samandira_isaret, samandira.samandira_kontrol]

k.MERKEZLI.add("Samandira_Isaret")  # önizlemede su altında çizilsin


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

    x = 0.0
    for o in nesneler:
        o.location.x = x + o.dimensions.x / 2
        x += o.dimensions.x + 1.0
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
