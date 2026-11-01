"""Parlayan plastik: Bölüm 5'te (su altı) yavruyu kendine çeken deniz çöpü ve çöp girdabının ortasındaki yığın.

Ölçüler gri kutulardan (GDD tehdit tablosu: "Plastik parçaları — Parıldar — yavruyu kendine çeker"):
    Parlayan_Plastik_1  0,5 x 0,2 x 0,4 (Unity x,y,z)   yırtık pet şişe: kapaklı boyun, ezik gövde, kopuk ağız
    Parlayan_Plastik_2  0,5 x 0,2 x 0,4                 poşet / streç film parçası: katlanmış, dalgalı, iki yüzlü
    Parlayan_Plastik_3  0,5 x 0,2 x 0,4                 küçük çöp kümesi: kapaklar, pipet, kırık parçalar
    Plastik_Yigini      2,5 x 1,5 x 2,5                 girdabın ortasındaki yığın (gri kutu ParlayanYigin)

Hepsi suda asılı: kök ortada (MERKEZLI). Plastiğin gövdesi düz palet renginde; ışığı yakalamış gibi
parlayan yerler kenarlar ve kırışıklar (P_Isik_Parilti). Yığının içinde sıcak ışıklı bir çekirdek (P_Kehribar)
aralıklardan sızar: yavruya yiyecek ya da ışık gibi görünür. Gövdeler beyaz ve sıcak renkte (P_Kopuk, P_Mercan,
P_Turuncu); turkuaz yalnız küçük vurgu, yoksa Bölüm 5 suyunda kaybolur.
Dikkat (Hazards/Parilti.cs): betik _BaseColor'ı malzeme dizini vermeden bütün renderer'a basar; bu çok malzemeli tek
mesh'lerde her dilim tek renge döner. Nabız yalnız P_Isik_Parilti / P_Kehribar dilimlerine (SetPropertyBlock(blok, i))
ya da _EmissionColor'a uygulanmalı.
Palete P_Mercan eklenir (palet.md'deki değer); yeni ışıma kaydı yok, iki parıltı rengi ana betikte zaten ışıyor.
"""

import math
import random

import bmesh
from mathutils import Euler, Matrix, Vector

import kumsal_objeleri_uret as k

PARILTI = "P_Isik_Parilti"   # soğuk parıltı: kenar, kırışık
SICAK = "P_Kehribar"         # sıcak parıltı: yığının çekirdeği, birkaç kırıntı

k.PALET.setdefault("P_Mercan", "#FF6F61")  # docs/palet.md: mercan, vurgu (ana betikte henüz yok)
k.MERKEZLI.update({"Parlayan_Plastik_1", "Parlayan_Plastik_2", "Parlayan_Plastik_3", "Plastik_Yigini"})


# ---------------------------------------------------------------- yardımcılar

def matris(konum=(0, 0, 0), donus=(0, 0, 0), olcek=(1, 1, 1)):
    """Konum, Euler dönüşü (radyan) ve eksen ölçeğinden 4x4 matris."""
    return Matrix.Translation(konum) @ Euler(donus).to_matrix().to_4x4() @ Matrix.Diagonal((*olcek, 1))


def yerlestir(p, M, kur):
    """kur() ile eklenen yeni köşeleri M ile taşır: parçalar kendi ekseninde (z boyunca) kurulur, sonra yerine konur."""
    once = set(p.bm.verts)
    kur()
    yeni = [v for v in p.bm.verts if v not in once]
    bmesh.ops.transform(p.bm, matrix=M, verts=yeni)


def ortala(p):
    """Sınır kutusunun ortasını köke (0,0,0) getirir: suda asılı objeler için."""
    xs = [v.co for v in p.bm.verts]
    lo = Vector((min(c.x for c in xs), min(c.y for c in xs), min(c.z for c in xs)))
    hi = Vector((max(c.x for c in xs), max(c.y for c in xs), max(c.z for c in xs)))
    bmesh.ops.translate(p.bm, vec=-(lo + hi) / 2, verts=p.bm.verts)


def sigdir(p, en, boy, yuk):
    """Ortalar ve tek ölçekle gri kutunun içine sığdırır (en x boy x yük, Blender x-y-z)."""
    ortala(p)
    xs = [v.co for v in p.bm.verts]
    olcu = Vector((max(c.x for c in xs) * 2, max(c.y for c in xs) * 2, max(c.z for c in xs) * 2))
    o = min(en / olcu.x, boy / olcu.y, yuk / olcu.z)
    bmesh.ops.scale(p.bm, vec=(o, o, o), verts=p.bm.verts)


def ortu(p, halkalar, malz_fn, dongu=True):
    """Parca.yuz gibi halkaları bağlar; her dörtgenin malzemesini malz_fn(a, b) seçer (kırışık parlasın diye)."""
    v = [[p.bm.verts.new(q) for q in h] for h in halkalar]
    n = len(v[0])
    for a in range(len(v) - 1):
        for b in range(n if dongu else n - 1):
            c = (b + 1) % n
            f = p.bm.faces.new((v[a][b], v[a][c], v[a + 1][c], v[a + 1][b]))
            f.material_index = p.dilim(malz_fn(a, b))
    return v


def levha(p, nu, nv, icinde, nokta, malz_fn, kalin=0.004, kenar_malz=PARILTI):
    """İnce, kalınlığı olan plastik levha (poşet, kırık şişe parçası): nu x nv ızgaranın icinde(u, v) hücreleri.
    Üst ve alt yüz ayrı (iki yüzlü görünür), kenar şeridiyle kapalı kabuk olur; malz_fn(i, j, kenarda) hücre rengi."""
    dolu = {(i, j) for i in range(nu) for j in range(nv) if icinde((i + 0.5) / nu, (j + 0.5) / nv)}
    ust, alt = {}, {}

    def kose(c):
        if c not in ust:
            u, v = c[0] / nu, c[1] / nv
            q = Vector(nokta(u, v))
            nrm = (Vector(nokta(u + 1e-3, v)) - q).cross(Vector(nokta(u, v + 1e-3)) - q).normalized()
            ust[c] = p.bm.verts.new(q + nrm * kalin / 2)
            alt[c] = p.bm.verts.new(q - nrm * kalin / 2)
        return c

    for i, j in sorted(dolu):
        ks = [kose(c) for c in ((i, j), (i + 1, j), (i + 1, j + 1), (i, j + 1))]
        komsu = [(i, j - 1), (i + 1, j), (i, j + 1), (i - 1, j)]
        kenarda = any(c not in dolu for c in komsu)
        m = p.dilim(malz_fn(i, j, kenarda))
        f = p.bm.faces.new([ust[c] for c in ks])
        f.material_index = m
        f = p.bm.faces.new([alt[c] for c in reversed(ks)])
        f.material_index = m
        for e in range(4):  # açık kenarlara ince şerit
            if komsu[e] not in dolu:
                c0, c1 = ks[e], ks[(e + 1) % 4]
                f = p.bm.faces.new((ust[c1], ust[c0], alt[c0], alt[c1]))
                f.material_index = p.dilim(kenar_malz)


def levha_yuvarlak(p, n_aci, halkalar_t, sinir, nokta, malz_fn, kalin=0.004, kenar_malz=PARILTI, alt_fn=None):
    """Yuvarlak hatlı ince levha (poşet, kırık parça): ortadan dışa halkalar, dış sınırı açıya göre sinir(a).
    Kenarı merdiven gibi kırılmaz. Üst ve alt yüz ayrı, kenar şeridiyle kapalı kabuk; malz_fn(halka, u, v, dilim) rengi.
    alt_fn(renk) alt yüzün rengini değiştirir (baskı yalnız üstte); kenar_malz bir fonksiyonsa dilime göre seçer."""
    def uv(t, s):
        a = 2 * math.pi * s / n_aci
        rr = 0.5 * t * sinir(a)
        return 0.5 + rr * math.cos(a), 0.5 + rr * math.sin(a)

    def cift(u, v):
        q = Vector(nokta(u, v))
        nrm = (Vector(nokta(u + 1e-3, v)) - q).cross(Vector(nokta(u, v + 1e-3)) - q).normalized()
        return p.bm.verts.new(q + nrm * kalin / 2), p.bm.verts.new(q - nrm * kalin / 2)

    orta = cift(0.5, 0.5)
    halka = [[cift(*uv(t, s)) for s in range(n_aci)] for t in halkalar_t]
    for kk in range(len(halkalar_t)):
        for s in range(n_aci):
            c = (s + 1) % n_aci
            t_orta = (halkalar_t[kk] + (halkalar_t[kk - 1] if kk else 0)) / 2
            ust_m = malz_fn(kk, *uv(t_orta, s + 0.5), s)
            for y in (0, 1):  # 0 üst, 1 alt (ters sırada)
                if kk == 0:
                    ks = [orta[y], halka[0][s][y], halka[0][c][y]]
                else:
                    ks = [halka[kk - 1][s][y], halka[kk][s][y], halka[kk][c][y], halka[kk - 1][c][y]]
                f = p.bm.faces.new(ks if y == 0 else list(reversed(ks)))
                f.material_index = p.dilim(alt_fn(ust_m) if y == 1 and alt_fn else ust_m)
    dis = halka[-1]
    for s in range(n_aci):
        c = (s + 1) % n_aci
        f = p.bm.faces.new((dis[c][0], dis[s][0], dis[s][1], dis[c][1]))
        f.material_index = p.dilim(kenar_malz(s) if callable(kenar_malz) else kenar_malz)


def halka_tel(p, merkez, R, kalinlik, malz, kenar=10):
    """Kapalı halka tel (simit): kapak ağzı, varil çemberi. Her yandan görünür, açık kenarı yok."""
    yerlestir(p, Matrix.Translation(merkez), lambda: p.tup(
        [(math.cos(2 * math.pi * s / kenar) * R, math.sin(2 * math.pi * s / kenar) * R, 0) for s in range(kenar)],
        kalinlik, malz, kenar=4, kapali=True))


def tup_renkli(p, noktalar, yaricap, malz_fn, kenar=6):
    """Parca.tup gibi tek parça tüp, ama her boğumun rengi malz_fn(boğum) (pipet şeritleri); iki ucu kapalı."""
    halkalar = []
    for i, q in enumerate(noktalar):
        a, b = noktalar[max(i - 1, 0)], noktalar[min(i + 1, len(noktalar) - 1)]
        t = (Vector(b) - Vector(a)).normalized()
        yardim = Vector((0, 0, 1)) if abs(t.z) < 0.9 else Vector((1, 0, 0))
        u = t.cross(yardim).normalized()
        w = t.cross(u).normalized()
        halkalar.append([Vector(q) + (u * math.cos(2 * math.pi * s / kenar) + w * math.sin(2 * math.pi * s / kenar)) * yaricap
                         for s in range(kenar)])
    v = ortu(p, halkalar, lambda a, b: malz_fn(a))
    p.kapak(v[0], malz_fn(0), ters=True)
    p.kapak(v[-1], malz_fn(len(noktalar) - 2))


def ezikler(r, n, z0, z1):
    """Şişe gövdesindeki ezikler: (yükseklik, açı, derinlik, genişlik)."""
    return [(r.uniform(z0, z1), r.uniform(0, 2 * math.pi), r.uniform(0.15, 0.3), r.uniform(0.04, 0.07)) for _ in range(n)]


def ezik_degeri(ez, z, a, R):
    """Bir noktadaki toplam ezik (0 = düz, 0,3 = yarıçapın %30'u içeri)."""
    t = 0.0
    for zc, ac, d, g in ez:
        da = math.atan2(math.sin(a - ac), math.cos(a - ac)) * R
        t += d * math.exp(-((z - zc) ** 2 + da ** 2) / (2 * g * g))
    return t


def sise_profili(z, L, R):
    """Pet şişenin yarıçapı (dip z = 0, boyun z = L): gövde, yuvarlak omuz, ince boyun."""
    omuz0, omuz1 = 0.6 * L, 0.84 * L
    if z < omuz0:
        return R
    if z < omuz1:
        t = (z - omuz0) / (omuz1 - omuz0)
        return 0.32 * R + 0.68 * R * (1 + math.cos(math.pi * t)) / 2
    return 0.32 * R


def kapak(p, z, Rb, malz, kenar=10):
    """Şişe kapağı (z'den yukarı): dişli halka, kapak gövdesi, kopmuş emniyet halkası parlar."""
    p.silindir((0, 0, z - 0.012), (0, 0, z - 0.004), Rb * 1.2, Rb * 1.2, PARILTI, kenar=kenar)
    p.silindir((0, 0, z), (0, 0, z + 0.035), Rb * 1.18, Rb * 1.12, malz, kenar=kenar)


def sise(p, L, R, govde, kapak_malz, r, ezik_sayisi=3, kenar=10, parlak_omuz=True):
    """Bütün, ezik pet şişe (z boyunca, dip z = 0, boy L, yarıçap R): kapaklı; omzunda ve kapak halkasında parıltı."""
    ez = ezikler(r, ezik_sayisi, 0.1 * L, 0.6 * L)
    zs = [0.0, 0.04 * L] + [L * t / 8 for t in range(1, 8)] + [0.97 * L]
    halkalar = []
    for z in zs:
        Rz = sise_profili(z, L, R) * (0.85 if z == 0 else 1)
        halkalar.append([(math.cos(a) * Rz * (1 - ezik_degeri(ez, z, a, R)), math.sin(a) * Rz * (1 - ezik_degeri(ez, z, a, R)), z)
                         for a in (2 * math.pi * s / kenar for s in range(kenar))])

    def renk(i, s):  # omuzda ışığı yakalayan tek şerit
        return PARILTI if parlak_omuz and abs((zs[i] + zs[i + 1]) / 2 - 0.69 * L) < 0.04 * L else govde

    v = ortu(p, halkalar, renk)
    p.kapak(v[0], govde, ters=True)
    p.kapak(v[-1], govde)
    kapak(p, 0.97 * L, 0.32 * R, kapak_malz, kenar=8)


# ---------------------------------------------------------------- küçük parçalar

def parlayan_plastik_1(r):
    """Yırtık pet şişe (0,5 x 0,4 x 0,2 m, Blender x-y-z): kapaklı boyun, ezilip yassılmış beyaz gövde, ince mercan
    etiket şeridi; kopuk ağzı sivri dişli, dışa açılmış ve dudağı parlıyor; ağızdan yana kıvrılmış bir şerit sarkıyor.
    Burulmuş katlanma çizgileri parlar. Gövde suda kaybolmasın diye beyaz (P_Kopuk), turkuaz yalnız içte."""
    r = random.Random(501)
    p = k.Parca("Parlayan_Plastik_1")
    govde, etiket, ici = "P_Kopuk", "P_Mercan", "P_Su_Turkuaz"
    L, R = 0.40, 0.085
    kenar = 12
    ez = [(0.12, 0.6, 0.2, 0.05), (0.2, 3.4, 0.16, 0.06)]
    kiriklar = [(0.3, 5.0, 0.02, 0.26), (2.9, -4.0, 0.04, 0.25), (4.3, 3.0, 0.02, 0.16)]  # (açı, burgu, z başı, z sonu)
    yassi = (1.2, 0.72)  # ezilmiş: yanlara açılır, yukarıdan basık
    tirtik = [-r.uniform(0.04, 0.08) if s % 2 else 0.0 for s in range(kenar)]  # sivri dişler ağızdan dışa uzanır

    def kirik_degeri(z, a):
        """Burulmuş katlanma çizgileri boyunca dar, derin oluk."""
        t = 0.0
        for a0, egim, z0, z1 in kiriklar:
            if z0 <= z <= z1:
                da = math.atan2(math.sin(a - a0 - egim * (z - z0)), math.cos(a - a0 - egim * (z - z0)))
                t += 0.13 * math.exp(-(da / 0.3) ** 2)
        return t

    def yuzey(z, a, olcek, dz=0.0):
        zz = z + dz
        acilma = 1 + 0.3 * min(max((0.06 - zz) / 0.06, 0.0), 1.6)  # yırtık ağız dışa açılır: içi görünür
        Rz = sise_profili(z, L, R) * (1 - ezik_degeri(ez, z, a, R) - kirik_degeri(z, a)) * olcek * acilma
        return (math.cos(a) * Rz * yassi[0], math.sin(a) * Rz * yassi[1], zz)

    def halka(z, olcek, tirtikli=False):
        return [yuzey(z, 2 * math.pi * s / kenar, olcek, tirtik[s] if tirtikli else 0) for s in range(kenar)]

    zs = [0.0, 0.03, 0.07, 0.11, 0.15, 0.19, 0.23, 0.27, 0.30, 0.33, 0.36, 0.385]
    dis = [halka(z, 1.0, z == 0) for z in zs]
    ic = [halka(z, 0.9, z == 0) for z in zs]
    n = len(zs)
    # Boyundan dış yüzle ağza, ağızdan iç yüzle boyuna, en son boyundaki halkayla kapanan kalın duvar
    halkalar = list(reversed(dis)) + ic + [dis[-1]]

    def renk(a, b):
        if a == n - 1:  # kopuk ağzın dişli dudağı ışığı yakalar
            return PARILTI
        if a < n - 1:
            z = (zs[n - 1 - a] + zs[n - 2 - a]) / 2
            return etiket if 0.13 < z < 0.21 else govde
        return ici if zs[a - n] < 0.1 else govde  # ağza yakın iç turkuaz gölge: ağız açık okunur; derini görünmez

    ortu(p, halkalar, renk)
    kapak(p, 0.385, 0.32 * R, "P_Turuncu")

    # Katlanma çizgilerinin sırtı parlar: oluğun boyunca, ezikten biraz taşan ışık teli
    for a0, egim, z0, z1 in kiriklar:
        zler = [z0 + (z1 + 0.04 - z0) * i / 7 for i in range(8)]
        p.tup([yuzey(z, a0 + egim * (min(z, z1) - z0), 1.02) for z in zler], 0.008, PARILTI, kenar=4)

    # Ağızdan yana kıvrılan yırtık şerit (gövdenin bir dilimi): aşağı değil yana sarkar, kutudan taşmaz
    def serit_nokta(u, v):
        a = math.pi / 2 - 0.35 + 0.75 * u    # arka yanda bir dilim: ağzı kapatmaz
        t = v                                 # 0 ağızda, 1 ucunda
        rr = R * 1.2 * (1.0 + 0.45 * t * t)
        z = -0.06 * t + 0.035 * t * t + 0.012 * math.sin(u * 5)  # geri kıvrılır
        return (math.cos(a) * rr * yassi[0] + 0.03 * t, math.sin(a) * rr * yassi[1], z + 0.01)

    def serit_ici(u, v):
        return v < 0.95 - 0.5 * abs(u - 0.45) - 0.12 * math.sin(u * 13)

    levha(p, 6, 6, serit_ici, serit_nokta, lambda i, j, kd: PARILTI if kd and j > 2 and i % 2 else govde,
          kalin=0.005, kenar_malz=govde)

    # Yatır: şişe x boyunca, boyun +x'te; ağzı kameraya ve biraz yukarı döner ki içi görünsün
    bmesh.ops.transform(p.bm, matrix=matris(donus=(0.15, math.pi / 2 + 0.14, 0.85)), verts=p.bm.verts)
    sigdir(p, 0.5, 0.4, 0.2)
    return p.nesne()


def parlayan_plastik_2(r):
    """Sürüklenen poşet parçası (0,5 x 0,4 x 0,2 m): yırtık kenarlı, bir yerden yumuşakça katlanmış, geniş dalgalı,
    iki yüzlü. Kenarın bir kısmı ve kat çizgisi parlar; üst yüzünde küçük bir mercan baskı (alttan beyaz görünür)."""
    r = random.Random(502)
    p = k.Parca("Parlayan_Plastik_2")
    en, boy = 0.64, 0.42
    loblar = [(r.uniform(0, 2 * math.pi), r.uniform(0.04, 0.08), r.randint(2, 4)) for _ in range(3)]
    yirtik = [r.uniform(-0.08, 0.04) for _ in range(24)]  # yırtık kenarın dişleri
    isikli = [r.random() < 0.6 for _ in range(28)]          # kenarın yalnız bir kısmı ışığı yakalar
    kat_u = 0.72              # katlanma çizgisi
    kat_aci = math.radians(115)
    bukum = 0.09              # katın yuvarlak dönüş boyu

    def sinir(a):
        s = int(round(a / (2 * math.pi) * 24)) % 24
        return 0.95 + sum(g * math.sin(kk * a + f) for f, g, kk in loblar) + yirtik[s]

    def nokta(u, v):
        x, y = (u - 0.5) * en, (v - 0.5) * boy
        # suda kabaran film: alçak frekanslı geniş dalga, üstüne hafif kırışık
        z = 0.06 * math.sin(u * 3.6 + 0.4) * math.cos(v * 2.6 - 0.6) + 0.035 * math.sin(v * 4.2 + u * 1.3) \
            + 0.008 * math.sin(u * 13 + v * 6)
        z -= 0.2 * ((u - 0.4) ** 2 + (v - 0.5) ** 2)  # ortası kabarık, kenarları sarkık
        xk = (kat_u - 0.5) * en + 0.04 * math.sin(v * 5)  # kat çizgisi düz değil
        if x > xk:  # katlanan kanat yuvarlakça geri döner
            d = x - xk
            rho = bukum / kat_aci
            th = kat_aci * min(d / bukum, 1.0)
            x, z2 = xk + rho * math.sin(th), rho * (1 - math.cos(th))
            if d > bukum:
                x += (d - bukum) * math.cos(kat_aci)
                z2 += (d - bukum) * math.sin(kat_aci) * 0.7
            z += z2 + 0.015 * math.sin(y * 14) * min(d / 0.1, 1)
        return (x, y, z)

    def renk(halka, u, v, s):
        if halka == 5:
            return PARILTI if isikli[s] else "P_Kopuk"
        if abs(u - kat_u) < 0.035 and 0.25 < v < 0.8:
            return PARILTI
        if 0.5 < v < 0.64 and 0.22 < u < 0.52:
            return "P_Mercan"
        return "P_Kopuk"

    levha_yuvarlak(p, 28, [0.3, 0.48, 0.63, 0.77, 0.89, 1.0], sinir, nokta, renk, kalin=0.005,
                   kenar_malz=lambda s: PARILTI if isikli[s] else "P_Kopuk",
                   alt_fn=lambda m: "P_Kopuk" if m == "P_Mercan" else m)
    bmesh.ops.transform(p.bm, matrix=matris(donus=(0.12, -0.1, 0.2)), verts=p.bm.verts)
    sigdir(p, 0.5, 0.4, 0.2)
    return p.nesne()


def parca_kirik(p, M, genislik, yukseklik, kavis, malz, r, n_aci=9):
    """Kırık şişe parçası: kavisli (silindir dilimi), tırtıklı kenarlı küçük levha; kırık hattın yalnız
    birkaç dişi ince bir şeritle parlar (halka gibi değil, ışığı yakalayan kenar gibi)."""
    tir = [r.uniform(0.55, 1.0) for _ in range(n_aci)]  # sivri, kırık hat

    def sinir(a):
        return tir[int(round(a / (2 * math.pi) * n_aci)) % n_aci]

    def nokta(u, v):
        a = (u - 0.5) * kavis
        rr = genislik / max(kavis, 1e-3)
        return (math.sin(a) * rr, (math.cos(a) - 1) * rr, (v - 0.5) * yukseklik)

    def isik(s):
        return s % 2 == 0 or tir[s] > 0.85  # dişlerin yarısı kadarı

    yerlestir(p, M, lambda: levha_yuvarlak(p, n_aci, [0.88, 1.0], sinir, nokta,
                                           lambda h, u, v, s: PARILTI if h == 1 and isik(s) else malz, kalin=0.008,
                                           kenar_malz=lambda s: PARILTI if isik(s) else malz))


def parlayan_plastik_3(r):
    """Gevşek küçük çöp kümesi (0,5 x 0,4 x 0,2 m): üç şişe kapağı, bükük pipet, üç kırık parça.
    Kapakların ağız halkası, pipetin kırık uçları ve parçaların birkaç kırık dişi parlar; parçalar biraz ayrık süzülür."""
    r = random.Random(503)
    p = k.Parca("Parlayan_Plastik_3")

    def sise_kapagi(malz, ic_parlak=PARILTI):
        def kur():
            p.silindir((0, 0, 0.004), (0, 0, 0.03), 0.042, 0.04, malz, kenar=10)
            halka_tel(p, (0, 0, 0.004), 0.042, 0.0055, ic_parlak)  # ağız halkası: kapalı tel
        return kur

    for konum, donus, malz, isik in (((-0.17, 0.1, 0.03), (0.6, 0.2, 0), "P_Mercan", PARILTI),
                                     ((0.15, 0.14, -0.04), (2.4, 0.4, 0.5), "P_Kopuk", PARILTI),
                                     ((0.04, -0.13, 0.05), (-0.5, 0.9, 1.0), "P_Turuncu", SICAK)):
        yerlestir(p, matris(konum, donus), sise_kapagi(malz, isik))

    # Pipet: dirsekten bükülmüş tek tüp; iki renk şerit, iki kırık ucu parlar
    yol = [(-0.235, -0.128, -0.042), (-0.22, -0.12, -0.04), (-0.12, -0.06, -0.02), (-0.02, 0.0, 0.0), (0.02, 0.03, 0.01),
           (0.05, 0.08, 0.03), (0.08, 0.18, 0.05), (0.085, 0.2, 0.052)]
    tup_renkli(p, yol, 0.009, lambda s: PARILTI if s in (0, len(yol) - 2) else "P_Mercan" if s % 2 else "P_Kopuk")

    # Kırık parçalar: beyaz, turuncu, mercan gövde; suda kaybolmaz
    parca_kirik(p, matris((0.19, -0.06, 0.0), (1.2, 0.3, 0.8)), 0.13, 0.1, 1.3, "P_Kopuk", r)
    parca_kirik(p, matris((-0.08, 0.12, -0.05), (0.3, 1.4, -0.4)), 0.1, 0.08, 1.0, "P_Turuncu", r)
    parca_kirik(p, matris((-0.04, -0.03, 0.06), (1.7, -0.2, 2.2)), 0.08, 0.06, 0.8, "P_Mercan", r, n_aci=7)
    sigdir(p, 0.5, 0.4, 0.2)
    return p.nesne()


# ---------------------------------------------------------------- girdabın ortasındaki yığın

def kasa(p, M, en, boy, yuk, malz):
    """Plastik kasa (yan duvarlar çıtalı, üstü açık), kendi tabanı z = 0."""
    def kur():
        t = 0.03
        p.kutu((0, 0, t / 2), (en, boy, t), malz)
        for sx in (-1, 1):
            for sy in (-1, 1):
                p.kutu((sx * (en / 2 - t / 2), sy * (boy / 2 - t / 2), yuk / 2), (t, t, yuk), malz)
        for z in (yuk * 0.4, yuk * 0.92):
            for sy in (-1, 1):
                p.kutu((0, sy * (boy / 2 - t / 2), z), (en, t, yuk * 0.14), malz)
            for sx in (-1, 1):
                p.kutu((sx * (en / 2 - t / 2), 0, z), (t, boy, yuk * 0.14), malz)
    yerlestir(p, M, kur)


def varil(p, M, R, L, malz):
    """Plastik varil (z boyunca, taban z = 0): iki çember kabartma, üst kapakta tıpa; çemberler parlar."""
    def kur():
        p.silindir((0, 0, 0), (0, 0, L), R, R, malz, kenar=10)
        for z in (L * 0.3, L * 0.7):
            halka_tel(p, (0, 0, z), R * 1.02, 0.022, PARILTI)  # kapalı çember: içten de görünür
        p.silindir((R * 0.5, 0, L), (R * 0.5, 0, L + 0.04), R * 0.15, R * 0.15, "P_Kopuk", kenar=6)
    yerlestir(p, M, kur)


def yama(kabukta, a0, b0, da, db, d, dalga=0.04):
    """Yığının kabuğuna oturan kavisli yama: (u, v) -> nokta. Poşet ve ağ yığına sarılır, dışarı taşmaz."""
    def nokta(u, v):
        return kabukta(a0 + (u - 0.5) * da, b0 + (v - 0.5) * db, d + dalga * math.sin(u * 7 + v * 5) * math.cos(v * 4 - u))
    return nokta


def poset(p, nokta, malz, r, n_aci=14, parlak=False):
    """Yığına sarılmış poşet: yamaya oturan, yırtık kenarlı levha; parlak ise kenarının yarısı ışığı yakalar."""
    tir = [r.uniform(0.7, 1.0) for _ in range(n_aci)]

    def sinir(a):
        return tir[int(round(a / (2 * math.pi) * n_aci)) % n_aci]

    def kenar(s):
        return PARILTI if parlak and s % 2 == 0 else malz

    levha_yuvarlak(p, n_aci, [0.45, 0.85, 1.0], sinir, nokta, lambda h, u, v, s: kenar(s) if h == 2 else malz,
                   kalin=0.015, kenar_malz=kenar)


def ag_parcasi(p, nokta, malz, n=6):
    """Yığına dolanmış ağ parçası: yamaya oturan n x n göz; ortadaki birkaç tel parlar."""
    for i in range(n + 1):
        p.tup([nokta(i / n, j / n) for j in range(n + 1)], 0.01, PARILTI if i == 2 else malz, kenar=3)
        p.tup([nokta(j / n, i / n) for j in range(n + 1)], 0.01, malz, kenar=3)


def cekirdek(p, R, oran, r):
    """Yığının içindeki sıcak ışıklı çekirdek: yumru, basık küre; parçaların aralarından sızar."""
    enlem, boylam = 7, 12
    tepe = [(r.uniform(0, 2 * math.pi), r.uniform(0.0, 3.0)) for _ in range(5)]

    def yaricap(a, b):
        return R * (1 + 0.2 * sum(math.cos(2 * (a - ta)) * math.sin(b * 3 + tb) for ta, tb in tepe) / 3)

    halkalar = []
    for e in range(1, enlem):
        b = math.pi * e / enlem
        halkalar.append([(math.cos(a) * math.sin(b) * yaricap(a, b), math.sin(a) * math.sin(b) * yaricap(a, b),
                          -math.cos(b) * yaricap(a, b) * oran) for a in (2 * math.pi * s / boylam for s in range(boylam))])
    v = p.yuz(halkalar, SICAK)
    i = p.dilim(SICAK)
    for uc, z in ((v[0], -R * oran), (v[-1], R * oran)):
        o = p.bm.verts.new((0, 0, z))
        for s in range(boylam):
            f = p.bm.faces.new((uc[s], uc[(s + 1) % boylam], o) if z > 0 else (uc[(s + 1) % boylam], uc[s], o))
            f.material_index = i


def plastik_yigini(r):
    """Çöp girdabının ortasındaki parlayan yığın (2,5 x 2,5 x 1,5 m, Blender x-y-z; kök ortada):
    sıcak ışıklı küçük bir çekirdeğin üstü variller, kasalar, yatık şişeler, sarılmış poşetler ve ağ parçalarıyla örtülü;
    ışık yalnız aralardan sızar, kırıntılar kabuğa yarı gömülü parlar. Yavru yığının yanında başlar; yığın girdabın
    parlayan kalbi gibi okunur. Gövde renkleri sıcak ve beyaz ağırlıklı ki turkuaz suda kaybolmasın."""
    r = random.Random(504)
    p = k.Parca("Plastik_Yigini")
    rx, rz = 1.0, 0.58   # yığının yarı eni ve yarı boyu (öğeler bu basık kürenin içinde ve kabuğunda)

    def kabukta(a, b, d):
        """Basık kürenin kabuğunda (a yatay açı, b yükseklik açısı, d: 0 merkez, 1 kabuk) bir nokta."""
        return Vector((math.cos(a) * math.cos(b) * rx * d, math.sin(a) * math.cos(b) * rx * d, math.sin(b) * rz * d))

    def normal(c):
        """Basık kürenin c noktasındaki dışa normali."""
        return Vector((c.x / rx ** 2, c.y / rx ** 2, c.z / rz ** 2)).normalized()

    def disa(c, sapma=0.0):
        """c noktasında dışa bakan (biraz saptırılmış) eksen: parçanın z'si bu yöne döner."""
        yon = (normal(c) + Vector((r.uniform(-1, 1), r.uniform(-1, 1), r.uniform(-1, 1))) * sapma).normalized()
        return yon, yon.to_track_quat("Z", "Y").to_matrix().to_4x4()

    def yatik(c, kalkma):
        """c noktasında kabuğa teğet yatan (ucu biraz dışa kalkan) eksen: şişe yığına yaslanır, diken gibi durmaz."""
        nrm = normal(c)
        t = nrm.cross(Vector((r.uniform(-1, 1), r.uniform(-1, 1), r.uniform(-1, 1)))).normalized()
        yon = (t + nrm * kalkma).normalized()
        return yon, yon.to_track_quat("Z", "Y").to_matrix().to_4x4()

    cekirdek(p, 0.46, 0.68, r)

    # Büyük parçalar: çekirdeğe yarı gömülü variller ve kasalar (biri tepeye yakın: üstten ışık yalnız aralardan)
    for a, b, malz in ((0.5, -0.25, "P_Turuncu"), (2.6, 0.15, "P_Mercan"), (4.4, -0.45, "P_Kopuk"), (5.4, 0.9, "P_Kopuk")):
        c = kabukta(a, b, 0.7)
        yon, don = disa(c, 0.5)
        varil(p, Matrix.Translation(c) @ don @ matris(donus=(math.pi / 2, 0, 0)) @ Matrix.Translation((0, 0, -0.35)), 0.24, 0.7, malz)
    for a, b, malz in ((1.6, 0.3, "P_Turuncu"), (3.6, -0.1, "P_Kopuk"), (5.6, 0.25, "P_Mercan"), (0.2, -0.7, "P_Turuncu"),
                       (1.9, -1.1, "P_Su_Turkuaz"), (1.2, 1.15, "P_Mercan"), (3.3, 1.05, "P_Turuncu")):
        c = kabukta(a, b, 0.76)
        yon, don = disa(c, 0.3)
        kasa(p, Matrix.Translation(c) @ don @ matris((0, 0, -0.22), (0, 0, r.uniform(0, 3))), 0.6, 0.42, 0.34, malz)

    # Poşetler ve ağ parçaları kabuğa sarılır; yalnız üç poşetin kenarı parlar
    for s, (a, b, malz) in enumerate(((0.95, 0.55, "P_Kopuk"), (3.0, 0.75, "P_Kopuk"), (5.1, 0.35, "P_Kopuk"),
                                      (2.2, -0.6, "P_Kopuk"), (4.9, -0.75, "P_Su_Turkuaz"), (0.6, -0.9, "P_Kopuk"),
                                      (3.4, -0.95, "P_Kopuk"), (5.6, -1.15, "P_Kopuk"), (0.1, 1.2, "P_Kopuk"),
                                      (2.3, 1.3, "P_Kopuk"), (4.4, 1.15, "P_Kopuk"),
                                      (1.0, -1.3, "P_Kopuk"), (3.9, -1.35, "P_Mercan"))):
        poset(p, yama(kabukta, a, b, 0.85, 0.75, 0.95, 0.06), malz, r, parlak=s in (1, 4, 9))
    for a, b, da in ((0.0, 0.15, 0.7), (3.5, -0.3, 0.8)):
        ag_parcasi(p, yama(kabukta, a, b, da, 0.65, 0.96, 0.1), "P_Turuncu", n=4)

    # Şişeler: kabuğa yaslanmış, yarı gömülü; ikisi ağzı dışarı saplanmış
    renkler = ["P_Kopuk", "P_Mercan", "P_Su_Turkuaz", "P_Kopuk", "P_Turuncu", "P_Kopuk", "P_Mercan", "P_Su_Turkuaz"]
    for s, govde in enumerate(renkler):
        a = 2 * math.pi * s / len(renkler) + r.uniform(-0.25, 0.25)
        b = (0.7 if s % 3 == 0 else -0.5 if s % 3 == 1 else 0.15) + r.uniform(-0.15, 0.15)
        c = kabukta(a, b, 0.92)
        yon, don = disa(c, 0.5) if s in (2, 5) else yatik(c, 0.3)
        L = r.uniform(0.42, 0.55)
        kapak_m = r.choice(["P_Mercan", "P_Kopuk", "P_Turuncu"])
        yerlestir(p, Matrix.Translation(c - yon * L * 0.55) @ don,
                  lambda: sise(p, L, L * 0.22, govde, kapak_m, r, kenar=8, parlak_omuz=s % 2 == 0))

    # Parlayan kırıntılar: kapaklar ve kırık parçalar kabuğa yarı gömülü
    for s in range(16):
        a = 2 * math.pi * s / 16 + r.uniform(-0.15, 0.15)
        b = r.uniform(-0.9, 1.2)
        c = kabukta(a, b, r.uniform(0.97, 1.02))
        yon, don = disa(c, 0.6)
        isik = SICAK if s % 3 == 0 else PARILTI
        if s % 2:
            govde = r.choice(["P_Mercan", "P_Turuncu", "P_Kopuk"])
            yerlestir(p, Matrix.Translation(c) @ don @ Matrix.Diagonal((1.7, 1.7, 1.7, 1)),
                      lambda: (p.silindir((0, 0, 0.004), (0, 0, 0.03), 0.042, 0.04, govde, kenar=8),
                               halka_tel(p, (0, 0, 0.004), 0.042, 0.006, isik, kenar=8)))
        else:
            parca_kirik(p, Matrix.Translation(c) @ don @ matris(donus=(math.pi / 2, 0, r.uniform(0, 6))), 0.22, 0.16, 1.1,
                        isik if s % 4 == 0 else r.choice(["P_Kopuk", "P_Mercan", "P_Turuncu"]), r, n_aci=7)
    sigdir(p, 2.5, 2.5, 1.5)
    return p.nesne()
