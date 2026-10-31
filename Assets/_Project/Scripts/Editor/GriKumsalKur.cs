using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Menu;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Kumsal sahnelerini kurar: deneme sahneleri (yuva gecesi, kaçış gecesi) ve oyunun Kumsal sahnesi (iki gece + geçici insan turu, adım adım açılır).</summary>
    public static class GriKumsalKur
    {
        private const string YuvaSahnesi = "Assets/_Project/Scenes/Prototypes/GriKumsal.unity";
        private const string KacisSahnesi = "Assets/_Project/Scenes/Prototypes/GriKacis.unity";
        private const string KumsalSahnesi = "Assets/_Project/Scenes/Levels/Kumsal.unity";
        private const string SistemlerKalibi = "Assets/_Project/Prefabs/Sistemler.prefab";
        private const string KalipKlasoru = "Assets/_Project/Prefabs/Hazards";
        private const string MalzemeKlasoru = "Assets/_Project/Scenes/Prototypes/GriKumsal";
        private const string AyarKlasoru = "Assets/_Project/ScriptableObjects/Settings";
        private const string YavruKalibi = "Assets/_Project/Prefabs/Turtles/Yavru.prefab";
        private const string GolgelikKalibi = "Assets/_Project/Prefabs/Structures/Golgelik.prefab";
        private const string KayipIziKalibi = "Assets/_Project/Prefabs/Environment/KayipIzi.prefab";

        // Gri kutu ölçüleri (metre). Oyun değeri değil; sadece deneme sahnesinin yerleşimi.
        // Kıyı x ekseni boyunca uzanır; z = 0 deniz kenarı, z arttıkça karaya doğru.
        private const float KumsalUzunlugu = 80f;
        private const float KumsalDerinligi = 40f;
        private const float DenizDerinligi = 30f;
        private const float DalgaCizgisiZ = 6f;
        private const int BolgeSayisi = 8;

        private static Material kum, kumKoyu, deniz, taban, beyaz, engel, cop, kaplumbaga, insan, leke, iz, koni, tilkiKonisi, golge, siper, tilki, termalSerit, ayYolu;

        [MenuItem("İki Dünya/Gri kutu kumsalı kur (yuva gecesi)")]
        public static void YuvaGecesiKur()
        {
            if (!SahneAc(YuvaSahnesi, out Scene sahne)) return;
            KumSicakligi kumSicakligi = OrtakKur(Bos("Insanlar", null));
            DenemeKumsaliKur();
            YuvaKur(kumSicakligi, null);
            Kaydet(sahne, YuvaSahnesi);
        }

        [MenuItem("İki Dünya/Gri kutu kaçışı kur")]
        public static void KacisGecesiKur()
        {
            if (!SahneAc(KacisSahnesi, out Scene sahne)) return;
            KumSicakligi kumSicakligi = OrtakKur(null);
            DenemeKumsaliKur();
            KacisKur(kumSicakligi, null);
            Kaydet(sahne, KacisSahnesi);
        }

        /// <summary>Oyunun Kumsal sahnesi (Bölüm 2–4 ve final): yuva gecesi, kaçış gecesi, geçici insan turu ve "30 yıl sonra"; her biri kendi adımında açılır.</summary>
        [MenuItem("İki Dünya/Kumsal sahnesini kur (oyun)")]
        public static void KumsalSahnesiKur()
        {
            if (!SahneAc(KumsalSahnesi, out Scene sahne)) return;
            Transform yuvaGrubu = Grup("YuvaGecesiNesneleri", AdimTuru.YuvaGecesi);
            // İnsanlar kumsalın durumundan kurulur, sadece yuva gecesinde görünür.
            KumSicakligi kumSicakligi = OrtakKur(Bos("Insanlar", yuvaGrubu));
            YuvaKur(kumSicakligi, yuvaGrubu);
            KacisKur(kumSicakligi, Grup("KacisGecesiNesneleri", AdimTuru.KacisGecesi));
            InsanTuruKur(Grup("InsanTuruNesneleri", AdimTuru.InsanTuru));
            MenuKur.FinalKur(Grup("FinalNesneleri", AdimTuru.Final), sahne, kaplumbaga, leke);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
            Kaydet(sahne, KumsalSahnesi);
        }

        // ---- Yuva gecesi ----

        private static void YuvaKur(KumSicakligi kumSicakligi, Transform grup)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");

            // Şezlong, çöp ve insanlar KumsalYapilari ile kumsalın durumundan kurulur.
            var baslangic = new GameObject("AnneBaslangic").transform;
            baslangic.SetPositionAndRotation(new Vector3(0f, 0.1f, 1f), Quaternion.identity);
            GameObject anne = AnneKur(baslangic);
            anne.transform.SetParent(grup, true);

            var gece = new GameObject("YuvaGecesi").AddComponent<YuvaGecesi>();
            Bagla(gece, "ayarlar", Ayar<AnneAyarlari>("AnneAyarlari"));
            Bagla(gece, "kumsal", kumsal);
            Bagla(gece, "kum", kumSicakligi);
            Bagla(gece, "anne", anne.GetComponent<AnneKara>());
            Bagla(gece, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(gece, "baslangic", baslangic);

            // Arayüz: enerji ve kazma çubuğu
            Transform tuval = TuvalKur("YuvaArayuzu", grup);
            Cubuk enerjiCubugu = CubukKur("EnerjiCubugu", tuval, new Vector2(0.03f, 0.04f), new Vector2(0.28f, 0.065f), new Color(0.4f, 0.8f, 0.45f), false);
            var enerjiGostergesi = enerjiCubugu.gameObject.AddComponent<EnerjiCubugu>();
            Bagla(enerjiGostergesi, "enerji", anne.GetComponent<Enerji>());
            Bagla(enerjiGostergesi, "dolgu", enerjiCubugu.transform.GetChild(0));

            Cubuk kazmaCubugu = CubukKur("KazmaCubugu", tuval, new Vector2(0.4f, 0.42f), new Vector2(0.6f, 0.45f), Color.white, false);
            var kazmaGostergesi = tuval.gameObject.AddComponent<KazmaGostergesi>();
            Bagla(kazmaGostergesi, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(kazmaGostergesi, "cubuk", kazmaCubugu);

            SonucPaneliKur(gece, null);
        }

        // ---- Kaçış gecesi ----

        private static void KacisKur(KumSicakligi kumSicakligi, Transform grup)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            var kacisAyarlari = Ayar<KacisAyarlari>("KacisAyarlari");
            var yavruAyarlari = Ayar<YavruAyarlari>("YavruAyarlari");
            var avciAyarlari = Ayar<AvciAyarlari>("AvciAyarlari");

            var gunDogumu = new GameObject("GunDogumu").AddComponent<GunDogumu>();
            Bagla(gunDogumu, "ayarlar", kacisAyarlari);

            // Dalga: kıyının son metrelerinde gelip giden su şeridi
            var dalga = new GameObject("Dalga").AddComponent<Dalga>();
            dalga.transform.SetParent(kumSicakligi.transform, false);
            GameObject serit = Kutu("SuSeridi", dalga.transform, new Vector3(0f, 0.02f, 0f), new Vector3(KumsalUzunlugu, 0.04f, 1f), deniz, "Water");
            Object.DestroyImmediate(serit.GetComponent<Collider>());
            serit.isStatic = false;
            Bagla(dalga, "ayarlar", kacisAyarlari);
            Bagla(dalga, "suSeridi", serit.transform);

            // Sıcak kum lekeleri: sıcak bantlarda
            Transform lekeler = Bos("SicakLekeler", grup);
            Vector3[] lekeYerleri = { new Vector3(-3f, 0f, 16f), new Vector3(4f, 0f, 12f), new Vector3(-12f, 0f, 19f), new Vector3(12f, 0f, 21f) };
            for (int i = 0; i < lekeYerleri.Length; i++)
                SicakLeke($"SicakLeke_{i}", lekeler, lekeYerleri[i], kacisAyarlari, gunDogumu, kumSicakligi, kumsal);

            // Çöp, şişe ve tekerlek izi KumsalYapilari ile kumsalın durumundan kurulur; kum kalesi atlanır.
            Transform copler = Bos("Copler", grup);
            Kutu("KumKalesi_0", copler, new Vector3(0f, 0.07f, 11f), new Vector3(1.5f, 0.14f, 0.5f), kumKoyu, "Trash");

            // Kumul gölgesi: tilkiden saklanılan siperler
            Transform siperler = Bos("Siperler", grup);
            Siper("KumulGolgesi_0", siperler, new Vector3(-6f, 0f, 22f), new Vector3(3f, 1f, 4f));
            Siper("KumulGolgesi_1", siperler, new Vector3(6f, 0f, 19f), new Vector3(3f, 1f, 4f));

            // Avcılar ve ışık
            Transform avcilar = Bos("Avcilar", grup);
            YengecKur("Yengec_0", avcilar, new Vector3(-5f, 0f, 8f), avciAyarlari);
            YengecKur("Yengec_1", avcilar, new Vector3(5f, 0f, 5f), avciAyarlari);
            MartiKur(avcilar, avciAyarlari);
            TilkiKur(avcilar, avciAyarlari, kumsal);

            // Sürü ve gözü
            var yuva = new GameObject("VarsayilanYuva").transform;
            yuva.position = new Vector3(0f, 0.1f, 22f);

            var suru = new GameObject("Suru").AddComponent<Suru>();
            suru.transform.SetParent(grup, false);
            suru.transform.SetPositionAndRotation(yuva.position, Quaternion.LookRotation(Vector3.back));
            var goz = KameraKur(suru.transform, new Vector3(0f, 0.5f, 0f), null);
            Bagla(suru, "ayarlar", yavruAyarlari);
            Bagla(suru, "yavruKalibi", YavruKalibiKur().GetComponent<Yavru>());
            Bagla(suru, "kamera", goz);

            var gece = new GameObject("KacisGecesi").AddComponent<KacisGecesi>();
            Bagla(gece, "ayarlar", yavruAyarlari);
            Bagla(gece, "kacisAyarlari", kacisAyarlari);
            Bagla(gece, "kumsal", kumsal);
            Bagla(gece, "kum", kumSicakligi);
            Bagla(gece, "suru", suru);
            Bagla(gece, "gunDogumu", gunDogumu);
            Bagla(gece, "dalga", dalga);
            Bagla(gece, "varsayilanYuva", yuva);

            // Arayüz: kumdan çıkış paneli, gün doğumu ve kurtarma çubuğu
            Transform tuval = TuvalKur("KacisArayuzu", grup);
            var panel = new GameObject("CikisPaneli", typeof(RectTransform), typeof(Image));
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(tuval, false);
            Yasla((RectTransform)panel.transform, Vector2.zero, Vector2.one);
            panel.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.05f, 0.92f);

            Cubuk kesit = CubukKur("KumKesiti", panel.transform, new Vector2(0.44f, 0.2f), new Vector2(0.49f, 0.8f), new Color(0.86f, 0.78f, 0.6f), true);
            Cubuk sicaklik = CubukKur("YuzeySicakligi", panel.transform, new Vector2(0.53f, 0.2f), new Vector2(0.56f, 0.8f), Color.red, true);
            Cubuk cikisEnerjisi = CubukKur("CikisEnerjisi", panel.transform, new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.125f), new Color(0.4f, 0.8f, 0.45f), false);
            Cubuk gunCubugu = CubukKur("GunDogumuCubugu", tuval, new Vector2(0.3f, 0.94f), new Vector2(0.7f, 0.96f), new Color(1f, 0.75f, 0.3f), false);
            Cubuk yardimCubugu = CubukKur("YardimCubugu", tuval, new Vector2(0.42f, 0.42f), new Vector2(0.58f, 0.445f), Color.white, false);

            var gosterge = tuval.gameObject.AddComponent<KacisGostergesi>();
            Bagla(gosterge, "gece", gece);
            Bagla(gosterge, "ayarlar", yavruAyarlari);
            Bagla(gosterge, "gunDogumu", gunDogumu);
            Bagla(gosterge, "termal", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(gosterge, "cikisPaneli", panel);
            Bagla(gosterge, "kumKesiti", kesit);
            Bagla(gosterge, "yuzeySicakligi", sicaklik);
            Bagla(gosterge, "cikisEnerjisi", cikisEnerjisi);
            Bagla(gosterge, "gunDogumuCubugu", gunCubugu);
            Bagla(gosterge, "suru", suru);
            Bagla(gosterge, "yardimCubugu", yardimCubugu);

            SonucPaneliKur(null, gece);
        }

        // ---- Geçici insan turu ----

        private static void InsanTuruKur(Transform grup)
        {
            // Kumsala tepeden bakan kamera; yönetim ekranı gelene kadar.
            var kamera = new GameObject("YonetimKamerasi");
            kamera.transform.SetParent(grup, false);
            kamera.transform.SetPositionAndRotation(new Vector3(0f, 55f, KumsalDerinligi / 2f), Quaternion.Euler(90f, 0f, 0f));
            kamera.AddComponent<UnityEngine.Camera>();
            kamera.AddComponent<AudioListener>();

            var tur = new GameObject("GeciciInsanTuru").AddComponent<GeciciInsanTuru>();
            Bagla(tur, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));

            // Ne yapılacağını söyleyen satır (insan_turu_gecici): "tıkla, bir hafta geçsin"
            Transform tuval = TuvalKur("InsanTuruArayuzu", grup);
            var ipucu = new GameObject("Ipucu", typeof(RectTransform), typeof(Text));
            ipucu.layer = LayerMask.NameToLayer("UI");
            ipucu.transform.SetParent(tuval, false);
            Yasla((RectTransform)ipucu.transform, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.12f));
            Text yazi = ipucu.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = 36;
            yazi.alignment = TextAnchor.MiddleCenter;
            yazi.color = Color.white;
            var metin = ipucu.AddComponent<MetinYazisi>();
            Bagla(metin, "tablo", Ayar<MetinTablosu>("MetinTablosu"));
            var so = new SerializedObject(metin);
            so.FindProperty("id").stringValue = "insan_turu_gecici";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---- Ortak ----

        internal static bool SahneAc(string yol, out Scene sahne)
        {
            sahne = default;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(yol) != null &&
                !EditorUtility.DisplayDialog("Gri kutu", $"{yol} var. Baştan kurulsun mu?", "Kur", "Vazgeç"))
                return false;

            KatmanKurulumu.Kur();
            // Yeni oyunun kumsalı başlangıç asset'inden kurulsun.
            var oyunAyarlari = Ayar<OyunAyarlari>("OyunAyarlari");
            if (oyunAyarlari.KumsalBaslangici == null)
            {
                Bagla(oyunAyarlari, "kumsalBaslangici", Ayar<KumsalBaslangici>("KumsalBaslangici"));
                EditorUtility.SetDirty(oyunAyarlari);
            }
            Klasor("Assets/_Project/Scenes/Prototypes", "GriKumsal");
            MalzemeleriHazirla();

            sahne = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.GetComponent<UnityEngine.Camera>() != null) Object.DestroyImmediate(kok);
            return true;
        }

        internal static void Kaydet(Scene sahne, string yol)
        {
            EditorSceneManager.SaveScene(sahne, yol);
            AssetDatabase.SaveAssets();
            Debug.Log($"Gri kutu kuruldu: {yol}");
        }

        private static void MalzemeleriHazirla()
        {
            kum = Malzeme("Kum", new Color(0.86f, 0.78f, 0.6f));
            kumKoyu = Malzeme("KumKoyu", new Color(0.8f, 0.72f, 0.55f));
            deniz = Malzeme("Deniz", new Color(0.15f, 0.35f, 0.55f));
            taban = Malzeme("Taban", new Color(0.45f, 0.42f, 0.35f));
            beyaz = Malzeme("Beyaz", Color.white);
            engel = Malzeme("Engel", new Color(0.55f, 0.3f, 0.25f));
            cop = Malzeme("Cop", new Color(0.3f, 0.5f, 0.3f));
            kaplumbaga = Malzeme("Kaplumbaga", new Color(0.35f, 0.3f, 0.2f));
            insan = Malzeme("Insan", new Color(0.3f, 0.35f, 0.6f));
            leke = Malzeme("SicakLeke", new Color(0.85f, 0.3f, 0.15f));
            iz = Malzeme("KayipIzi", new Color(0.25f, 0.1f, 0.1f));
            termalSerit = Malzeme("TermalSerit", Color.white, "Universal Render Pipeline/Unlit");
            koni = SaydamMalzeme("GorusKonisi", new Color(1f, 0.95f, 0.5f, 0.3f));
            tilkiKonisi = SaydamMalzeme("TilkiKonisi", new Color(1f, 0.5f, 0.2f, 0.3f));
            golge = SaydamMalzeme("Golge", new Color(0f, 0f, 0f, 0.5f));
            siper = Malzeme("Siper", new Color(0.62f, 0.56f, 0.42f));
            tilki = Malzeme("Tilki", new Color(0.85f, 0.45f, 0.15f));
            ayYolu = Malzeme("AyYolu", new Color(0.85f, 0.9f, 1f), "Universal Render Pipeline/Unlit");
        }

        /// <summary>Kum, deniz, kumul, dalga çizgisi, bölgeler, kum sıcaklığı ve termal görünüm, kumsal yapıları.</summary>
        private static KumSicakligi OrtakKur(Transform insanKoku)
        {
            var dunya = new GameObject("Dunya").transform;

            Kutu("Kum", dunya, new Vector3(0f, -0.5f, KumsalDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, KumsalDerinligi), kum, "Ground");
            Kutu("DenizTabani", dunya, new Vector3(0f, -2f, -DenizDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, DenizDerinligi), taban, "Ground");
            GameObject su = Kutu("Deniz", dunya, new Vector3(0f, -0.6f, -DenizDerinligi / 2f), new Vector3(KumsalUzunlugu, 0.1f, DenizDerinligi), deniz, "Water");
            Object.DestroyImmediate(su.GetComponent<Collider>());

            var kumul = new GameObject("Kumul").transform;
            kumul.SetParent(dunya);
            for (int i = 0; i < 8; i++)
            {
                float x = -KumsalUzunlugu / 2f + 5f + i * 10f;
                Kutu($"Kumul_{i}", kumul, new Vector3(x, 0.8f, KumsalDerinligi - 3f), new Vector3(9f, 2.5f, 6f), kumKoyu, "Ground");
            }

            var dalgaCizgisi = new GameObject("DalgaCizgisi").transform;
            dalgaCizgisi.SetParent(dunya);
            for (float x = -KumsalUzunlugu / 2f + 1f; x < KumsalUzunlugu / 2f; x += 2f)
            {
                GameObject parca = Kutu("Parca", dalgaCizgisi, new Vector3(x, 0.01f, DalgaCizgisiZ), new Vector3(1f, 0.02f, 0.15f), beyaz, "Default");
                Object.DestroyImmediate(parca.GetComponent<Collider>());
            }

            var bolgeler = new GameObject("Bolgeler").transform;
            bolgeler.SetParent(dunya);
            float bolgeGenisligi = KumsalUzunlugu / BolgeSayisi;
            for (int i = 0; i < BolgeSayisi; i++)
            {
                float x = -KumsalUzunlugu / 2f + bolgeGenisligi * (i + 0.5f);
                GameObject bolge = Kutu($"Bolge_{i}", bolgeler, new Vector3(x, 0.005f, KumsalDerinligi / 2f),
                    new Vector3(bolgeGenisligi, 0.01f, KumsalDerinligi), i % 2 == 0 ? kum : kumKoyu, "Default");
                Object.DestroyImmediate(bolge.GetComponent<Collider>());
            }

            KumSicakligi kumSicakligi = TermalKur();
            // Ay ışığı: denizin üstünde doğru yönü gösteren parlak yol
            GameObject ay = Kutu("AyYolu", dunya, new Vector3(-6f, -0.54f, -DenizDerinligi / 2f), new Vector3(3f, 0.01f, DenizDerinligi), ayYolu, "Default");
            Object.DestroyImmediate(ay.GetComponent<Collider>());

            KumsalYapilariKur(kumSicakligi, insanKoku);
            return kumSicakligi;
        }

        private static KumSicakligi TermalKur()
        {
            // Konumu deniz kenarının ortası; +z karaya doğru.
            var nesne = new GameObject("KumSicakligi");
            var kumSicakligi = nesne.AddComponent<KumSicakligi>();
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            Bagla(kumSicakligi, "ayarlar", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(kumSicakligi, "kumsal", kumsal);
            var so = new SerializedObject(kumSicakligi);
            so.FindProperty("uzunluk").floatValue = KumsalUzunlugu;
            so.FindProperty("derinlik").floatValue = KumsalDerinligi;
            so.FindProperty("bolgeSayisi").intValue = BolgeSayisi;
            so.FindProperty("dalgaCizgisi").floatValue = DalgaCizgisiZ;
            so.ApplyModifiedPropertiesWithoutUndo();

            var termal = nesne.AddComponent<TermalGorunum>();
            Bagla(termal, "kum", kumSicakligi);
            Bagla(termal, "kumsal", kumsal);
            Bagla(termal, "seritMalzemesi", termalSerit);
            return kumSicakligi;
        }

        private static void KumsalYapilariKur(KumSicakligi kumSicakligi, Transform insanKoku)
        {
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            var yapilar = new GameObject("KumsalYapilari").AddComponent<KumsalYapilari>();
            Bagla(yapilar, "kumsal", kumsal);
            Bagla(yapilar, "kum", kumSicakligi);
            Bagla(yapilar, "golgelikKalibi", GolgelikKalibiKur());
            Bagla(yapilar, "kayipIziKalibi", KayipIziKalibiKur());
            Bagla(yapilar, "copYiginiKalibi", TuzakKalibi("CopYigini", new Vector3(1.2f, 0.4f, 1.2f), TrashTrap.Turu.CopYigini));
            Bagla(yapilar, "agKalibi", TuzakKalibi("Ag", new Vector3(0.8f, 0.02f, 0.8f), TrashTrap.Turu.Ag));
            Bagla(yapilar, "tekerlekIziKalibi", TuzakKalibi("TekerlekIzi", new Vector3(6f, 0.02f, 0.5f), TrashTrap.Turu.TekerlekIzi));
            Bagla(yapilar, "siseKalibi", KutuKalibi("Sise", new Vector3(0.15f, 0.3f, 0.15f), cop, "Trash"));
            Bagla(yapilar, "sezlongKalibi", KutuKalibi("Sezlong", new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle"));
            Bagla(yapilar, "isikKalibi", KalipKaydet(IsikKur("OtelIsigi", null, Vector3.zero, Ayar<KacisAyarlari>("KacisAyarlari"), kumsal), $"{KalipKlasoru}/OtelIsigi.prefab"));
            Bagla(yapilar, "insanKalibi", InsanKalibiKur().GetComponent<Insan>());
            Bagla(yapilar, "insanKoku", insanKoku);
        }

        /// <summary>Deneme sahnelerinde Play başında kumsalı ilk haline getiren nesne.</summary>
        private static void DenemeKumsaliKur()
        {
            var deneme = new GameObject("DenemeKumsali").AddComponent<DenemeKumsali>();
            Bagla(deneme, "ayarlar", Ayar<OyunAyarlari>("OyunAyarlari"));
            Bagla(deneme, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
        }

        // ---- Yuva gecesi parçaları ----

        private static GameObject AnneKur(Transform baslangic)
        {
            var anne = new GameObject("Anne");
            anne.layer = LayerMask.NameToLayer("Turtle");
            anne.transform.SetPositionAndRotation(baslangic.position, baslangic.rotation);

            var kontrol = anne.AddComponent<CharacterController>();
            kontrol.radius = 0.4f;
            kontrol.height = 0.8f;
            kontrol.center = new Vector3(0f, 0.4f, 0f);
            kontrol.stepOffset = 0.2f;

            var anneAyarlari = Ayar<AnneAyarlari>("AnneAyarlari");
            anne.AddComponent<Enerji>();
            Bagla(anne.AddComponent<AnneKara>(), "ayarlar", anneAyarlari);
            Bagla(anne.AddComponent<AnneKazma>(), "ayarlar", anneAyarlari);

            Govde(anne.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.35f, 1.1f));
            KameraKur(anne.transform, new Vector3(0f, 0.35f, 0.6f), anne.transform);
            return anne;
        }

        /// <summary>Takan çöp kalıbı: kök yerde, collider ve TrashTrap çocukta.</summary>
        private static GameObject TuzakKalibi(string ad, Vector3 boyut, TrashTrap.Turu tur)
        {
            var kok = new GameObject(ad);
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject tuzak = Kutu("Govde", kok.transform, new Vector3(0f, boyut.y / 2f, 0f), boyut, cop, "Trash");
            tuzak.isStatic = false;
            var kutu = (BoxCollider)tuzak.GetComponent<Collider>();
            kutu.isTrigger = true;
            // Yassı tuzaklar da yavruyu içine alsın diye collider yukarı uzatılır.
            if (boyut.y < 0.1f)
            {
                kutu.size = new Vector3(1f, 0.3f / boyut.y, 1f);
                kutu.center = new Vector3(0f, 0.15f / boyut.y, 0f);
            }
            var bilesen = tuzak.AddComponent<TrashTrap>();
            var so = new SerializedObject(bilesen);
            so.FindProperty("tur").enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
            return KalipKaydet(kok, $"{KalipKlasoru}/{ad}.prefab");
        }

        /// <summary>Tek kutudan engel kalıbı: kök yerde, kutu üstünde.</summary>
        private static GameObject KutuKalibi(string ad, Vector3 boyut, Material malzeme, string katman)
        {
            var kok = new GameObject(ad);
            kok.layer = LayerMask.NameToLayer(katman);
            Kutu("Govde", kok.transform, new Vector3(0f, boyut.y / 2f, 0f), boyut, malzeme, katman).isStatic = false;
            return KalipKaydet(kok, $"{KalipKlasoru}/{ad}.prefab");
        }

        private static GameObject InsanKalibiKur()
        {
            // Kök yerde durur; kapsül gövde tabanı yere değsin diye bir alt objede.
            var kok = new GameObject("Insan");
            GameObject govde = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            govde.name = "Govde";
            govde.transform.SetParent(kok.transform, false);
            govde.transform.localPosition = new Vector3(0f, 1f, 0f);
            govde.GetComponent<Renderer>().sharedMaterial = insan;

            var insanBileseni = kok.AddComponent<Insan>();
            Bagla(insanBileseni, "ayarlar", Ayar<InsanAyarlari>("InsanAyarlari"));
            Bagla(insanBileseni, "koniMalzemesi", koni);
            return KalipKaydet(kok, $"{KalipKlasoru}/Insan.prefab");
        }

        // ---- Kaçış parçaları ----

        private static void SicakLeke(string ad, Transform ebeveyn, Vector3 konum, KacisAyarlari ayarlar, GunDogumu gunDogumu, KumSicakligi kumSicakligi, KumsalDurumu kumsal)
        {
            GameObject nesne = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nesne.name = ad;
            nesne.layer = LayerMask.NameToLayer("Hazard");
            nesne.transform.SetParent(ebeveyn, false);
            nesne.transform.position = konum + Vector3.up * 0.015f;
            nesne.transform.localScale = new Vector3(2f, 0.01f, 2f);
            nesne.GetComponent<Renderer>().sharedMaterial = leke;

            // Silindirin kendi collider'ı yassı ölçekte küreye döner; yerine yavruyu içine alan bir kutu.
            Object.DestroyImmediate(nesne.GetComponent<Collider>());
            var kutu = nesne.AddComponent<BoxCollider>();
            kutu.isTrigger = true;
            kutu.size = new Vector3(1f, 60f, 1f);
            kutu.center = new Vector3(0f, 30f, 0f);

            var bolge = nesne.AddComponent<HazardZone>();
            Bagla(bolge, "ayarlar", ayarlar);
            Bagla(bolge, "gunDogumu", gunDogumu);
            Bagla(bolge, "kum", kumSicakligi);
            Bagla(bolge, "kumsal", kumsal);
        }

        private static void Siper(string ad, Transform ebeveyn, Vector3 konum, Vector3 boyut)
        {
            GameObject nesne = Kutu(ad, ebeveyn, konum + Vector3.up * 0.5f, boyut, siper, "Cover");
            nesne.GetComponent<Collider>().isTrigger = true;
            Object.DestroyImmediate(nesne.GetComponent<MeshRenderer>());
            // Görünen gölge: kumun üstünde ince koyu şerit
            GameObject gorunus = Kutu($"{ad}_Gorunus", ebeveyn, konum + Vector3.up * 0.006f, new Vector3(boyut.x, 0.01f, boyut.z), siper, "Default");
            Object.DestroyImmediate(gorunus.GetComponent<Collider>());
            nesne.AddComponent<Cover>();
        }

        private static void YengecKur(string ad, Transform ebeveyn, Vector3 konum, AvciAyarlari ayarlar)
        {
            var kok = Bos(ad, ebeveyn);
            kok.position = konum;
            GameObject delik = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            delik.name = "TitreyenKum";
            Object.DestroyImmediate(delik.GetComponent<Collider>());
            delik.transform.SetParent(kok, false);
            delik.transform.localPosition = Vector3.up * 0.01f;
            delik.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
            delik.GetComponent<Renderer>().sharedMaterial = kumKoyu;

            var yengec = kok.gameObject.AddComponent<Yengec>();
            Bagla(yengec, "ayarlar", ayarlar);
            Bagla(yengec, "titreyenKum", delik.transform);
        }

        private static void MartiKur(Transform ebeveyn, AvciAyarlari ayarlar)
        {
            var kok = Bos("Marti", ebeveyn);
            kok.position = new Vector3(0f, 15f, 15f);
            GameObject golgeNesnesi = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            golgeNesnesi.name = "Golge";
            Object.DestroyImmediate(golgeNesnesi.GetComponent<Collider>());
            golgeNesnesi.transform.SetParent(kok, false);
            golgeNesnesi.transform.localScale = new Vector3(ayarlar.MartiDalisYaricapi * 2f, 0.005f, ayarlar.MartiDalisYaricapi * 2f);
            var renderer = golgeNesnesi.GetComponent<Renderer>();
            renderer.sharedMaterial = golge;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var marti = kok.gameObject.AddComponent<Marti>();
            Bagla(marti, "ayarlar", ayarlar);
            Bagla(marti, "golge", golgeNesnesi.transform);
        }

        private static void TilkiKur(Transform ebeveyn, AvciAyarlari ayarlar, KumsalDurumu kumsal)
        {
            var noktaA = Bos("TilkiYolu_A", ebeveyn);
            noktaA.position = new Vector3(-25f, 0f, 30f);
            var noktaB = Bos("TilkiYolu_B", ebeveyn);
            noktaB.position = new Vector3(25f, 0f, 30f);

            var kok = Bos("Tilki", ebeveyn);
            kok.SetPositionAndRotation(noktaA.position, Quaternion.LookRotation(Vector3.right));
            GameObject govde = Kutu("Govde", kok, new Vector3(0f, 0.2f, 0f), new Vector3(0.3f, 0.35f, 0.8f), tilki, "Predator");
            Object.DestroyImmediate(govde.GetComponent<Collider>());
            govde.isStatic = false;

            var bilesen = kok.gameObject.AddComponent<Tilki>();
            Bagla(bilesen, "ayarlar", ayarlar);
            Bagla(bilesen, "kumsal", kumsal);
            Bagla(bilesen, "koniMalzemesi", tilkiKonisi);
            Bagla(bilesen, "govde", govde);
            var so = new SerializedObject(bilesen);
            SerializedProperty noktalar = so.FindProperty("noktalar");
            noktalar.arraySize = 2;
            noktalar.GetArrayElementAtIndex(0).objectReferenceValue = noktaA;
            noktalar.GetArrayElementAtIndex(1).objectReferenceValue = noktaB;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject IsikKur(string ad, Transform ebeveyn, Vector3 konum, KacisAyarlari ayarlar, KumsalDurumu kumsal)
        {
            var kok = Bos(ad, ebeveyn);
            kok.position = konum;
            kok.gameObject.layer = LayerMask.NameToLayer("Hazard");
            GameObject direk = Kutu("Direk", kok, new Vector3(0f, 1.5f, 0f), new Vector3(0.2f, 3f, 0.2f), engel, "Obstacle");
            direk.isStatic = false;

            var lamba = new GameObject("Lamba").AddComponent<Light>();
            lamba.transform.SetParent(kok, false);
            lamba.transform.localPosition = Vector3.up * 3f;
            lamba.type = LightType.Point;
            lamba.range = 16f;
            lamba.intensity = 4f;
            lamba.color = new Color(1f, 0.85f, 0.6f);

            // Çekim alanı: lambanın çevresindeki kum
            var alan = kok.gameObject.AddComponent<SphereCollider>();
            alan.isTrigger = true;
            alan.radius = 9f;

            var isik = kok.gameObject.AddComponent<IsikKaynagi>();
            Bagla(isik, "ayarlar", ayarlar);
            Bagla(isik, "kumsal", kumsal);
            Bagla(isik, "isik", lamba);
            return kok.gameObject;
        }

        // ---- Sonuç paneli ----

        private static void SonucPaneliKur(YuvaGecesi yuvaGecesi, KacisGecesi kacisGecesi)
        {
            Transform tuval = TuvalKur("SonucArayuzu", null);
            tuval.GetComponent<Canvas>().sortingOrder = 10;
            var panel = tuval.gameObject.AddComponent<GeceSonucuPaneli>();
            Bagla(panel, "termal", Ayar<TermalAyarlari>("TermalAyarlari"));

            if (yuvaGecesi != null)
            {
                Bagla(panel, "yuvaGecesi", yuvaGecesi);
                RectTransform kutu = PanelKur("YuvaSonucu", tuval);
                Bagla(panel, "yuvaPaneli", kutu.gameObject);
                Bagla(panel, "yuvaBandi", RenkKaresi(kutu, 0, Color.white));
                Bagla(panel, "yumurtaSayisi", SayiYazisi(kutu, 0));
            }

            if (kacisGecesi != null)
            {
                Bagla(panel, "kacisGecesi", kacisGecesi);
                RectTransform kutu = PanelKur("KacisSonucu", tuval);
                Bagla(panel, "kacisPaneli", kutu.gameObject);
                Color[] renkler = { new Color(0.4f, 0.8f, 0.45f), new Color(0.85f, 0.25f, 0.2f), new Color(1f, 0.55f, 0.75f), new Color(0.4f, 0.65f, 1f) };
                string[] alanlar = { "kurtulan", "kaybedilen", "disi", "erkek" };
                for (int i = 0; i < alanlar.Length; i++)
                {
                    RenkKaresi(kutu, i, renkler[i]);
                    Bagla(panel, alanlar[i], SayiYazisi(kutu, i));
                }
            }
        }

        private static RectTransform PanelKur(string ad, Transform tuval)
        {
            var panel = new GameObject(ad, typeof(RectTransform), typeof(Image));
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(tuval, false);
            var rect = (RectTransform)panel.transform;
            Yasla(rect, new Vector2(0.4f, 0.3f), new Vector2(0.6f, 0.7f));
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            panel.SetActive(false);
            return rect;
        }

        /// <summary>Panelde i. satırın solundaki renkli kare.</summary>
        private static Image RenkKaresi(RectTransform panel, int satir, Color renk)
        {
            var kare = new GameObject($"Renk_{satir}", typeof(RectTransform), typeof(Image));
            kare.layer = LayerMask.NameToLayer("UI");
            kare.transform.SetParent(panel, false);
            float ust = 0.9f - satir * 0.2f;
            Yasla((RectTransform)kare.transform, new Vector2(0.15f, ust - 0.12f), new Vector2(0.35f, ust));
            var resim = kare.GetComponent<Image>();
            resim.color = renk;
            return resim;
        }

        /// <summary>Panelde i. satırın sağındaki sayı.</summary>
        private static Text SayiYazisi(RectTransform panel, int satir)
        {
            var nesne = new GameObject($"Sayi_{satir}", typeof(RectTransform), typeof(Text));
            nesne.layer = LayerMask.NameToLayer("UI");
            nesne.transform.SetParent(panel, false);
            float ust = 0.9f - satir * 0.2f;
            Yasla((RectTransform)nesne.transform, new Vector2(0.45f, ust - 0.14f), new Vector2(0.9f, ust + 0.02f));
            var yazi = nesne.GetComponent<Text>();
            yazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            yazi.fontSize = 48;
            yazi.alignment = TextAnchor.MiddleLeft;
            yazi.color = Color.white;
            return yazi;
        }

        // ---- Kalıplar (prefab) ----

        private static GameObject YavruKalibiKur()
        {
            var yavru = new GameObject("Yavru");
            yavru.layer = LayerMask.NameToLayer("Turtle");
            var kontrol = yavru.AddComponent<CharacterController>();
            kontrol.radius = 0.1f;
            kontrol.height = 0.2f;
            kontrol.center = new Vector3(0f, 0.1f, 0f);
            kontrol.stepOffset = 0.05f;
            kontrol.skinWidth = 0.01f;
            yavru.AddComponent<Enerji>();
            yavru.AddComponent<Yavru>();
            Govde(yavru.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.16f, 0.06f, 0.2f));
            return KalipKaydet(yavru, YavruKalibi);
        }

        private static GameObject GolgelikKalibiKur()
        {
            var golgelik = new GameObject("Golgelik");
            int katman = LayerMask.NameToLayer("Structure");
            golgelik.layer = katman;
            Vector3[] direkler = { new Vector3(-1.5f, 1f, -1.5f), new Vector3(1.5f, 1f, -1.5f), new Vector3(-1.5f, 1f, 1.5f), new Vector3(1.5f, 1f, 1.5f) };
            foreach (Vector3 direk in direkler)
                Kutu("Direk", golgelik.transform, direk, new Vector3(0.1f, 2f, 0.1f), beyaz, "Structure").isStatic = false;
            Kutu("Tente", golgelik.transform, new Vector3(0f, 2f, 0f), new Vector3(3.4f, 0.05f, 3.4f), beyaz, "Structure").isStatic = false;
            return KalipKaydet(golgelik, GolgelikKalibi);
        }

        private static GameObject KayipIziKalibiKur()
        {
            GameObject isaret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            isaret.name = "KayipIzi";
            Object.DestroyImmediate(isaret.GetComponent<Collider>());
            isaret.transform.localScale = new Vector3(0.3f, 0.005f, 0.3f);
            isaret.GetComponent<Renderer>().sharedMaterial = iz;
            return KalipKaydet(isaret, KayipIziKalibi);
        }

        internal static GameObject KalipKaydet(GameObject nesne, string yol)
        {
            GameObject kalip = PrefabUtility.SaveAsPrefabAsset(nesne, yol);
            Object.DestroyImmediate(nesne);
            return kalip;
        }

        // ---- Küçük yardımcılar ----

        internal static void Govde(Transform ebeveyn, Vector3 konum, Vector3 olcek)
        {
            GameObject kabuk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kabuk.name = "Kabuk";
            Object.DestroyImmediate(kabuk.GetComponent<Collider>());
            kabuk.layer = ebeveyn.gameObject.layer;
            kabuk.transform.SetParent(ebeveyn, false);
            kabuk.transform.localPosition = konum;
            kabuk.transform.localScale = olcek;
            kabuk.GetComponent<Renderer>().sharedMaterial = kaplumbaga;
        }

        internal static BirinciSahisKamera KameraKur(Transform ebeveyn, Vector3 konum, Transform govde)
        {
            var goz = new GameObject("Goz");
            goz.transform.SetParent(ebeveyn, false);
            goz.transform.localPosition = konum;
            goz.tag = "MainCamera";
            goz.AddComponent<UnityEngine.Camera>().nearClipPlane = 0.03f;
            goz.AddComponent<AudioListener>();
            var bakis = goz.AddComponent<BirinciSahisKamera>();
            Bagla(bakis, "ayarlar", Ayar<KameraAyarlari>("KameraAyarlari"));
            if (govde != null) Bagla(bakis, "govde", govde);
            return bakis;
        }

        internal static Transform TuvalKur(string ad, Transform grup)
        {
            // GraphicRaycaster olmadan düğmeler tıklanmaz.
            var tuval = new GameObject(ad, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            tuval.layer = LayerMask.NameToLayer("UI");
            tuval.transform.SetParent(grup, false);
            tuval.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var olcek = tuval.GetComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = new Vector2(1920f, 1080f);
            return tuval.transform;
        }

        /// <summary>Adımına göre açılıp kapanan nesne grubu (AdimNesneleri).</summary>
        private static Transform Grup(string ad, AdimTuru tur)
        {
            var grup = new GameObject(ad).AddComponent<AdimNesneleri>();
            var so = new SerializedObject(grup);
            SerializedProperty turler = so.FindProperty("turler");
            turler.arraySize = 1;
            turler.GetArrayElementAtIndex(0).enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
            return grup.transform;
        }

        internal static Transform Bos(string ad, Transform ebeveyn)
        {
            var nesne = new GameObject(ad).transform;
            nesne.SetParent(ebeveyn, false);
            return nesne;
        }

        /// <summary>Koyu arka plan ve renkli dolgudan bir çubuk; Cubuk bileşeni arka plandadır, dolgu ilk çocuğudur.</summary>
        internal static Cubuk CubukKur(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, Color renk, bool dikey)
        {
            int ui = LayerMask.NameToLayer("UI");
            var arka = new GameObject(ad, typeof(RectTransform), typeof(Image));
            arka.layer = ui;
            arka.transform.SetParent(ebeveyn, false);
            Yasla((RectTransform)arka.transform, enAz, enCok);
            arka.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var dolgu = new GameObject("Dolgu", typeof(RectTransform), typeof(Image));
            dolgu.layer = ui;
            dolgu.transform.SetParent(arka.transform, false);
            Yasla((RectTransform)dolgu.transform, Vector2.zero, Vector2.one);
            var resim = dolgu.GetComponent<Image>();
            resim.color = renk;

            var cubuk = arka.AddComponent<Cubuk>();
            Bagla(cubuk, "dolgu", dolgu.transform);
            Bagla(cubuk, "dolguResmi", resim);
            var so = new SerializedObject(cubuk);
            so.FindProperty("dikey").boolValue = dikey;
            so.ApplyModifiedPropertiesWithoutUndo();
            return cubuk;
        }

        internal static void Yasla(RectTransform rect, Vector2 enAz, Vector2 enCok)
        {
            rect.anchorMin = enAz;
            rect.anchorMax = enCok;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        internal static GameObject Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 boyut, Material malzeme, string katman)
        {
            GameObject kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kutu.name = ad;
            kutu.transform.SetParent(ebeveyn, false);
            kutu.transform.localPosition = konum;
            kutu.transform.localScale = boyut;
            kutu.layer = LayerMask.NameToLayer(katman);
            kutu.isStatic = true;
            kutu.GetComponent<Renderer>().sharedMaterial = malzeme;
            return kutu;
        }

        internal static void Klasor(string ust, string ad)
        {
            if (!AssetDatabase.IsValidFolder($"{ust}/{ad}")) AssetDatabase.CreateFolder(ust, ad);
        }

        internal static Material Malzeme(string ad, Color renk, string shader = "Universal Render Pipeline/Lit")
        {
            string yol = $"{MalzemeKlasoru}/{ad}.mat";
            var malzeme = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (malzeme == null)
            {
                malzeme = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(malzeme, yol);
            }
            malzeme.SetColor("_BaseColor", renk);
            EditorUtility.SetDirty(malzeme);
            return malzeme;
        }

        internal static Material SaydamMalzeme(string ad, Color renk)
        {
            Material malzeme = Malzeme(ad, renk, "Universal Render Pipeline/Unlit");
            malzeme.SetFloat("_Surface", 1f);
            malzeme.SetFloat("_Blend", 0f);
            malzeme.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            malzeme.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            malzeme.SetFloat("_ZWrite", 0f);
            malzeme.SetOverrideTag("RenderType", "Transparent");
            malzeme.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            malzeme.renderQueue = (int)RenderQueue.Transparent;
            return malzeme;
        }

        /// <summary>Ayar asset'ini Settings klasöründe (önce adıyla) bulur; yoksa varsayılan değerlerle oluşturur.</summary>
        internal static T Ayar<T>(string ad) where T : ScriptableObject
        {
            // Aynı türden birden çok asset olabilir (ör. anne ve yavru deniz ayarları): önce adıyla bakılır.
            var adiyla = AssetDatabase.LoadAssetAtPath<T>($"{AyarKlasoru}/{ad}.asset");
            if (adiyla != null) return adiyla;

            string[] bulunan = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { AyarKlasoru });
            if (bulunan.Length > 0) return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(bulunan[0]));

            var ayar = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ayar, $"{AyarKlasoru}/{ad}.asset");
            return ayar;
        }

        internal static void Bagla(Object hedef, string alan, Object deger)
        {
            var so = new SerializedObject(hedef);
            so.FindProperty(alan).objectReferenceValue = deger;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
