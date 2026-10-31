using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Editor;
using UnityEditor;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>İki Dünya → İnsan kısmı içeriğini kur: kart, gönüllü görevi ve olay asset'lerini GDD'deki başlangıç değerleriyle yazar (varsa üstüne) ve YonetimAyarlari'na bağlar.
    /// Değerler sadece başlangıç; denge Unity'de asset'lerden yapılır ve denge-gunlugu.md'ye yazılır. Bu menü dengeyi baştan yazar.</summary>
    public static class InsanIcerigiKur
    {
        private const string KartKlasoru = "Assets/_Project/ScriptableObjects/Cards";
        private const string GorevKlasoru = "Assets/_Project/ScriptableObjects/VolunteerTasks";
        private const string OlayKlasoru = "Assets/_Project/ScriptableObjects/Events";

        private static PaydasFarki P(int otel = 0, int belediye = 0, int balikci = 0) => new PaydasFarki(otel, belediye, balikci);

        [MenuItem("İki Dünya/İnsan kısmı içeriğini kur")]
        public static void Kur()
        {
            if (!EditorUtility.DisplayDialog("İnsan kısmı içeriği", "Kart, görev ve olay asset'leri GDD'deki başlangıç değerleriyle yeniden yazılacak; asset'lerde yapılan denge kaybolur.", "Kur", "Vazgeç"))
                return;
            YonetimAyarlari ayarlar = Kur(GriKumsalKur.Ayar<YonetimAyarlari>("YonetimAyarlari"));
            Debug.Log($"İnsan kısmı içeriği kuruldu: {ayarlar.Kartlar.Count} kart, {ayarlar.Gorevler.Count} görev, {ayarlar.Olaylar.Count} olay.");
        }

        /// <summary>İçeriği yazar ve ayarlara bağlar; sahne kurulumu da bunu çağırabilir.</summary>
        public static YonetimAyarlari Kur(YonetimAyarlari ayarlar)
        {
            var kartlar = new List<KartTanimi>
            {
                // Altyapı
                Kart("kart_golgelik", KartEtkisi.Golgelik, KartTuru.Altyapi, Hedef.Bolge, 2, kanit: KanitTuru.Isi, ucuzlatir: true),
                Kart("kart_kumul", KartEtkisi.KumulOnarimi, KartTuru.Altyapi, Hedef.Bolge, 3, kanit: KanitTuru.Su),
                Kart("kart_tilki_citi", KartEtkisi.TilkiCiti, KartTuru.Altyapi, Hedef.Yok, 3, bastan: false, kanit: KanitTuru.Avci, tek: true),
                Kart("kart_aydinlatma", KartEtkisi.Aydinlatma, KartTuru.Altyapi, Hedef.Bolge, 2, deger: 2, kanit: KanitTuru.Isik, ucuzlatir: true),
                Kart("kart_gozcu_kulubesi", KartEtkisi.GozcuKulubesi, KartTuru.Altyapi, Hedef.Bolge, 3, bakim: 1),
                Kart("kart_kulucka", KartEtkisi.Kuluckahane, KartTuru.Altyapi, Hedef.Bolge, 5, tek: true, destek: 1),
                // Kampanya
                Kart("kart_temizlik", KartEtkisi.TemizlikKampanyasi, KartTuru.Kampanya, Hedef.Yok, 2, deger: 3, guclu: 4, kanit: KanitTuru.Cop),
                Kart("kart_yuva_tasima", KartEtkisi.YuvaTasima, KartTuru.Kampanya, Hedef.Yuva, 2, bastan: false, kanit: KanitTuru.Su),
                Kart("kart_ag_temizligi", KartEtkisi.HayaletAgTemizligi, KartTuru.Kampanya, Hedef.Yok, 3, paydas: P(balikci: 1)),
                Kart("kart_gonullu_cagrisi", KartEtkisi.GonulluCagrisi, KartTuru.Kampanya, Hedef.Yok, 1, deger: 2),
                Kart("kart_plaj_tabela", KartEtkisi.PlajTabelalari, KartTuru.Kampanya, Hedef.Yok, 1, guclu: 2, kanit: KanitTuru.Isik),
                Kart("kart_bagis", KartEtkisi.Bagis, KartTuru.Kampanya, Hedef.Yok, 0, deger: 2, destek: -1),
                Kart("kart_basin", KartEtkisi.BasinBulteni, KartTuru.Kampanya, Hedef.Yok, 1, paydas: P(otel: -1)),
                Kart("kart_lobi", KartEtkisi.Lobi, KartTuru.Kampanya, Hedef.Yok, 2, paydas: P(belediye: 1)),
                // Politika
                Kart("kart_isik", KartEtkisi.IsikYasagi, KartTuru.Politika, Hedef.Yok, 3, bastan: false, kanit: KanitTuru.Isik, tek: true,
                    paydas: P(otel: -2), kosul: true, kimden: Paydas.Belediye, enAz: 0),
                Kart("kart_arac", KartEtkisi.AracYasagi, KartTuru.Politika, Hedef.Yok, 1, bastan: false, kanit: KanitTuru.Cop, tek: true, paydas: P(balikci: -1)),
                Kart("kart_sezlong", KartEtkisi.SezlongDuzenlemesi, KartTuru.Politika, Hedef.Yok, 2, bastan: false, kanit: KanitTuru.Cop, tek: true, paydas: P(otel: -1)),
                Kart("kart_koruma_alani", KartEtkisi.KorumaAlani, KartTuru.Politika, Hedef.Bolge, 4, tek: true, bakim: 1,
                    paydas: P(otel: -2), kosul: true, kimden: Paydas.Belediye, enAz: 2),
                Kart("kart_otel_ortakligi", KartEtkisi.OtelOrtakligi, KartTuru.Politika, Hedef.Yok, 2, tek: true, paydas: P(otel: 2)),
                Kart("kart_kooperatif", KartEtkisi.Kooperatif, KartTuru.Politika, Hedef.Yok, 3, tek: true, paydas: P(balikci: 2)),
                Kart("kart_temiz_enerji", KartEtkisi.TemizEnerji, KartTuru.Politika, Hedef.Yok, 4, tek: true, destek: 1),
                // Acil: uyarı gelince ele girer
                Kart("kart_kum_torbasi", KartEtkisi.KumTorbasi, KartTuru.Acil, Hedef.Bolge, 2, acil: OlayEtkisi.Firtina),
                Kart("kart_tahliye", KartEtkisi.YuvaTahliyesi, KartTuru.Acil, Hedef.Yok, 3, acil: OlayEtkisi.Firtina),
                Kart("kart_acil_sulama", KartEtkisi.AcilSulama, KartTuru.Acil, Hedef.Yok, 2, guclu: 2, kanit: KanitTuru.Isi, acil: OlayEtkisi.SicakDalga),
                // Araştırma
                Kart("kart_saha_arastirmasi", KartEtkisi.SahaArastirmasi, KartTuru.Arastirma, Hedef.Yok, 1),
                Kart("kart_dron", KartEtkisi.DronTaramasi, KartTuru.Arastirma, Hedef.Yok, 2),
                Kart("kart_universite", KartEtkisi.Universite, KartTuru.Arastirma, Hedef.Yok, 3, tek: true),
            };

            var gorevler = new List<GorevTanimi>
            {
                Gorev("gorev_temizlik", GorevEtkisi.Temizlik, Hedef.Bolge),
                Gorev("gorev_devriye", GorevEtkisi.Devriye, Hedef.Bolge),
                Gorev("gorev_kafes", GorevEtkisi.Kafes, Hedef.Yuva),
                Gorev("gorev_golge", GorevEtkisi.GolgeSulama, Hedef.Yuva),
                Gorev("gorev_bilgilendirme", GorevEtkisi.Bilgilendirme, Hedef.Yok),
            };

            // Zincirle gelenler önce: her zaman gelenler onlara bağlanır.
            OlayTanimi otelBar = Olay("olay_otel_bar", OlayEtkisi.OtelBar, ab: 4, ap: P(otel: 1), bp: P(otel: -2), destekIster: true, kosul: OlayKosulu.SadeceZincir);
            OlayTanimi festivalCop = Olay("olay_festival_cop", OlayEtkisi.FestivalCop, ag: -2, kosul: OlayKosulu.SadeceZincir);
            OlayTanimi agGetirdi = Olay("olay_balikci_ag_getirdi", OlayEtkisi.Genel, ad: 2, bb: 2, kosul: OlayKosulu.SadeceZincir);

            var olaylar = new List<OlayTanimi>
            {
                Olay("olay_otel", OlayEtkisi.Otel, ab: 3, ap: P(otel: 1), bp: P(otel: -1), destekIster: true, aSonraki: otelBar),
                otelBar,
                Olay("olay_festival", OlayEtkisi.Festival, ad: 2, ap: P(belediye: 1), bp: P(belediye: -1), destekIster: true, aSonraki: festivalCop),
                festivalCop,
                Olay("olay_balikci", OlayEtkisi.Balikci, ab: -2, ap: P(balikci: 2), bp: P(balikci: -1), aSonraki: agGetirdi),
                agGetirdi,
                Olay("olay_sicak_dalga", OlayEtkisi.SicakDalga, uyari: true),
                Olay("olay_firtina", OlayEtkisi.Firtina, uyari: true),
                Olay("olay_okul", OlayEtkisi.Okul, ad: 2, ag: -1),
                Olay("olay_gazeteci", OlayEtkisi.Genel, ad: 2, ap: P(otel: -1, belediye: -1), bd: -1, bp: P(belediye: 1)),
                Olay("olay_kopek", OlayEtkisi.Kopek, ab: -1, ap: P(otel: -1)),
                Olay("olay_bagisci", OlayEtkisi.Genel, ab: 5, ad: -1, ap: P(otel: 1)),
                Olay("olay_universite", OlayEtkisi.Genel, ab: -1, ag: 2, kosul: OlayKosulu.EnAzTur, enAzTur: 5),
                Olay("olay_otel_isik", OlayEtkisi.OtelIsik, ab: -2, ap: P(belediye: 1), kosul: OlayKosulu.PaydasKus, kimden: Paydas.Otel),
                Olay("olay_belediye_kesinti", OlayEtkisi.Genel, ag: -1, ap: P(belediye: 1), bb: -3, kosul: OlayKosulu.PaydasKus, kimden: Paydas.Belediye),
                Olay("olay_balikci_ag_atti", OlayEtkisi.BalikciAgAtti, ab: -2, bp: P(balikci: 1), kosul: OlayKosulu.PaydasKus, kimden: Paydas.Balikci),
            };

            ayarlar.IcerikAyarla(kartlar, gorevler, olaylar);
            var so = new SerializedObject(ayarlar);
            so.FindProperty("eldekiKart").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ayarlar);
            AssetDatabase.SaveAssets();
            return ayarlar;
        }

        private static KartTanimi Kart(string id, KartEtkisi etki, KartTuru tur, Hedef hedef, int bedel, int deger = 1, int guclu = 0,
            bool bastan = true, KanitTuru kanit = KanitTuru.Cop, bool ucuzlatir = false, bool tek = false, int bakim = 0,
            OlayEtkisi acil = OlayEtkisi.Genel, int destek = 0, PaydasFarki paydas = default, bool kosul = false, Paydas kimden = Paydas.Otel, int enAz = 0)
        {
            var kart = Asset<KartTanimi>(KartKlasoru, id);
            kart.Kur(id, etki, hedef, bedel, deger, bastan, kanit, ucuzlatir, tek);
            kart.Ayrinti(tur, bakim, guclu, acil, destek, paydas, kosul, kimden, enAz);
            EditorUtility.SetDirty(kart);
            return kart;
        }

        private static GorevTanimi Gorev(string id, GorevEtkisi etki, Hedef hedef)
        {
            var gorev = Asset<GorevTanimi>(GorevKlasoru, id);
            gorev.Kur(id, etki, hedef);
            EditorUtility.SetDirty(gorev);
            return gorev;
        }

        private static OlayTanimi Olay(string id, OlayEtkisi etki, bool uyari = false, bool destekIster = false,
            int ab = 0, int ad = 0, PaydasFarki ap = default, int ag = 0, OlayTanimi aSonraki = null,
            int bb = 0, int bd = 0, PaydasFarki bp = default, int bg = 0, OlayTanimi bSonraki = null,
            OlayKosulu kosul = OlayKosulu.Her, Paydas kimden = Paydas.Otel, int enAzTur = 0)
        {
            var olay = Asset<OlayTanimi>(OlayKlasoru, id);
            olay.Kur(id, etki, uyari, destekIster, ab, ad, bb, bd);
            olay.Ayrinti(kosul, kimden, enAzTur, ap, ag, bp, bg);
            olay.ZincirKur(aSonraki, bSonraki);
            EditorUtility.SetDirty(olay);
            return olay;
        }

        private static T Asset<T>(string klasor, string ad) where T : ScriptableObject
        {
            string yol = $"{klasor}/{ad}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(yol);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, yol);
            return asset;
        }
    }
}
