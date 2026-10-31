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

        /// <summary>Kartı oynar: bedeli düşülür, etkisi uygulanır. Hedef bölge ya da yuva sırasıdır (hedefsizse -1).</summary>
        public void KartOyna(KartTanimi kart, int hedef)
        {
            yonetim.ButceDegistir(-yonetim.Bedel(kart));
            switch (kart.Etki)
            {
                case KartEtkisi.Golgelik:
                    kumsal.YapiKoy(hedef, BolgeYapisi.Golgelik);
                    break;
                case KartEtkisi.TemizlikKampanyasi:
                    // En kirli bölgelerden başlayarak.
                    foreach (int bolge in EnCok(BolgeKatmani.Cop, kart.Deger)) kumsal.KatmanDegistir(bolge, BolgeKatmani.Cop, -1);
                    break;
                case KartEtkisi.KumulOnarimi:
                    kumsal.YapiKoy(hedef, BolgeYapisi.KumulOnarimi);
                    break;
                case KartEtkisi.YuvaTasima:
                    YuvaTasi(hedef);
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
                    kumsal.DenizdekiAgDegistir(-kart.Deger);
                    break;
                case KartEtkisi.GonulluCagrisi:
                    yonetim.EkGonulluEkle(kart.Deger);
                    break;
                case KartEtkisi.SahaArastirmasi:
                    SahaArastirmasi();
                    break;
                case KartEtkisi.TemizEnerji:
                    kumsal.TemizEnerjiAyarla(true);
                    break;
            }
            yonetim.KartOynandi(kart);
        }

        /// <summary>Yuvayı aynı bölgede dalga çizgisinin üstüne, serin banda taşır; gölgelikli bir bölge varsa oraya.</summary>
        private void YuvaTasi(int yuva)
        {
            Yuva y = kumsal.Yuvalar[yuva];
            int bolge = y.Bolge;
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.Golgelik)) { bolge = i; break; }

            Vector3 konum = kum.BolgeNoktasi(bolge, Ayarlar.TasimaUzakligi);
            SicaklikBandi eski = y.Bant;
            kumsal.YuvaTasi(yuva, bolge, konum, kum.DalgaAltinda(konum));
            int fark = (int)kum.Bant(konum) - (int)eski;
            if (fark != 0) kumsal.YuvaBantDegistir(yuva, fark);
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

        // ---- Olaylar ----

        /// <summary>A seçeneği açık mı: bütçe yetiyor mu, halk desteği yeterli mi.</summary>
        public bool SecenekAcik(OlayTanimi olay, bool a)
        {
            if (yonetim.Butce + olay.Butce(a) < 0) return false;
            return !(a && olay.DestekIster && yonetim.Destek < Ayarlar.DusukDestekEsigi);
        }

        public void OlaySec(OlayTanimi olay, bool a)
        {
            yonetim.ButceDegistir(olay.Butce(a));
            yonetim.DestekDegistir(olay.Destek(a));
            if (olay.Uyari)
            {
                yonetim.UyariAyarla(olay.Etki);
                return;
            }
            if (!a) return;

            switch (olay.Etki)
            {
                case OlayEtkisi.Otel:
                    kumsal.KatmanDegistir(RastgeleBolge(), BolgeKatmani.Sezlong, Ayarlar.OtelSezlongu);
                    break;
                case OlayEtkisi.Festival:
                    int bolge = RastgeleBolge();
                    kumsal.GeciciKatman(bolge, BolgeKatmani.Isik, Ayarlar.FestivalArtisi, 1);
                    kumsal.GeciciKatman(bolge, BolgeKatmani.Insan, Ayarlar.FestivalArtisi, 1);
                    break;
                case OlayEtkisi.Balikci:
                    kumsal.DenizdekiAgDegistir(-1);
                    break;
                case OlayEtkisi.Okul:
                    yonetim.SonrakiTurMesgul(1);
                    break;
            }
        }

        /// <summary>Tur başında geçen turun uyarısı gerçekleşir: sıcak dalgası bu tur ve gelen gece kumu ısıtır, fırtına dalga altındaki yuvaları basar.</summary>
        public void UyariyiUygula()
        {
            if (yonetim.UyariBekliyor(OlayEtkisi.SicakDalga))
            {
                // Temiz Enerji oynandıysa sıcak dalgası daha zayıf: bazen kumu ısıtmaz.
                if (!kumsal.TemizEnerji || Random.value >= Ayarlar.TemizEnerjiKorumasi)
                {
                    kumsal.GeciciKumIsinmasi(1, 1);
                    yonetim.RaporEkle("rapor_sicak_dalga", 1);
                }
                else yonetim.RaporEkle("rapor_sicak_dalga_zayif", 0);
            }
            else if (yonetim.UyariBekliyor(OlayEtkisi.Firtina))
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
                yonetim.RaporEkle("rapor_firtina", basan);
            }
            yonetim.UyariAyarla(null);
        }

        // ---- Tur sonu ----

        /// <summary>GDD: çöp birikir, korunmayan yuvalara avcı gelebilir. Sonuçlar gelecek turun raporuna yazılır.</summary>
        public void TurSonu()
        {
            int copArtan = 0;
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
            {
                if (Random.value >= Ayarlar.CopBirikimi) continue;
                kumsal.KatmanDegistir(i, BolgeKatmani.Cop, 1);
                copArtan++;
            }

            int bozulan = 0;
            // Tilki Çiti tilkinin gelme ihtimalini düşürür (GDD: yarıya).
            float ihtimal = Ayarlar.AvciIhtimali * (CitVar() ? Ayarlar.CitCarpani : 1f);
            for (int i = 0; i < kumsal.Yuvalar.Count; i++)
            {
                Yuva y = kumsal.Yuvalar[i];
                // Kafesli yuvaya giremez; çıkmış (haftası dolmuş) yuvada yumurta kalmadı sayılır.
                if (y.Kafesli || y.Yumurta <= 0 || y.CikisinaHafta <= 0) continue;
                if (Random.value >= ihtimal) continue;
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

        private int RastgeleBolge() => Random.Range(0, kumsal.BolgeSayisi);

        private List<int> EnCok(BolgeKatmani katman, int adet)
        {
            var sira = new List<int>();
            for (int i = 0; i < kumsal.BolgeSayisi; i++) sira.Add(i);
            sira.Sort((a, b) => kumsal.BolgeAl(b).Seviye(katman).CompareTo(kumsal.BolgeAl(a).Seviye(katman)));
            return sira.GetRange(0, Mathf.Min(adet, sira.Count));
        }
    }
}
