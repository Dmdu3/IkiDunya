"""Yuva kafesi: gönüllülerin yuvanın üstüne kurduğu tel kafes (Toprak'ın "Kafes" kartı; yuvaya tilki giremez).

Ölçü: 1,04 x 1,04 m kare tel kafes, kumdan 0,35 m yukarıda; tel z = -0,05'ten başlar (eteği kuma gömülü görünsün).
Kök tabanın ortasında (z = 0 kum). Yuva_Cukuru kafesin altına ortalanır: yumurta çanağı (r 0,16) kafesin tam
ortasında kalır, duvarlar gerçekteki gibi yuva tümseğine (r 0,32-0,7) gömülür. Kafesin kendi kum kabarığı
duvar boyunca r 0,48-0,68 arasında (köşelerde 0,63-0,89); tümseğin üstünde onun içinde kaybolur.
"""

import math
import random

from mathutils import Vector

import kumsal_objeleri_uret as k

k.PALET.setdefault("P_Tas_Harabe", "#C9B6A3")
k.PALET.setdefault("P_Kabuk_Teal", "#2BA6A0")


def serit(p, noktalar, genislik, malz, yukari=Vector((0, 0, 1)), kalin=0.003):
    """Noktalar boyunca yassı şerit (bant): kapalı, ince dikdörtgen kesitli; iki taraftan da görünür."""
    halkalar = []
    for i, n in enumerate(noktalar):
        a = Vector(noktalar[max(i - 1, 0)])
        b = Vector(noktalar[min(i + 1, len(noktalar) - 1)])
        t = (b - a).normalized()
        yan = t.cross(yukari).normalized() * genislik / 2
        dik = yan.cross(t).normalized() * kalin / 2
        n = Vector(n)
        halkalar.append([n - yan - dik, n + yan - dik, n + yan + dik, n - yan + dik])
    v = p.yuz(halkalar, malz)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[-1], malz)


def levha(p, koseler, y0, y1, malz):
    """xz düzleminde çokgen levha, y0'dan y1'e kalınlıklı (tabela resmi, yazı çizgisi); kapalı prizma."""
    on = [(x, y0, z) for x, z in koseler]
    arka = [(x, y1, z) for x, z in koseler]
    v = p.yuz([on, arka], malz)
    p.kapak(v[0], malz, ters=True)
    p.kapak(v[1], malz)


def oval(cx, cz, rx, rz, n=10):
    """xz düzleminde n köşeli elips (tabeladaki kabuk ve baş)."""
    return [(cx + rx * math.cos(2 * math.pi * s / n), cz + rz * math.sin(2 * math.pi * s / n)) for s in range(n)]


def yuva_kafesi(r):
    """Yuva kafesi, 1,04 x 1,04 x 0,35 m: kare galvaniz tel kafes (yanlarda 10 x 6 cm göz, üstte 10 x 10 cm),
    köşelerde ahşap kazık ve turuncu işaret şeridi, önde kaplumbağa resimli küçük tabela, kafeste numara
    etiketi, gömüldüğü yerin alçak kum kabarığı. Yumurta çanağı tam ortada, üst ızgaradan görünür."""
    r = random.Random(23)
    p = k.Parca("Yuva_Kafesi")
    tel_renk = "P_Tas_Harabe"   # soluk gri-bej: galvaniz tel; kumdan soğuk, gökten sıcak, ikisinin önünde de okunur
    yari = 0.52
    alt, ust = -0.05, 0.35
    tel = 0.006          # göz teli (oyunda okunsun, titremesin diye gerçeğinden kalın)
    cerceve = 0.013      # kenarlardaki kalın tel

    # Kum kabarığı: kafesin gömüldüğü yerde, duvar boyunca alçak, yuvarlak köşeli kare halka.
    # Duvar dibinde 1 cm'yi geçmez (en alttaki göz açık kalsın, yavru çıkabilsin); tepesi dışarıda.
    def kare(h, z, n=48):
        halka = []
        for s in range(n):
            a = 2 * math.pi * s / n
            c, si = math.cos(a), math.sin(a)
            m = max(abs(c), abs(si))
            yuvar = 0.8   # köşe yuvarlaklığı: köşede h * 1,31
            d = h / (m * yuvar + (1 - yuvar))
            bozuk = 1 + 0.012 * math.sin(3 * a + 1.3) + r.uniform(-0.005, 0.005)
            halka.append((c * d * bozuk, si * d * bozuk, z))
        return halka
    p.yuz([kare(0.48, 0.0), kare(0.52, 0.009), kare(0.57, 0.026), kare(0.62, 0.018), kare(0.68, 0.0)], "P_Kum_Parlak")

    # Yan teller: dikeyler 10 cm arayla, yataylar 6 cm arayla; en alt göz kumdan başlar (yavru çıkar, tilki giremez)
    n_dik = 10
    for i in range(1, n_dik):
        t = -yari + 2 * yari * i / n_dik
        for x, y in ((t, -yari), (t, yari), (-yari, t), (yari, t)):
            p.tup([(x, y, alt), (x, y, ust - cerceve)], tel, tel_renk, kenar=4)  # ucu üst çerçevenin içinde biter
    kose = [(-yari, -yari), (yari, -yari), (yari, yari), (-yari, yari)]
    d = yari + tel * 1.5   # yataylar dikeylerin dışına kaynaklı (iç içe geçen teller kararmasın)
    for z in (0.055, 0.115, 0.175, 0.235, 0.295):
        dis = [(-d, -d), (d, -d), (d, d), (-d, d)]
        for a in range(4):
            (x0, y0), (x1, y1) = dis[a], dis[(a + 1) % 4]
            p.tup([(x0, y0, z), (x1, y1, z)], tel, tel_renk, kenar=4)

    # Üst ızgara: 10 x 10 cm göz; iki yöndeki teller farklı yükseklikte, birbirinin üstüne oturur (kesişip kararmasın)
    for i in range(1, n_dik):
        t = -yari + 2 * yari * i / n_dik
        p.tup([(t, -yari, ust + tel * 1.5), (t, yari, ust + tel * 1.5)], tel, tel_renk, kenar=4)
        p.tup([(-yari, t, ust + tel * 3.2), (yari, t, ust + tel * 3.2)], tel, tel_renk, kenar=4)

    # Kalın çerçeve: üst kenar ve dört köşe dikmesi
    for a in range(4):
        (x0, y0), (x1, y1) = kose[a], kose[(a + 1) % 4]
        p.tup([(x0, y0, ust), (x1, y1, ust)], cerceve, tel_renk, kenar=4)
        p.tup([(x0, y0, alt), (x0, y0, ust)], cerceve, tel_renk, kenar=4)

    # Köşe kazıkları (kafesin hemen dışında) ve tepelerine bağlı turuncu işaret şeridi
    kazik_boy = 0.6
    ruzgar = Vector((0.8, 0.6, 0)).normalized()   # bütün şeritler aynı yöne uçar
    for a, (x, y) in enumerate(kose):
        sx, sy = math.copysign(1, x), math.copysign(1, y)
        kx, ky = x + sx * 0.05, y + sy * 0.05
        egim = Vector((sx * 0.02, sy * 0.02, 0))
        p.silindir((kx, ky, -0.05), Vector((kx, ky, kazik_boy)) + egim, 0.025, 0.02, "P_Kaya_Pas", kenar=4)
        # Şerit: kazığın tepesine sarılı halka (kazığa oturur), ucu rüzgârda dalgalanan iki kuyruk
        uc = Vector((kx, ky, kazik_boy - 0.06)) + egim * 0.9
        sar = [uc + Vector((math.cos(2 * math.pi * s / 6) * 0.023, math.sin(2 * math.pi * s / 6) * 0.023, 0)) for s in range(7)]
        serit(p, sar, 0.035, "P_Turuncu", yukari=Vector((0, 0, 1)))
        for kuyruk in range(2):
            bas = uc + Vector((math.cos(0.6 + kuyruk) * 0.022, math.sin(0.6 + kuyruk) * 0.022, -0.005))
            boy = 0.22 + 0.06 * kuyruk + r.uniform(-0.02, 0.02)
            nok = []
            for s in range(7):
                t = s / 6
                nok.append(bas + ruzgar * boy * t + Vector((0, 0, -0.06 * t * t - 0.02 * kuyruk * t + 0.012 * math.sin(t * 7 + a))))
            # Yatık şerit: yüzü çoğunlukla gökyüzüne bakar, haritadan turuncu okunur
            serit(p, nok, 0.04, "P_Turuncu", yukari=(Vector((0, 0, 1)).cross(ruzgar) * 0.35 + Vector((0, 0, 1))).normalized())

    # Tabela: önde (-y) ayrı kazık üstünde, kaplumbağa resmi kumsala bakar
    tx, ty = 0.24, -yari - 0.17
    p.silindir((tx, ty, -0.05), (tx, ty, 0.42), 0.018, 0.016, "P_Kaya_Pas", kenar=4)
    gen, yuk, kal = 0.32, 0.24, 0.015
    tz = 0.42
    p.kutu((tx, ty - 0.02, tz), (gen, kal, yuk), "P_Kopuk")
    # Kaplumbağa silüeti (iri kabuk, ayrı oval baş, geriye süpürülmüş ön yüzgeçler): tabelayı delip iki yüzden görünür
    y0, y1 = ty - 0.02 - kal / 2 - 0.003, ty - 0.02 + kal / 2 + 0.003
    rz = tz + 0.03    # resmin ortası (altında yazı çizgileri)

    def resim(koseler, pay=0.0, malz="P_Kabuk_Teal"):
        levha(p, [(tx + 1.1 * u, rz + 1.1 * w) for u, w in koseler], y0 - pay, y1 + pay, malz)
    # Kabuk biraz daha kalın: üst üste binen parçaların yüzleri çakışıp kararmasın
    resim(oval(-0.005, 0, 0.06, 0.042, 12), pay=0.001)                              # kabuk
    resim(oval(0.08, 0, 0.02, 0.016, 8))                                              # baş (boyun boşluğuyla ayrı)
    for yon in (1, -1):
        resim([(0.03, 0.03 * yon), (0.028, 0.062 * yon), (-0.025, 0.078 * yon), (-0.004, 0.036 * yon)][::yon])  # ön yüzgeç
        resim([(-0.045, 0.026 * yon), (-0.072, 0.05 * yon), (-0.08, 0.036 * yon), (-0.058, 0.02 * yon)][::yon])  # arka yüzgeç
    # Kabuk çizgisi: ortada tek boyuna çizgi; kabuğun önünden taşar, kabuk diye okunur (daha fazlası ızgara gibi durur)
    resim([(-0.04, -0.0035), (0.03, -0.0035), (0.03, 0.0035), (-0.04, 0.0035)], pay=0.002, malz="P_Kopuk")
    # Tabelanın altında yazı çizgileri (yüzgeç uçlarının altında kalır, üst üste binmez)
    for i, w in enumerate((0.22, 0.15)):
        z0 = tz - 0.085 - i * 0.02
        levha(p, [(tx - w / 2, z0), (tx + w / 2, z0), (tx + w / 2, z0 + 0.011), (tx - w / 2, z0 + 0.011)], y0, y1, "P_Gece_Mavi")

    # Numara etiketi: ön yüzde tele bağlı küçük turuncu plaka
    ex = -0.22
    p.kutu((ex, -yari - 0.014, 0.205), (0.1, 0.008, 0.06), "P_Turuncu")
    for i, w in enumerate((0.06, 0.04)):
        z0 = 0.21 - i * 0.02
        levha(p, [(ex - w / 2, z0), (ex + w / 2, z0), (ex + w / 2, z0 + 0.008), (ex - w / 2, z0 + 0.008)],
              -yari - 0.02, -yari - 0.008, "P_Kopuk")
    o = p.nesne()
    # Teller yumuşak gölgeli: dört köşeli tüp düz gölgede yassı çıta gibi durur, yumuşakta yuvarlak tel gibi
    tel_dilim = [s.material.name for s in o.material_slots].index(tel_renk)
    for f in o.data.polygons:
        f.use_smooth = f.material_index == tel_dilim
    return o
