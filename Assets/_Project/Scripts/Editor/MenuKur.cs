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
            kamera.backgroundColor = GriKumsalKur.Tema().Gece;
            kamera.gameObject.tag = "MainCamera";
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));

            // "Belgesel" teması: alacakaranlık ufku, solda serif başlık ve altında vurgu çizgisi, kutusuz seçenek listesi.
            Transform tuval = GriKumsalKur.TuvalKur("AnaMenu", null);
            Zemin(tuval, false);
            Transform dugmeler = BaslikVeListe(tuval, "oyun_adi", 96, 5, metinler);
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

        // Menü düzeni (tuval birimi, sol üstten): başlık, vurgu çizgisi, seçenek listesi. En büyük yazı boyutunda (1280x720) da sığar.
        private const float MenuSol = 150f, BaslikUstu = 110f, BaslikBoyu = 130f, CizgiUstu = 268f, ListeUstu = 320f;
        private const float SecenekBoyu = 60f, SecenekAraligi = 4f, ListeEni = 700f;

        /// <summary>Tam ekran zemin: ufuk (ana menü, ayarlar) ya da oyunun üstünde soldan koyulaşan perde (duraklatma).</summary>
        private static Transform Zemin(Transform ebeveyn, bool perde)
        {
            var zemin = new GameObject(perde ? "Panel" : "Zemin", typeof(RectTransform), typeof(RawImage));
            zemin.layer = LayerMask.NameToLayer("UI");
            zemin.transform.SetParent(ebeveyn, false);
            zemin.transform.SetAsFirstSibling();
            GriKumsalKur.Yasla((RectTransform)zemin.transform, Vector2.zero, Vector2.one);
            var ufuk = zemin.AddComponent<UfukZemini>();
            GriKumsalKur.Bagla(ufuk, "tema", GriKumsalKur.Tema());
            var so = new SerializedObject(ufuk);
            so.FindProperty("yanKarartma").boolValue = perde;
            so.ApplyModifiedPropertiesWithoutUndo();
            return zemin.transform;
        }

        /// <summary>Solda serif başlık, altında kısa vurgu çizgisi ve seçenek listesi (sol üste çapalı); listeyi döner.</summary>
        private static Transform BaslikVeListe(Transform ebeveyn, string baslikId, int boyut, int secenekSayisi, MetinTablosu metinler)
        {
            ArayuzTemasi tema = GriKumsalKur.Tema();
            Vector2 solUst = new Vector2(0f, 1f);
            Yazi("Baslik", ebeveyn, Vector2.zero, Vector2.one, boyut, baslikId, metinler);
            Text baslik = ebeveyn.Find("Baslik").GetComponent<Text>();
            GriKumsalKur.TemaYazisi(baslik, true, tema.Yazi);
            baslik.alignment = TextAnchor.LowerLeft;
            GriKumsalKur.Yerles((RectTransform)baslik.transform, solUst, new Vector2(MenuSol, -BaslikUstu), new Vector2(1000f, BaslikBoyu));
            Cizgi(ebeveyn, new Vector2(MenuSol + 6f, -CizgiUstu));

            Transform liste = Liste("Dugmeler", ebeveyn, Vector2.zero, Vector2.one);
            GriKumsalKur.Yerles((RectTransform)liste, solUst, new Vector2(MenuSol, -ListeUstu),
                new Vector2(ListeEni, secenekSayisi * SecenekBoyu + (secenekSayisi - 1) * SecenekAraligi));
            var dizi = liste.GetComponent<VerticalLayoutGroup>();
            dizi.spacing = SecenekAraligi;
            dizi.childAlignment = TextAnchor.UpperLeft;
            return liste;
        }

        /// <summary>Başlık altındaki kısa vurgu çizgisi (sol üstten konum).</summary>
        private static void Cizgi(Transform ebeveyn, Vector2 konum)
        {
            var cizgi = new GameObject("Cizgi", typeof(RectTransform), typeof(Image));
            cizgi.layer = LayerMask.NameToLayer("UI");
            cizgi.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yerles((RectTransform)cizgi.transform, new Vector2(0f, 1f), konum, new Vector2(120f, 4f));
            Image resim = cizgi.GetComponent<Image>();
            resim.color = GriKumsalKur.Tema().Vurgu;
            resim.raycastTarget = false;
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
            // Oyunun üstünde soldan koyulaşan perde; ana menüyle aynı düzen.
            Transform panel = Zemin(tuval, true);
            Transform dugmeler = BaslikVeListe(panel, "oyun_adi", 80, 3, metinler);
            Button oyunaDon = Dugme(dugmeler, "menu_oyuna_don", metinler);
            Button ayarlar = Dugme(dugmeler, "menu_ayarlar", metinler);
            Button anaMenu = Dugme(dugmeler, "menu_ana_menu", metinler);
            AyarlarPaneli ayarlarPaneli = AyarlarKur(panel, dugmeler.gameObject, metinler);

            var menu = tuval.gameObject.AddComponent<DuraklatmaMenusu>();
            GriKumsalKur.Bagla(menu, "panel", panel.gameObject);
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
            ArayuzTemasi tema = GriKumsalKur.Tema();
            kutu.GetComponent<Image>().color = tema.Panel;
            // Solda boydan boya ince vurgu çizgisi.
            var serit = new GameObject("Serit", typeof(RectTransform), typeof(Image));
            serit.layer = LayerMask.NameToLayer("UI");
            serit.transform.SetParent(kutu.transform, false);
            var seritAlani = (RectTransform)serit.transform;
            seritAlani.anchorMin = Vector2.zero;
            seritAlani.anchorMax = new Vector2(0f, 1f);
            seritAlani.pivot = new Vector2(0f, 0.5f);
            seritAlani.offsetMin = Vector2.zero;
            seritAlani.offsetMax = new Vector2(4f, 0f);
            serit.GetComponent<Image>().color = tema.Vurgu;

            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = LayerMask.NameToLayer("UI");
            yaziNesnesi.transform.SetParent(kutu.transform, false);
            var yaziAlani = (RectTransform)yaziNesnesi.transform;
            GriKumsalKur.Yasla(yaziAlani, Vector2.zero, Vector2.one);
            yaziAlani.offsetMin = new Vector2(30f, 16f);
            yaziAlani.offsetMax = new Vector2(-26f, -16f);
            Text yazi = yaziNesnesi.GetComponent<Text>();
            GriKumsalKur.TemaYazisi(yazi, false, tema.Yazi);
            yazi.fontSize = 29;
            yazi.lineSpacing = 1.1f;
            yazi.alignment = TextAnchor.MiddleLeft;
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
            // Tam ekran ufuk zemini (arkadaki başlık ve düğmeler görünmez); sol üstte serif başlık ve çizgi, altında iki sütun:
            // solda ses, fare, ekran ve erişilebilirlik; sağda tuşlar. Sütunlar en büyük yazı boyutunda (1280x720) da sığar.
            ArayuzTemasi tema = GriKumsalKur.Tema();
            Transform kok = Zemin(ebeveyn, false);
            kok.SetAsLastSibling();
            kok.name = "AyarlarPaneli";
            Yazi("Baslik", kok, Vector2.zero, Vector2.one, 56, "menu_ayarlar", metinler);
            Text baslik = kok.Find("Baslik").GetComponent<Text>();
            GriKumsalKur.TemaYazisi(baslik, true, tema.Yazi);
            baslik.alignment = TextAnchor.LowerLeft;
            GriKumsalKur.Yerles((RectTransform)baslik.transform, new Vector2(0f, 1f), new Vector2(MenuSol, -40f), new Vector2(800f, 84f));
            Cizgi(kok, new Vector2(MenuSol + 4f, -136f));
            var icerik = new GameObject("Icerik", typeof(RectTransform));
            icerik.layer = LayerMask.NameToLayer("UI");
            icerik.transform.SetParent(kok, false);
            var icerikAlani = (RectTransform)icerik.transform;
            GriKumsalKur.Yasla(icerikAlani, Vector2.zero, Vector2.one);
            icerikAlani.offsetMin = new Vector2(MenuSol, 36f);
            icerikAlani.offsetMax = new Vector2(-MenuSol, -166f);

            Transform sol = Liste("Genel", icerik.transform, new Vector2(0f, 0f), new Vector2(0.44f, 1f));
            var solDizi = sol.GetComponent<VerticalLayoutGroup>();
            solDizi.spacing = 4f;
            solDizi.childAlignment = TextAnchor.UpperLeft;
            Etiket(sol, "SesYazisi", "ayar_ses", metinler);
            Slider ses = Kaydirici(sol, 0f, 1f);
            Etiket(sol, "FareYazisi", "ayar_fare", metinler);
            Slider fare = Kaydirici(sol, 0.2f, 3f);
            Etiket(sol, "YaziBoyuYazisi", "ayar_yazi_boyutu", metinler);
            Slider yaziBoyu = Kaydirici(sol, 0.75f, 1.5f);
            Toggle ipuclari = Secim(sol, "ayar_ipuclari", metinler);
            Toggle tamEkran = Secim(sol, "ayar_tam_ekran", metinler);
            Etiket(sol, "CozunurlukYazisi", "ayar_cozunurluk", metinler);
            Dropdown cozunurluk = Acilir(sol);
            Etiket(sol, "KaliteYazisi", "ayar_kalite", metinler);
            Dropdown kalite = Acilir(sol);
            Button geri = Dugme(sol, "menu_geri", metinler);
            geri.GetComponent<LayoutElement>().preferredHeight = 50f;
            geri.GetComponentInChildren<Text>().fontSize = 28;

            Transform sag = Liste("Tuslar", icerik.transform, new Vector2(0.52f, 0f), new Vector2(1f, 1f));
            var sagDizi = sag.GetComponent<VerticalLayoutGroup>();
            sagDizi.spacing = 5f;
            sagDizi.childAlignment = TextAnchor.UpperLeft;
            Etiket(sag, "TuslarYazisi", "ayar_tuslar", metinler);
            foreach ((string id, string eylem, int sira) in TusSatirlari) TusSatiriKur(sag, id, eylem, sira, metinler);
            // Tuşları sıfırlayan seçenek tuş sütununda; sol sütun en büyük yazı boyutunda da sığsın.
            Button varsayilan = Dugme(sag, "menu_varsayilan", metinler);
            varsayilan.GetComponent<LayoutElement>().preferredHeight = 50f;
            varsayilan.GetComponentInChildren<Text>().fontSize = 28;

            var panel = kok.gameObject.AddComponent<AyarlarPaneli>();
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
            kok.gameObject.SetActive(false);
            return panel;
        }

        /// <summary>Ayarlarda bir değerin adı: küçük, soluk, sola yaslı.</summary>
        private static void Etiket(Transform ebeveyn, string ad, string id, MetinTablosu metinler)
        {
            Yazi(ad, ebeveyn, Vector2.zero, Vector2.zero, 22, id, metinler);
            Text yazi = ebeveyn.Find(ad).GetComponent<Text>();
            yazi.color = GriKumsalKur.Tema().SolukYazi;
            yazi.alignment = TextAnchor.LowerLeft;
            yazi.GetComponent<LayoutElement>().preferredHeight = 32f;
        }

        /// <summary>Yanında yazısı olan aç/kapa kutusu.</summary>
        private static Toggle Secim(Transform ebeveyn, string id, MetinTablosu metinler)
        {
            GameObject nesne = DefaultControls.CreateToggle(new DefaultControls.Resources());
            nesne.transform.SetParent(ebeveyn, false);
            Katman(nesne);
            nesne.AddComponent<LayoutElement>().preferredHeight = 32f;
            ArayuzTemasi tema = GriKumsalKur.Tema();
            Text yazi = nesne.GetComponentInChildren<Text>();
            YaziAyarla(yazi, 24, id, metinler);
            yazi.alignment = TextAnchor.MiddleLeft;
            ((RectTransform)yazi.transform).offsetMin += new Vector2(12f, 0f);
            yazi.color = tema.Yazi;
            // Kutu: ince çerçeveli koyu kare, işaret vurgu renginde.
            var toggle = nesne.GetComponent<Toggle>();
            var kutu = (Image)toggle.targetGraphic;
            kutu.color = tema.Cizgi;
            Image isaret = kutu.transform.GetChild(0).GetComponent<Image>();
            isaret.color = tema.Vurgu;
            isaret.rectTransform.sizeDelta = new Vector2(10f, 10f);
            Renkler(toggle);
            return toggle;
        }

        /// <summary>Seçilebilirin renk geçişi: üstüne gelince/seçilince biraz aydınlanır.</summary>
        private static void Renkler(Selectable secilebilir)
        {
            ColorBlock renkler = secilebilir.colors;
            renkler.normalColor = Color.white;
            renkler.highlightedColor = renkler.selectedColor = new Color(1.6f, 1.5f, 1.3f, 1f);
            renkler.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            renkler.colorMultiplier = 1.5f;
            secilebilir.colors = renkler;
        }

        /// <summary>Açılır liste (seçenekleri panel kurar).</summary>
        private static Dropdown Acilir(Transform ebeveyn)
        {
            GameObject nesne = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            nesne.transform.SetParent(ebeveyn, false);
            Katman(nesne);
            nesne.AddComponent<LayoutElement>().preferredHeight = 40f;
            ArayuzTemasi tema = GriKumsalKur.Tema();
            foreach (Text yazi in nesne.GetComponentsInChildren<Text>(true))
            {
                GriKumsalKur.TemaYazisi(yazi, false, tema.Yazi);
                yazi.fontSize = 22;
            }
            // Koyu kutu, açılan listede koyu panel; ok ve seçili işareti vurgu renginde.
            var acilir = nesne.GetComponent<Dropdown>();
            foreach (Image resim in nesne.GetComponentsInChildren<Image>(true))
            {
                string ad = resim.gameObject.name;
                resim.color = ad == "Arrow" || ad == "Item Checkmark" ? tema.Vurgu
                    : ad == "Template" ? new Color(tema.Gece.r, tema.Gece.g, tema.Gece.b, 0.97f)
                    : ad == "Handle" ? tema.SolukYazi
                    : ad == "Scrollbar" ? Color.clear
                    : tema.PanelIci;
            }
            Renkler(acilir);
            // Ok görseli yok: küçük, 45° dönmüş kare (eşkenar dörtgen) açılır olduğunu belli eder.
            var ok = (RectTransform)nesne.transform.Find("Arrow");
            ok.sizeDelta = new Vector2(9f, 9f);
            ok.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Toggle oge = acilir.template.GetComponentInChildren<Toggle>(true);
            if (oge != null) Renkler(oge);
            ListeSablonu(acilir, tema);
            nesne.AddComponent<AcilirListeKaydirma>();
            return acilir;
        }

        // Açılan listenin satır boyu ve en fazla boyu (tuval birimi). Unity'nin hazır şablonu 20'lik satır kurar, yazı okunmaz.
        private const float ListeSatiri = 40f, ListeBoyu = 280f;

        /// <summary>Açılan liste: okunur boyda satırlar, seçili satırın solunda ince vurgu çubuğu, ince kaydırma çubuğu.</summary>
        private static void ListeSablonu(Dropdown acilir, ArayuzTemasi tema)
        {
            RectTransform sablon = acilir.template;
            sablon.sizeDelta = new Vector2(sablon.sizeDelta.x, ListeBoyu);
            sablon.anchoredPosition = new Vector2(0f, -4f);
            var icerik = (RectTransform)sablon.Find("Viewport/Content");
            icerik.sizeDelta = new Vector2(icerik.sizeDelta.x, ListeSatiri);
            var satir = (RectTransform)icerik.Find("Item");
            satir.sizeDelta = new Vector2(satir.sizeDelta.x, ListeSatiri);

            var isaret = (RectTransform)satir.Find("Item Checkmark");
            isaret.anchorMin = new Vector2(0f, 0.5f);
            isaret.anchorMax = new Vector2(0f, 0.5f);
            isaret.pivot = new Vector2(0f, 0.5f);
            isaret.anchoredPosition = new Vector2(8f, 0f);
            isaret.sizeDelta = new Vector2(4f, ListeSatiri - 16f);

            var etiket = (RectTransform)satir.Find("Item Label");
            etiket.offsetMin = new Vector2(24f, 2f);
            etiket.offsetMax = new Vector2(-12f, -2f);
            Text etiketYazisi = etiket.GetComponent<Text>();
            etiketYazisi.fontSize = 24;
            etiketYazisi.alignment = TextAnchor.MiddleLeft;

            // Kaydırma çubuğu ince; listede çok çözünürlük olunca görünür.
            var kaydirma = (RectTransform)sablon.Find("Scrollbar");
            kaydirma.sizeDelta = new Vector2(6f, kaydirma.sizeDelta.y);
            var tutamak = (RectTransform)kaydirma.Find("Sliding Area/Handle");
            tutamak.sizeDelta = new Vector2(6f, 6f);
            ((RectTransform)kaydirma.Find("Sliding Area")).sizeDelta = new Vector2(-6f, -6f);
            ((RectTransform)sablon.Find("Viewport")).sizeDelta = new Vector2(-8f, 0f);
            // Fare tekerleği bir satır kadar kaydırsın (varsayılan 1 birim, liste yerinden oynamıyor gibi görünür).
            sablon.GetComponent<ScrollRect>().scrollSensitivity = ListeSatiri;

            Text baslik = acilir.captionText;
            baslik.fontSize = 24;
            baslik.color = tema.Yazi;
            var baslikAlani = (RectTransform)baslik.transform;
            baslikAlani.offsetMin = new Vector2(14f, baslikAlani.offsetMin.y);
        }

        private static void Katman(GameObject nesne)
        {
            int ui = LayerMask.NameToLayer("UI");
            foreach (Transform t in nesne.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ui;
        }

        /// <summary>Parkurlar paneli: her parkur için bir düğme (yazısı rekorla birlikte kodda kurulur) ve Geri.</summary>
        private static ParkurlarPaneli ParkurlarKur(Transform ebeveyn, GameObject oncekiPanel, MetinTablosu metinler)
        {
            // Ana menü seçeneklerinin yerinde: üstte soluk küçük başlık, altında parkurlar ve Geri.
            var dugmeler = new Object[System.Enum.GetValues(typeof(ParkurTuru)).Length];
            Transform kutu = Liste("ParkurlarPaneli", ebeveyn, Vector2.zero, Vector2.one);
            int satir = dugmeler.Length + 1;
            GriKumsalKur.Yerles((RectTransform)kutu, new Vector2(0f, 1f), new Vector2(MenuSol, -ListeUstu - 30f),
                new Vector2(ListeEni, satir * SecenekBoyu + (satir - 1) * SecenekAraligi));
            var dizi = kutu.GetComponent<VerticalLayoutGroup>();
            dizi.spacing = SecenekAraligi;
            dizi.childAlignment = TextAnchor.UpperLeft;
            // Küçük başlık sıranın dışında, seçenek yazılarıyla aynı hizada (çizgi payı kadar içeride).
            Etiket(kutu, "ParkurlarYazisi", "menu_parkurlar", metinler);
            Text etiket = kutu.Find("ParkurlarYazisi").GetComponent<Text>();
            etiket.fontSize = 26;
            etiket.GetComponent<LayoutElement>().ignoreLayout = true;
            GriKumsalKur.Yerles(etiket.rectTransform, new Vector2(0f, 1f), new Vector2(44f, 0f), new Vector2(ListeEni - 44f, 40f));
            etiket.rectTransform.pivot = new Vector2(0f, 0f);
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

            // Sağ üstte, ipucu kutusunun (üstten 114) üstünde kalacak boyda.
            GameObject sure = Kutucuk("Sure", tuval, new Vector2(0.773f, 0.9f), new Vector2(0.992f, 0.985f));
            Text sureYazisi = KutuYazisi(sure.transform, new Vector2(0.03f, 0.5f), new Vector2(0.97f, 1f), 28);
            Text bilgiYazisi = KutuYazisi(sure.transform, new Vector2(0.03f, 0f), new Vector2(0.97f, 0.5f), 24);
            GameObject sonuc = Kutucuk("Sonuc", tuval, new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.65f));
            Text sonucYazisi = KutuYazisi(sonuc.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), 36);
            sonucYazisi.alignment = TextAnchor.MiddleCenter;
            sonucYazisi.font = GriKumsalKur.Tema().BaslikYazisi;
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
            GriKumsalKur.Sabitle((RectTransform)kutu.transform, enAz, enCok);
            kutu.GetComponent<Image>().color = GriKumsalKur.Tema().Panel;
            return kutu;
        }

        private static Text KutuYazisi(Transform ebeveyn, Vector2 enAz, Vector2 enCok, int boyut)
        {
            var nesne = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            GriKumsalKur.Yasla((RectTransform)nesne.transform, enAz, enCok);
            Text yazi = nesne.GetComponent<Text>();
            GriKumsalKur.TemaYazisi(yazi, false, GriKumsalKur.Tema().Yazi);
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleRight;
            return yazi;
        }

        /// <summary>Bir satır: solda eylemin adı, sağda şu anki tuşu gösteren düğme.</summary>
        private static void TusSatiriKur(Transform ebeveyn, string id, string eylem, int sira, MetinTablosu metinler)
        {
            int ui = LayerMask.NameToLayer("UI");
            var satir = new GameObject(id, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            satir.layer = ui;
            satir.transform.SetParent(ebeveyn, false);
            // Esnek değil: yatay grup yüksekliği zorla genişlettiği için kendini esnek bildirir, satırlar sütunu doldurmasın.
            satir.GetComponent<LayoutElement>().preferredHeight = 32f;
            satir.GetComponent<LayoutElement>().flexibleHeight = 0f;
            ArayuzTemasi tema = GriKumsalKur.Tema();
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
            // Tuş: koyu kutu, ince çerçeve, krem yazı.
            dugme.GetComponent<Image>().color = tema.PanelIci;
            var cerceve = dugme.AddComponent<Outline>();
            cerceve.effectColor = tema.Cizgi;
            cerceve.effectDistance = new Vector2(1f, -1f);
            Renkler(dugme.GetComponent<Button>());
            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = ui;
            yaziNesnesi.transform.SetParent(dugme.transform, false);
            GriKumsalKur.Yasla((RectTransform)yaziNesnesi.transform, Vector2.zero, Vector2.one);
            Text tusYazisi = yaziNesnesi.GetComponent<Text>();
            GriKumsalKur.TemaYazisi(tusYazisi, false, tema.Yazi);
            tusYazisi.fontSize = 22;
            tusYazisi.alignment = TextAnchor.MiddleCenter;

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

        /// <summary>Kutusuz menü seçeneği: görünmez tıklama alanı, sola yaslı yazı, seçiliyken solunda vurgu çizgisi (MenuSecenegi).
        /// Çocukta eski tip Text (Unity sürümüne göre hazır düğme başka yazı bileşeni kurabiliyor).</summary>
        private static Button Dugme(Transform ebeveyn, string id, MetinTablosu metinler)
        {
            int ui = LayerMask.NameToLayer("UI");
            ArayuzTemasi tema = GriKumsalKur.Tema();
            var dugme = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            dugme.layer = ui;
            dugme.transform.SetParent(ebeveyn, false);
            dugme.GetComponent<Image>().color = Color.clear;
            dugme.GetComponent<LayoutElement>().preferredHeight = SecenekBoyu;
            var button = dugme.GetComponent<Button>();
            button.transition = Selectable.Transition.None;

            var isaret = new GameObject("Isaret", typeof(RectTransform), typeof(Image));
            isaret.layer = ui;
            isaret.transform.SetParent(dugme.transform, false);
            GriKumsalKur.Yerles((RectTransform)isaret.transform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(28f, 3f));
            Image isaretResmi = isaret.GetComponent<Image>();
            isaretResmi.color = tema.Vurgu;
            isaretResmi.raycastTarget = false;
            isaretResmi.enabled = false;

            var yaziNesnesi = new GameObject("Yazi", typeof(RectTransform), typeof(Text));
            yaziNesnesi.layer = ui;
            yaziNesnesi.transform.SetParent(dugme.transform, false);
            var yaziAlani = (RectTransform)yaziNesnesi.transform;
            GriKumsalKur.Yasla(yaziAlani, Vector2.zero, Vector2.one);
            yaziAlani.offsetMin = new Vector2(44f, 0f);
            Text yazi = yaziNesnesi.GetComponent<Text>();
            YaziAyarla(yazi, 34, id, metinler);
            yazi.alignment = TextAnchor.MiddleLeft;
            yazi.color = tema.Yazi;
            yazi.raycastTarget = false;

            var secenek = dugme.AddComponent<MenuSecenegi>();
            GriKumsalKur.Bagla(secenek, "tema", tema);
            GriKumsalKur.Bagla(secenek, "yazi", yazi);
            GriKumsalKur.Bagla(secenek, "isaret", isaretResmi);
            return button;
        }

        private static Slider Kaydirici(Transform ebeveyn, float enAz, float enCok)
        {
            GameObject nesne = DefaultControls.CreateSlider(new DefaultControls.Resources());
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.AddComponent<LayoutElement>().preferredHeight = 26f;
            var kaydirici = nesne.GetComponent<Slider>();
            kaydirici.minValue = enAz;
            kaydirici.maxValue = enCok;
            // İnce çizgi: iz soluk, dolu kısım vurgu, tutamak küçük krem dikdörtgen.
            ArayuzTemasi tema = GriKumsalKur.Tema();
            foreach (string ad in new[] { "Background", "Fill Area" })
            {
                var alan = (RectTransform)nesne.transform.Find(ad);
                alan.anchorMin = new Vector2(alan.anchorMin.x, 0.42f);
                alan.anchorMax = new Vector2(alan.anchorMax.x, 0.58f);
            }
            nesne.transform.Find("Background").GetComponent<Image>().color = tema.Cizgi;
            kaydirici.fillRect.GetComponent<Image>().color = tema.Vurgu;
            var tutamak = (Image)kaydirici.targetGraphic;
            tutamak.color = tema.Yazi;
            tutamak.rectTransform.sizeDelta = new Vector2(10f, 0f);
            Renkler(kaydirici);
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
            yazi.color = GriKumsalKur.Tema().Yazi;
        }

        private static void YaziAyarla(Text yazi, int boyut, string id, MetinTablosu metinler)
        {
            yazi.font = GriKumsalKur.Tema().GovdeYazisi;
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
            GriKumsalKur.TemaYazisi(yazi, ad == "Baslik", GriKumsalKur.Tema().Yazi);
            yazi.fontSize = boyut;
            yazi.alignment = TextAnchor.MiddleCenter;
            // Açık gökyüzü ve kum üstünde okunsun: koyu yumuşak gölge ve ince kontur.
            var kontur = nesne.AddComponent<Outline>();
            kontur.effectColor = new Color(0f, 0f, 0f, 0.3f);
            kontur.effectDistance = new Vector2(1.5f, -1.5f);
            var golge = nesne.AddComponent<Shadow>();
            golge.effectColor = new Color(0f, 0f, 0f, 0.55f);
            golge.effectDistance = new Vector2(0f, -3f);
            return yazi;
        }
    }
}
