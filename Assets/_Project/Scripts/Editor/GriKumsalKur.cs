using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Gri kutu deneme sahnelerini kurar: yuva gecesi (anne, kazma, insanlar, çöp) ve kaçış gecesi (sürü, sıcak lekeler, dalga, gün doğumu).</summary>
    public static class GriKumsalKur
    {
        private const string YuvaSahnesi = "Assets/_Project/Scenes/Prototypes/GriKumsal.unity";
        private const string KacisSahnesi = "Assets/_Project/Scenes/Prototypes/GriKacis.unity";
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

        private static Material kum, kumKoyu, deniz, taban, beyaz, engel, cop, kaplumbaga, insan, leke, iz, koni, termalSerit;

        [MenuItem("İki Dünya/Gri kutu kumsalı kur (yuva gecesi)")]
        public static void YuvaGecesiKur()
        {
            if (!SahneAc(YuvaSahnesi, out Scene sahne)) return;

            KumSicakligi kumSicakligi = OrtakKur();
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");

            // Engeller: şezlonglar (dolaşılır) ve çöp yığınları (takılır)
            var engeller = new GameObject("Engeller").transform;
            Kutu("Sezlong_0", engeller, new Vector3(-6f, 0.25f, 12f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            Kutu("Sezlong_1", engeller, new Vector3(-4f, 0.25f, 12f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            Kutu("Sezlong_2", engeller, new Vector3(5f, 0.25f, 18f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            CopYigini("CopYigini_0", engeller, new Vector3(2f, 0.2f, 9f));
            CopYigini("CopYigini_1", engeller, new Vector3(-10f, 0.2f, 20f));

            // İnsanlar: biri telefon fenerli
            var insanlar = new GameObject("Insanlar").transform;
            InsanKur("Insan_0", insanlar, new Vector3(-22f, 0f, 26f), new Vector3(-6f, 0f, 26f), false);
            InsanKur("Insan_1", insanlar, new Vector3(6f, 0f, 16f), new Vector3(22f, 0f, 16f), true);

            // Anne
            var baslangic = new GameObject("AnneBaslangic").transform;
            baslangic.SetPositionAndRotation(new Vector3(0f, 0.1f, 1f), Quaternion.identity);
            GameObject anne = AnneKur(baslangic);

            var gece = new GameObject("YuvaGecesi").AddComponent<YuvaGecesi>();
            Bagla(gece, "ayarlar", Ayar<AnneAyarlari>("AnneAyarlari"));
            Bagla(gece, "kumsal", kumsal);
            Bagla(gece, "kum", kumSicakligi);
            Bagla(gece, "anne", anne.GetComponent<AnneKara>());
            Bagla(gece, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(gece, "baslangic", baslangic);

            // Arayüz: enerji ve kazma çubuğu
            Transform tuval = TuvalKur();
            Cubuk enerjiCubugu = CubukKur("EnerjiCubugu", tuval, new Vector2(0.03f, 0.04f), new Vector2(0.28f, 0.065f), new Color(0.4f, 0.8f, 0.45f), false);
            var enerjiGostergesi = enerjiCubugu.gameObject.AddComponent<EnerjiCubugu>();
            Bagla(enerjiGostergesi, "enerji", anne.GetComponent<Enerji>());
            Bagla(enerjiGostergesi, "dolgu", enerjiCubugu.transform.GetChild(0));

            Cubuk kazmaCubugu = CubukKur("KazmaCubugu", tuval, new Vector2(0.4f, 0.42f), new Vector2(0.6f, 0.45f), Color.white, false);
            var kazmaGostergesi = tuval.gameObject.AddComponent<KazmaGostergesi>();
            Bagla(kazmaGostergesi, "kazma", anne.GetComponent<AnneKazma>());
            Bagla(kazmaGostergesi, "cubuk", kazmaCubugu);

            Kaydet(sahne, YuvaSahnesi);
        }

        [MenuItem("İki Dünya/Gri kutu kaçışı kur")]
        public static void KacisGecesiKur()
        {
            if (!SahneAc(KacisSahnesi, out Scene sahne)) return;

            KumSicakligi kumSicakligi = OrtakKur();
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            var kacisAyarlari = Ayar<KacisAyarlari>("KacisAyarlari");
            var yavruAyarlari = Ayar<YavruAyarlari>("YavruAyarlari");

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
            var lekeler = new GameObject("SicakLekeler").transform;
            Vector3[] lekeYerleri = { new Vector3(-3f, 0f, 16f), new Vector3(4f, 0f, 12f), new Vector3(-12f, 0f, 19f), new Vector3(12f, 0f, 21f) };
            for (int i = 0; i < lekeYerleri.Length; i++)
                SicakLeke($"SicakLeke_{i}", lekeler, lekeYerleri[i], kacisAyarlari, gunDogumu, kumSicakligi, kumsal);

            // Sürü ve gözü
            var yuva = new GameObject("VarsayilanYuva").transform;
            yuva.position = new Vector3(0f, 0.1f, 22f);

            var suru = new GameObject("Suru").AddComponent<Suru>();
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

            // Arayüz: kumdan çıkış paneli ve gün doğumu çubuğu
            Transform tuval = TuvalKur();
            var panel = new GameObject("CikisPaneli", typeof(RectTransform), typeof(Image));
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(tuval, false);
            Yasla((RectTransform)panel.transform, Vector2.zero, Vector2.one);
            panel.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.05f, 0.92f);

            Cubuk kesit = CubukKur("KumKesiti", panel.transform, new Vector2(0.44f, 0.2f), new Vector2(0.49f, 0.8f), new Color(0.86f, 0.78f, 0.6f), true);
            Cubuk sicaklik = CubukKur("YuzeySicakligi", panel.transform, new Vector2(0.53f, 0.2f), new Vector2(0.56f, 0.8f), Color.red, true);
            Cubuk cikisEnerjisi = CubukKur("CikisEnerjisi", panel.transform, new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.125f), new Color(0.4f, 0.8f, 0.45f), false);
            Cubuk gunCubugu = CubukKur("GunDogumuCubugu", tuval, new Vector2(0.3f, 0.94f), new Vector2(0.7f, 0.96f), new Color(1f, 0.75f, 0.3f), false);

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

            Kaydet(sahne, KacisSahnesi);
        }

        // ---- Ortak ----

        private static bool SahneAc(string yol, out Scene sahne)
        {
            sahne = default;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(yol) != null &&
                !EditorUtility.DisplayDialog("Gri kutu", $"{yol} var. Baştan kurulsun mu?", "Kur", "Vazgeç"))
                return false;

            KatmanKurulumu.Kur();
            Klasor("Assets/_Project/Scenes/Prototypes", "GriKumsal");
            MalzemeleriHazirla();

            sahne = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.GetComponent<UnityEngine.Camera>() != null) Object.DestroyImmediate(kok);
            return true;
        }

        private static void Kaydet(Scene sahne, string yol)
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
        }

        /// <summary>Kum, deniz, kumul, dalga çizgisi, bölgeler, kum sıcaklığı ve termal görünüm, kumsal yapıları.</summary>
        private static KumSicakligi OrtakKur()
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
            KumsalYapilariKur(kumSicakligi);
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

        private static void KumsalYapilariKur(KumSicakligi kumSicakligi)
        {
            var yapilar = new GameObject("KumsalYapilari").AddComponent<KumsalYapilari>();
            Bagla(yapilar, "kumsal", Ayar<KumsalDurumu>("KumsalDurumu"));
            Bagla(yapilar, "kum", kumSicakligi);
            Bagla(yapilar, "golgelikKalibi", GolgelikKalibiKur());
            Bagla(yapilar, "kayipIziKalibi", KayipIziKalibiKur());
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

        private static void CopYigini(string ad, Transform ebeveyn, Vector3 konum)
        {
            GameObject yigin = Kutu(ad, ebeveyn, konum, new Vector3(1.2f, 0.4f, 1.2f), cop, "Trash");
            yigin.GetComponent<Collider>().isTrigger = true;
            yigin.AddComponent<TrashTrap>();
        }

        private static void InsanKur(string ad, Transform ebeveyn, Vector3 a, Vector3 b, bool fenerli)
        {
            // Kök yerde durur; kapsül gövde tabanı yere değsin diye bir alt objede.
            var kok = new GameObject(ad).transform;
            kok.SetParent(ebeveyn, false);
            kok.SetPositionAndRotation(a, Quaternion.LookRotation(b - a));
            GameObject govde = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            govde.name = "Govde";
            govde.transform.SetParent(kok, false);
            govde.transform.localPosition = new Vector3(0f, 1f, 0f);
            govde.transform.localRotation = Quaternion.identity;
            govde.GetComponent<Renderer>().sharedMaterial = insan;

            var noktaA = new GameObject($"{ad}_A").transform;
            noktaA.SetParent(ebeveyn, false);
            noktaA.position = a;
            var noktaB = new GameObject($"{ad}_B").transform;
            noktaB.SetParent(ebeveyn, false);
            noktaB.position = b;

            var insanBileseni = kok.gameObject.AddComponent<Insan>();
            Bagla(insanBileseni, "ayarlar", Ayar<InsanAyarlari>("InsanAyarlari"));
            Bagla(insanBileseni, "koniMalzemesi", koni);
            var so = new SerializedObject(insanBileseni);
            SerializedProperty noktalar = so.FindProperty("noktalar");
            noktalar.arraySize = 2;
            noktalar.GetArrayElementAtIndex(0).objectReferenceValue = noktaA;
            noktalar.GetArrayElementAtIndex(1).objectReferenceValue = noktaB;
            so.FindProperty("fenerli").boolValue = fenerli;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static GameObject KalipKaydet(GameObject nesne, string yol)
        {
            GameObject kalip = PrefabUtility.SaveAsPrefabAsset(nesne, yol);
            Object.DestroyImmediate(nesne);
            return kalip;
        }

        // ---- Küçük yardımcılar ----

        private static void Govde(Transform ebeveyn, Vector3 konum, Vector3 olcek)
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

        private static BirinciSahisKamera KameraKur(Transform ebeveyn, Vector3 konum, Transform govde)
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

        private static Transform TuvalKur()
        {
            var tuval = new GameObject("Arayuz", typeof(Canvas), typeof(CanvasScaler));
            tuval.layer = LayerMask.NameToLayer("UI");
            tuval.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var olcek = tuval.GetComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = new Vector2(1920f, 1080f);
            return tuval.transform;
        }

        /// <summary>Koyu arka plan ve renkli dolgudan bir çubuk; Cubuk bileşeni arka plandadır, dolgu ilk çocuğudur.</summary>
        private static Cubuk CubukKur(string ad, Transform ebeveyn, Vector2 enAz, Vector2 enCok, Color renk, bool dikey)
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

        private static void Yasla(RectTransform rect, Vector2 enAz, Vector2 enCok)
        {
            rect.anchorMin = enAz;
            rect.anchorMax = enCok;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static GameObject Kutu(string ad, Transform ebeveyn, Vector3 konum, Vector3 boyut, Material malzeme, string katman)
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

        private static void Klasor(string ust, string ad)
        {
            if (!AssetDatabase.IsValidFolder($"{ust}/{ad}")) AssetDatabase.CreateFolder(ust, ad);
        }

        private static Material Malzeme(string ad, Color renk, string shader = "Universal Render Pipeline/Lit")
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

        private static Material SaydamMalzeme(string ad, Color renk)
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

        /// <summary>Ayar asset'ini Settings klasöründe bulur; yoksa varsayılan değerlerle oluşturur.</summary>
        private static T Ayar<T>(string ad) where T : ScriptableObject
        {
            string[] bulunan = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { AyarKlasoru });
            if (bulunan.Length > 0) return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(bulunan[0]));

            var ayar = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ayar, $"{AyarKlasoru}/{ad}.asset");
            return ayar;
        }

        private static void Bagla(Object hedef, string alan, Object deger)
        {
            var so = new SerializedObject(hedef);
            so.FindProperty(alan).objectReferenceValue = deger;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
