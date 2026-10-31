"""Deniz tabanı, kayalar, Akdeniz mercanları ve deniz çayırı (Posidonia) üretir.

Bölüm 1, Bölüm 5, Poşet sürüsü ve Çöp girdabı sahnelerinin ortak tabanı (Editor/GriDenizKur.cs, Hazirla()).

Çalıştırma (Blender 5.2, arka planda):
    blender -b -P deniz_tabani_uret.py -- <cikti_klasoru> [kumsal_zemin.fbx]
ya da pip'teki bpy modülüyle:
    python deniz_tabani_uret.py -- <cikti_klasoru> [kumsal_zemin.fbx]

Eksenler KumsalArazi ile aynı: Blender X = Unity x, Blender Y = Unity z (kıyı 0, deniz -), Blender Z = yukarı.
FBX varsayılan ayarlarla dışa verilir; Unity'de prefab'ın kökü Y ekseninde 180° döner (KumsalArazi.prefab gibi).
Bütün ölçüler metre. Aynı tohumla her seferinde aynı sonuç çıkar.
"""

import math
import os
import random
import sys

import bpy
import numpy as np

# ---------------------------------------------------------------- ayarlar

TOHUM = 7

# Gri kutudaki basamaklar (GriDenizKur.Hazirla): üst yüzeyleri
DERIN = -15.0          # açık deniz, z -200 .. -120
SIG = -6.0             # sığ sular, z -120 .. -50
KIYI_EGIMI = 0.085     # kıyıdan sığ sulara eğim (m/m), Kumsal_Zemin'in su altındaki eğimine yakın
KIYI_YUKSEKLIK = -0.75  # z = 0'da taban (KumsalArazi kumunun ~0,45 m altı)
BASAMAK_SIG = (-40.0, -66.0)    # kıyı eğiminden sığ düzlüğe geçiş
BASAMAK_DERIN = (-114.0, -128.0)  # sığdan derine kayalık şev

# Izgara (Unity x, z)
X_MIN, X_MAX = -70.0, 70.0
Z_MIN, Z_MAX = -240.0, 12.0
ADIM = 0.5

# Oyun alanı: duvarlar x = ±30. Dışarıda kayalık yamaçlar yükselir.
YAMAC_BASI = 27.0
YAMAC_YUKSEKLIK = 11.0
YAMAC_TAVAN = -1.3     # yamaç su yüzeyine çıkmasın

# KumsalArazi deniz sahnelerinde 0,55 m yukarı konunca kumun altında kalma payı
ARAZI_KAYMA = 0.55

DOKU_EN, DOKU_BOY = 1024, 2048

# Doku atlası: her sütun bir malzeme, aşağıdan yukarı koyudan açığa
ATLAS = 256
SUTUN = 32
RENKLER = {
    # ad: [(v, (r, g, b)), ...]
    "kaya": [(0.0, (88, 70, 50)), (0.6, (142, 116, 82)), (1.0, (176, 146, 106))],
    "kabuk": [(0.0, (122, 84, 76)), (1.0, (192, 140, 124))],          # kırmızı kabuklu algler (coralligène)
    "gorgonya": [(0.0, (96, 28, 52)), (0.55, (176, 44, 64)), (0.85, (214, 96, 70)), (1.0, (236, 196, 84))],
    "kirmizi_mercan": [(0.0, (140, 30, 32)), (0.8, (214, 66, 54)), (1.0, (238, 214, 206))],
    "tas_mercan": [(0.0, (92, 78, 48)), (0.7, (168, 146, 96)), (1.0, (214, 200, 150))],
    "sunger": [(0.0, (166, 112, 22)), (1.0, (240, 200, 62))],
    "posidonia": [(0.0, (34, 54, 30)), (0.55, (96, 120, 62)), (0.85, (124, 132, 86)), (1.0, (156, 138, 84))],
    "cakil": [(0.0, (124, 110, 88)), (1.0, (196, 182, 152))],
}
SIRA = list(RENKLER)

# Oyun nesnelerinin yerleri (GriDenizKur): mercanlar bunlardan uzak durur.
# (x, z, en düşük y) ; derin bölgedeki nesneler -12'nin altına inmez.
OYUN_NESNELERI = [
    (-8, -50), (9, -65), (-3, -85), (12, -100),            # kayalar (Bölüm 5)
    (6, -60), (-10, -80), (4, -95),                          # deniz çayırı
    (0, -60), (-5, -90),                                     # avcı balık
    (-6, -45), (8, -70), (-8, -75), (9, -58), (-8, -70),     # ağlar
    (-4, -125), (10, -140), (-8, -160), (5, -135), (-6, -125), (7, -112), (0, -100),
    (10, -40), (-12, -125),                                  # sıcak su
    (0, -150),                                               # girdap merkezi
    (0, -118), (0, -38), (0, -3), (0, -195),                 # başlangıç / devam / bitiş
]
DERIN_TAVAN = -10.5    # derindeki süslerin en yüksek noktası (oyun nesneleri -12'nin üstünde)
SIG_SUS_BOY = 0.7      # sığ düzlükte süs en fazla bu kadar yükselir


# ---------------------------------------------------------------- gürültü

def _hash(ix, iy, tohum):
    h = (ix.astype(np.int64) * 374761393 + iy.astype(np.int64) * 668265263 + tohum * 1442695041) & 0xFFFFFFFF
    h = ((h ^ (h >> 13)) * 1274126177) & 0xFFFFFFFF
    h = h ^ (h >> 16)
    return (h & 0xFFFFFF) / float(0xFFFFFF)


def deger_gurultusu(x, y, tohum):
    x = np.asarray(x, dtype=np.float64)
    y = np.asarray(y, dtype=np.float64)
    x0 = np.floor(x)
    y0 = np.floor(y)
    fx = x - x0
    fy = y - y0
    ux = fx * fx * (3 - 2 * fx)
    uy = fy * fy * (3 - 2 * fy)
    x0 = x0.astype(np.int64)
    y0 = y0.astype(np.int64)
    a = _hash(x0, y0, tohum)
    b = _hash(x0 + 1, y0, tohum)
    c = _hash(x0, y0 + 1, tohum)
    d = _hash(x0 + 1, y0 + 1, tohum)
    return (a * (1 - ux) + b * ux) * (1 - uy) + (c * (1 - ux) + d * ux) * uy


def fbm(x, y, tohum, oktav=4):
    toplam = 0.0
    genlik = 0.5
    norm = 0.0
    for i in range(oktav):
        toplam = toplam + genlik * deger_gurultusu(x, y, tohum + i * 101)
        norm += genlik
        x = x * 2.03
        y = y * 2.03
        genlik *= 0.5
    return toplam / norm


def smoothstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)


# ---------------------------------------------------------------- taban yüksekliği

def taban(X, Z, arazi=None):
    """Unity (x, z) için taban yüksekliği (y) ve kaya maskesi."""
    X = np.asarray(X, dtype=np.float64)
    Z = np.asarray(Z, dtype=np.float64)

    kiyi = KIYI_YUKSEKLIK + KIYI_EGIMI * Z
    h = kiyi + (SIG - kiyi) * smoothstep(BASAMAK_SIG[0], BASAMAK_SIG[1], Z)

    # Şevin yeri x boyunca biraz kıvrılır
    kivrim = (fbm(X * 0.04, 3.1, TOHUM + 5, 3) - 0.5) * 6.0
    t_derin = smoothstep(BASAMAK_DERIN[0], BASAMAK_DERIN[1], Z + kivrim)
    h = h + (DERIN - h) * t_derin

    # Kum: yumuşak engebe. Kıyıda sadece aşağı (KumsalArazi'nin altında kalsın).
    n = fbm(X / 14.0, Z / 14.0, TOHUM + 11)
    kiyi_bolgesi = 1.0 - smoothstep(-30.0, -55.0, Z)
    h = h - n * 0.35 * kiyi_bolgesi
    sig_bolgesi = (1.0 - kiyi_bolgesi) * (1.0 - t_derin)
    h = h + (n - 0.55) * 0.45 * sig_bolgesi
    # Derinde kum dalgaları (kıyıya paralel) ve geniş engebe
    dalga = np.sin(Z * (2 * math.pi / 9.0) + fbm(X / 20.0, Z / 20.0, TOHUM + 13) * 5.0)
    h = h + t_derin * ((n - 0.5) * 1.4 + dalga * 0.22)

    # Şevin ortası kayalık
    sev = 4.0 * t_derin * (1.0 - t_derin)
    kayalik = fbm(X / 3.5, Z / 3.5, TOHUM + 17, 5)
    h = h + sev * (kayalik - 0.5) * 2.4

    # Oyun alanının dışında kayalık yamaçlar (kıyıda yok)
    ax = np.abs(X)
    t_yamac = np.clip((ax - YAMAC_BASI) / 22.0, 0.0, 1.0)
    yamac_var = smoothstep(-45.0, -64.0, Z)
    sirt = 1.0 - np.abs(fbm(X / 6.0, Z / 6.0, TOHUM + 23, 4) * 2.0 - 1.0)  # sırtlı kaya
    yukselis = (t_yamac ** 1.5) * YAMAC_YUKSEKLIK + t_yamac * (kayalik - 0.5) * 3.0 + t_yamac * sirt * 1.5
    h_yamac = h + yukselis * yamac_var
    tavan = YAMAC_TAVAN - fbm(X / 9.0, Z / 9.0, TOHUM + 29) * 0.8
    # yumuşak min (tavanda düz bir kesik görünmesin)
    k = 0.8
    fark = np.clip(0.5 + 0.5 * (tavan - h_yamac) / k, 0.0, 1.0)
    h_yamac = tavan * (1 - fark) + h_yamac * fark - k * fark * (1 - fark)
    h = np.where(yukselis * yamac_var > 0, np.maximum(h, h_yamac), h)

    if arazi is not None:
        h = arazinin_altinda(h, X, Z, arazi)

    kaya = np.clip(sev * 1.3 + np.clip((ax - YAMAC_BASI + 1.0) / 4.0, 0, 1) * yamac_var * 2.0, 0.0, 1.0)
    return h, kaya


def arazinin_altinda(h, X, Z, arazi):
    """KumsalArazi (deniz sahnesinde +0,55 m) kumunun altında kal; aradaki pay kıyıda 0,35 m, arazinin ucunda 0,12 m."""
    ax, az, ah = arazi  # 0,5 m ızgara: x -150..150, z -30..48
    ix = np.round((X - ax[0]) / 0.5).astype(np.int64)
    iz = np.round((Z - az[0]) / 0.5).astype(np.int64)
    icinde = (ix >= 0) & (ix < len(ax)) & (iz >= 0) & (iz < len(az))
    ust = np.full(h.shape, np.inf)
    ust[icinde] = ah[iz[icinde], ix[icinde]] + ARAZI_KAYMA
    pay = 0.12 + (0.35 - 0.12) * smoothstep(-30.0, -22.0, Z)
    return np.minimum(h, ust - pay)


def arazi_oku(yol):
    """Kumsal_Zemin.fbx'i açıp 0,5 m ızgarada yükseklikleri döner (x, z, y[z, x])."""
    if not yol or not os.path.exists(yol):
        return None
    once = set(bpy.data.objects)
    bpy.ops.wm.fbx_import(filepath=yol)
    yeni = [o for o in bpy.data.objects if o not in once and o.type == "MESH"]
    if not yeni:
        return None
    o = yeni[0]
    n = len(o.data.vertices)
    co = np.zeros(n * 3)
    o.data.vertices.foreach_get("co", co)
    co = co.reshape(-1, 3)
    m = np.array(o.matrix_world)
    w = (np.c_[co, np.ones(n)] @ m.T)[:, :3]
    xs = np.unique(np.round(w[:, 0] * 2) / 2)
    zs = np.unique(np.round(w[:, 1] * 2) / 2)
    tablo = np.full((len(zs), len(xs)), np.inf)
    ix = np.round((w[:, 0] - xs[0]) / 0.5).astype(int)
    iz = np.round((w[:, 1] - zs[0]) / 0.5).astype(int)
    tablo[iz, ix] = np.minimum(tablo[iz, ix], w[:, 2])
    for ob in yeni:
        bpy.data.objects.remove(ob)
    print(f"KumsalArazi okundu: x {xs[0]}..{xs[-1]}, z {zs[0]}..{zs[-1]}")
    return xs, zs, tablo


# ---------------------------------------------------------------- dokular

def renk_rampasi(duraklar, v):
    v = np.clip(v, 0.0, 1.0)
    vs = [d[0] for d in duraklar]
    sonuc = np.zeros(np.shape(v) + (3,))
    for k in range(3):
        sonuc[..., k] = np.interp(v, vs, [d[1][k] for d in duraklar])
    return sonuc


def atlas_yap(yol):
    img = np.zeros((ATLAS, ATLAS, 3))
    v = 1.0 - (np.arange(ATLAS) + 0.5) / ATLAS  # satır 0 = üst = v 1
    for i, ad in enumerate(SIRA):
        sutun = renk_rampasi(RENKLER[ad], v)
        img[:, i * SUTUN:(i + 1) * SUTUN, :] = sutun[:, None, :]
    png_yaz(img, yol)


def atlas_u(ad):
    return (SIRA.index(ad) * SUTUN + SUTUN / 2) / ATLAS


def png_yaz(rgb, yol):
    """rgb: (boy, en, 3) 0..255, satır 0 üstte."""
    boy, en, _ = rgb.shape
    im = bpy.data.images.new(os.path.basename(yol), width=en, height=boy, alpha=False)
    piksel = np.ones((boy, en, 4), dtype=np.float32)
    piksel[..., :3] = np.clip(rgb, 0, 255)[::-1] / 255.0  # Blender'da satır 0 altta
    im.pixels.foreach_set(piksel.ravel())  # 8 bitlik görüntüde değerler olduğu gibi yazılır (sRGB)
    im.update()
    im.save(filepath=yol)
    return im


def taban_dokusu(yol, arazi):
    # Dokunun satır 0'ı kıyı tarafı (z = Z_MAX), son satırı derin (z = Z_MIN)
    u = (np.arange(DOKU_EN) + 0.5) / DOKU_EN
    v = 1.0 - (np.arange(DOKU_BOY) + 0.5) / DOKU_BOY
    X = X_MIN + u[None, :] * (X_MAX - X_MIN)
    Z = Z_MIN + v[:, None] * (Z_MAX - Z_MIN)
    X, Z = np.broadcast_arrays(X, Z)
    h, kaya = taban(X, Z, arazi)
    dx = (X_MAX - X_MIN) / DOKU_EN
    dz = (Z_MAX - Z_MIN) / DOKU_BOY
    gz, gx = np.gradient(h, dz, dx)
    egim = np.sqrt(gx * gx + gz * gz)
    lap = (np.roll(h, 1, 0) + np.roll(h, -1, 0) + np.roll(h, 1, 1) + np.roll(h, -1, 1) - 4 * h) / (dx * dz)
    lap = np.clip(lap, -2, 2)

    # Kum: Kumsal_Zemin'in su altı rengi (#AA8B61), derinde biraz açık ve serin
    derinlik = smoothstep(-2.0, -15.0, h)
    kum = renk_rampasi([(0.0, (170, 139, 97)), (0.6, (178, 152, 110)), (1.0, (180, 158, 120))], derinlik)
    lekeler = fbm(X / 9.0, Z / 9.0, TOHUM + 41)
    kum *= (0.93 + 0.14 * lekeler)[..., None]
    # Kumsal_Zemin'deki gibi yumuşak, kıyıya paralel açık çizgiler
    cizgi_faz = Z * (2 * math.pi / 2.6) + fbm(X / 6.0, Z / 6.0, TOHUM + 43) * 6.0
    cizgi = smoothstep(0.93, 0.995, np.sin(cizgi_faz)) * (1 - smoothstep(0.0, 0.4, egim))
    kum = kum * (1 - 0.07 * cizgi[..., None]) + np.array([214, 190, 150]) * (0.07 * cizgi[..., None])

    # Kaya: soluk taş, üstünde pembe kabuklu algler
    kabuk = smoothstep(0.55, 0.68, fbm(X / 4.0, Z / 4.0, TOHUM + 47, 3))
    tas = renk_rampasi([(0.0, (110, 92, 68)), (1.0, (164, 138, 102))], fbm(X / 5.0, Z / 5.0, TOHUM + 53, 3))
    tas = tas * (1 - 0.4 * kabuk[..., None]) + np.array([176, 120, 122]) * (0.4 * kabuk[..., None])
    kaya_m = np.maximum(kaya, smoothstep(0.7, 1.3, egim))
    kaya_m = smoothstep(0.25, 0.75, kaya_m * (0.75 + 0.5 * fbm(X / 4.0, Z / 4.0, TOHUM + 59, 3)))
    kaya_m = kaya_m[..., None]
    renk = kum * (1 - kaya_m) + tas * kaya_m

    # Çukurlar biraz koyu, sırtlar biraz açık (yumuşak boyama hissi)
    renk *= np.clip(1.0 + lap[..., None] * 0.06, 0.82, 1.08)
    png_yaz(renk, yol)


# ---------------------------------------------------------------- mesh yardımcıları

class Govde:
    """Köşe, yüz ve köşe başına UV biriktirip tek mesh yapar."""

    def __init__(self):
        self.koseler = []
        self.yuzler = []
        self.uvler = []  # her yüzün köşe sırasıyla (u, v)

    def kose(self, p):
        self.koseler.append((float(p[0]), float(p[1]), float(p[2])))
        return len(self.koseler) - 1

    def yuz(self, idx, uv):
        self.yuzler.append(tuple(idx))
        self.uvler.extend(uv)

    def ekle(self, diger):
        kayma = len(self.koseler)
        self.koseler.extend(diger.koseler)
        self.yuzler.extend(tuple(i + kayma for i in f) for f in diger.yuzler)
        self.uvler.extend(diger.uvler)

    def nesne(self, ad, malzeme, yumusak=True):
        me = bpy.data.meshes.new(ad)
        me.from_pydata(self.koseler, [], self.yuzler)
        uv = me.uv_layers.new(name="UVMap")
        uv.data.foreach_set("uv", np.array(self.uvler, dtype=np.float32).ravel())
        me.polygons.foreach_set("use_smooth", [yumusak] * len(me.polygons))
        me.validate()
        me.update()
        me.materials.append(malzeme)
        ob = bpy.data.objects.new(ad, me)
        bpy.context.scene.collection.objects.link(ob)
        return ob


def cerceve(yon):
    yon = yon / np.linalg.norm(yon)
    yardimci = np.array([0.0, 0.0, 1.0]) if abs(yon[2]) < 0.9 else np.array([1.0, 0.0, 0.0])
    a = np.cross(yon, yardimci)
    a /= np.linalg.norm(a)
    b = np.cross(yon, a)
    return a, b


def boru(g, noktalar, yaricaplar, u, v0, v1, kenar=5, uc_kapali=True):
    """Noktalar boyunca boru; ucu sivri kapanır. v, tabandan uca v0..v1."""
    noktalar = [np.asarray(p, dtype=np.float64) for p in noktalar]
    n = len(noktalar)
    halkalar = []
    a_once = None
    for i, p in enumerate(noktalar):
        if i < n - 1:
            yon = noktalar[i + 1] - p
        else:
            yon = p - noktalar[i - 1]
        if np.linalg.norm(yon) < 1e-6:
            yon = np.array([0.0, 0.0, 1.0])
        yon = yon / np.linalg.norm(yon)
        if a_once is None:
            a, b = cerceve(yon)
        else:
            a = a_once - yon * np.dot(a_once, yon)
            if np.linalg.norm(a) < 1e-6:
                a, b = cerceve(yon)
            a = a / np.linalg.norm(a)
            b = np.cross(yon, a)
        a_once = a
        r = yaricaplar[i]
        halka = []
        for k in range(kenar):
            t = 2 * math.pi * k / kenar
            halka.append(g.kose(p + (a * math.cos(t) + b * math.sin(t)) * r))
        halkalar.append(halka)
    for i in range(n - 1):
        va = v0 + (v1 - v0) * i / (n - 1)
        vb = v0 + (v1 - v0) * (i + 1) / (n - 1)
        for k in range(kenar):
            k2 = (k + 1) % kenar
            g.yuz([halkalar[i][k], halkalar[i][k2], halkalar[i + 1][k2], halkalar[i + 1][k]],
                  [(u, va), (u, va), (u, vb), (u, vb)])
    if uc_kapali:
        son = noktalar[-1]
        yon = son - noktalar[-2]
        uc = g.kose(son + yon / max(np.linalg.norm(yon), 1e-6) * yaricaplar[-1] * 0.8)
        for k in range(kenar):
            k2 = (k + 1) % kenar
            g.yuz([halkalar[-1][k], halkalar[-1][k2], uc], [(u, v1), (u, v1), (u, v1)])


# ---------------------------------------------------------------- kaya

def kaya_govdesi(rng, boy, yukseklik_orani=0.8, kabuk_orani=0.25):
    """Gürültüyle bozulmuş, tabanı düz kaya. Taban z=0, ortası (0,0). Dönüş: Govde, üst/yan noktalar."""
    import bmesh
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=3 if boy > 2.0 else (2 if boy > 0.6 else 1), radius=1.0)
    tohum = int(rng.integers(1, 10_000))
    olcek = np.array([1.0, rng.uniform(0.75, 1.0), yukseklik_orani]) * boy / 2
    noktalar = np.array([v.co[:] for v in bm.verts])
    yonler = noktalar / np.linalg.norm(noktalar, axis=1)[:, None]
    gur = fbm(yonler[:, 0] * 1.6 + yonler[:, 2] * 0.7, yonler[:, 1] * 1.6 - yonler[:, 2] * 0.5, tohum, 4)
    ince = fbm(yonler[:, 0] * 5 + 3.3, yonler[:, 1] * 5 + yonler[:, 2] * 4, tohum + 3, 3)
    r = 0.62 + gur * 0.75 + ince * 0.2
    p = yonler * r[:, None] * olcek
    # kireçtaşı gibi hafif katmanlı: yüksekliği yumuşakça basamakla
    kat = olcek[2] * 0.35
    q = p[:, 2] / kat
    p[:, 2] = (np.floor(q) + smoothstep(0.0, 1.0, (q - np.floor(q)) * 1.6 - 0.3)) * kat
    # tabanı düzleştir, gömülü olsun
    zmin = -olcek[2] * 0.2
    p[:, 2] = np.maximum(p[:, 2], zmin)
    p[:, 2] -= zmin
    for v, q in zip(bm.verts, p):
        v.co = q
    bm.normal_update()
    bm.verts.index_update()
    g = Govde()
    ust = p[:, 2].max()
    for v in bm.verts:
        g.kose(v.co)
    ust_noktalar = []
    yan_noktalar = []
    for f in bm.faces:
        merkez = f.calc_center_median()
        n = f.normal
        kabuk = n.z > 0.35 and fbm(merkez.x * 0.9 + 7, merkez.y * 0.9 + merkez.z, tohum + 9, 2) > (1 - kabuk_orani)
        ad = "kabuk" if kabuk else "kaya"
        u = atlas_u(ad)
        uv = [(u, min(0.98, max(0.02, l.vert.co.z / max(ust, 1e-3)))) for l in f.loops]
        g.yuz([l.vert.index for l in f.loops], uv)
        if n.z > 0.6 and merkez.z > ust * 0.45:
            ust_noktalar.append((np.array(merkez[:]), np.array(n[:])))
        elif -0.4 < n.z < 0.3 and merkez.z > ust * 0.2:
            yan_noktalar.append((np.array(merkez[:]), np.array(n[:])))
    bm.free()
    return g, ust, ust_noktalar, yan_noktalar


def tasi(g, konum, aci=0.0, olcek=1.0):
    c, s = math.cos(aci), math.sin(aci)
    yeni = []
    for x, y, z in g.koseler:
        x, y, z = x * olcek, y * olcek, z * olcek
        yeni.append((konum[0] + x * c - y * s, konum[1] + x * s + y * c, konum[2] + z))
    g.koseler = yeni
    return g


def nokta_tasi(p, konum, aci, olcek):
    c, s = math.cos(aci), math.sin(aci)
    x, y, z = p * olcek
    return np.array([konum[0] + x * c - y * s, konum[1] + x * s + y * c, konum[2] + z])


def yon_tasi(n, aci):
    c, s = math.cos(aci), math.sin(aci)
    return np.array([n[0] * c - n[1] * s, n[0] * s + n[1] * c, n[2]])


# ---------------------------------------------------------------- Akdeniz canlıları

def gorgonya(rng, konum, boy, yon_aci):
    """Paramuricea clavata: tek düzlemde sık dallanan deniz yelpazesi. Kırmızı-mor, uçları sarı."""
    g = Govde()
    u = atlas_u("gorgonya")
    duzlem = np.array([math.cos(yon_aci), math.sin(yon_aci), 0.0])
    dik = np.array([-math.sin(yon_aci), math.cos(yon_aci), 0.0])
    yukari = np.array([0.0, 0.0, 1.0])
    en_derin = 6 if boy > 1.6 else 5

    def dal(bas, aci, uzunluk, kalinlik, derinlik, v0):
        noktalar = [bas]
        p = bas.copy()
        for _ in range(2):
            aci += rng.normal(0, 0.08)
            p = p + (duzlem * math.sin(aci) + yukari * math.cos(aci)) * (uzunluk / 2) + dik * rng.normal(0, 0.015) * uzunluk
            noktalar.append(p)
        v1 = min(1.0, v0 + 1.0 / en_derin)
        boru(g, noktalar, [kalinlik, kalinlik * 0.9, kalinlik * 0.8], u, v0, v1, kenar=3, uc_kapali=derinlik == en_derin)
        if derinlik < en_derin:
            acilim = rng.uniform(0.45, 0.7)
            for isaret in (-1, 1):
                # yelpaze dışa açılsın, ama yatayı geçmesin
                yeni = max(-1.4, min(1.4, aci + isaret * acilim))
                dal(noktalar[-1], yeni, uzunluk * rng.uniform(0.72, 0.85), kalinlik * 0.75, derinlik + 1, v1)

    govde_uzunluk = boy * 0.18
    dal(np.array(konum, dtype=float), rng.normal(0, 0.1), govde_uzunluk, boy * 0.035, 1, 0.0)
    return g


def kirmizi_mercan(rng, konum, normal, boy):
    """Corallium rubrum: kaya altında, küçük kırmızı dallı mercan; yüzeyin normaline doğru büyür."""
    g = Govde()
    u = atlas_u("kirmizi_mercan")
    ana_yon = np.array(normal, dtype=float)
    ana_yon = ana_yon / np.linalg.norm(ana_yon)

    def dal(bas, yon, uzunluk, kalinlik, derinlik, v0):
        noktalar = [bas]
        p = bas.copy()
        for _ in range(3):
            yon = yon + rng.normal(0, 0.25, 3)
            yon /= np.linalg.norm(yon)
            p = p + yon * uzunluk / 3
            noktalar.append(p)
        v1 = min(1.0, v0 + 0.3)
        boru(g, noktalar, [kalinlik, kalinlik * 0.85, kalinlik * 0.75, kalinlik * 0.7], u, v0, v1, kenar=4,
             uc_kapali=True)
        if derinlik < 2:
            for _ in range(int(rng.integers(1, 3))):
                dal(noktalar[int(rng.integers(1, 4))], yon + rng.normal(0, 0.6, 3), uzunluk * 0.7, kalinlik * 0.75,
                    derinlik + 1, v1 * 0.9)

    for _ in range(int(rng.integers(1, 3))):
        dal(np.array(konum, dtype=float), ana_yon + rng.normal(0, 0.35, 3), boy * 0.45, boy * 0.06, 1, 0.1)
    return g


def tas_mercan(rng, konum, boy):
    """Cladocora caespitosa: kısa borulardan yarım küre öbek; kahverengi, uçları açık."""
    g = Govde()
    u = atlas_u("tas_mercan")
    yaricap = boy / 2
    adet = int(5 + boy * 12)
    for _ in range(adet):
        # yarım küre üstünde rastgele yön
        z = rng.uniform(0.15, 1.0)
        t = rng.uniform(0, 2 * math.pi)
        r = math.sqrt(1 - z * z)
        yon = np.array([r * math.cos(t), r * math.sin(t), z])
        bas = np.array(konum, dtype=float) + yon * yaricap * 0.25
        uc = np.array(konum, dtype=float) + yon * yaricap * rng.uniform(0.85, 1.05)
        orta = (bas + uc) / 2 + rng.normal(0, 0.02, 3)
        kal = yaricap * rng.uniform(0.07, 0.1)
        boru(g, [bas, uc], [kal * 0.9, kal * 1.05], u, 0.15, 0.95, kenar=4, uc_kapali=True)
    return g


def sunger(rng, konum, boy):
    """Sarı boru sünger (Aplysina / Axinella): birkaç parmak, uçları yuvarlak."""
    g = Govde()
    u = atlas_u("sunger")
    for _ in range(int(rng.integers(3, 7))):
        aci = rng.uniform(0, 2 * math.pi)
        egik = rng.uniform(0.05, 0.35)
        yon = np.array([math.cos(aci) * egik, math.sin(aci) * egik, 1.0])
        yon /= np.linalg.norm(yon)
        uz = boy * rng.uniform(0.55, 1.0)
        bas = np.array(konum, dtype=float) + np.array([math.cos(aci), math.sin(aci), 0]) * boy * 0.1 - np.array([0, 0, 0.05])
        nokt = [bas + yon * uz * i / 4 + rng.normal(0, 0.012, 3) for i in range(5)]
        kal = boy * rng.uniform(0.09, 0.13)
        boru(g, nokt, [kal * 1.1, kal, kal * 1.05, kal * 0.95, kal * 0.9], u, 0.1, 1.0, kenar=5, uc_kapali=True)
    return g


def posidonia(rng, en, uzun, yogunluk=9.0, boy=(0.8, 1.9)):
    """Posidonia oceanica çayırı: en x uzun metrelik alan, taban ortası (0,0,0). Şerit yapraklar."""
    g = Govde()
    u = atlas_u("posidonia")
    adet = int(en * uzun * yogunluk)
    for _ in range(adet):
        x = rng.uniform(-en / 2, en / 2)
        y = rng.uniform(-uzun / 2, uzun / 2)
        # kenarlara doğru seyrek ve kısa
        kenar = min(1 - abs(x) / (en / 2), 1 - abs(y) / (uzun / 2))
        if rng.random() > 0.35 + 1.3 * kenar:
            continue
        boy_carpan = 0.55 + 0.45 * min(1.0, kenar * 3)
        egilme = rng.uniform(0, 2 * math.pi)
        for _ in range(int(rng.integers(4, 7))):
            l = rng.uniform(*boy) * boy_carpan
            gen = rng.uniform(0.08, 0.12)
            a = egilme + rng.normal(0, 0.5)
            yan = np.array([math.cos(a + math.pi / 2), math.sin(a + math.pi / 2), 0.0])
            yan_kayma = rng.normal(0, 0.15)
            yan = yan * math.cos(yan_kayma) + np.array([math.cos(a), math.sin(a), 0.0]) * math.sin(yan_kayma)
            egim = rng.uniform(0.1, 0.45)
            parca = 5
            sol, sag = [], []
            for i in range(parca + 1):
                t = i / parca
                bukulme = egim * t * t
                merkez = np.array([x + math.cos(a) * l * math.sin(bukulme) * t,
                                   y + math.sin(a) * l * math.sin(bukulme) * t,
                                   l * t * math.cos(bukulme * 0.8)])
                w = gen * (1.0 - 0.25 * t) * 0.5
                sol.append(g.kose(merkez - yan * w))
                sag.append(g.kose(merkez + yan * w))
            for i in range(parca):
                va, vb = i / parca, (i + 1) / parca
                g.yuz([sol[i], sag[i], sag[i + 1], sol[i + 1]], [(u, va), (u, va), (u, vb), (u, vb)])
    return g


# ---------------------------------------------------------------- yerleşim

def oyun_nesnesine_yakin(x, z, mesafe):
    return any((x - ox) ** 2 + (z - oz) ** 2 < mesafe * mesafe for ox, oz in OYUN_NESNELERI)


def resif(rng, merkez, yaricap, kayalar, canlilar, arazi, tavan, buyuk=True):
    """Bir mercan kayalığı: birkaç kaya, üstlerinde ve yanlarında Akdeniz canlıları."""
    cx, cz = merkez
    sayi = int(rng.integers(3, 7)) if buyuk else int(rng.integers(1, 3))
    for _ in range(sayi):
        x = cx + rng.normal(0, yaricap * 0.45)
        z = cz + rng.normal(0, yaricap * 0.45)
        zemin = float(taban(np.array([x]), np.array([z]), arazi)[0][0])
        bosluk = tavan - zemin
        if bosluk < 0.8:
            continue
        boy = min(rng.uniform(1.4, 3.6) if buyuk else rng.uniform(0.6, 1.4), bosluk / 0.9)
        g, ust, ustler, yanlar = kaya_govdesi(rng, boy, yukseklik_orani=rng.uniform(0.55, 0.85))
        aci = rng.uniform(0, 2 * math.pi)
        konum = (x, z, zemin - 0.15 * boy)
        kayalar.ekle(tasi(g, konum, aci))
        kalan = tavan - (konum[2] + ust)
        # tepesine gorgonya ya da taş mercan
        for p, n in [ustler[i] for i in rng.permutation(len(ustler))[:2]]:
            q = nokta_tasi(p, konum, aci, 1.0)
            r = rng.random()
            if r < 0.6 and kalan > 0.9:
                canlilar.ekle(gorgonya(rng, q, min(rng.uniform(1.2, 2.2), kalan), rng.normal(0, 0.8)))
            elif r < 0.8 and kalan > 0.4:
                canlilar.ekle(tas_mercan(rng, q, min(rng.uniform(0.35, 0.7), kalan)))
            elif kalan > 0.4:
                canlilar.ekle(sunger(rng, q, min(rng.uniform(0.3, 0.55), kalan)))
        # yanlarına kırmızı mercan
        for p, n in [yanlar[i] for i in rng.permutation(len(yanlar))[:1]]:
            if rng.random() < 0.8:
                q = nokta_tasi(p, konum, aci, 1.0)
                canlilar.ekle(kirmizi_mercan(rng, q, yon_tasi(n, aci) + np.array([0, 0, -0.3]), rng.uniform(0.35, 0.55)))
    # kayalığın dibine öbekler
    for _ in range(int(rng.integers(2, 5)) if buyuk else 1):
        x = cx + rng.normal(0, yaricap * 0.7)
        z = cz + rng.normal(0, yaricap * 0.7)
        zemin = float(taban(np.array([x]), np.array([z]), arazi)[0][0])
        if tavan - zemin < 0.5:
            continue
        if rng.random() < 0.6:
            canlilar.ekle(tas_mercan(rng, (x, z, zemin - 0.05), rng.uniform(0.35, 0.8)))
        else:
            canlilar.ekle(sunger(rng, (x, z, zemin), min(rng.uniform(0.3, 0.6), tavan - zemin)))


def susleri_yerlestir(arazi):
    rng = np.random.default_rng(TOHUM)
    kayalar = Govde()
    canlilar = Govde()

    # 1) Açık denizde mercan kayalıkları (oyun nesneleri -12'nin üstünde; süsler -10,5'i geçmez)
    merkezler = [(-18, -140), (20, -150), (-14, -178), (16, -186), (2, -205), (-22, -212), (22, -222),
                 (-6, -136), (9, -168), (-2, -228), (-22, -158), (22, -134), (-20, -196), (20, -170),
                 (-12, -232), (14, -238), (12, -212), (-10, -220)]
    for cx, cz in merkezler:
        if oyun_nesnesine_yakin(cx, cz, 3.0) and abs(cx) < 6:
            continue
        resif(rng, (cx, cz), rng.uniform(3, 6), kayalar, canlilar, arazi, DERIN_TAVAN,
              buyuk=abs(cx) > 10 or cz < -200)

    # 2) Kayalık şev (sığdan derine): şevin alt yarısı
    for z in np.arange(-132, -112, 3.0):
        for x in np.arange(-26, 27, 4.0):
            x2 = x + rng.uniform(-1.5, 1.5)
            z2 = z + rng.uniform(-1.5, 1.5)
            zemin = float(taban(np.array([x2]), np.array([z2]), arazi)[0][0])
            if zemin > -9.5 or oyun_nesnesine_yakin(x2, z2, 4.0) or rng.random() < 0.45:
                continue
            resif(rng, (x2, z2), 1.5, kayalar, canlilar, arazi, min(DERIN_TAVAN, zemin + 2.2), buyuk=False)

    # 3) Yan yamaçlar (oyun alanının dışı): sık coralligène
    for taraf in (-1, 1):
        for z in np.arange(-236, -56, 10.0):
            for ax in (33.0, 42.0):
                x = taraf * (ax + rng.uniform(-2, 2))
                z2 = z + rng.uniform(-2.5, 2.5)
                if rng.random() < 0.3:
                    continue
                zemin = float(taban(np.array([x]), np.array([z2]), arazi)[0][0])
                tavan = min(-0.8, zemin + 3.5)
                resif(rng, (x, z2), 2.0, kayalar, canlilar, arazi, tavan, buyuk=ax > 35 and zemin < -6)

    # 4) Sığ düzlükte kenarlarda alçak öbekler ve küçük taşlar (oyun şeridi |x| < 18 boş)
    for z in np.arange(-112, -55, 5.0):
        for taraf in (-1, 1):
            x = taraf * rng.uniform(19, 27)
            z2 = z + rng.uniform(-2, 2)
            zemin = float(taban(np.array([x]), np.array([z2]), arazi)[0][0])
            if rng.random() < 0.5:
                canlilar.ekle(tas_mercan(rng, (x, z2, zemin - 0.05), rng.uniform(0.3, SIG_SUS_BOY)))
            else:
                g, ust, _, _ = kaya_govdesi(rng, rng.uniform(0.5, 1.0), yukseklik_orani=0.5)
                kayalar.ekle(tasi(g, (x, z2, zemin - 0.1), rng.uniform(0, 6.28)))

    # 5) Kıyıya yakın kumda seyrek çakıl (KumsalArazi'nin bittiği yerden önce)
    for _ in range(40):
        x = rng.uniform(-28, 28)
        z = rng.uniform(-48, -32)
        if oyun_nesnesine_yakin(x, z, 2.0):
            continue
        zemin = float(taban(np.array([x]), np.array([z]), arazi)[0][0])
        g, ust, _, _ = kaya_govdesi(rng, rng.uniform(0.15, 0.45), yukseklik_orani=0.45, kabuk_orani=0.0)
        for i in range(len(g.uvler)):
            g.uvler[i] = (atlas_u("cakil"), g.uvler[i][1])
        kayalar.ekle(tasi(g, (x, z, zemin - 0.04), rng.uniform(0, 6.28)))

    return kayalar, canlilar


# ---------------------------------------------------------------- sahne

def malzeme(ad, doku_yolu, iki_yuzlu=False, parlaklik=0.08):
    m = bpy.data.materials.new(ad)
    m.use_nodes = True
    agac = m.node_tree
    bsdf = agac.nodes.get("Principled BSDF")
    bsdf.inputs["Roughness"].default_value = 1.0 - parlaklik
    if doku_yolu:
        tex = agac.nodes.new("ShaderNodeTexImage")
        tex.image = bpy.data.images.load(doku_yolu, check_existing=True)
        if "atlas" in ad.lower() or ad in ("DenizObjeleri", "DenizCayiri"):
            tex.interpolation = "Closest"
        agac.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    m.use_backface_culling = not iki_yuzlu
    return m


def zemin_mesh(mat, arazi):
    xs = np.arange(X_MIN, X_MAX + 1e-6, ADIM)
    zs = np.arange(Z_MIN, Z_MAX + 1e-6, ADIM)
    X, Z = np.meshgrid(xs, zs)
    h, _ = taban(X, Z, arazi)
    nx, nz = len(xs), len(zs)
    koseler = np.stack([X.ravel(), Z.ravel(), h.ravel()], axis=1)
    i = np.arange(nz - 1)[:, None] * nx + np.arange(nx - 1)[None, :]
    i = i.ravel()
    yuzler = np.stack([i, i + 1, i + 1 + nx, i + nx], axis=1)
    me = bpy.data.meshes.new("DenizTabani_Zemin")
    me.vertices.add(len(koseler))
    me.vertices.foreach_set("co", koseler.astype(np.float32).ravel())
    me.loops.add(len(yuzler) * 4)
    me.loops.foreach_set("vertex_index", yuzler.astype(np.int32).ravel())
    me.polygons.add(len(yuzler))
    me.polygons.foreach_set("loop_start", (np.arange(len(yuzler)) * 4).astype(np.int32))
    me.polygons.foreach_set("use_smooth", np.ones(len(yuzler), dtype=bool))
    uv_kose = np.stack([(koseler[:, 0] - X_MIN) / (X_MAX - X_MIN), (koseler[:, 1] - Z_MIN) / (Z_MAX - Z_MIN)], axis=1)
    uv = me.uv_layers.new(name="UVMap")
    uv.data.foreach_set("uv", uv_kose[yuzler.ravel()].astype(np.float32).ravel())
    me.update(calc_edges=True)
    me.validate()
    me.materials.append(mat)
    ob = bpy.data.objects.new("DenizTabani_Zemin", me)
    bpy.context.scene.collection.objects.link(ob)
    print(f"Zemin: {len(koseler)} köşe, {len(yuzler) * 2} üçgen, y {h.min():.2f} .. {h.max():.2f}")
    return ob


def disa_ver(nesneler, yol):
    bpy.ops.object.select_all(action="DESELECT")
    for o in nesneler:
        o.select_set(True)
    bpy.context.view_layer.objects.active = nesneler[0]
    bpy.ops.export_scene.fbx(filepath=yol, use_selection=True, object_types={"MESH", "EMPTY"},
                             mesh_smooth_type="FACE", path_mode="STRIP", add_leaf_bones=False)
    print("Yazıldı:", yol)


def ucgen(o):
    return sum(len(p.vertices) - 2 for p in o.data.polygons)


def main():
    arg = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    cikti = os.path.abspath(arg[0] if arg else ".")
    arazi_yolu = arg[1] if len(arg) > 1 else None
    kaynak = os.path.join(cikti, "Kaynak~")
    os.makedirs(kaynak, exist_ok=True)

    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o)
    arazi = arazi_oku(arazi_yolu)

    zemin_png = os.path.join(cikti, "DenizTabani_Renk.png")
    atlas_png = os.path.join(cikti, "DenizObjeleri_Renk.png")
    taban_dokusu(zemin_png, arazi)
    atlas_yap(atlas_png)

    m_zemin = malzeme("DenizTabani_Zemin", zemin_png)
    m_obje = malzeme("DenizObjeleri", atlas_png)
    m_cayir = malzeme("DenizCayiri", atlas_png, iki_yuzlu=True)

    # 1) Taban + süsler
    zemin = zemin_mesh(m_zemin, arazi)
    kayalar, canlilar = susleri_yerlestir(arazi)
    o_kaya = kayalar.nesne("Dekor_Kayalar", m_obje)
    o_canli = canlilar.nesne("Dekor_Mercanlar", m_obje)
    print(f"Dekor kayalar: {ucgen(o_kaya)} üçgen, mercanlar: {ucgen(o_canli)} üçgen")
    disa_ver([zemin, o_kaya, o_canli], os.path.join(cikti, "DenizTabani.fbx"))

    # 2) Gri kutu kayaların yerine (3 m, taban ortası pivot)
    rng = np.random.default_rng(TOHUM + 100)
    kaya_nesneleri = []
    for i in range(3):
        g, ust, _, _ = kaya_govdesi(rng, 3.0, yukseklik_orani=rng.uniform(0.75, 0.95), kabuk_orani=0.4)
        kaya_nesneleri.append(g.nesne(f"Kaya_{i + 1}", m_obje))
    disa_ver(kaya_nesneleri, os.path.join(cikti, "Kayalar.fbx"))
    # .blend'de üst üste durmasınlar
    for i, o in enumerate(kaya_nesneleri):
        o.location = (400 + i * 5, 0, 0)

    # 3) Deniz çayırı: GriDenizKur'daki üç saklanma alanının boyunda (x en, z uzunluk)
    cayirlar = []
    for en, uzun in [(6, 10), (6, 8), (8, 8)]:
        g = posidonia(rng, en, uzun)
        o = g.nesne(f"DenizCayiri_{en}x{uzun}", m_cayir)
        print(f"{o.name}: {ucgen(o)} üçgen")
        cayirlar.append(o)
    disa_ver(cayirlar, os.path.join(cikti, "DenizCayiri.fbx"))
    for i, o in enumerate(cayirlar):
        o.location = (440 + i * 12, 0, 0)

    bpy.context.preferences.filepaths.save_version = 0  # .blend1 yedeği yazma
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(kaynak, "DenizTabani.blend"), compress=True)
    print("Bitti.")


if __name__ == "__main__":
    main()
