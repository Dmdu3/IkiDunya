"""Tekerlek izi (yeni hali): kumda bir aracın tek lastiğinin açtığı oluk.

Gri kutu TuzakKalibi("TekerlekIzi", 6 x 0,02 x 0,5); kök tabanın ortasında, boy X boyunca.
Arazi kesilemediği için iz kumun üstüne oturan alçak bir kabartmadır: ortada basık oluk (taban ~0,005 m),
iki yanda lastiğin ittiği yuvarlak, düzensiz kum sırtları (~0,06 m), tabanda kabarık diş izleri,
sırtların dibine savrulmuş kum topakları. Uçlar incelip kumla (P_Kum_Bej) birleşir.
"""

import math
import random

import bmesh
import kumsal_objeleri_uret as k
from mathutils import Matrix, Vector

k.PALET["P_Kum_Acik"] = "#F2C98A"  # uçlarda oluk tabanından kuma geçiş


def gurultu(r, adet=3, dalga=(0.6, 3.5)):
    """Birkaç sinüsün toplamı: boy boyunca yumuşak, tekrarsız dalgalanma (-1..1 civarı)."""
    dalgalar = [(r.uniform(*dalga), r.uniform(0, 2 * math.pi), r.uniform(0.5, 1.0)) for _ in range(adet)]
    toplam = sum(g for _, _, g in dalgalar)
    return lambda x: sum(g * math.sin(f * x + faz) for f, faz, g in dalgalar) / toplam


def yumusak(a, b, t):
    """smoothstep: t, a'dan b'ye giderken 0'dan 1'e yumuşakça çıkar."""
    t = min(max((t - a) / (b - a), 0.0), 1.0)
    return t * t * (3 - 2 * t)


def topak(p, merkez, boyut, aci, malz, zemin):
    """Savrulmuş kum topağı (5-7 cm): basık sekizyüzlü, z ekseninde döndürülmüş.
    Dört köşesi zemin(x, y) yüksekliğine oturur; eğimli yamaçta da ne havada kalır ne gömülür.
    Alt ucu en alçak köşede (en az -2 mm): model kumun altına sarkmaz."""
    sx, sy, sz = boyut
    don = Matrix.Rotation(aci, 3, "Z")
    c = Vector(merkez)
    kose = [c + don @ Vector(v) for v in ((sx, 0, 0), (0, sy, 0), (-sx, 0, 0), (0, -sy, 0))]
    for q in kose:
        q.z = zemin(q.x, q.y) - 0.002
    ust = p.bm.verts.new(c + Vector((0, 0, sz)))
    alt = p.bm.verts.new(Vector((c.x, c.y, max(min(q.z for q in kose), -0.002))))
    v = [p.bm.verts.new(q) for q in kose]
    i = p.dilim(malz)
    for s in range(4):
        a, b = v[s], v[(s + 1) % 4]
        p.bm.faces.new((a, b, ust)).material_index = i
        p.bm.faces.new((b, a, alt)).material_index = i


def dis_sirti(p, a, b, en, yuk, malz, zemin):
    """Oluk tabanında lastik dişinin bıraktığı kabarık sırt (üçgen kesitli, ~1 cm yüksek): a'dan b'ye.
    Taban köşeleri zemin(x, y) yüksekliğine oturur (taban kenara doğru hafif yükselir)."""
    a, b = Vector(a), Vector(b)
    t = (b - a).normalized()
    u = Vector((-t.y, t.x, 0)) * en / 2
    h = Vector((0, 0, yuk))

    def otur(q):
        return Vector((q.x, q.y, zemin(q.x, q.y) - 0.001))

    v = p.yuz([[otur(a - u), otur(a) + h, otur(a + u)], [otur(b - u), otur(b) + h, otur(b + u)]], malz, dongu=False)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[1], malz)


def oluk_izi(p, x, y, yon, uz, en, zemin, malz, parca=3):
    """Diş sırtının hemen arkasındaki koyu basık iz (3 cm enli şerit): boyunca `parca` dilime bölünür,
    her köşe zemin(x, y) + 3,5 mm'ye konur; taban kenara doğru yükselse de gömülmez, uzak üst kamerada titreşmez."""
    t = Vector(yon).normalized()
    n = Vector((-t.y, t.x, 0))
    a = Vector((x, y, 0))

    def nokta(i, yan):
        q = a + t * (uz * i / parca) + n * (yan * en / 2)
        return p.bm.verts.new((q.x, q.y, zemin(q.x, q.y) + 0.0035))

    sira = [(nokta(i, -1), nokta(i, 1)) for i in range(parca + 1)]
    m = p.dilim(malz)
    for (a1, a2), (b1, b2) in zip(sira, sira[1:]):
        p.bm.faces.new((a1, b1, b2, a2)).material_index = m


def acik_yuzeyleri_cevir(o):
    """Parca.nesne() açık yüzeylerin (iz şeridi) yönünü bazen aşağı seçer; Unity arka yüzü çizmez.
    Kenarı açık her parçada ortalama normal aşağı bakıyorsa o parçanın yüzlerini ters çevirir."""
    bm = bmesh.new()
    bm.from_mesh(o.data)
    gorulen = set()
    for f in bm.faces:
        if f.index in gorulen:
            continue
        grup, yigin = [], [f]
        gorulen.add(f.index)
        while yigin:
            g = yigin.pop()
            grup.append(g)
            for e in g.edges:
                for h in e.link_faces:
                    if h.index not in gorulen:
                        gorulen.add(h.index)
                        yigin.append(h)
        acik = any(e.is_boundary for g in grup for e in g.edges)
        if acik and sum(g.normal.z * g.calc_area() for g in grup) < 0:
            for g in grup:
                g.normal_flip()
    bm.to_mesh(o.data)
    bm.free()


def tekerlek_izi(r):
    """Kumsaldan geçmiş aracın tek lastik izi (6 x 0,5 m, 0,07 m yüksek): yavrunun düşüp ters döndüğü oluk.
    Taban okra (sıkışmış kum), iki yanda lastiğin ittiği düzensiz kum sırtları, tabanda ok başı diş izleri,
    sırt diplerinde 7 kum topağı; hafif kıvrımlı, uçlarda iki sırt %30 ene toplanıp sivri biter, renk P_Kum_Bej'e söner.
    2816 üçgen.
    Kendi tohumu (4127) var; verilen r kullanılmaz, model çağrı sırasından bağımsızdır."""
    r = random.Random(4127)
    p = k.Parca("Tekerlek_Izi")
    boy = 6.0
    adim = 0.1
    n = int(round(boy / adim))
    taban_yari = 0.12      # oluk tabanının yarı eni (oluk ~0,3 m)
    taban_z = 0.005
    uc_boy = 0.7           # uçlarda incelme mesafesi

    kivrim = gurultu(r, 2, (0.35, 0.7))

    def orta(x):
        return 0.022 * kivrim(x)

    def incelme(x):
        return yumusak(0.0, uc_boy, boy / 2 - abs(x))

    def genislik(x):
        """Uçta iz ortaya doğru toplanır: iki sırt son 0,7 m'de birleşip yumuşak bir sivri uç yapar (çatal kalmaz)."""
        return 0.3 + 0.7 * incelme(x)

    def taban_yuk(x):
        """Oluk tabanı: boyunca ~0,005 m; uçta son 15 cm'de 0'a iner, kenarı açıkta kalmaz."""
        return taban_z * (0.5 + 0.5 * incelme(x)) * yumusak(0.0, 0.15, boy / 2 - abs(x))

    # Her yan için sırt biçimi: tepe yüksekliği, tepe yeri, dış genişlik boy boyunca dalgalanır.
    # Uzun dalga (lastiğin yalpası) + kısa dalga (kumun topaklanması) birlikte: sırt ray gibi düz durmasın.
    yanlar = {}
    for kenar in (-1, 1):
        uzun = (gurultu(r, 3, (1.0, 3.0)), gurultu(r, 3, (1.2, 4.0)), gurultu(r, 3, (0.8, 3.5)))
        kisa = (gurultu(r, 4, (7.0, 16.0)), gurultu(r, 3, (6.0, 13.0)), gurultu(r, 4, (8.0, 18.0)))
        yanlar[kenar] = (uzun, kisa)

    def sirt_yuk(x, kenar):
        """Sırt tepesinin bu x'teki yüksekliği (incelmeden önce), ~0,04-0,075 m."""
        uzun, kisa = yanlar[kenar]
        return 0.058 + 0.01 * uzun[0](x) + 0.012 * kisa[0](x)

    def kesit(x, kenar):
        """Ortadan dışa: taban ortası, taban, taban kenarı, duvar, iç omuz, tepe, dış omuz, dış etek, dış uç (y kayması, z).
        Dış etek ile uç arasındaki 1,5 cm'lik basık şerit, yumuşak gölgede kenar normalini yukarı çevirir: sırt kumla dikişsiz birleşir.
        Uçlarda sırt yüksekliği tabanın renk geçişiyle aynı eğriyle söner ve iz %30 enine daralır; son satır kumla aynı düzlemde,
        orada renk de P_Kum_Bej olduğu için zeminle çakışma görünmez."""
        uzun, kisa = yanlar[kenar]
        f = incelme(x)
        g = genislik(x)
        h = sirt_yuk(x, kenar) * f
        tepe = 0.168 + 0.01 * uzun[1](x) + 0.007 * kisa[1](x)
        dis = 0.222 + 0.01 * uzun[2](x) + 0.012 * kisa[2](x)
        tz = taban_yuk(x)
        return [(0.0, tz), (taban_yari * 0.6 * g, tz), (taban_yari * g, tz + 0.003 * f), (0.143 * g, max(h * 0.55, tz)),
                ((tepe - 0.014) * g, h * 0.93), ((tepe + 0.008) * g, h), (((tepe + dis) / 2 + 0.006) * g, h * 0.45),
                (dis * g, 0.002 + 0.002 * f), (dis * g + 0.015, 0.0)]

    # Sütun malzemeleri: taban, taban, duvar dibi, duvar, iç omuz, tepe, dış yamaç.
    # İç duvar okra (sıkışmış kum), tepe boydan boya parlak: gölge olmasa da sırtın ağzı renk çizgisiyle okunur.
    # İç omuz sırtın kabarık yerlerinde parlak, alçak yerlerinde açık kum: parlak şeridin eni dalgalanır.
    # Uçta (incelme < 0,75) taban ve duvarın rengini uc_rengi verir; okra alan sivri bir dille biter.
    def uc_rengi(x, y):
        """Uçta taban/duvar üçgeninin rengi: okra alan, ortadan uzaklık oranı incelmeyle küçülen sivri bir dil;
        çevresinde ince P_Kum_Acik geçiş, dışı P_Kum_Bej (arazi kumu)."""
        d = abs(y - orta(x)) / (0.155 * genislik(x))
        esik = (incelme(x) - 0.08) / 0.45
        if d < esik - 0.25:
            return "P_Kaya_Okra"
        return "P_Kum_Acik" if d < esik else "P_Kum_Bej"

    def serit_malz(s, x, kenar):
        if s <= 3:
            return "P_Kaya_Okra"
        f = incelme(x)
        if s == 5 and f > 0.45:
            return "P_Kum_Parlak"
        if s == 4 and f > 0.5:
            return "P_Kum_Parlak" if sirt_yuk(x, kenar) > 0.057 else "P_Kum_Acik"
        return "P_Kum_Bej"

    def yuzey_z(x, kenar, d):
        """Kesitte ortadan d uzaklıktaki yüzey yüksekliği (diş izlerini ve topakları yüzeye oturtmak için)."""
        noktalar = kesit(x, kenar)
        for (y1, z1), (y2, z2) in zip(noktalar, noktalar[1:]):
            if y1 <= d <= y2:
                return z1 + (z2 - z1) * (d - y1) / max(y2 - y1, 1e-6)
        return 0.0

    def zemin(x, y):
        """Dünya x, y'deki iz yüzeyinin yüksekliği."""
        d = y - orta(x)
        return yuzey_z(x, 1 if d >= 0 else -1, abs(d))

    for kenar in (-1, 1):
        satirlar = []
        for j in range(n + 1):
            x = -boy / 2 + j * adim
            y0 = orta(x)
            satirlar.append([(x, y0 + kenar * yy, z) for yy, z in kesit(x, kenar)])
        sutun = len(satirlar[0])
        for j in range(n):
            xm = -boy / 2 + (j + 0.5) * adim
            for s in range(sutun - 1):
                a, b = satirlar[j], satirlar[j + 1]
                q = [a[s], a[s + 1], b[s + 1], b[s]]
                if kenar < 0:
                    q.reverse()
                v = [p.bm.verts.new(c) for c in q]
                if s <= 3 and incelme(xm) < 0.75:
                    # uçta taban/duvar dörtgeni iki üçgene bölünür, her üçgen kendi ortasına göre boyanır:
                    # okra alan basamak basamak değil, çapraz kenarlarla sivrilerek biter (üçgen sayısı değişmez)
                    for uc in ((v[0], v[1], v[2]), (v[0], v[2], v[3])):
                        yuz = p.bm.faces.new(uc)
                        m = sum((t.co for t in uc), Vector()) / 3
                        yuz.material_index = p.dilim(uc_rengi(m.x, m.y))
                        yuz.smooth = True
                    continue
                yuz = p.bm.faces.new(v)
                yuz.material_index = p.dilim(serit_malz(s, xm, kenar))
                yuz.smooth = True  # sırtlar yuvarlak görünsün; diş sırtları ve topaklar köşeli kalır

    # Diş izleri: ok başı (hareket yönüne bakan), her birinin arkasında koyu basık iz
    aralik = 0.15
    x = -boy / 2 + uc_boy * 0.6
    while x < boy / 2 - uc_boy * 0.6:
        f = incelme(x)
        y0 = orta(x)
        tz = taban_yuk(x)
        uc = Vector((x + 0.07, y0, tz))
        for kenar in (-1, 1):
            kol = Vector((x, y0 + kenar * taban_yari * 0.92 * genislik(x), tz))
            yon = (uc - kol)
            # koyu oluk: sırtın arkasında, aynı açıyla
            geri = Vector((-0.028, 0, 0))
            oluk_izi(p, kol.x + geri.x, kol.y, (yon.x, yon.y, 0), yon.length * 0.96, 0.03, zemin, "P_Kum_Sicak")
            # kol ucu tepeye 6 mm kala biter: iki kolun kapakları kesişip tepede çentik bırakmasın
            dis_sirti(p, kol, uc - yon.normalized() * 0.006, 0.03, 0.011 * f, "P_Kum_Bej", zemin)
        x += aralik

    # Lastiğin savurduğu kum topakları (5-7 cm, 7 tane): sırtın en kabarık üç yerinin dış eteğinde ikişerli öbek,
    # biri oluğun içinde. Öbeğin biri P_Kaya_Okra (oluktan kopmuş sıkı kum), öteki P_Kum_Acik; içerideki okra.
    # İkisi de arazi kumundan koyu: karışmış kum gibi okunur.
    adaylar = [(sirt_yuk(x, kn), x, kn) for kn in (-1, 1)
               for x in [-boy / 2 + 0.9 + i * 0.05 for i in range(int((boy - 1.8) / 0.05))]]
    adaylar.sort(reverse=True)
    secilen = []
    for _, x, kn in adaylar:
        if all(abs(x - sx) > 0.8 for sx, _ in secilen):
            secilen.append((x, kn))
        if len(secilen) == 3:
            break
    topaklar = []
    for x, kn in secilen:  # her öbekte iki topak
        topaklar += [(x + r.uniform(-0.04, 0.04), kn, r.uniform(0.215, 0.228), "P_Kaya_Okra"),
                     (x + r.uniform(0.08, 0.14), kn, r.uniform(0.222, 0.235), "P_Kum_Acik")]
    topaklar.append((r.uniform(-1.5, 1.5), r.choice((-1, 1)), r.uniform(0.02, 0.05), "P_Kaya_Okra"))  # oluğun içinde
    for x, kenar, d, malz in topaklar:
        d *= genislik(x)
        s = r.uniform(0.026, 0.036)
        y = orta(x) + kenar * d
        topak(p, (x, y, zemin(x, y)), (s, s * r.uniform(0.7, 1.0), s * r.uniform(0.65, 0.85)),
              r.uniform(0, math.pi), malz, zemin)
    o = p.nesne()
    acik_yuzeyleri_cevir(o)
    return o
