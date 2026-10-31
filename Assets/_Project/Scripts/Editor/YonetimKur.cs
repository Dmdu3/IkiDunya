using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Management;
using IkiDunya.UI.Management;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>İnsan kısmını kurar: GDD'deki 12 kart, 5 görev, 6 olay asset'i, YonetimAyarlari ve YonetimDurumu; kumsal sahnesinde harita seçimi, yuva simgeleri, tur akışı ve yönetim ekranı.</summary>
    public static class YonetimKur
    {
        private const string KartKlasoru = "Assets/_Project/ScriptableObjects/Cards";
        private const string GorevKlasoru = "Assets/_Project/ScriptableObjects/VolunteerTasks";
        private const string OlayKlasoru = "Assets/_Project/ScriptableObjects/Events";

        /// <summary>İçerik asset'lerini GDD'deki değerlerle oluşturur (varsa üstüne yazar) ve ayarlara bağlar; durum asset'ini verir.</summary>
        internal static YonetimDurumu IcerikKur()
        {
            var kartlar = new List<KartTanimi>
            {
                Kart("kart_golgelik", KartEtkisi.Golgelik, Hedef.Bolge, 2, 1, true, KanitTuru.Isi, true, false),
                Kart("kart_temizlik", KartEtkisi.TemizlikKampanyasi, Hedef.Yok, 2, 3, true, KanitTuru.Cop, false, false),
                Kart("kart_kumul", KartEtkisi.KumulOnarimi, Hedef.Bolge, 3, 1, true, KanitTuru.Su, false, false),
                Kart("kart_yuva_tasima", KartEtkisi.YuvaTasima, Hedef.Yuva, 2, 1, false, KanitTuru.Su, false, false),
                Kart("kart_isik", KartEtkisi.IsikYasagi, Hedef.Yok, 3, 1, false, KanitTuru.Isik, false, true),
                Kart("kart_arac", KartEtkisi.AracYasagi, Hedef.Yok, 1, 1, false, KanitTuru.Cop, false, true),
                Kart("kart_sezlong", KartEtkisi.SezlongDuzenlemesi, Hedef.Yok, 2, 1, false, KanitTuru.Cop, false, true),
                Kart("kart_tilki_citi", KartEtkisi.TilkiCiti, Hedef.Yok, 3, 1, false, KanitTuru.Avci, false, true),
                Kart("kart_ag_temizligi", KartEtkisi.HayaletAgTemizligi, Hedef.Yok, 3, 1, true, KanitTuru.Cop, false, false),
                Kart("kart_gonullu_cagrisi", KartEtkisi.GonulluCagrisi, Hedef.Yok, 1, 2, true, KanitTuru.Cop, false, false),
                Kart("kart_saha_arastirmasi", KartEtkisi.SahaArastirmasi, Hedef.Yok, 1, 1, true, KanitTuru.Cop, false, false),
                Kart("kart_temiz_enerji", KartEtkisi.TemizEnerji, Hedef.Yok, 4, 1, true, KanitTuru.Isi, false, true),
            };
            var gorevler = new List<GorevTanimi>
            {
                Gorev("gorev_temizlik", GorevEtkisi.Temizlik, Hedef.Bolge),
                Gorev("gorev_devriye", GorevEtkisi.Devriye, Hedef.Bolge),
                Gorev("gorev_kafes", GorevEtkisi.Kafes, Hedef.Yuva),
                Gorev("gorev_golge", GorevEtkisi.GolgeSulama, Hedef.Yuva),
                Gorev("gorev_bilgilendirme", GorevEtkisi.Bilgilendirme, Hedef.Yok),
            };
            var olaylar = new List<OlayTanimi>
            {
                Olay("olay_otel", OlayEtkisi.Otel, false, true, 3, 0, 0, -1),
                Olay("olay_festival", OlayEtkisi.Festival, false, true, 0, 2, 0, -1),
                Olay("olay_balikci", OlayEtkisi.Balikci, false, false, -2, 0, 0, 0),
                Olay("olay_sicak_dalga", OlayEtkisi.SicakDalga, true, false, 0, 0, 0, 0),
                Olay("olay_firtina", OlayEtkisi.Firtina, true, false, 0, 0, 0, 0),
                Olay("olay_okul", OlayEtkisi.Okul, false, false, 0, 2, 0, 0),
            };

            var ayarlar = GriKumsalKur.Ayar<YonetimAyarlari>("YonetimAyarlari");
            ayarlar.IcerikAyarla(kartlar, gorevler, olaylar);
            EditorUtility.SetDirty(ayarlar);

            var durum = GriKumsalKur.Ayar<YonetimDurumu>("YonetimDurumu");
            GriKumsalKur.Bagla(durum, "ayarlar", ayarlar);
            EditorUtility.SetDirty(durum);
            AssetDatabase.SaveAssets();
            return durum;
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

        private static KartTanimi Kart(string id, KartEtkisi etki, Hedef hedef, int bedel, int deger, bool bastan, KanitTuru kanit, bool ucuzlatir, bool tek)
        {
            var kart = Asset<KartTanimi>(KartKlasoru, id);
            kart.Kur(id, etki, hedef, bedel, deger, bastan, kanit, ucuzlatir, tek);
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

        private static OlayTanimi Olay(string id, OlayEtkisi etki, bool uyari, bool destek, int ab, int ad, int bb, int bd)
        {
            var olay = Asset<OlayTanimi>(OlayKlasoru, id);
            olay.Kur(id, etki, uyari, destek, ab, ad, bb, bd);
            EditorUtility.SetDirty(olay);
            return olay;
        }

        // ---- Sahne ----

        /// <summary>Kumsal sahnesine insan turunu kurar. Tur akışı, harita seçimi ve ekran sahnenin kökünde durur (adım başladığında dinliyor olmaları gerekir); yuva simgeleri insan turu grubunda.</summary>
        internal static void SahneyeKur(Transform grup, UnityEngine.Camera kamera, KumSicakligi kum, Material simgeMalzemesi)
        {
            YonetimDurumu durum = IcerikKur();
            var kumsal = GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu");
            var metinler = GriKumsalKur.Ayar<MetinTablosu>("MetinTablosu");

            // Haritada imlecin altındaki bölgenin vurgusu
            Material vurguMalzemesi = GriKumsalKur.SaydamMalzeme("BolgeVurgusu", new Color(1f, 0.9f, 0.4f, 0.25f));
            GameObject vurgu = GriKumsalKur.Kutu("BolgeVurgusu", null, Vector3.zero, new Vector3(1f, 0.02f, 1f), vurguMalzemesi, "Default");
            Object.DestroyImmediate(vurgu.GetComponent<Collider>());
            vurgu.isStatic = false;
            vurgu.SetActive(false);

            var harita = new GameObject("HaritaSecici").AddComponent<HaritaSecici>();
            GriKumsalKur.Bagla(harita, "kamera", kamera);
            GriKumsalKur.Bagla(harita, "kum", kum);
            GriKumsalKur.Bagla(harita, "kumsal", kumsal);
            GriKumsalKur.Bagla(harita, "vurgu", vurgu.transform);

            var yuvalar = GriKumsalKur.Bos("YuvaHaritasi", grup).gameObject.AddComponent<YuvaHaritasi>();
            GriKumsalKur.Bagla(yuvalar, "kumsal", kumsal);
            GriKumsalKur.Bagla(yuvalar, "termal", GriKumsalKur.Ayar<TermalAyarlari>("TermalAyarlari"));
            GriKumsalKur.Bagla(yuvalar, "malzeme", simgeMalzemesi);

            var tur = new GameObject("InsanTuru").AddComponent<InsanTuru>();
            GriKumsalKur.Bagla(tur, "kumsal", kumsal);
            GriKumsalKur.Bagla(tur, "yonetim", durum);
            GriKumsalKur.Bagla(tur, "kum", kum);
            GriKumsalKur.Bagla(tur, "harita", harita);

            ArayuzKur(tur, harita, metinler);
        }

        private static void ArayuzKur(InsanTuru tur, HaritaSecici harita, MetinTablosu metinler)
        {
            Transform tuval = GriKumsalKur.TuvalKur("YonetimArayuzu", null);
            tuval.GetComponent<Canvas>().sortingOrder = 5;
            var arayuz = tuval.gameObject.AddComponent<YonetimArayuzu>();

            // Açılıp kapanan içerik; arayüz bileşeni tuvalde kalır, olayları dinlemeye devam eder.
            Transform icerik = Alan("Icerik", tuval, Vector2.zero, Vector2.one, Color.clear);
            ((RectTransform)icerik).gameObject.GetComponent<Image>().raycastTarget = false;

            Transform ust = Alan("UstCubuk", icerik, new Vector2(0f, 0.93f), Vector2.one, new Color(0f, 0f, 0f, 0.7f));
            Text ustBilgi = Yazi("Kaynaklar", ust, new Vector2(0.02f, 0f), new Vector2(0.7f, 1f), 28, TextAnchor.MiddleLeft);
            Text asama = Yazi("Asama", ust, new Vector2(0.7f, 0f), new Vector2(0.98f, 1f), 32, TextAnchor.MiddleRight);

            Transform sol = Alan("SolPanel", icerik, new Vector2(0f, 0.34f), new Vector2(0.24f, 0.92f), new Color(0f, 0f, 0f, 0.55f));
            Text yuvaListesi = Yazi("Yuvalar", sol, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.98f), 20, TextAnchor.UpperLeft);
            Text bolgeBilgisi = Yazi("Bolge", sol, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.48f), 20, TextAnchor.UpperLeft);

            Text hedef = Yazi("Hedef", icerik, new Vector2(0.3f, 0.85f), new Vector2(0.7f, 0.92f), 30, TextAnchor.MiddleCenter);
            hedef.color = new Color(1f, 0.9f, 0.4f);

            Vector2 altEnAz = new Vector2(0.26f, 0.02f), altEnCok = new Vector2(0.84f, 0.32f);
            Transform rapor = Alan("RaporPaneli", icerik, altEnAz, altEnCok, new Color(0f, 0f, 0f, 0.7f));
            Text raporYazisi = Yazi("Rapor", rapor, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f), 24, TextAnchor.UpperLeft);

            Transform gorev = Alan("GorevPaneli", icerik, altEnAz, altEnCok, new Color(0f, 0f, 0f, 0.7f));
            Transform gorevDugmeleri = Sira("Dugmeler", gorev);
            Transform kart = Alan("KartPaneli", icerik, altEnAz, altEnCok, new Color(0f, 0f, 0f, 0.7f));
            Transform kartDugmeleri = Sira("Dugmeler", kart);

            Transform olay = Alan("OlayPaneli", icerik, altEnAz, altEnCok, new Color(0f, 0f, 0f, 0.8f));
            Text olayYazisi = Yazi("Metin", olay, new Vector2(0.03f, 0.4f), new Vector2(0.97f, 0.95f), 26, TextAnchor.UpperLeft);
            Button a = Dugme("A", olay, new Vector2(0.05f, 0.05f), new Vector2(0.48f, 0.35f));
            Button b = Dugme("B", olay, new Vector2(0.52f, 0.05f), new Vector2(0.95f, 0.35f));

            Button ileri = Dugme("Ileri", icerik, new Vector2(0.86f, 0.02f), new Vector2(0.98f, 0.12f));
            var ileriYazi = ileri.GetComponentInChildren<Text>().gameObject.AddComponent<IkiDunya.UI.Menu.MetinYazisi>();
            GriKumsalKur.Bagla(ileriYazi, "tablo", metinler);
            var so = new SerializedObject(ileriYazi);
            so.FindProperty("id").stringValue = "yonetim_ileri";
            so.ApplyModifiedPropertiesWithoutUndo();

            Button kalip = Dugme("DugmeKalibi", icerik, Vector2.zero, new Vector2(0.1f, 0.1f));
            kalip.gameObject.AddComponent<LayoutElement>().preferredWidth = 260f;
            kalip.gameObject.SetActive(false);

            GriKumsalKur.Bagla(arayuz, "tur", tur);
            GriKumsalKur.Bagla(arayuz, "harita", harita);
            GriKumsalKur.Bagla(arayuz, "metinler", metinler);
            GriKumsalKur.Bagla(arayuz, "kok", icerik.gameObject);
            GriKumsalKur.Bagla(arayuz, "ustBilgi", ustBilgi);
            GriKumsalKur.Bagla(arayuz, "asamaBasligi", asama);
            GriKumsalKur.Bagla(arayuz, "yuvaListesi", yuvaListesi);
            GriKumsalKur.Bagla(arayuz, "bolgeBilgisi", bolgeBilgisi);
            GriKumsalKur.Bagla(arayuz, "hedefYazisi", hedef);
            GriKumsalKur.Bagla(arayuz, "raporPaneli", rapor.gameObject);
            GriKumsalKur.Bagla(arayuz, "raporYazisi", raporYazisi);
            GriKumsalKur.Bagla(arayuz, "gorevPaneli", gorev.gameObject);
            GriKumsalKur.Bagla(arayuz, "gorevDugmeleri", gorevDugmeleri);
            GriKumsalKur.Bagla(arayuz, "kartPaneli", kart.gameObject);
            GriKumsalKur.Bagla(arayuz, "kartDugmeleri", kartDugmeleri);
            GriKumsalKur.Bagla(arayuz, "olayPaneli", olay.gameObject);
            GriKumsalKur.Bagla(arayuz, "olayYazisi", olayYazisi);
            GriKumsalKur.Bagla(arayuz, "aDugmesi", a);
            GriKumsalKur.Bagla(arayuz, "bDugmesi", b);
            GriKumsalKur.Bagla(arayuz, "ileriDugmesi", ileri);
            GriKumsalKur.Bagla(arayuz, "dugmeKalibi", kalip);
        }

        private static Transform Alan(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, Color renk)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Image));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            nesne.GetComponent<Image>().color = renk;
            return nesne.transform;
        }

        /// <summary>Düğmeleri yan yana dizen alan.</summary>
        private static Transform Sira(string ad, Transform ebeveyn)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.92f));
            var dizi = nesne.GetComponent<HorizontalLayoutGroup>();
            dizi.spacing = 12f;
            dizi.childControlWidth = true;
            dizi.childControlHeight = true;
            dizi.childForceExpandWidth = true;
            dizi.childForceExpandHeight = true;
            return nesne.transform;
        }

        private static Text Yazi(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, int boyut, TextAnchor hiza)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            var yazi = nesne.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = boyut;
            yazi.alignment = hiza;
            yazi.color = Color.white;
            yazi.supportRichText = true;
            yazi.raycastTarget = false;
            return yazi;
        }

        private static Button Dugme(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok)
        {
            Transform alan = Alan(ad, ebeveyn, enAz, enCok, new Color(0.92f, 0.92f, 0.88f));
            var dugme = alan.gameObject.AddComponent<Button>();
            dugme.targetGraphic = alan.GetComponent<Image>();
            Text yazi = Yazi("Yazi", alan, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), 22, TextAnchor.MiddleCenter);
            yazi.color = new Color(0.1f, 0.1f, 0.1f);
            return dugme;
        }
    }
}
