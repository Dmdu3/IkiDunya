"""Kumsal objeleri (2. parti): ağ parçası, plastik halka, ışık direği, gölgelik.

Ölçüler oyundaki gri kutulardan (Editor/GriKumsalKur.cs):
    Ag            TuzakKalibi("Ag", 0,8 x 0,02 x 0,8)          kumda yatan balıkçı ağı
    Plastik_Halka TrashTrap.Turu.Halka                          altılı içecek halkası (gerçek boy)
    Isik_Diregi   IsikKur: direk 0,2 x 3 x 0,2, lamba y = 3     lamba ışığın durduğu yerde
    Golgelik      GolgelikKalibiKur: direkler ±1,5, boy 2; tente 3,4 x 3,4, y = 2

Çalıştırma (Blender 5.2, arka planda):
    blender -b -P kumsal_objeleri_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]
ya da pip'teki bpy modülüyle:
    python kumsal_objeleri_uret.py -- <fbx_klasoru> [blend_yolu] [onizleme_klasoru]

Düzen öteki kumsal objeleriyle (Sezlong, Kutu) aynı: kök tabanın ortasında, metre, iskelet yok;
mesh verisi Y-up, nesne X'te +90° döner, FBX varsayılan ayarlarla yazılır (Unity'de dik durur).
Renkler docs/palet.md'den (P_ adlarıyla). Aynı tohumla her seferinde aynı sonuç çıkar.
"""

import math
import os
import random
import sys

import bpy  # bmesh, pip bpy modülünde bpy'den sonra yüklenir
import bmesh
from mathutils import Matrix, Vector

TOHUM = 11

PALET = {
    "P_Su_Derin": "#167FA1",
    "P_Su_Turkuaz": "#19BEC4",
    "P_Kopuk": "#F5F3EF",
    "P_Turuncu": "#ED7208",
    "P_Kehribar": "#F2A94C",
    "P_Gece_Mavi": "#1E2A5A",
    "P_Kaya_Okra": "#DFAC6A",
    "P_Kaya_Pas": "#99471E",
    "P_Kum_Bej": "#EFCC92",  # sadece önizlemedeki kum
}
ISIYAN = {"P_Kehribar": 3.0}  # lamba camı


def hex_lineer(h):
    h = h.lstrip("#")
    out = []
    for i in (0, 2, 4):
        c = int(h[i:i + 2], 16) / 255.0
        out.append(c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4)
    return (*out, 1.0)


def malzeme(ad):
    m = bpy.data.materials.get(ad)
    if m:
        return m
    m = bpy.data.materials.new(ad)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    renk = hex_lineer(PALET[ad])
    p.inputs["Base Color"].default_value = renk
    p.inputs["Roughness"].default_value = 0.8
    if ad in ISIYAN:
        p.inputs["Emission Color"].default_value = renk
        p.inputs["Emission Strength"].default_value = ISIYAN[ad]
    m.diffuse_color = renk
    return m


# ---------------------------------------------------------------- mesh yardımcıları

class Parca:
    """Tek nesne, birden çok malzeme: her parça bir malzeme dilimine yazılır."""

    def __init__(self, ad):
        self.ad = ad
        self.bm = bmesh.new()
        self.dilimler = []

    def dilim(self, malz):
        if malz not in self.dilimler:
            self.dilimler.append(malz)
        return self.dilimler.index(malz)

    def yuz(self, koseler, malz, dongu=True):
        """koseler: halkalar listesi (her halka aynı sayıda nokta). Halkaları birbirine bağlar."""
        i = self.dilim(malz)
        v = [[self.bm.verts.new(p) for p in h] for h in koseler]
        n = len(v[0])
        son = n if dongu else n - 1
        for a in range(len(v) - 1):
            for b in range(son):
                c = (b + 1) % n
                f = self.bm.faces.new((v[a][b], v[a][c], v[a + 1][c], v[a + 1][b]))
                f.material_index = i
        return v

    def kapak(self, halka, malz, ters=False):
        f = self.bm.faces.new(list(reversed(halka)) if ters else halka)
        f.material_index = self.dilim(malz)

    def tup(self, noktalar, yaricap, malz, kenar=4, kapali=False):
        """Noktalar boyunca ince tüp (ip, tel)."""
        halkalar = []
        for k, p in enumerate(noktalar):
            a = noktalar[max(k - 1, 0)]
            b = noktalar[min(k + 1, len(noktalar) - 1)]
            t = (Vector(b) - Vector(a)).normalized()
            yardim = Vector((0, 0, 1)) if abs(t.z) < 0.9 else Vector((1, 0, 0))
            u = t.cross(yardim).normalized()
            w = t.cross(u).normalized()
            halkalar.append([Vector(p) + (u * math.cos(2 * math.pi * s / kenar) + w * math.sin(2 * math.pi * s / kenar)) * yaricap
                             for s in range(kenar)])
        if kapali:
            halkalar.append(halkalar[0])
        v = self.yuz(halkalar, malz)
        if not kapali:
            self.kapak(v[0], malz, ters=True)
            self.kapak(v[-1], malz)

    def silindir(self, alt, ust, r_alt, r_ust, malz, kenar=8, kapakli=True):
        alt, ust = Vector(alt), Vector(ust)
        h = [[alt + Vector((math.cos(2 * math.pi * s / kenar) * r_alt, math.sin(2 * math.pi * s / kenar) * r_alt, 0)) for s in range(kenar)],
             [ust + Vector((math.cos(2 * math.pi * s / kenar) * r_ust, math.sin(2 * math.pi * s / kenar) * r_ust, 0)) for s in range(kenar)]]
        v = self.yuz(h, malz)
        if kapakli:
            self.kapak(v[0], malz, ters=True)
            self.kapak(v[1], malz)

    def kutu(self, merkez, boyut, malz):
        x, y, z = (s / 2 for s in boyut)
        c = Vector(merkez)
        alt = [c + Vector(p) for p in ((-x, -y, -z), (x, -y, -z), (x, y, -z), (-x, y, -z))]
        ust = [c + Vector(p) for p in ((-x, -y, z), (x, -y, z), (x, y, z), (-x, y, z))]
        v = self.yuz([alt, ust], malz)
        self.kapak(v[0], malz, ters=True)
        self.kapak(v[1], malz)

    def nesne(self):
        me = bpy.data.meshes.new(self.ad)
        bmesh.ops.remove_doubles(self.bm, verts=self.bm.verts, dist=1e-5)
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces)
        self.bm.to_mesh(me)
        self.bm.free()
        for m in self.dilimler:
            me.materials.append(malzeme(m))
        o = bpy.data.objects.new(self.ad, me)
        bpy.context.scene.collection.objects.link(o)
        return o


# ---------------------------------------------------------------- modeller

def ag(r):
    """Kumda yatan, buruşmuş balıkçı ağı: düzensiz kenar, iki mantar şamandıra, sarkan ip ucu."""
    p = Parca("Ag")
    yari = 0.42
    goz = 0.06            # ağ gözü
    ip = 0.0045

    tepeler = [(r.uniform(-yari, yari), r.uniform(-yari, yari), r.uniform(0.02, 0.05), r.uniform(0.12, 0.25)) for _ in range(5)]

    def yukseklik(x, y):
        h = 0.004
        for cx, cy, a, s in tepeler:
            h += a * math.exp(-((x - cx) ** 2 + (y - cy) ** 2) / (2 * s * s))
        return h

    # Düzensiz kenar: açıya göre yarıçap
    loblar = [(r.uniform(0, 2 * math.pi), r.uniform(0.05, 0.11), r.randint(2, 4)) for _ in range(3)]

    def icinde(x, y):
        a = math.atan2(y, x)
        sinir = 0.33 + sum(g * math.sin(k * a + f) for f, g, k in loblar)
        return math.hypot(x, y) < sinir

    adim = goz / 3
    for eksen in (0, 1):
        k = -yari
        while k <= yari + 1e-6:
            seg, parcalar = [], []
            t = -yari
            while t <= yari + 1e-6:
                x, y = (t, k) if eksen == 0 else (k, t)
                x += r.uniform(-0.004, 0.004)
                y += r.uniform(-0.004, 0.004)
                if icinde(x, y):
                    seg.append((x, y, yukseklik(x, y)))
                elif len(seg) > 1:
                    parcalar.append(seg)
                    seg = []
                else:
                    seg = []
                t += adim
            if len(seg) > 1:
                parcalar.append(seg)
            for s in parcalar:
                p.tup(s, ip, "P_Su_Derin", kenar=3)
            k += goz

    # Mantar şamandıralar (kenarda)
    for a in (0.6, 3.4):
        cx, cy = math.cos(a) * 0.3, math.sin(a) * 0.3
        z = yukseklik(cx, cy)
        p.silindir((cx, cy, z - 0.005), (cx, cy, z + 0.035), 0.03, 0.026, "P_Turuncu", kenar=8)

    # Kopmuş ip ucu
    bas = Vector((-0.28, -0.22, 0.0))
    nokta = [bas + Vector((-0.04 * i, -0.025 * i + 0.012 * math.sin(i * 1.3), 0.004 + 0.003 * math.sin(i))) for i in range(9)]
    p.tup([(v.x, v.y, v.z + yukseklik(bas.x, bas.y) * (1 - i / 8)) for i, v in enumerate(nokta)], 0.007, "P_Kaya_Okra", kenar=5)
    return p.nesne()


def plastik_halka(r):
    """Altılı içecek halkası: 3 x 2 göz, hafif buruşuk, yassı."""
    p = Parca("Plastik_Halka")
    rx, ry = 0.036, 0.031  # göz yarıçapları
    bant = 0.006
    kalin = 0.002
    seg = 18
    for i in range(3):
        for j in range(2):
            cx = (i - 1) * (2 * rx + bant * 0.6)
            cy = (j - 0.5) * (2 * ry + bant * 0.6)
            egim = r.uniform(-0.06, 0.06)
            dis, ic = [], []
            for s in range(seg):
                a = 2 * math.pi * s / seg
                c, si = math.cos(a), math.sin(a)
                z = 0.003 + egim * c * rx + 0.002 * math.sin(3 * a + i)
                dis.append((cx + c * (rx + bant / 2), cy + si * (ry + bant / 2), z))
                ic.append((cx + c * (rx - bant / 2), cy + si * (ry - bant / 2), z))
            ust_dis = [(x, y, z + kalin) for x, y, z in dis]
            ust_ic = [(x, y, z + kalin) for x, y, z in ic]
            p.yuz([ic, dis, ust_dis, ust_ic, ic], "P_Kopuk")
    return p.nesne()


def isik_diregi(r):
    """Kumsal arkasındaki yol lambası: taban, ince direk, fener başlığı. Fenerin ortası y = 3 (ışık orada)."""
    p = Parca("Isik_Diregi")
    metal = "P_Gece_Mavi"
    p.silindir((0, 0, 0), (0, 0, 0.12), 0.16, 0.14, metal, kenar=8)
    p.silindir((0, 0, 0.12), (0, 0, 0.3), 0.09, 0.07, metal, kenar=8)
    p.silindir((0, 0, 0.3), (0, 0, 2.78), 0.055, 0.042, metal, kenar=8)
    p.silindir((0, 0, 2.62), (0, 0, 2.7), 0.06, 0.06, metal, kenar=8)       # süs halkası
    p.silindir((0, 0, 2.78), (0, 0, 2.84), 0.07, 0.1, metal, kenar=6)       # fener tablası
    p.silindir((0, 0, 2.84), (0, 0, 3.14), 0.1, 0.13, "P_Kehribar", kenar=6)  # cam
    for s in range(6):                                                        # cam çerçevesi
        a = 2 * math.pi * s / 6
        p.tup([(math.cos(a) * 0.104, math.sin(a) * 0.104, 2.84), (math.cos(a) * 0.134, math.sin(a) * 0.134, 3.14)], 0.009, metal, kenar=4)
    p.silindir((0, 0, 3.14), (0, 0, 3.18), 0.15, 0.15, metal, kenar=6)      # şapka
    p.silindir((0, 0, 3.18), (0, 0, 3.32), 0.15, 0.02, metal, kenar=6)
    p.silindir((0, 0, 3.32), (0, 0, 3.38), 0.025, 0.012, metal, kenar=6)
    return p.nesne()


def golgelik(r):
    """Yuvayı serinleten gölgelik: dört ahşap direk, çizgili tente, saçak, gergi ipleri ve kazıklar."""
    p = Parca("Golgelik")
    ahsap = "P_Kaya_Pas"
    kose = 1.5
    boy = 2.0
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * kose, sy * kose
            p.silindir((x, y, 0), (x, y, boy + 0.06), 0.05, 0.045, ahsap, kenar=6)
            # gergi ipi ve kazık
            kx, ky = sx * (kose + 0.7), sy * (kose + 0.7)
            p.tup([(x, y, boy - 0.05), (kx, ky, 0.08)], 0.008, "P_Kaya_Okra", kenar=4)
            p.silindir((kx, ky, -0.02), (kx, ky, 0.12), 0.025, 0.015, ahsap, kenar=4)

    # Tente: 3,4 x 3,4, ortası hafif sarkık; 8 şerit, iki renk sırayla
    yari = 1.7
    n = 16
    serit = 8

    def z_tente(x, y):
        return boy + 0.02 - 0.12 * (1 - (x / yari) ** 2) * (1 - (y / yari) ** 2)

    for s in range(serit):
        renk = "P_Kopuk" if s % 2 == 0 else "P_Su_Turkuaz"
        x0 = -yari + 2 * yari * s / serit
        x1 = -yari + 2 * yari * (s + 1) / serit
        satirlar = []
        for k in range(n + 1):
            y = -yari + 2 * yari * k / n
            satirlar.append([(x, y, z_tente(x, y)) for x in (x0, x1)])
        p.yuz(satirlar, renk, dongu=False)
        # Alt yüz (tente iki yüzlü görünsün; aşağıdan bakan kamera için)
        alt = [[(x, y, z - 0.012) for x, y, z in reversed(h)] for h in satirlar]
        p.yuz(alt, renk, dongu=False)

    # Saçak: dört kenarda dilimli sarkan şerit
    dil = 12
    for kenar in range(4):
        for d in range(dil):
            t0 = -yari + 2 * yari * d / dil
            t1 = -yari + 2 * yari * (d + 1) / dil
            renk = "P_Kopuk" if d % 2 == 0 else "P_Su_Turkuaz"
            def nokta(t, z):
                return {0: (t, -yari, z), 1: (yari, t, z), 2: (-t, yari, z), 3: (-yari, -t, z)}[kenar]
            ust = boy + 0.02
            orta = (t0 + t1) / 2
            q = [nokta(t0, ust), nokta(t1, ust), nokta(t1, ust - 0.12), nokta(orta, ust - 0.18), nokta(t0, ust - 0.12)]
            i = p.dilim(renk)
            v = [p.bm.verts.new(c) for c in q]
            f = p.bm.faces.new(v)
            f.material_index = i
            v2 = [p.bm.verts.new(c) for c in reversed(q)]
            f2 = p.bm.faces.new(v2)
            f2.material_index = i
    return p.nesne()


MODELLER = [ag, plastik_halka, isik_diregi, golgelik]


# ---------------------------------------------------------------- dışa verme ve önizleme

def unity_duzeni(o):
    """Sezlong.fbx ile aynı: mesh verisi Y-up, nesne X'te +90°."""
    o.data.transform(Matrix.Rotation(-math.pi / 2, 4, "X"))
    o.rotation_euler = (math.pi / 2, 0, 0)


def disa_ver(o, klasor):
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    yol = os.path.join(klasor, o.name + ".fbx")
    bpy.ops.export_scene.fbx(filepath=yol, use_selection=True, object_types={"MESH"},
                             mesh_smooth_type="FACE", path_mode="STRIP", add_leaf_bones=False)
    print("Yazıldı:", yol)


def ucgen(o):
    return sum(len(p.vertices) - 2 for p in o.data.polygons)


def onizleme(o, klasor):
    sahne = bpy.context.scene
    for b in sahne.objects:
        b.hide_render = b is not o and not b.name.startswith("Onizleme")
    # Cycles CPU: GPU'suz makinede de çalışır
    sahne.render.engine = "CYCLES"
    sahne.cycles.device = "CPU"
    sahne.cycles.samples = 48
    sahne.render.resolution_x = sahne.render.resolution_y = 640
    sahne.render.film_transparent = False
    if sahne.world is None:
        sahne.world = bpy.data.worlds.new("Dunya")
        sahne.world.use_nodes = True
        sahne.world.node_tree.nodes["Background"].inputs["Color"].default_value = hex_lineer("#7CCEDD")  # P_Gok_Mavi
        sahne.world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.8
    if "OnizlemeGunes" not in bpy.data.objects:
        gunes = bpy.data.objects.new("OnizlemeGunes", bpy.data.lights.new("OnizlemeGunes", "SUN"))
        gunes.data.energy = 3.5
        gunes.rotation_euler = (math.radians(50), 0, math.radians(30))
        sahne.collection.objects.link(gunes)
        me = bpy.data.meshes.new("OnizlemeKum")
        bm = bmesh.new()
        bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=20)
        bm.to_mesh(me)
        bm.free()
        me.materials.append(malzeme("P_Kum_Bej"))
        sahne.collection.objects.link(bpy.data.objects.new("OnizlemeKum", me))
    sahne.view_settings.view_transform = "Standard"
    boyut = max(o.dimensions)
    merkez = Vector((0, 0, o.dimensions.y / 2))  # nesne X'te döndüğü için dikey boy y'de
    kam = bpy.data.objects.get("OnizlemeKamera") or bpy.data.objects.new("OnizlemeKamera", bpy.data.cameras.new("OnizlemeKamera"))
    if kam.name not in sahne.collection.objects:
        sahne.collection.objects.link(kam)
    yon = Vector((1.0, -1.3, 0.9 if boyut > 1 else 1.2)).normalized()
    kam.location = merkez + yon * boyut * 1.9
    kam.rotation_euler = (merkez - kam.location).to_track_quat("-Z", "Y").to_euler()
    kam.data.lens = 50
    kam.data.clip_start = 0.001
    sahne.camera = kam
    sahne.render.filepath = os.path.join(klasor, o.name + ".png")
    bpy.ops.render.render(write_still=True)


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
        unity_duzeni(o)
        nesneler.append(o)
        print(f"{o.name}: {ucgen(o)} üçgen, boyut {tuple(round(v, 3) for v in o.dimensions)}")

    os.makedirs(cikti, exist_ok=True)
    for o in nesneler:
        disa_ver(o, cikti)

    # Blend dosyasında yan yana dursunlar
    for i, o in enumerate(nesneler):
        o.location.x = i * 4.0
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
