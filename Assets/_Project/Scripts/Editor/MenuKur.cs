using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IkiDunya.Core;
using IkiDunya.Management;
using IkiDunya.Turtle;
using IkiDunya.UI.Menu;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Ana menü sahnesini, Sistemler'e duraklatma menüsünü, ipucu kutusunu, ses yöneticisini ve EventSystem'i kurar; docs/metinler.md'yi metin tablosuna aktarır.</summary>
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
            // Sonraki sütunlar başlığına göre ek id olur: Açıklama → id_aciklama, Not → id_not, Seçenek A/B → id_a / id_b.
            var ekler = new Dictionary<string, string> { { "Açıklama", "_aciklama" }, { "Not", "_not" }, { "Seçenek A", "_a" }, { "Seçenek B", "_b" } };
            string[] baslik = new string[0];
            foreach (string satir in File.ReadAllLines(yol))
            {
                string[] hucreler = satir.Split('|');
                if (hucreler.Length < 4) continue;
                string id = hucreler[1].Trim();
                if (id == "id")
                {
                    baslik = hucreler;
                    continue;
                }
                if (!idBicimi.IsMatch(id)) continue;
                satirlar.Add(new MetinTablosu.Satir(id, hucreler[2].Trim()));
                for (int i = 3; i < hucreler.Length - 1 && i < baslik.Length; i++)
                {
                    string metin = hucreler[i].Trim();
                    if (metin.Length == 0 || metin == "—" || !ekler.TryGetValue(baslik[i].Trim(), out string ek)) continue;
                    satirlar.Add(new MetinTablosu.Satir(id + ek, metin));
                }
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
            // Menüde de bir kulak olsun (menü müziği, "no audio listener" uyarısı).
            kamera.gameObject.AddComponent<AudioListener>();
            kamera.gameObject.AddComponent<TekKulak>();
            kamera.clearFlags = CameraClearFlags.SolidColor;
            kamera.backgroundColor = new Color(0.05f, 0.12f, 0.18f);
            kamera.gameObject.tag = "MainCamera";
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));

            Transform tuval = GriKumsalKur.TuvalKur("AnaMenu", null);
            Yazi("Baslik", tuval, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.88f), 64, "oyun_adi", metinler);

            Transform dugmeler = Liste("Dugmeler", tuval, new Vector2(0.38f, 0.2f), new Vector2(0.62f, 0.62f));
            Button yeni = Dugme(dugmeler, "menu_yeni_oyun", metinler);
            Button devam = Dugme(dugmeler, "menu_devam_et", metinler);
            Button parkurlar = Dugme(dugmeler, "menu_parkurlar", metinler);
            Button ayarlar = Dugme(dugmeler, "menu_ayarlar", metinler);
            Button cikis = Dugme(dugmeler, "menu_cikis", metinler);
            AyarlarPaneli panel = AyarlarKur(tuval, dugmeler.gameObject, metinler);
            ParkurlarPaneli parkurPaneli = ParkurlarKur(tuval, dugmeler.gameObject, metinler);

            var menu = tuval.gameObject.AddComponent<AnaMenu>();
            GriKumsalKur.Bagla(menu, "yeniOyun", yeni);
            GriKumsalKur.Bagla(menu, "devamEt", devam);
            GriKumsalKur.Bagla(menu, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(menu, "cikis", cikis);
            GriKumsalKur.Bagla(menu, "ayarlarPaneli", panel);
            GriKumsalKur.Bagla(menu, "parkurlar", parkurlar);
            GriKumsalKur.Bagla(menu, "parkurlarPaneli", parkurPaneli);

            GriKumsalKur.Kaydet(sahne, MenuSahnesi);
        }

        /// <summary>Sistemler kalıbına EventSystem, duraklatma menüsü, ipucu kutusu ve ses yöneticisi ekler, deneme panelini çıkarır.</summary>
        private static void SistemlereEkle(MetinTablosu metinler)
        {
            GameObject kok = PrefabUtility.LoadPrefabContents(SistemlerKalibi);

            foreach (string eski in new[] { "EventSystem", "DuraklatmaMenusu", "IpucuArayuzu", "SesYoneticisi", "ParkurArayuzu", "EkranEfektleri" })
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

            IpucuKur(kok.transform);
            ParkurGostergesiKur(kok.transform);
            EkranEfektleriKur(kok.transform);
            SesKur(kok.transform);

            // İnsan kısmının durumu da kayda girer.
            YonetimDurumu yonetim = YonetimKur.IcerikKur();
            OyunAkisi akis = kok.GetComponent<OyunAkisi>();
            if (akis != null) GriKumsalKur.DiziBagla(akis, "ekVeriler", yonetim);
            else Debug.LogWarning("MenuKur: Sistemler'de OyunAkisi yok; YonetimDurumu kayda bağlanamadı.");

            PrefabUtility.SaveAsPrefabAsset(kok, SistemlerKalibi);
            PrefabUtility.UnloadPrefabContents(kok);
        }

        /// <summary>Ekranın altında ipucu kutusu (duraklatmanın altında, oyunun üstünde).</summary>
        private static void IpucuKur(Transform kok)
        {
            Transform tuval = GriKumsalKur.TuvalKur("IpucuArayuzu", kok);
            tuval.GetComponent<Canvas>().sortingOrder = 40;
            var kutu = new GameObject("Kutu", typeof(RectTransform), typeof(Image));
            kutu.layer = LayerMask.NameToLayer("UI");
            kutu.transform.SetParent(tuval, false);
            // Üstte ortada (yönetimin üst çubuğunun altı); boyunu ve enini panel yazıya göre verir.
            var kutuAlani = (RectTransform)kutu.transform;
            kutuAlani.anchorMin = kutuAlani.anchorMax = kutuAlani.pivot = new Vector2(0.5f, 1f);
            kutuAlani.anchoredPosition = new Vector2(0f, -114f);
            kutuAlani.sizeDelta = new Vector2(900f, 80f);
            kutu.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = LayerMask.NameToLayer("UI");
            yaziNesnesi.transform.SetParent(kutu.transform, false);
            var yaziAlani = (RectTransform)yaziNesnesi.transform;
            GriKumsalKur.Yasla(yaziAlani, Vector2.zero, Vector2.one);
            yaziAlani.offsetMin = new Vector2(24f, 14f);
            yaziAlani.offsetMax = new Vector2(-24f, -14f);
            Text yazi = yaziNesnesi.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = 30;
            yazi.alignment = TextAnchor.MiddleCenter;
            yazi.color = Color.white;
            kutu.SetActive(false);

            var panel = tuval.gameObject.AddComponent<IpucuPaneli>();
            GriKumsalKur.Bagla(panel, "ayarlar", GriKumsalKur.Ayar<OyunAyarlari>("OyunAyarlari"));
            GriKumsalKur.Bagla(panel, "kutu", kutu);
            GriKumsalKur.Bagla(panel, "yazi", yazi);
        }

        /// <summary>Ses yöneticisi ve ses yuvaları: SesAyarlari'nda her ses türü ve her adım için boş yuva açılır; Abdü klipleri oraya sürükler.</summary>
        private static void SesKur(Transform kok)
        {
            var ayarlar = GriKumsalKur.Ayar<SesAyarlari>("SesAyarlari");
            ayarlar.EksikYuvalariEkle();
            EditorUtility.SetDirty(ayarlar);
            AssetDatabase.SaveAssets();

            var nesne = new GameObject("SesYoneticisi");
            nesne.transform.SetParent(kok, false);
            GriKumsalKur.Bagla(nesne.AddComponent<SesYoneticisi>(), "ayarlar", ayarlar);
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
            // Tam ekran koyu zemin (arkadaki başlık ve düğmeler görünmesin); içinde iki sütun: solda ses, fare, ekran ve erişilebilirlik; sağda tuşlar.
            var kok = new GameObject("AyarlarPaneli", typeof(RectTransform), typeof(Image));
            kok.layer = LayerMask.NameToLayer("UI");
            kok.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)kok.transform, Vector2.zero, Vector2.one);
            kok.GetComponent<Image>().color = new Color(0.04f, 0.1f, 0.15f, 1f);
            var icerik = new GameObject("Icerik", typeof(RectTransform));
            icerik.layer = LayerMask.NameToLayer("UI");
            icerik.transform.SetParent(kok.transform, false);
            GriKumsalKur.Yasla((RectTransform)icerik.transform, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.96f));

            Transform sol = Liste("Genel", icerik.transform, new Vector2(0f, 0f), new Vector2(0.46f, 1f));
            sol.GetComponent<VerticalLayoutGroup>().spacing = 6f;
            Yazi("SesYazisi", sol, Vector2.zero, Vector2.zero, 24, "ayar_ses", metinler);
            Slider ses = Kaydirici(sol, 0f, 1f);
            Yazi("FareYazisi", sol, Vector2.zero, Vector2.zero, 24, "ayar_fare", metinler);
            Slider fare = Kaydirici(sol, 0.2f, 3f);
            Yazi("YaziBoyuYazisi", sol, Vector2.zero, Vector2.zero, 24, "ayar_yazi_boyutu", metinler);
            Slider yaziBoyu = Kaydirici(sol, 0.75f, 1.5f);
            Toggle ipuclari = Secim(sol, "ayar_ipuclari", metinler);
            Toggle tamEkran = Secim(sol, "ayar_tam_ekran", metinler);
            Yazi("CozunurlukYazisi", sol, Vector2.zero, Vector2.zero, 24, "ayar_cozunurluk", metinler);
            Dropdown cozunurluk = Acilir(sol);
            Yazi("KaliteYazisi", sol, Vector2.zero, Vector2.zero, 24, "ayar_kalite", metinler);
            Dropdown kalite = Acilir(sol);
            Button geri = Dugme(sol, "menu_geri", metinler);

            Transform sag = Liste("Tuslar", icerik.transform, new Vector2(0.52f, 0f), new Vector2(1f, 1f));
            sag.GetComponent<VerticalLayoutGroup>().spacing = 6f;
            Yazi("TuslarYazisi", sag, Vector2.zero, Vector2.zero, 24, "ayar_tuslar", metinler);
            foreach ((string id, string eylem, int sira) in TusSatirlari) TusSatiriKur(sag, id, eylem, sira, metinler);
            // Tuşları sıfırlayan düğme tuş sütununda; sol sütun en büyük yazı boyutunda da sığsın.
            Button varsayilan = Dugme(sag, "menu_varsayilan", metinler);
            varsayilan.GetComponent<LayoutElement>().preferredHeight = 52f;

            var panel = kok.AddComponent<AyarlarPaneli>();
            GriKumsalKur.Bagla(panel, "ses", ses);
            GriKumsalKur.Bagla(panel, "hassasiyet", fare);
            GriKumsalKur.Bagla(panel, "geri", geri);
            GriKumsalKur.Bagla(panel, "varsayilanTuslar", varsayilan);
            GriKumsalKur.Bagla(panel, "oncekiPanel", oncekiPanel);
            GriKumsalKur.Bagla(panel, "metinler", metinler);
            GriKumsalKur.Bagla(panel, "ipuclari", ipuclari);
            GriKumsalKur.Bagla(panel, "tamEkran", tamEkran);
            GriKumsalKur.Bagla(panel, "cozunurluk", cozunurluk);
            GriKumsalKur.Bagla(panel, "kalite", kalite);
            GriKumsalKur.Bagla(panel, "arayuzOlcegi", yaziBoyu);
            kok.SetActive(false);
            return panel;
        }

        /// <summary>Yanında yazısı olan aç/kapa kutusu.</summary>
        private static Toggle Secim(Transform ebeveyn, string id, MetinTablosu metinler)
        {
            GameObject nesne = DefaultControls.CreateToggle(new DefaultControls.Resources());
            nesne.transform.SetParent(ebeveyn, false);
            Katman(nesne);
            nesne.AddComponent<LayoutElement>().preferredHeight = 36f;
            Text yazi = nesne.GetComponentInChildren<Text>();
            YaziAyarla(yazi, 24, id, metinler);
            yazi.alignment = TextAnchor.MiddleLeft;
            ((RectTransform)yazi.transform).offsetMin += new Vector2(12f, 0f);
            yazi.color = Color.white;
            return nesne.GetComponent<Toggle>();
        }

        /// <summary>Açılır liste (seçenekleri panel kurar).</summary>
        private static Dropdown Acilir(Transform ebeveyn)
        {
            GameObject nesne = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            nesne.transform.SetParent(ebeveyn, false);
            Katman(nesne);
            nesne.AddComponent<LayoutElement>().preferredHeight = 40f;
            foreach (Text yazi in nesne.GetComponentsInChildren<Text>(true))
            {
                yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                yazi.fontSize = 22;
                yazi.color = new Color(0.1f, 0.1f, 0.1f);
            }
            return nesne.GetComponent<Dropdown>();
        }

        private static void Katman(GameObject nesne)
        {
            int ui = LayerMask.NameToLayer("UI");
            foreach (Transform t in nesne.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ui;
        }

        /// <summary>Parkurlar paneli: her parkur için bir düğme (yazısı rekorla birlikte kodda kurulur) ve Geri.</summary>
        private static ParkurlarPaneli ParkurlarKur(Transform ebeveyn, GameObject oncekiPanel, MetinTablosu metinler)
        {
            Transform kutu = Liste("ParkurlarPaneli", ebeveyn, new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.62f));
            Yazi("ParkurlarYazisi", kutu, Vector2.zero, Vector2.zero, 30, "menu_parkurlar", metinler);
            var dugmeler = new Object[System.Enum.GetValues(typeof(ParkurTuru)).Length];
            for (int i = 0; i < dugmeler.Length; i++)
            {
                Button dugme = Dugme(kutu, Parkur.MetinId((ParkurTuru)i), metinler);
                // Yazıyı panel kurar (ad + rekor); sabit metin bileşeni üstüne yazmasın.
                Object.DestroyImmediate(dugme.GetComponentInChildren<MetinYazisi>());
                dugmeler[i] = dugme;
            }
            Button geri = Dugme(kutu, "menu_geri", metinler);

            var panel = kutu.gameObject.AddComponent<ParkurlarPaneli>();
            GriKumsalKur.Bagla(panel, "metinler", metinler);
            GriKumsalKur.DiziBagla(panel, "dugmeler", dugmeler);
            GriKumsalKur.Bagla(panel, "geri", geri);
            GriKumsalKur.Bagla(panel, "oncekiPanel", oncekiPanel);
            kutu.gameObject.SetActive(false);
            return panel;
        }

        /// <summary>Ekran efektleri: tam ekran flaş ve tehlikede kararan kenar (tıklamayı engellemez).</summary>
        private static void EkranEfektleriKur(Transform kok)
        {
            Transform tuval = GriKumsalKur.TuvalKur("EkranEfektleri", kok);
            tuval.GetComponent<Canvas>().sortingOrder = 30;
            var kenar = new GameObject("Kenar", typeof(RectTransform), typeof(RawImage));
            kenar.layer = LayerMask.NameToLayer("UI");
            kenar.transform.SetParent(tuval, false);
            GriKumsalKur.Yasla((RectTransform)kenar.transform, Vector2.zero, Vector2.one);
            kenar.GetComponent<RawImage>().raycastTarget = false;
            var flas = new GameObject("Flas", typeof(RectTransform), typeof(Image));
            flas.layer = LayerMask.NameToLayer("UI");
            flas.transform.SetParent(tuval, false);
            GriKumsalKur.Yasla((RectTransform)flas.transform, Vector2.zero, Vector2.one);
            flas.GetComponent<Image>().raycastTarget = false;

            var efektler = tuval.gameObject.AddComponent<IkiDunya.UI.Night.EkranEfektleri>();
            GriKumsalKur.Bagla(efektler, "ayarlar", GriKumsalKur.Ayar<GeribildirimAyarlari>("GeribildirimAyarlari"));
            GriKumsalKur.Bagla(efektler, "flas", flas.GetComponent<Image>());
            GriKumsalKur.Bagla(efektler, "kenar", kenar.GetComponent<RawImage>());
        }

        /// <summary>Parkur süresi (sağ üst) ve sonucu (ortada) gösteren kutular.</summary>
        private static void ParkurGostergesiKur(Transform kok)
        {
            Transform tuval = GriKumsalKur.TuvalKur("ParkurArayuzu", kok);
            tuval.GetComponent<Canvas>().sortingOrder = 45;

            GameObject sure = Kutucuk("Sure", tuval, new Vector2(0.68f, 0.86f), new Vector2(0.98f, 0.98f));
            Text sureYazisi = KutuYazisi(sure.transform, new Vector2(0.03f, 0.5f), new Vector2(0.97f, 1f), 28);
            Text bilgiYazisi = KutuYazisi(sure.transform, new Vector2(0.03f, 0f), new Vector2(0.97f, 0.5f), 24);
            GameObject sonuc = Kutucuk("Sonuc", tuval, new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.65f));
            Text sonucYazisi = KutuYazisi(sonuc.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), 36);
            sonucYazisi.alignment = TextAnchor.MiddleCenter;
            sure.SetActive(false);
            sonuc.SetActive(false);

            var gosterge = tuval.gameObject.AddComponent<IkiDunya.UI.Night.ParkurGostergesi>();
            GriKumsalKur.Bagla(gosterge, "ayarlar", GriKumsalKur.Ayar<OyunAyarlari>("OyunAyarlari"));
            GriKumsalKur.Bagla(gosterge, "sureKutusu", sure);
            GriKumsalKur.Bagla(gosterge, "sureYazisi", sureYazisi);
            GriKumsalKur.Bagla(gosterge, "bilgiYazisi", bilgiYazisi);
            GriKumsalKur.Bagla(gosterge, "sonucKutusu", sonuc);
            GriKumsalKur.Bagla(gosterge, "sonucYazisi", sonucYazisi);
        }

        private static GameObject Kutucuk(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok)
        {
            var kutu = new GameObject(ad, typeof(RectTransform), typeof(Image));
            kutu.layer = LayerMask.NameToLayer("UI");
            kutu.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)kutu.transform, enAz, enCok);
            kutu.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            return kutu;
        }

        private static Text KutuYazisi(Transform ebeveyn, Vector2 enAz, Vector2 enCok, int boyut)
        {
            var nesne = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            Text yazi = nesne.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleRight;
            yazi.color = Color.white;
            return yazi;
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
            yatay.childForceExpandWidth = false;
            yatay.childForceExpandHeight = true;

            // Ad kalan genişliği alır, tuş düğmeleri aynı enle tek sütun olur.
            Yazi("Ad", satir.transform, Vector2.zero, Vector2.zero, 22, id, metinler);
            Text ad = satir.transform.Find("Ad").GetComponent<Text>();
            ad.alignment = TextAnchor.MiddleLeft;
            LayoutElement adYeri = ad.GetComponent<LayoutElement>();
            adYeri.minWidth = 0f;
            adYeri.preferredWidth = 0f;
            adYeri.flexibleWidth = 1f;

            var dugme = new GameObject("Tus", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            LayoutElement tusYeri = dugme.GetComponent<LayoutElement>();
            tusYeri.minWidth = tusYeri.preferredWidth = 200f;
            tusYeri.flexibleWidth = 0f;
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
            kamera.gameObject.AddComponent<TekKulak>();
            kamera.transform.SetParent(grup, false);
            kamera.transform.SetPositionAndRotation(new Vector3(6f, 3f, 14f), Quaternion.LookRotation(new Vector3(-6f, -2.5f, -12f)));

            // Yetişkin kaplumbağa: Yusuf'un caretta'sı (yoksa gri küre); denizden kumsala yürür.
            Transform kaplumbaga = GriKumsalKur.Bos("YetiskinKaplumbaga", grup);
            kaplumbaga.position = new Vector3(0f, 0f, -4f);
            GameObject yetiskinModeli = GriKumsalKur.ModelYerlestir(kaplumbaga, "Anne_Caretta", Vector3.zero, 1.5f);
            if (yetiskinModeli != null) GriKumsalKur.SalinimEkle(kaplumbaga.gameObject, yetiskinModeli.transform, SalinimTuru.Kara);
            else
            {
                GameObject kure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.DestroyImmediate(kure.GetComponent<Collider>());
                kure.transform.SetParent(kaplumbaga, false);
                kure.transform.localScale = new Vector3(1.2f, 0.45f, 1.5f);
                kure.GetComponent<Renderer>().sharedMaterial = kaplumbagaMalzemesi;
            }
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
            // Başlık ve kurtarılan bir grupta belirir: ikisini bir CanvasGroup'un altına al.
            var yazilar = new GameObject("Yazilar", typeof(RectTransform), typeof(CanvasGroup));
            yazilar.layer = LayerMask.NameToLayer("UI");
            yazilar.transform.SetParent(tuval, false);
            GriKumsalKur.Yasla((RectTransform)yazilar.transform, Vector2.zero, Vector2.one);
            baslik.transform.SetParent(yazilar.transform, false);
            kurtarilan.transform.SetParent(yazilar.transform, false);
            CanvasGroup yaziGrubu = yazilar.GetComponent<CanvasGroup>();
            yaziGrubu.alpha = 0f;
            yaziGrubu.blocksRaycasts = false;

            // Jenerik: ekranın altından başlar, yukarı akar.
            Text jenerik = FinalYazisi("Jenerik", tuval, new Vector2(0.15f, 0f), new Vector2(0.85f, 0f), 32);
            var jenerikKutusu = (RectTransform)jenerik.transform;
            jenerikKutusu.pivot = new Vector2(0.5f, 1f);
            jenerikKutusu.anchoredPosition = Vector2.zero;
            jenerikKutusu.sizeDelta = new Vector2(0f, 1400f);
            jenerik.alignment = TextAnchor.UpperCenter;
            jenerik.lineSpacing = 1.3f;
            jenerik.gameObject.SetActive(false);

            // Kamera yürüyüş boyunca kaplumbağanın arkasından kumsala doğru kayar.
            Transform kameraSon = GriKumsalKur.Bos("KameraSon", grup);
            kameraSon.SetPositionAndRotation(new Vector3(-2.5f, 1.6f, 16f), Quaternion.LookRotation(new Vector3(2.5f, -0.8f, -8f)));

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
            GriKumsalKur.Bagla(final, "kamera", kamera.transform);
            GriKumsalKur.Bagla(final, "kameraSon", kameraSon);
            GriKumsalKur.Bagla(final, "yazilar", yaziGrubu);
            GriKumsalKur.Bagla(final, "jenerik", jenerikKutusu);
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
