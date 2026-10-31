using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IkiDunya.Core;
using IkiDunya.UI.Menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Ana menü sahnesini, Sistemler'e duraklatma menüsünü ve EventSystem'i kurar; docs/metinler.md'yi metin tablosuna aktarır.</summary>
    public static class MenuKur
    {
        private const string MenuSahnesi = "Assets/_Project/Scenes/Levels/Menu.unity";
        private const string SistemlerKalibi = "Assets/_Project/Prefabs/Sistemler.prefab";
        private const string MetinlerDosyasi = "docs/metinler.md";

        [MenuItem("İki Dünya/Metinleri aktar")]
        public static void MetinleriAktar()
        {
            MetinTablosu tablo = GriKumsalKur.Ayar<MetinTablosu>("MetinTablosu");
            string yol = Path.Combine(Path.GetDirectoryName(Application.dataPath), MetinlerDosyasi);
            var satirlar = new List<MetinTablosu.Satir>();
            var idBicimi = new Regex("^[a-z0-9_]+$");

            // Her tablo satırı: | id | metin (ya da ad) | ... ; başlık ve ayraç satırları atlanır.
            foreach (string satir in File.ReadAllLines(yol))
            {
                string[] hucreler = satir.Split('|');
                if (hucreler.Length < 4) continue;
                string id = hucreler[1].Trim();
                if (id == "id" || !idBicimi.IsMatch(id)) continue;
                satirlar.Add(new MetinTablosu.Satir(id, hucreler[2].Trim()));
            }

            tablo.Yaz(satirlar);
            EditorUtility.SetDirty(tablo);

            var ayarlar = GriKumsalKur.Ayar<OyunAyarlari>("OyunAyarlari");
            GriKumsalKur.Bagla(ayarlar, "metinler", tablo);
            EditorUtility.SetDirty(ayarlar);
            AssetDatabase.SaveAssets();
            Debug.Log($"Metinler aktarıldı: {satirlar.Count} satır.");
        }

        [MenuItem("İki Dünya/Menü ve duraklatmayı kur")]
        public static void Kur()
        {
            MetinleriAktar();
            MetinTablosu metinler = GriKumsalKur.Ayar<MetinTablosu>("MetinTablosu");
            SistemlereEkle(metinler);

            if (!GriKumsalKur.SahneAc(MenuSahnesi, out Scene sahne)) return;
            var kamera = new GameObject("Kamera").AddComponent<UnityEngine.Camera>();
            kamera.clearFlags = CameraClearFlags.SolidColor;
            kamera.backgroundColor = new Color(0.05f, 0.12f, 0.18f);
            kamera.gameObject.tag = "MainCamera";
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));

            Transform tuval = GriKumsalKur.TuvalKur("AnaMenu", null);
            Yazi("Baslik", tuval, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.88f), 64, "oyun_adi", metinler);

            Transform dugmeler = Liste("Dugmeler", tuval, new Vector2(0.38f, 0.2f), new Vector2(0.62f, 0.62f));
            Button yeni = Dugme(dugmeler, "menu_yeni_oyun", metinler);
            Button devam = Dugme(dugmeler, "menu_devam_et", metinler);
            Button ayarlar = Dugme(dugmeler, "menu_ayarlar", metinler);
            Button cikis = Dugme(dugmeler, "menu_cikis", metinler);
            AyarlarPaneli panel = AyarlarKur(tuval, dugmeler.gameObject, metinler);

            var menu = tuval.gameObject.AddComponent<AnaMenu>();
            GriKumsalKur.Bagla(menu, "yeniOyun", yeni);
            GriKumsalKur.Bagla(menu, "devamEt", devam);
            GriKumsalKur.Bagla(menu, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(menu, "cikis", cikis);
            GriKumsalKur.Bagla(menu, "ayarlarPaneli", panel);

            GriKumsalKur.Kaydet(sahne, MenuSahnesi);
        }

        /// <summary>Sistemler kalıbına EventSystem ve duraklatma menüsü ekler, deneme panelini çıkarır.</summary>
        private static void SistemlereEkle(MetinTablosu metinler)
        {
            GameObject kok = PrefabUtility.LoadPrefabContents(SistemlerKalibi);

            foreach (string eski in new[] { "EventSystem", "DuraklatmaMenusu" })
            {
                Transform mevcut = kok.transform.Find(eski);
                if (mevcut != null) Object.DestroyImmediate(mevcut.gameObject);
            }
            foreach (MonoBehaviour bilesen in kok.GetComponents<MonoBehaviour>())
                if (bilesen != null && bilesen.GetType().Name == "AkisDeneme") Object.DestroyImmediate(bilesen);

            var olay = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            olay.transform.SetParent(kok.transform, false);

            Transform tuval = GriKumsalKur.TuvalKur("DuraklatmaMenusu", kok.transform);
            tuval.GetComponent<Canvas>().sortingOrder = 50;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(tuval, false);
            GriKumsalKur.Yasla((RectTransform)panel.transform, Vector2.zero, Vector2.one);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            Transform dugmeler = Liste("Dugmeler", panel.transform, new Vector2(0.38f, 0.3f), new Vector2(0.62f, 0.6f));
            Button oyunaDon = Dugme(dugmeler, "menu_oyuna_don", metinler);
            Button ayarlar = Dugme(dugmeler, "menu_ayarlar", metinler);
            Button anaMenu = Dugme(dugmeler, "menu_ana_menu", metinler);
            AyarlarPaneli ayarlarPaneli = AyarlarKur(panel.transform, dugmeler.gameObject, metinler);

            var menu = tuval.gameObject.AddComponent<DuraklatmaMenusu>();
            GriKumsalKur.Bagla(menu, "panel", panel);
            GriKumsalKur.Bagla(menu, "dugmeler", dugmeler.gameObject);
            GriKumsalKur.Bagla(menu, "oyunaDon", oyunaDon);
            GriKumsalKur.Bagla(menu, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(menu, "anaMenu", anaMenu);
            GriKumsalKur.Bagla(menu, "ayarlarPaneli", ayarlarPaneli);

            PrefabUtility.SaveAsPrefabAsset(kok, SistemlerKalibi);
            PrefabUtility.UnloadPrefabContents(kok);
        }

        // Ayarlardaki tuş satırları: metin id'si, GameInput eylemi, bağlama sırası (Hareket: 1 ileri, 2 geri, 3 sol, 4 sağ).
        private static readonly (string id, string eylem, int sira)[] TusSatirlari =
        {
            ("tus_ileri", "Hareket", 1), ("tus_geri", "Hareket", 2), ("tus_sol", "Hareket", 3), ("tus_sag", "Hareket", 4),
            ("tus_hizli", "Hizli", 0), ("tus_zipla", "ZiplaDal", 0), ("tus_etkiles", "Etkiles", 0), ("tus_termal", "Termal", 0),
            ("tus_kaz_sol", "KazSol", 0), ("tus_kaz_sag", "KazSag", 0), ("tus_kaz_yukari", "KazYukari", 0),
        };

        private static AyarlarPaneli AyarlarKur(Transform ebeveyn, GameObject oncekiPanel, MetinTablosu metinler)
        {
            Transform kutu = Liste("AyarlarPaneli", ebeveyn, new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.97f));
            kutu.GetComponent<VerticalLayoutGroup>().spacing = 6f;
            Yazi("SesYazisi", kutu, Vector2.zero, Vector2.zero, 24, "ayar_ses", metinler);
            Slider ses = Kaydirici(kutu, 0f, 1f);
            Yazi("FareYazisi", kutu, Vector2.zero, Vector2.zero, 24, "ayar_fare", metinler);
            Slider fare = Kaydirici(kutu, 0.2f, 3f);

            Yazi("TuslarYazisi", kutu, Vector2.zero, Vector2.zero, 24, "ayar_tuslar", metinler);
            foreach ((string id, string eylem, int sira) in TusSatirlari) TusSatiriKur(kutu, id, eylem, sira, metinler);
            Button varsayilan = Dugme(kutu, "menu_varsayilan", metinler);
            Button geri = Dugme(kutu, "menu_geri", metinler);

            var panel = kutu.gameObject.AddComponent<AyarlarPaneli>();
            GriKumsalKur.Bagla(panel, "ses", ses);
            GriKumsalKur.Bagla(panel, "hassasiyet", fare);
            GriKumsalKur.Bagla(panel, "geri", geri);
            GriKumsalKur.Bagla(panel, "varsayilanTuslar", varsayilan);
            GriKumsalKur.Bagla(panel, "oncekiPanel", oncekiPanel);
            kutu.gameObject.SetActive(false);
            return panel;
        }

        /// <summary>Bir satır: solda eylemin adı, sağda şu anki tuşu gösteren düğme.</summary>
        private static void TusSatiriKur(Transform ebeveyn, string id, string eylem, int sira, MetinTablosu metinler)
        {
            int ui = LayerMask.NameToLayer("UI");
            var satir = new GameObject(id, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            satir.layer = ui;
            satir.transform.SetParent(ebeveyn, false);
            satir.GetComponent<LayoutElement>().preferredHeight = 34f;
            var yatay = satir.GetComponent<HorizontalLayoutGroup>();
            yatay.spacing = 12f;
            yatay.childControlWidth = yatay.childControlHeight = true;
            yatay.childForceExpandWidth = yatay.childForceExpandHeight = true;

            Yazi("Ad", satir.transform, Vector2.zero, Vector2.zero, 22, id, metinler);

            var dugme = new GameObject("Tus", typeof(RectTransform), typeof(Image), typeof(Button));
            dugme.layer = ui;
            dugme.transform.SetParent(satir.transform, false);
            dugme.GetComponent<Image>().color = new Color(0.92f, 0.9f, 0.85f);
            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = ui;
            yaziNesnesi.transform.SetParent(dugme.transform, false);
            GriKumsalKur.Yasla((RectTransform)yaziNesnesi.transform, Vector2.zero, Vector2.one);
            Text tusYazisi = yaziNesnesi.GetComponent<Text>();
            tusYazisi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tusYazisi.fontSize = 22;
            tusYazisi.alignment = TextAnchor.MiddleCenter;
            tusYazisi.color = new Color(0.1f, 0.1f, 0.1f);

            var bilesen = satir.AddComponent<TusSatiri>();
            var so = new SerializedObject(bilesen);
            so.FindProperty("eylem").stringValue = eylem;
            so.FindProperty("baglamaSirasi").intValue = sira;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(bilesen, "tusYazisi", tusYazisi);
            GriKumsalKur.Bagla(bilesen, "dugme", dugme.GetComponent<Button>());
        }

        /// <summary>Çocuklarını alt alta dizen bir kutu.</summary>
        private static Transform Liste(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok)
        {
            var kutu = new GameObject(ad, typeof(RectTransform), typeof(VerticalLayoutGroup));
            kutu.layer = LayerMask.NameToLayer("UI");
            kutu.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)kutu.transform, enAz, enCok);
            var dizi = kutu.GetComponent<VerticalLayoutGroup>();
            dizi.spacing = 16f;
            dizi.childAlignment = TextAnchor.MiddleCenter;
            dizi.childControlWidth = true;
            dizi.childControlHeight = true;
            dizi.childForceExpandWidth = true;
            dizi.childForceExpandHeight = false;
            return kutu.transform;
        }

        /// <summary>Elle kurulan düğme: arka plan, Button ve çocukta eski tip Text (Unity sürümüne göre hazır düğme başka yazı bileşeni kurabiliyor).</summary>
        private static Button Dugme(Transform ebeveyn, string id, MetinTablosu metinler)
        {
            int ui = LayerMask.NameToLayer("UI");
            var dugme = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            dugme.layer = ui;
            dugme.transform.SetParent(ebeveyn, false);
            dugme.GetComponent<Image>().color = new Color(0.92f, 0.9f, 0.85f);
            dugme.GetComponent<LayoutElement>().preferredHeight = 64f;

            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = ui;
            yaziNesnesi.transform.SetParent(dugme.transform, false);
            GriKumsalKur.Yasla((RectTransform)yaziNesnesi.transform, Vector2.zero, Vector2.one);
            Text yazi = yaziNesnesi.GetComponent<Text>();
            YaziAyarla(yazi, 30, id, metinler);
            yazi.color = new Color(0.1f, 0.1f, 0.1f);
            return dugme.GetComponent<Button>();
        }

        private static Slider Kaydirici(Transform ebeveyn, float enAz, float enCok)
        {
            GameObject nesne = DefaultControls.CreateSlider(new DefaultControls.Resources());
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.AddComponent<LayoutElement>().preferredHeight = 30f;
            var kaydirici = nesne.GetComponent<Slider>();
            kaydirici.minValue = enAz;
            kaydirici.maxValue = enCok;
            return kaydirici;
        }

        private static void Yazi(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, int boyut, string id, MetinTablosu metinler)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            if (enCok != Vector2.zero) GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            else nesne.AddComponent<LayoutElement>().preferredHeight = boyut * 1.5f;
            Text yazi = nesne.GetComponent<Text>();
            YaziAyarla(yazi, boyut, id, metinler);
            yazi.color = Color.white;
        }

        private static void YaziAyarla(Text yazi, int boyut, string id, MetinTablosu metinler)
        {
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleCenter;
            yazi.text = metinler.Al(id);
            var metin = yazi.gameObject.AddComponent<MetinYazisi>();
            GriKumsalKur.Bagla(metin, "tablo", metinler);
            var so = new SerializedObject(metin);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Kumsal sahnesinin final grubu: tepeden bakan kamera, denizden çıkan yetişkin kaplumbağa, başlık ve kurtarılan yavru sayısı.</summary>
        internal static void FinalKur(Transform grup, Scene sahne, Material kaplumbagaMalzemesi, Material izMalzemesi)
        {
            MetinTablosu metinler = GriKumsalKur.Ayar<MetinTablosu>("MetinTablosu");
            Light gunes = null;
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.TryGetComponent(out Light isik) && isik.type == LightType.Directional) gunes = isik;

            var kamera = new GameObject("FinalKamerasi").AddComponent<UnityEngine.Camera>();
            kamera.transform.SetParent(grup, false);
            kamera.transform.SetPositionAndRotation(new Vector3(6f, 3f, 14f), Quaternion.LookRotation(new Vector3(-6f, -2.5f, -12f)));
            kamera.gameObject.AddComponent<AudioListener>();

            var kaplumbaga = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            kaplumbaga.name = "YetiskinKaplumbaga";
            Object.DestroyImmediate(kaplumbaga.GetComponent<Collider>());
            kaplumbaga.SetParent(grup, false);
            kaplumbaga.position = new Vector3(0f, 0f, -4f);
            kaplumbaga.localScale = new Vector3(1.2f, 0.45f, 1.5f);
            kaplumbaga.GetComponent<Renderer>().sharedMaterial = kaplumbagaMalzemesi;
            Transform son = GriKumsalKur.Bos("YuruyusSonu", grup);
            son.position = new Vector3(0f, 0f, 10f);

            // Temiz Enerji oynanmadıysa kumda sıcak izler
            Transform izler = GriKumsalKur.Bos("SicakIzler", grup);
            for (int i = 0; i < 6; i++)
            {
                GameObject iz = GriKumsalKur.Kutu("Iz", izler, new Vector3(-8f + i * 3.2f, 0.01f, 6f + (i % 3) * 3f), new Vector3(2f, 0.02f, 2f), izMalzemesi, "Default");
                Object.DestroyImmediate(iz.GetComponent<Collider>());
            }

            Transform tuval = GriKumsalKur.TuvalKur("FinalArayuzu", grup);
            Text baslik = FinalYazisi("Baslik", tuval, new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.9f), 56);
            Text kurtarilan = FinalYazisi("Kurtarilan", tuval, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.18f), 36);

            // Grubun çocukları adımına göre açılıp kapanır; sahne de çocukta olmalı ki sadece finalde çalışsın.
            var final = GriKumsalKur.Bos("Final", grup).gameObject.AddComponent<FinalSahnesi>();
            GriKumsalKur.Bagla(final, "kumsal", GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu"));
            GriKumsalKur.Bagla(final, "metinler", metinler);
            GriKumsalKur.Bagla(final, "kaplumbaga", kaplumbaga);
            GriKumsalKur.Bagla(final, "yuruyusSonu", son);
            GriKumsalKur.Bagla(final, "gunes", gunes);
            GriKumsalKur.Bagla(final, "sicakIzler", izler.gameObject);
            GriKumsalKur.Bagla(final, "baslik", baslik);
            GriKumsalKur.Bagla(final, "kurtarilan", kurtarilan);
        }

        private static Text FinalYazisi(string ad, Transform tuval, Vector2 enAz, Vector2 enCok, int boyut)
        {
            var nesne = new GameObject(ad, typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(tuval, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            Text yazi = nesne.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleCenter;
            yazi.color = Color.white;
            return yazi;
        }
    }
}
