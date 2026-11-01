"""Yosun_Adasi: Bölüm 5'in sonu, yavrunun açık denizden yüzüp sığındığı yüzen sargassum adası.

Ölçü gri kutudan: 12 x 0,6 x 12 m, su yüzünde (y = 0), KÖK ORTADA ve su çizgisinde.
Keçeleşmiş bir çekirdek sudan 0,2-0,4 m yükselir; çevre loblar arasında çekirdeğe doğru açık su kanalları,
kenarlarda suya yayılan dallar, yakında birkaç kopuk küçük öbek (hepsi 14 m içinde).
Alttan (su altından bakan yavru): ince, topaklı keçe; 0,5-1,5 m sarkan kıvrık dallar; ortanın altında
yavrunun (0,16 m) saklanabileceği açık renkli bir oyuk. Oyuğun perdesinde -Y (Unity -Z) yönünde bir kapı
aralığı var; önündeki koy yavrunun yaklaştığı yol. Üstten (uzaklaşan kamera): saplı filizler ve
üzüm gibi açık renk kese salkımlarıyla benekli altın-kahve bir örtü.

Gerçek sargassum: dallanan sap, küçük testere dişli yaprakçıklar, salkım salkım yuvarlak hava keseleri.
Burada yaprak yassı baklava (iki yüzlü kapalı küçük çift piramit), kese 16 üçgenlik küçük küre.
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k

k.PALET["P_Yaprak_Zeytin"] = "#7C8456"  # yaşlı dallar (docs/palet.md)
k.MERKEZLI.add("Yosun_Adasi")           # suda yüzer: kök ortada, su çizgisinde

KAPI_ACI = -math.pi / 2  # oyuğun kapısı ve önündeki koy: Blender -Y (Unity -Z)
KAPI_GENIS = 0.55        # kapı aralığının yarı açısı (~63° toplam)


def aci_farki(a, b):
    """İki açı arasındaki en kısa fark (radyan, 0..pi)."""
    return abs((a - b + math.pi) % (2 * math.pi) - math.pi)


# ---------------------------------------------------------------- yardımcılar

class Lob:
    """Örtünün bir parçası: düzensiz kenarlı, basık kubbe. Üstü h kadar yükselir, altı d kadar iner.
    oyuk > 0 ise altının ortası yukarı kıvrılır (yavrunun saklandığı cep)."""

    def __init__(self, r, cx, cy, yaricap, h, d, en=1.0, aci=0.0, oyuk=0.0):
        self.cx, self.cy, self.R, self.h, self.d = cx, cy, yaricap, h, d
        self.en, self.aci, self.oyuk = en, aci, oyuk
        # Kenar bozulması: üç yumuşak sinüs (lob) + iki sık sinüs (yırtık saçak)
        self.dalga = [(r.uniform(0.06, 0.14), r.randint(2, 5), r.uniform(0, 2 * math.pi)) for _ in range(3)]
        self.dalga += [(r.uniform(0.02, 0.035), r.randint(6, 9), r.uniform(0, 2 * math.pi)) for _ in range(2)]

    def kenar(self, a):
        return self.R * (1 + sum(g * math.sin(n * a + f) for g, n, f in self.dalga))

    def nokta(self, a, t, oyna=0.0):
        """Yerel açı a ve kenara oranla t (0 orta, 1 kenar) -> dünya x, y. oyna: kenara yüzdelik sapma."""
        rr = self.kenar(a) * t * (1 + oyna)
        u, v = math.cos(a) * rr * self.en, math.sin(a) * rr
        c, s = math.cos(self.aci), math.sin(self.aci)
        return self.cx + u * c - v * s, self.cy + u * s + v * c

    def oran(self, x, y):
        """Dünya noktası için t (1'den büyükse dışarıda)."""
        c, s = math.cos(-self.aci), math.sin(-self.aci)
        dx, dy = x - self.cx, y - self.cy
        u, v = (dx * c - dy * s) / self.en, dx * s + dy * c
        a = math.atan2(v, u)
        return math.hypot(u, v) / self.kenar(a), a

    def ust(self, t):
        """Üst yüzey: kenara yakın da sudan en az 5 cm yukarıda (dalgalı su çizgisi titremesin)."""
        return 0.05 + (self.h - 0.05) * max(0.0, 1 - t * t) ** 0.6

    def alt(self, t):
        """Alt yüzey: ince keçe; oyuklu lobda ortası -0,1 m'lik tavana kıvrılır."""
        z = -0.05 - (self.d - 0.05) * max(0.0, 1 - t * t) ** 0.5
        if self.oyuk > 0 and t < 0.55:
            k_ = (0.55 - t) / 0.55
            z = z + (-0.1 - z) * (math.sin(k_ * math.pi / 2) ** 0.7) * self.oyuk
        return z


def lob_mesh(p, lob, r, seg=18):
    """Lobu kapalı tek kabuk olarak kurar: topaklı üst kubbe, suya inen sığ pah, ince topaklı alt.
    Malzeme: üst Pişmiş üstünde yumuşak Pas/Kum_Sıcak lekeler (kenara doğru Pas artar), pah ve yan duvar Pas,
    alt Pişmiş, oyuğun tavanı Okra (su altında aydınlık bir köşe gibi okunsun)."""
    ust_t = [0.12, 0.28, 0.44, 0.58, 0.7, 0.8, 0.9]
    alt_t = [0.95, 0.8, 0.62, 0.45, 0.3, 0.16] if lob.oyuk else [0.92, 0.72, 0.48, 0.24]
    oyna = [r.uniform(-0.06, 0.06) for _ in range(seg)]  # kenar başına yırtık saçak

    def halka(t, z):
        return [(*lob.nokta(2 * math.pi * s / seg, t, oyna[s] * t), z(s)) for s in range(seg)]

    halkalar, mlz = [], []
    for t in ust_t:  # dış iki halkada gürültü yok: su çizgisinden uzak dursun
        g = 0.06 if t < 0.75 else 0.0
        halkalar.append(halka(t, lambda s, t=t, g=g: lob.ust(t) + r.uniform(-g, g)))
    halkalar.append(halka(1.0, lambda s: -0.03))  # pah: suya dalan kenar
    for t in alt_t:
        if lob.oyuk and t < 0.5:
            z = lambda s, t=t: lob.alt(t) + r.uniform(-0.03, 0.03)
        else:
            z = lambda s, t=t: min(-0.06, lob.alt(t) + r.uniform(-0.08, 0.08))
        halkalar.append(halka(t, z))
    # Bant malzemeleri (halka i -> i+1). Üstte yumuşak lekeler: gölgeli (Pas) ve güneş yanığı (Kum_Sicak)
    # yamalar; düz turuncu tabak değil, iç içe geçmiş keçe gibi okunsun. Kenara yaklaştıkça Pas artar.
    def alaca(x, y, esik):
        n = math.sin(x * 1.3 + 1.1) * math.sin(y * 1.1 + 0.4) + 0.6 * math.sin(x * 2.3 - y * 1.9 + 2.0)
        n += 0.35 * math.sin(x * 4.1 + y * 3.3) + r.uniform(-0.25, 0.25)  # halka sınırları çizgi gibi durmasın
        return "P_Kaya_Pas" if n > esik else ("P_Kum_Sicak" if n < -1.0 else "P_Kaya_Pismis")

    for i in range(len(halkalar) - 1):
        if i < len(ust_t) - 1:
            esik = 0.4 if i < len(ust_t) - 2 else -0.1
            mlz.append(lambda q, esik=esik: alaca(q[0], q[1], esik))
        elif i <= len(ust_t):  # pah ve dış alt bant: yan duvar
            mlz.append(lambda q: "P_Kaya_Pas")
        else:
            t_ic = alt_t[i - len(ust_t)]
            mlz.append(lambda q, t_ic=t_ic: "P_Kaya_Okra" if lob.oyuk and t_ic < 0.5 else "P_Kaya_Pismis")
    v = [[p.bm.verts.new(q) for q in h] for h in halkalar]
    for i in range(len(v) - 1):
        for s in range(seg):
            c = (s + 1) % seg
            p.bm.faces.new((v[i][s], v[i][c], v[i + 1][c], v[i + 1][s])).material_index = p.dilim(mlz[i](halkalar[i][s]))
    tepe = p.bm.verts.new((lob.cx, lob.cy, lob.ust(0) + r.uniform(-0.03, 0.03)))
    dip = p.bm.verts.new((lob.cx, lob.cy, lob.alt(0)))
    for uc, o, m, ters in ((v[0], tepe, "P_Kaya_Pismis", False), (v[-1], dip, "P_Kaya_Okra" if lob.oyuk else "P_Kaya_Pismis", True)):
        i = p.dilim(m)
        for s in range(seg):
            f = (uc[s], uc[(s + 1) % seg], o)
            p.bm.faces.new(f[::-1] if ters else f).material_index = i


def yaprak(p, merkez, yon, boy, en, malz, normal=(0, 0, 1), kalin=0.012, bukum=0.03, tek=False):
    """Yassı baklava yaprakçık (kapalı çift piramit, 8 üçgen). Ucu normalin tersine hafif bükük.
    tek=True: sadece üst yarı (4 üçgen); örtünün üstüne yatan, altı görünmeyen yapraklar için."""
    c, d, n = Vector(merkez), Vector(yon).normalized(), Vector(normal).normalized()
    yan = n.cross(d).normalized()
    if yan.length < 0.5:
        yan = d.orthogonal().normalized()
    n = d.cross(yan).normalized() * (1 if d.cross(yan).dot(Vector(normal)) >= 0 else -1)
    kok = c - d * boy * 0.5
    uc = c + d * boy * 0.5 - n * bukum
    sol = c - d * boy * 0.08 + yan * en * 0.5
    sag = c - d * boy * 0.08 - yan * en * 0.5
    ust = c + n * kalin
    alt = c - n * kalin
    if tek:
        kok, uc = kok - n * kalin, uc - n * kalin * 0.5
    i = p.dilim(malz)
    kenar = [p.bm.verts.new(q) for q in (kok, sol, uc, sag)]
    vu, va = p.bm.verts.new(ust), p.bm.verts.new(alt)
    for s in range(4):
        a, b = kenar[s], kenar[(s + 1) % 4]
        p.bm.faces.new((a, b, vu)).material_index = i
        if not tek:
            p.bm.faces.new((b, a, va)).material_index = i


def kese(p, merkez, yaricap, malz):
    """Hava kesesi: iki dörtlü halka + iki kutup, 16 üçgenlik küçük küre."""
    cx, cy, cz = merkez
    halkalar = []
    for z, don in ((-0.4, 0.0), (0.4, math.pi / 4)):
        rr = yaricap * 0.92
        halkalar.append([(cx + math.cos(don + math.pi / 2 * b) * rr, cy + math.sin(don + math.pi / 2 * b) * rr, cz + z * yaricap)
                         for b in range(4)])
    i = p.dilim(malz)
    v = [[p.bm.verts.new(q) for q in h] for h in halkalar]
    for b in range(4):
        a0, a1 = v[0][b], v[0][(b + 1) % 4]
        b0, b1 = v[1][b], v[1][(b + 1) % 4]
        p.bm.faces.new((a0, a1, b0)).material_index = i
        p.bm.faces.new((a1, b1, b0)).material_index = i
    alt = p.bm.verts.new((cx, cy, cz - yaricap))
    ust = p.bm.verts.new((cx, cy, cz + yaricap))
    for b in range(4):
        p.bm.faces.new((v[0][(b + 1) % 4], v[0][b], alt)).material_index = i
        p.bm.faces.new((v[1][b], v[1][(b + 1) % 4], ust)).material_index = i


def kese_salkimi(p, merkez, r, adet, boy, malz, dz=(-0.6, 0.6)):
    """Birbirine değen 3-5 keseden üzüm salkımı. dz: dağılımın dikey payı (alttakiler aşağı sarkar)."""
    c = Vector(merkez)
    for _ in range(adet):
        o = Vector((r.uniform(-1, 1), r.uniform(-1, 1), r.uniform(*dz))).normalized() * boy * r.uniform(0.8, 1.3)
        kese(p, c + o, boy * r.uniform(0.75, 1.1), malz)


def sap(p, noktalar, r0, r1, malz, kenar=3):
    """Uca doğru incelen sap (r0 -> r1): ring başına kenar köşe, iki uç kapaklı."""
    n = len(noktalar)
    halkalar = []
    for i, q in enumerate(noktalar):
        a = Vector(noktalar[max(i - 1, 0)])
        b = Vector(noktalar[min(i + 1, n - 1)])
        t = (b - a).normalized()
        yardim = Vector((0, 0, 1)) if abs(t.z) < 0.9 else Vector((1, 0, 0))
        u = t.cross(yardim).normalized()
        w = t.cross(u).normalized()
        rr = r0 + (r1 - r0) * i / max(1, n - 1)
        halkalar.append([Vector(q) + (u * math.cos(2 * math.pi * s / kenar) + w * math.sin(2 * math.pi * s / kenar)) * rr
                         for s in range(kenar)])
    v = p.yuz(halkalar, malz)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[-1], malz)


# ---------------------------------------------------------------- model

def yosun_adasi(r):
    """Yüzen sargassum adası, ~12 m: loblu keçe çekirdek (sudan 0,35 m'ye kadar yükselir), çekirdeğe inen
    kanallar, suya yayılan dallar, kopuk öbekler; altta 0,5-1,5 m sarkan kıvrık dallar ve ortada -Y'den
    kapısı açık, yavrunun saklandığı aydınlık oyuk."""
    r = random.Random(5150)
    p = k.Parca("Yosun_Adasi")

    # Loblar: ortada oyuklu çekirdek; çevre loblar aralarında kanal bırakır, -Y'de kapıya giden koy açık
    loblar = [Lob(r, 0.1, 0.1, 2.25, 0.34, 0.32, en=1.12, aci=0.4, oyuk=1.0)]
    for a, mesafe, yr, h, en in ((0.1, 3.75, 1.05, 0.22, 1.45), (1.0, 3.65, 1.0, 0.2, 1.35), (1.85, 3.8, 1.05, 0.18, 1.45),
                                 (2.75, 3.7, 1.1, 0.24, 1.35), (3.6, 3.75, 0.95, 0.16, 1.4), (5.75, 3.7, 1.0, 0.2, 1.35)):
        loblar.append(Lob(r, math.cos(a) * mesafe, math.sin(a) * mesafe, yr, h, h + r.uniform(0.0, 0.04), en=en, aci=a))
    kopuklar = []
    for a, mesafe, yr in ((0.55, 5.7, 0.5), (2.3, 5.75, 0.45), (4.15, 5.3, 0.55), (4.95, 5.6, 0.42), (6.1, 5.6, 0.4)):
        kopuklar.append(Lob(r, math.cos(a) * mesafe, math.sin(a) * mesafe, yr, 0.12, 0.13, en=1.4, aci=a))
    for lob in loblar + kopuklar:
        lob_mesh(p, lob, r, seg=36 if lob.R > 1.5 else (22 if lob.R > 0.8 else 10))
    hepsi = loblar + kopuklar
    merkez = loblar[0]

    def yuzey(x, y):
        """Örtünün o noktadaki üst yüksekliği (lob dışında None: su yüzü)."""
        z = None
        for lob in hepsi:
            t, _ = lob.oran(x, y)
            if t < 1:
                z = max(z or 0, lob.ust(t))
        return z

    def ust_z(x, y, pay=0.04):
        z = yuzey(x, y)
        return (z + pay) if z is not None else 0.012

    # Filizler: görünür ince sap, yaprakçıklar sapa ~40° sırayla sağ-sol, ucunda çoğu zaman kese salkımı
    filiz_renk = ["P_Kaya_Okra", "P_Kaya_Okra", "P_Kum_Sicak", "P_Kaya_Pismis", "P_Kaya_Okra", "P_Kaya_Pas"]

    def filiz(x, y, a, adet):
        noktalar = []
        adim = 0.13 * adet / 2  # sap üç noktalı (ucuz), yapraklar boyunca iki sıra
        for s in range(3):
            noktalar.append(Vector((x, y, ust_z(x, y, 0.035))))
            a += r.uniform(-0.3, 0.3)
            x += math.cos(a) * adim
            y += math.sin(a) * adim
        sap(p, noktalar, 0.018, 0.012, "P_Kaya_Pas" if r.random() < 0.5 else "P_Kaya_Pismis")
        for s in range(1, adet + 1):
            u = (s - 1) / adet * 2  # sap boyunca 0..2: ilk yarı birinci parça, ikinci yarı ikinci parça
            q0 = noktalar[int(u)].lerp(noktalar[int(u) + 1], u - int(u))
            q1 = noktalar[int(u) + 1]
            d = (q1 - q0)
            d.z = 0
            d.normalize()
            taraf = 1 if s % 2 else -1
            yan = Vector((-d.y, d.x, 0)) * taraf
            yd = (d * math.cos(0.7) + yan * math.sin(0.7))
            yd.z = r.uniform(0.05, 0.2)
            yd.normalize()
            boy = r.uniform(0.26, 0.36)
            c = q0.lerp(q1, 0.6) + yd * boy * 0.5
            disarda = yuzey(c.x, c.y) is None
            if disarda:
                c.z = 0.012  # suyun üstüne yatar; alttan da görünsün diye kapalı
            yaprak(p, c, yd, boy, boy * r.uniform(0.32, 0.38), r.choice(filiz_renk), kalin=0.018, bukum=0.03, tek=not disarda)
        if r.random() < 0.5:
            kese_salkimi(p, noktalar[-1] + Vector((0, 0, 0.06)), r, r.randint(3, 4), r.uniform(0.075, 0.1),
                         r.choice(("P_Kum_Sicak", "P_Kum_Sicak", "P_Kaya_Okra")), dz=(0.0, 0.7))

    for lob in hepsi:
        alan = math.pi * lob.R * lob.R * lob.en
        for _ in range(max(2, int(alan * (4.0 if lob is merkez else 3.0)))):  # dış loblar seyrek kalmasın
            t = r.uniform(0.0, 1.0) ** 0.5 * 0.92
            x, y = lob.nokta(r.uniform(0, 2 * math.pi), t)
            filiz(x, y, r.uniform(0, 2 * math.pi), r.choice((4, 6, 6)))

    # Kenar dalları: sap + bazen yan dal, yaprakçıklar ve uçta kese salkımı; suya taşar, kanalları aşar
    def dal(bas, yon_a, boy, malz_sap, renkler, kese_rengi, derinlik=0):
        noktalar = []
        x, y = bas
        a = yon_a
        adim = boy / 5
        for i in range(6):
            z = yuzey(x, y)
            noktalar.append((x, y, (z + 0.04) if z is not None else 0.015))
            a += r.uniform(-0.35, 0.35)
            x += math.cos(a) * adim
            y += math.sin(a) * adim
            if math.hypot(x, y) > 6.8:  # 14 m izin içinde kal
                break
        if len(noktalar) < 3:
            return
        sap(p, noktalar, 0.022 if derinlik == 0 else 0.016, 0.012, malz_sap)
        for i in range(1, len(noktalar)):
            q0, q1 = Vector(noktalar[i - 1]), Vector(noktalar[i])
            d = q1 - q0
            taraf = 1 if i % 2 else -1  # sargassum yaprakları sırayla
            yan = Vector((-d.y, d.x, 0)).normalized() * taraf
            yd = (d.normalized() * 0.75 + yan * 0.65).normalized()
            c = q0.lerp(q1, 0.5) + yd * 0.12
            yaprak(p, c, yd, r.uniform(0.24, 0.32), r.uniform(0.08, 0.1), r.choice(renkler))
        if r.random() < 0.7:
            kese_salkimi(p, Vector(noktalar[-1]) + Vector((0, 0, 0.04)), r, r.randint(3, 4), r.uniform(0.07, 0.09), kese_rengi,
                         dz=(-0.2, 0.6))
        if derinlik == 0 and r.random() < 0.5:
            q = noktalar[2]
            dal((q[0], q[1]), yon_a + r.choice((1, -1)) * r.uniform(0.6, 1.0), boy * 0.6, malz_sap, renkler, kese_rengi, 1)

    ust_renk = ["P_Kaya_Okra", "P_Kaya_Okra", "P_Kaya_Pismis", "P_Kum_Sicak", "P_Yaprak_Zeytin"]
    for lob in hepsi:
        for _ in range(max(2, int(lob.R * lob.en * lob.R * (0.7 if lob is merkez else 1.15)))):
            a = r.uniform(0, 2 * math.pi)
            t = r.uniform(0.6, 0.95)
            x, y = lob.nokta(a, t)
            disari = math.atan2(y - lob.cy * 0.3, x - lob.cx * 0.3) + r.uniform(-0.6, 0.6)
            dal((x, y), disari, r.uniform(0.9, 1.6), "P_Kaya_Pas" if r.random() < 0.5 else "P_Kaya_Pismis",
                ust_renk, "P_Kum_Sicak" if r.random() < 0.6 else "P_Kaya_Okra")

    # Sarkan dallar: kıvrık, incelen, üstü Pişmiş altı Okra; ortada sık, oyuğun çevresinde kapısı açık perde
    def sarkan(bas, boy, kese_var=True, yan_dal=True):
        x, y, z = bas
        noktalar = [Vector((x, y, z + 0.04))]
        n = max(2, int(boy / 0.25))
        sx, sy = r.uniform(-1, 1), r.uniform(-1, 1)  # birikimli sürüklenme yönü
        faz = r.uniform(0, 2 * math.pi)
        for i in range(1, n + 1):
            s = i / n
            sx += r.uniform(-0.6, 0.6)
            sy += r.uniform(-0.6, 0.6)
            sapma = r.uniform(0.06, 0.12)
            x += sx / max(1.0, math.hypot(sx, sy)) * sapma
            y += sy / max(1.0, math.hypot(sx, sy)) * sapma
            kivrim = 0.09 * math.sin(s * math.pi * 2 + faz)  # hafif S
            noktalar.append(Vector((x + kivrim, y - kivrim * 0.6, z - boy * s)))
        bol = max(1, int(n * 0.66))
        sap(p, noktalar[:bol + 1], 0.016, 0.011, "P_Kaya_Pismis")
        sap(p, noktalar[bol:], 0.011, 0.007, "P_Kaya_Okra")
        a = r.uniform(0, 2 * math.pi)
        for i in range(1, n + 1):
            for ara in ((0.0, 0.5) if i < n else (0.0,)):  # düğüm başına iki yaprak, sapın çevresinde döner
                q = noktalar[i].lerp(noktalar[i - 1], ara)
                a += math.pi * r.uniform(0.6, 1.0)
                yan = Vector((math.cos(a), math.sin(a), 0))
                yd = (yan + Vector((0, 0, -0.6))).normalized()
                yaprak(p, q + yd * 0.1, yd, r.uniform(0.18, 0.25), r.uniform(0.065, 0.085),
                       r.choice(("P_Kaya_Pismis", "P_Kaya_Okra", "P_Kaya_Okra", "P_Kum_Sicak")),
                       normal=Vector((0, 0, 1)).cross(yan))
        if kese_var and r.random() < 0.35:
            kese_salkimi(p, noktalar[-1], r, 3, r.uniform(0.05, 0.065), "P_Kum_Sicak", dz=(-0.8, 0.2))
        if yan_dal and boy > 0.75 and r.random() < 0.4:  # kısa yan dal + kese salkımı
            q = noktalar[max(1, n // 2)]
            ya = r.uniform(0, 2 * math.pi)
            uc = q + Vector((math.cos(ya) * 0.22, math.sin(ya) * 0.22, -0.18))
            orta = q.lerp(uc, 0.5) + Vector((0, 0, -0.03))
            sap(p, [q, orta, uc], 0.01, 0.007, "P_Kaya_Pismis")
            kese_salkimi(p, uc + Vector((0, 0, -0.04)), r, 3, r.uniform(0.045, 0.06), "P_Kaya_Okra", dz=(-0.8, 0.2))

    def dunya_aci(x, y):
        return math.atan2(y - merkez.cy, x - merkez.cx)

    for i in range(30):  # oyuğun çevresi: perde, içi boş; kapı yönünde aralık
        a = 2 * math.pi * i / 30 + r.uniform(-0.05, 0.05)
        t = r.uniform(0.55, 0.72)
        x, y = merkez.nokta(a, t)
        fark = aci_farki(dunya_aci(x, y), KAPI_ACI)
        if fark < KAPI_GENIS:  # kapının üstü: kısa saçak (lento), altı yavruya açık
            sarkan((x, y, merkez.alt(t)), r.uniform(0.18, 0.3), kese_var=False, yan_dal=False)
            continue
        if fark < KAPI_GENIS + 0.3:  # kapı pervazı: iki sıra uzun dal, kapının kenarı belli olsun
            x2, y2 = merkez.nokta(a, t + 0.1)
            sarkan((x2, y2, merkez.alt(t + 0.1)), r.uniform(1.0, 1.3), yan_dal=False)
        sarkan((x, y, merkez.alt(t)), r.uniform(0.8, 1.2))
    for i in range(16):  # çekirdeğin dışı
        a = r.uniform(0, 2 * math.pi)
        t = r.uniform(0.78, 0.94)
        x, y = merkez.nokta(a, t)
        if aci_farki(dunya_aci(x, y), KAPI_ACI) < KAPI_GENIS + 0.1:
            continue
        sarkan((x, y, merkez.alt(t)), r.uniform(0.6, 1.0))
    # Kapının hemen içinde tavandan sarkan kese salkımları: oyuk uzaktan "içi dolu, aydınlık köşe" diye okunsun
    for i in range(7):
        a = KAPI_ACI + r.uniform(-0.7, 0.7)
        rr = r.uniform(0.55, 0.95)
        x, y = merkez.cx + math.cos(a) * rr, merkez.cy + math.sin(a) * rr
        t, _ = merkez.oran(x, y)
        tavan = Vector((x, y, merkez.alt(t) + 0.02))
        uc = tavan + Vector((r.uniform(-0.05, 0.05), r.uniform(-0.05, 0.05), -r.uniform(0.12, 0.28)))
        sap(p, [tavan, tavan.lerp(uc, 0.5), uc], 0.01, 0.007, "P_Kaya_Pismis")
        kese_salkimi(p, uc, r, r.randint(3, 5), r.uniform(0.04, 0.055), r.choice(("P_Kum_Sicak", "P_Kaya_Okra")), dz=(-0.9, 0.1))
    for i in range(6):  # tavanda yapışık kese salkımları
        a, rr = r.uniform(0, 2 * math.pi), r.uniform(0.1, 0.8)
        x, y = merkez.cx + math.cos(a) * rr, merkez.cy + math.sin(a) * rr
        t, _ = merkez.oran(x, y)
        kese_salkimi(p, (x, y, merkez.alt(t) - 0.03), r, 3, r.uniform(0.04, 0.05), "P_Kum_Sicak", dz=(-0.6, 0.0))

    for lob in loblar[1:]:  # çevre loblar: merkeze yakın olanlar daha uzun
        for _ in range(4):
            t = r.uniform(0.1, 0.75)
            x, y = lob.nokta(r.uniform(0, 2 * math.pi), t)
            uzun = max(0.0, 1 - math.hypot(x, y) / 6.0)
            sarkan((x, y, lob.alt(t)), r.uniform(0.35, 0.55) + uzun * 0.7)
        for _ in range(3):  # kenar saçağı: alttan bakınca lobun kenarı düz tabak gibi durmasın
            t = r.uniform(0.8, 0.92)
            x, y = lob.nokta(r.uniform(0, 2 * math.pi), t)
            sarkan((x, y, lob.alt(t)), r.uniform(0.3, 0.45), kese_var=False, yan_dal=False)
    for lob in hepsi:  # altta yapışık kese salkımları: alt yüzey gövde değil, keçeleşmiş dal gibi okunsun
        for _ in range(max(1, int(lob.R * lob.en * 0.8))):
            t = r.uniform(0.2, 0.85) if lob is not merkez else r.uniform(0.6, 0.9)
            x, y = lob.nokta(r.uniform(0, 2 * math.pi), t)
            kese_salkimi(p, (x, y, lob.alt(t) - 0.05), r, 3, r.uniform(0.05, 0.07),
                         r.choice(("P_Kaya_Okra", "P_Kum_Sicak")), dz=(-0.6, 0.0))
    for lob in kopuklar:
        for _ in range(2):
            x, y = lob.nokta(r.uniform(0, 2 * math.pi), 0.3)
            sarkan((x, y, lob.alt(0.3)), r.uniform(0.3, 0.5), yan_dal=False)
    return p.nesne()
