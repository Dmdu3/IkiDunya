using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Kartların, görevlerin, olayların ve tur sonu simülasyonunun etkileri. Kumsal sadece KumsalDurumu metotlarıyla değişir (docs/kumsal-durumu.md).</summary>
    public class Etkiler
    {
        private readonly KumsalDurumu kumsal;
        private readonly YonetimDurumu yonetim;
        private readonly KumSicakligi kum;

        private YonetimAyarlari Ayarlar => yonetim.Ayarlar;

        public Etkiler(KumsalDurumu kumsal, YonetimDurumu yonetim, KumSicakligi kum)
        {
            this.kumsal = kumsal;
            this.yonetim = yonetim;
            this.kum = kum;
        }

        // ---- Kartlar ----

        /// <summary>Kartı oynar: bedeli düşülür, etkisi uygulanır, halk ve paydaş farkı işlenir. Hedef bölge ya da yuva sırasıdır (hedefsizse -1).</summary>
        public void KartOyna(KartTanimi kart, int hedef)
        {
            yonetim.ButceDegistir(-yonetim.Bedel(kart));
            int deger = yonetim.Deger(kart);
            switch (kart.Etki)
            {
                case KartEtkisi.Golgelik:
                    // Bölgedeki yuvalar da serinler; gölgelik zaten varsa ikinci kez serinletmez.
                    if (!kumsal.BolgeAl(hedef).Var(BolgeYapisi.Golgelik))
                        for (int i = 0; i < kumsal.Yuvalar.Count; i++)
                            if (kumsal.Yuvalar[i].Bolge == hedef && kumsal.Yuvalar[i].Yumurta > 0)
                                kumsal.YuvaBantDegistir(i, -kum.Ayarlar.GolgelikSerinletme);
                    kumsal.YapiKoy(hedef, BolgeYapisi.Golgelik);
                    break;
                case KartEtkisi.TemizlikKampanyasi:
                    // En kirli bölgelerden başlayarak.
                    foreach (int bolge in EnCok(BolgeKatmani.Cop, deger)) kumsal.KatmanDegistir(bolge, BolgeKatmani.Cop, -1);
                    break;
                case KartEtkisi.KumulOnarimi:
                    kumsal.YapiKoy(hedef, BolgeYapisi.KumulOnarimi);
                    break;
                case KartEtkisi.YuvaTasima:
                    YuvaTasi(hedef, SerinBolge(kumsal.Yuvalar[hedef].Bolge), 0.5f);
                    break;
                case KartEtkisi.IsikYasagi:
                    kumsal.IsikYasagiAyarla(true);
                    break;
                case KartEtkisi.AracYasagi:
                    for (int i = 0; i < kumsal.BolgeSayisi; i++) kumsal.YapiKaldir(i, BolgeYapisi.AracIzi);
                    break;
                case KartEtkisi.SezlongDuzenlemesi:
                    // Engeller yarıya iner.
                    for (int i = 0; i < kumsal.BolgeSayisi; i++)
                    {
                        int sezlong = kumsal.BolgeAl(i).Seviye(BolgeKatmani.Sezlong);
                        if (sezlong > 0) kumsal.KatmanDegistir(i, BolgeKatmani.Sezlong, -Mathf.CeilToInt(sezlong / 2f));
                    }
                    break;
                case KartEtkisi.TilkiCiti:
                    // Kumsalın kenarına çit: tilki kenardan gelir, bir bölge yeter.
                    kumsal.YapiKoy(0, BolgeYapisi.TilkiCiti);
                    break;
                case KartEtkisi.HayaletAgTemizligi:
                    kumsal.DenizdekiAgDegistir(-deger * (yonetim.Dost(Paydas.Balikci) ? Ayarlar.BalikciAgCarpani : 1));
                    break;
                case KartEtkisi.GonulluCagrisi:
                    // Gönüllüler kartlardan önce atanır: çağrı gelecek tura gönüllü ekler.
                    yonetim.SonrakiTurGonullu(deger);
                    break;
                case KartEtkisi.SahaArastirmasi:
                    SahaArastirmasi();
                    break;
                case KartEtkisi.TemizEnerji:
                    kumsal.TemizEnerjiAyarla(true);
                    break;
                case KartEtkisi.Aydinlatma:
                    kumsal.KatmanDegistir(hedef, BolgeKatmani.Isik, -deger);
                    break;
                case KartEtkisi.GozcuKulubesi:
                    kumsal.YapiKoy(hedef, BolgeYapisi.Devriye);
                    break;
                case KartEtkisi.Kuluckahane:
                    Kuluckahane(hedef);
                    break;
                case KartEtkisi.PlajTabelalari:
                    for (int i = 0; i < kumsal.BolgeSayisi; i++) kumsal.GeciciKatman(i, BolgeKatmani.Insan, -deger, 1);
                    break;
                case KartEtkisi.Bagis:
                    // Değer bölendir: halk desteğinin yarısı.
                    yonetim.ButceDegistir(yonetim.Destek / Mathf.Max(1, deger));
                    break;
                case KartEtkisi.BasinBulteni:
                    yonetim.DestekDegistir(Mathf.Max(deger, yonetim.Kanitlar.Count));
                    break;
                case KartEtkisi.Lobi:
                    break;
                case KartEtkisi.KorumaAlani:
                    // Sıfırlama bekleyen süreli etkileri de düşer: Plaj Tabelaları bitince insan geri gelmez.
                    foreach (BolgeKatmani katman in new[] { BolgeKatmani.Sezlong, BolgeKatmani.Insan, BolgeKatmani.Isik })
                        kumsal.KatmanSifirla(hedef, katman);
                    kumsal.YapiKaldir(hedef, BolgeYapisi.AracIzi);
                    yonetim.KorumaEkle(hedef);
                    break;
                case KartEtkisi.OtelOrtakligi:
                    for (int i = 0; i < kumsal.BolgeSayisi; i++) kumsal.KatmanDegistir(i, BolgeKatmani.Isik, -deger);
                    int yer = SerbestBolge();
                    if (yer >= 0) kumsal.KatmanDegistir(yer, BolgeKatmani.Sezlong, Ayarlar.OrtaklikSezlongu);
                    break;
                case KartEtkisi.Kooperatif:
                    kumsal.DenizdekiAgDegistir(-deger);
                    break;
                case KartEtkisi.KumTorbasi:
                    // Fırtına bu turun sonunda; süre 1: ardından gelen gece de sürer.
                    kumsal.GeciciYapi(hedef, BolgeYapisi.KumulOnarimi, 1);
                    break;
                case KartEtkisi.YuvaTahliyesi:
                    for (int i = 0; i < kumsal.Yuvalar.Count; i++)
                    {
                        Yuva y = kumsal.Yuvalar[i];
                        if (y.Yumurta > 0 && kum.DalgaAltinda(y.Konum)) YuvaTasi(i, SerinBolge(y.Bolge), 0.5f);
                    }
                    break;
                case KartEtkisi.AcilSulama:
                    for (int i = 0; i < kumsal.Yuvalar.Count; i++)
                        if (kumsal.Yuvalar[i].Yumurta > 0) kumsal.GeciciYuvaBant(i, -deger, 1);
                    break;
                case KartEtkisi.DronTaramasi:
                    // Açılan her iz KanitAcildi ile InsanTuru'ya gelir, kanıt orada eklenir.
                    for (int i = 0; i < kumsal.KayipIzleri.Count; i++)
                        if (!kumsal.KayipIzleri[i].KanitAcildi) kumsal.KanitAc(i);
                    break;
                case KartEtkisi.Universite:
                    yonetim.EkKartEkle(deger);
                    break;
            }
            if (kart.DestekFarki != 0) yonetim.DestekDegistir(kart.DestekFarki);
            yonetim.IliskiDegistir(kart.PaydasFarki);
            yonetim.KartOynandi(kart);
        }

        /// <summary>Yuvayı verilen bölgede dalga çizgisinin üstüne, serin banda taşır; bandı yeni yerin zemininden kurulur.</summary>
        private void YuvaTasi(int yuva, int bolge, float bolgeIcinde)
        {
            Vector3 konum = kum.BolgeNoktasi(bolge, bolgeIcinde, Ayarlar.TasimaUzakligi);
            kumsal.YuvaTasi(yuva, bolge, konum, kum.DalgaAltinda(konum), kum.ZeminBandi(konum));
        }

        /// <summary>Taşıma hedefi: gölgelikli bir bölge varsa orası, yoksa yuvanın kendi bölgesi.</summary>
        private int SerinBolge(int varsayilan)
        {
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.Golgelik)) return i;
            return varsayilan;
        }

        /// <summary>Yumurtası olan bütün yuvalar seçilen bölgeye yan yana taşınır ve kafeslenir.</summary>
        private void Kuluckahane(int bolge)
        {
            var tasinacak = new List<int>();
            for (int i = 0; i < kumsal.Yuvalar.Count; i++)
                if (kumsal.Yuvalar[i].Yumurta > 0 && kumsal.Yuvalar[i].CikisinaHafta > 0) tasinacak.Add(i);
            for (int s = 0; s < tasinacak.Count; s++)
            {
                int yuva = tasinacak[s];
                YuvaTasi(yuva, bolge, (s + 1f) / (tasinacak.Count + 1f));
                if (!kumsal.Yuvalar[yuva].Kafesli) kumsal.YuvaKafesle(yuva);
            }
        }

        private void SahaArastirmasi()
        {
            var eksik = new List<KanitTuru>();
            foreach (KanitTuru kanit in System.Enum.GetValues(typeof(KanitTuru)))
                if (!yonetim.KanitVar(kanit)) eksik.Add(kanit);
            if (eksik.Count == 0) return;
            KanitTuru bulunan = eksik[Random.Range(0, eksik.Count)];
            yonetim.KanitEkle(bulunan);
            yonetim.RaporEkle($"kanit_{bulunan.ToString().ToLowerInvariant()}", 0);
        }

        // ---- Gönüllü görevleri ----

        /// <summary>Görev bu hedefte bir şey değiştirir mi (çöpsüz bölgeye temizlik, zaten devriyeli bölgeye devriye, kafesli ya da çıkmış yuvaya kafes değiştirmez).</summary>
        public bool GorevIseYarar(GorevTanimi gorev, int hedef) => GorevHedefEngeli(gorev, hedef) == null;

        /// <summary>Görev bu hedefte neden işe yaramaz (metin id'si); yararsa null.</summary>
        public string GorevHedefEngeli(GorevTanimi gorev, int hedef)
        {
            switch (gorev.Etki)
            {
                case GorevEtkisi.Temizlik:
                    return kumsal.BolgeAl(hedef).Seviye(BolgeKatmani.Cop) > 0 ? null : "engel_cop_yok";
                case GorevEtkisi.Devriye:
                    return kumsal.BolgeAl(hedef).Var(BolgeYapisi.Devriye) ? "engel_devriye_var" : null;
                case GorevEtkisi.Kafes:
                case GorevEtkisi.GolgeSulama:
                    Yuva y = kumsal.Yuvalar[hedef];
                    if (y.Yumurta <= 0 || y.CikisinaHafta <= 0) return "engel_yuva_bos";
                    return gorev.Etki == GorevEtkisi.Kafes && y.Kafesli ? "engel_kafes_var" : null;
                default:
                    return null;
            }
        }

        public void GorevVer(GorevTanimi gorev, int hedef)
        {
            yonetim.GonulluKullan();
            switch (gorev.Etki)
            {
                case GorevEtkisi.Temizlik:
                    kumsal.KatmanDegistir(hedef, BolgeKatmani.Cop, -1);
                    break;
                case GorevEtkisi.Devriye:
                    kumsal.GeciciYapi(hedef, BolgeYapisi.Devriye, 1);
                    break;
                case GorevEtkisi.Kafes:
                    kumsal.YuvaKafesle(hedef);
                    break;
                case GorevEtkisi.GolgeSulama:
                    kumsal.GeciciYuvaBant(hedef, -1, 1);
                    break;
                case GorevEtkisi.Bilgilendirme:
                    yonetim.DestekDegistir(1);
                    break;
            }
        }

        /// <summary>Bu turdaki geçerli bir gönüllü atamasının etkisini geri alır.</summary>
        public bool GorevGeriAl(GorevTanimi gorev, int hedef)
        {
            switch (gorev.Etki)
            {
                case GorevEtkisi.Temizlik:
                    kumsal.KatmanDegistir(hedef, BolgeKatmani.Cop, 1);
                    break;
                case GorevEtkisi.Devriye:
                    kumsal.YapiKaldir(hedef, BolgeYapisi.Devriye);
                    break;
                case GorevEtkisi.Kafes:
                    kumsal.YuvaKafesiniKaldir(hedef);
                    break;
                case GorevEtkisi.GolgeSulama:
                    if (!kumsal.GeciciYuvaBantGeriAl(hedef, -1)) return false;
                    break;
                case GorevEtkisi.Bilgilendirme:
                    yonetim.DestekDegistir(-1);
                    break;
                default:
                    return false;
            }
            yonetim.GonulluIadeEt();
            return true;
        }

        // ---- Olaylar ----

        /// <summary>Olay havuza girer mi: koşulu (paydaş küs, en az tur) sağlanıyor mu; sadece zincirle gelenler girmez.</summary>
        public bool OlayUygun(OlayTanimi olay)
        {
            switch (olay.Kosul)
            {
                case OlayKosulu.PaydasKus: return yonetim.Kus(olay.KosulPaydas);
                case OlayKosulu.EnAzTur: return yonetim.Tur >= olay.KosulTur;
                case OlayKosulu.SadeceZincir: return false;
                default: return true;
            }
        }

        /// <summary>Seçenek açık mı: bütçe yetiyor mu, halk desteği yeterli mi.</summary>
        public bool SecenekAcik(OlayTanimi olay, bool a)
        {
            if (yonetim.Butce + olay.Butce(a) < 0) return false;
            return !(a && olay.DestekIster && yonetim.Destek < Ayarlar.DusukDestekEsigi);
        }

        public void OlaySec(OlayTanimi olay, bool a)
        {
            yonetim.ButceDegistir(olay.Butce(a));
            yonetim.DestekDegistir(olay.Destek(a));
            yonetim.IliskiDegistir(olay.Paydaslar(a));
            if (olay.Gonullu(a) != 0) yonetim.SonrakiTurGonullu(olay.Gonullu(a));
            yonetim.ZincirAyarla(Ayarlar.OlaySirasi(olay.Sonraki(a)));
            if (olay.Uyari)
            {
                yonetim.UyariAyarla(olay.Etki);
                return;
            }

            int bolge;
            switch (olay.Etki)
            {
                case OlayEtkisi.Otel when a:
                    bolge = SerbestBolge();
                    if (bolge >= 0) kumsal.KatmanDegistir(bolge, BolgeKatmani.Sezlong, Ayarlar.OtelSezlongu);
                    break;
                case OlayEtkisi.OtelBar when a:
                    bolge = SerbestBolge();
                    if (bolge < 0) break;
                    kumsal.KatmanDegistir(bolge, BolgeKatmani.Isik, Ayarlar.OtelBarArtisi);
                    kumsal.KatmanDegistir(bolge, BolgeKatmani.Insan, Ayarlar.OtelBarArtisi);
                    break;
                case OlayEtkisi.Festival when a:
                    bolge = SerbestBolge();
                    if (bolge < 0) break;
                    kumsal.GeciciKatman(bolge, BolgeKatmani.Isik, Ayarlar.FestivalArtisi, 1);
                    kumsal.GeciciKatman(bolge, BolgeKatmani.Insan, Ayarlar.FestivalArtisi, 1);
                    break;
                case OlayEtkisi.FestivalCop when !a:
                    foreach (int b in EnCok(BolgeKatmani.Insan, Ayarlar.FestivalCopBolgesi)) kumsal.KatmanDegistir(b, BolgeKatmani.Cop, 1);
                    break;
                case OlayEtkisi.Balikci when a:
                    kumsal.DenizdekiAgDegistir(-1);
                    break;
                case OlayEtkisi.Kopek when !a:
                    yonetim.KopekAyarla(true);
                    break;
                case OlayEtkisi.OtelIsik when !a:
                    for (int i = 0; i < kumsal.BolgeSayisi; i++)
                        kumsal.GeciciKatman(i, BolgeKatmani.Isik, Ayarlar.OtelIsikArtisi, Ayarlar.OtelIsikSuresi);
                    break;
                case OlayEtkisi.BalikciAgAtti when !a:
                    kumsal.DenizdekiAgDegistir(1);
                    break;
            }
        }

        /// <summary>Tur sonunda geçen turun uyarısı gerçekleşir: sıcak dalgası kumu ısıtır (gelen gece sürer), fırtına dalga altındaki yuvaları basar.
        /// Kumul Onarımı ve Kum Torbası dalga çizgisini geri çeker (KumSicakligi.DalgaAltinda).</summary>
        private void UyariyiUygula()
        {
            if (yonetim.UyariAktif(OlayEtkisi.SicakDalga))
            {
                // Temiz Enerji oynandıysa sıcak dalgası daha zayıf: bazen kumu ısıtmaz.
                if (!kumsal.TemizEnerji || Random.value >= Ayarlar.TemizEnerjiKorumasi)
                {
                    kumsal.GeciciKumIsinmasi(1, 1);
                    yonetim.SonrakiRaporaEkle("rapor_sicak_dalga", 1);
                }
                else yonetim.SonrakiRaporaEkle("rapor_sicak_dalga_zayif", 0);
            }
            else if (yonetim.UyariAktif(OlayEtkisi.Firtina))
            {
                int basan = 0;
                for (int i = 0; i < kumsal.Yuvalar.Count; i++)
                {
                    Yuva y = kumsal.Yuvalar[i];
                    if (y.Yumurta <= 0 || !kum.DalgaAltinda(y.Konum)) continue;
                    kumsal.YuvaYumurtaKaybet(i, Mathf.CeilToInt(y.Yumurta * Ayarlar.FirtinaKaybi));
                    kumsal.KayipIziEkle(new KayipIzi(y.Bolge, y.Konum, KayipNedeni.Dalga));
                    basan++;
                }
                yonetim.SonrakiRaporaEkle("rapor_firtina", basan);
            }
            if (yonetim.UyariVar) yonetim.UyariBitti();
        }

        // ---- Tur sonu ----

        /// <summary>Yuvaya bu tur sonunda avcı gelme ihtimali (0–1): kafes, Tilki Çiti ve köpekler hesaba katılır. Haritada gösterilir.</summary>
        public float AvciRiski(int yuva)
        {
            Yuva y = kumsal.Yuvalar[yuva];
            // Kafesli yuvaya giremez; çıkmış (haftası dolmuş) yuvada yumurta kalmadı sayılır.
            if (y.Kafesli || y.Yumurta <= 0 || y.CikisinaHafta <= 0) return 0f;
            float ihtimal = Ayarlar.AvciIhtimali * (CitVar() ? Ayarlar.CitCarpani : 1f);
            if (yonetim.Kopek) ihtimal *= Ayarlar.KopekAvciCarpani;
            return Mathf.Clamp01(ihtimal);
        }

        /// <summary>GDD: geçen turun uyarısı gerçekleşir, çöp birikir (kalabalık bölgede daha hızlı), korunmayan yuvalara avcı gelebilir. Sonuçlar gelecek turun raporuna yazılır.</summary>
        public void TurSonu()
        {
            UyariyiUygula();

            int copArtan = 0;
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
            {
                Bolge b = kumsal.BolgeAl(i);
                float ihtimal = Ayarlar.CopBirikimi + Ayarlar.KalabalikCopu * (b.Seviye(BolgeKatmani.Sezlong) + b.Seviye(BolgeKatmani.Insan));
                if (Random.value >= ihtimal) continue;
                kumsal.KatmanDegistir(i, BolgeKatmani.Cop, 1);
                copArtan++;
            }

            int bozulan = 0;
            for (int i = 0; i < kumsal.Yuvalar.Count; i++)
            {
                if (Random.value >= AvciRiski(i)) continue;
                Yuva y = kumsal.Yuvalar[i];
                kumsal.YuvaYumurtaKaybet(i, Mathf.CeilToInt(y.Yumurta * Ayarlar.AvciYumurtaKaybi));
                kumsal.KayipIziEkle(new KayipIzi(y.Bolge, y.Konum, KayipNedeni.Tilki));
                bozulan++;
            }

            // Gelecek turun raporuna (kayda da girer).
            if (copArtan > 0) yonetim.SonrakiRaporaEkle("rapor_cop_birikti", copArtan);
            if (bozulan > 0) yonetim.SonrakiRaporaEkle("rapor_yuva_bozuldu", bozulan);
        }

        private bool CitVar()
        {
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.TilkiCiti)) return true;
            return false;
        }

        /// <summary>Otel ve festivalin gelebileceği rastgele bölge (koruma alanı değil); hepsi korunuyorsa -1.</summary>
        private int SerbestBolge()
        {
            var serbest = new List<int>();
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (!yonetim.Korunuyor(i)) serbest.Add(i);
            return serbest.Count > 0 ? serbest[Random.Range(0, serbest.Count)] : -1;
        }

        private List<int> EnCok(BolgeKatmani katman, int adet)
        {
            var sira = new List<int>();
            for (int i = 0; i < kumsal.BolgeSayisi; i++) sira.Add(i);
            sira.Sort((a, b) => kumsal.BolgeAl(b).Seviye(katman).CompareTo(kumsal.BolgeAl(a).Seviye(katman)));
            return sira.GetRange(0, Mathf.Min(adet, sira.Count));
        }
    }
}
