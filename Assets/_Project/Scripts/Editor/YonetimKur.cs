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
    /// <summary>Kumsal sahnesine insan turunu kurar: harita seçimi, yuva simgeleri, tur akışı ve yönetim ekranı; YonetimDurumu'nu ayarlara bağlar. İçerik (kart, görev, olay) Toprak'ın aracında (İnsan kısmı içeriğini kur).</summary>
    public static class YonetimKur
    {
        /// <summary>Yönetim ayarlarını ve durumunu bulur (yoksa oluşturur), durumu ayarlara bağlar. Kart, görev ve olay içeriği Toprak'ın:
        /// burada yeniden yazılmaz; ayarlarda hiç kart yoksa (ilk kurulum) Toprak'ın aracı (İnsan kısmı içeriğini kur) bir kez çalıştırılır.</summary>
        internal static YonetimDurumu IcerikKur()
        {
            var ayarlar = GriKumsalKur.Ayar<YonetimAyarlari>("YonetimAyarlari");
            if (ayarlar.Kartlar.Count == 0)
            {
                InsanIcerigiKur.Kur(ayarlar);
                EditorUtility.SetDirty(ayarlar);
            }

            var durum = GriKumsalKur.Ayar<YonetimDurumu>("YonetimDurumu");
            GriKumsalKur.Bagla(durum, "ayarlar", ayarlar);
            EditorUtility.SetDirty(durum);
            AssetDatabase.SaveAssets();
            return durum;
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
            GriKumsalKur.Bagla(arayuz, "tema", GriKumsalKur.Tema());
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
