using System;
using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace IkiDunya.Editor
{
    /// <summary>Deniz bölümlerinin gri kutu sahnelerini kurar: Bölüm 1 (anne kıyıya döner) ve Bölüm 5 (yavru yosun adasına yüzer).</summary>
    public static class GriDenizKur
    {
        private const string Bolum1Sahnesi = "Assets/_Project/Scenes/Levels/Bolum1_Donus.unity";
        private const string Bolum5Sahnesi = "Assets/_Project/Scenes/Levels/Bolum5_IlkYuzus.unity";
        private const string AyarKlasoru = "Assets/_Project/ScriptableObjects/Settings";
        private const string KalipKlasoru = "Assets/_Project/Prefabs/Hazards";
        private const string SistemlerKalibi = "Assets/_Project/Prefabs/Sistemler.prefab";

        // Gri kutu ölçüleri (metre). Kıyı z = 0, deniz -z yönünde; su yüzeyi y = 0.
        private const float DenizUzunlugu = 200f;
        private const float DenizGenisligi = 60f;

        private static Material su, taban, kaya, cayir, ag, misina, denizanasi, poset, tekne, avci, sicakSu, akinti, yosun, isaret, plastik, dalga, agKenari, tekneGolgesi, kopuk;

        [MenuItem("İki Dünya/Bölüm 1 sahnesini kur (deniz)")]
        public static void Bolum1Kur()
        {
            if (!GriKumsalKur.SahneAc(Bolum1Sahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var ayarlar = AyarAdla<DenizAyarlari>("AnneDenizAyarlari", null);

            Quaternion kiyiya = Quaternion.identity;
            DenizHareketi anne = YuzucuKur("Anne", ayarlar, tehlike, new Vector3(0.9f, 0.35f, 1.1f), 0.45f, new Vector3(0f, 0.15f, 0.5f));
            Transform baslangic = Nokta("Baslangic", new Vector3(0f, -3f, -195f), kiyiya);

            Kontrol("KontrolNoktasi_1", -120f, kiyiya, false);
            Kontrol("KontrolNoktasi_2", -50f, kiyiya, false);
            Kontrol("Kiyi", -4f, kiyiya, true);

            // Akıntılar: biri kıyıya doğru (yardım eder), biri tersine, biri yana
            AkintiKur(new Vector3(-15f, -6f, -160f), new Vector3(20f, 12f, 40f), 0f, tehlike);
            AkintiKur(new Vector3(15f, -6f, -160f), new Vector3(20f, 12f, 40f), 180f, tehlike);
            AkintiKur(new Vector3(0f, -3f, -85f), new Vector3(30f, 6f, 20f), 90f, tehlike);

            // Besin: denizanası ve ona benzeyen poşet
            Vector3[] denizanalari = { new Vector3(-8f, -4f, -170f), new Vector3(12f, -5f, -150f), new Vector3(6f, -2f, -140f), new Vector3(-10f, -3f, -100f), new Vector3(8f, -3f, -70f), new Vector3(0f, -2f, -35f) };
            Vector3[] posetler = { new Vector3(-4f, -3f, -155f), new Vector3(3f, -4f, -130f), new Vector3(10f, -2f, -110f), new Vector3(-12f, -2f, -60f) };
            foreach (Vector3 yer in denizanalari) YiyecekKur(Yiyecek.Turu.Denizanasi, yer, tehlike);
            foreach (Vector3 yer in posetler) YiyecekKur(Yiyecek.Turu.Poset, yer, tehlike);

            Object.Instantiate(HayaletAgKalibi(), new Vector3(5f, -6f, -135f), Quaternion.Euler(0f, 30f, 0f));
            Object.Instantiate(HayaletAgKalibi(), new Vector3(-8f, -3f, -75f), Quaternion.Euler(0f, -20f, 0f));
            TekneKur("Tekne_0", -140f, false, tehlike, null);
            TekneKur("Tekne_1", -80f, false, tehlike, null);
            SicakSuKur(new Vector3(-12f, -5f, -125f), 5f, tehlike);
            SicakSuKur(new Vector3(10f, -2f, -40f), 4f, tehlike);

            BolumKur(AdimTuru.Donus, ayarlar, anne, baslangic, false);
            GriKumsalKur.Kaydet(sahne, Bolum1Sahnesi);
        }

        [MenuItem("İki Dünya/Bölüm 5 sahnesini kur (deniz)")]
        public static void Bolum5Kur()
        {
            if (!GriKumsalKur.SahneAc(Bolum5Sahnesi, out Scene sahne)) return;
            Hazirla();
            var tehlike = GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari");
            var kumsal = GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu");
            var ayarlar = AyarAdla<DenizAyarlari>("YavruDenizAyarlari", so =>
            {
                // GDD Bölüm 5: çılgın yüzüş (saniyede 0,5 enerji, yemek yok), nefes yok, ağ 10 basış, Space sadece daldırır.
                so.FindProperty("yuzmeHizi").floatValue = 2.5f;
                so.FindProperty("hareketEnerjisi").floatValue = 0f;
                so.FindProperty("surekliEnerji").floatValue = 0.5f;
                so.FindProperty("nefesSuresi").floatValue = 0f;
                so.FindProperty("agBasisi").intValue = 10;
                so.FindProperty("spaceSadeceDalar").boolValue = true;
            });

            Quaternion denize = Quaternion.Euler(0f, 180f, 0f);
            DenizHareketi yavru = YuzucuKur("Yavru", ayarlar, tehlike, new Vector3(0.16f, 0.06f, 0.2f), 0.12f, new Vector3(0f, 0.05f, 0.15f));
            Transform baslangic = Nokta("Baslangic", new Vector3(0f, -0.5f, -3f), denize);

            // Dalga hattı: kıyıya gelen dalgaların altından dalınır
            var dalgaKok = new GameObject("DalgaHatti").AddComponent<DenizDalgasi>();
            GameObject tepe = GriKumsalKur.Kutu("Tepe", dalgaKok.transform, Vector3.zero, new Vector3(DenizGenisligi, 0.8f, 1.5f), dalga, "Water");
            Object.DestroyImmediate(tepe.GetComponent<Collider>());
            tepe.isStatic = false;
            GriKumsalKur.Bagla(dalgaKok, "ayarlar", tehlike);
            GriKumsalKur.Bagla(dalgaKok, "tepe", tepe.transform);

            Kontrol("KontrolNoktasi_1", -30f, denize, false);
            Kontrol("KontrolNoktasi_2", -110f, denize, false);
            GameObject ada = GriKumsalKur.Kutu("YosunAdasi", null, new Vector3(0f, 0f, -195f), new Vector3(12f, 0.6f, 12f), yosun, "Default");
            Object.DestroyImmediate(ada.GetComponent<Collider>());
            Kontrol("YosunAdasi_Son", -195f, denize, true);

            // Sığ sular: kayalar ve deniz çayırı (avcı balıktan saklanılır)
            Vector3[] kayalar = { new Vector3(-8f, -5f, -50f), new Vector3(9f, -4.5f, -65f), new Vector3(-3f, -5f, -85f), new Vector3(12f, -5f, -100f) };
            foreach (Vector3 yer in kayalar) GriKumsalKur.Kutu("Kaya", null, yer, new Vector3(3f, 3f, 3f), kaya, "Ground");
            CayirKur(new Vector3(6f, -5f, -60f), new Vector3(6f, 2.5f, 10f));
            CayirKur(new Vector3(-10f, -5f, -80f), new Vector3(6f, 2.5f, 8f));
            CayirKur(new Vector3(4f, -5f, -95f), new Vector3(8f, 2.5f, 8f));
            AvciKur(new Vector3(0f, -3f, -60f), tehlike);
            AvciKur(new Vector3(-5f, -4f, -90f), tehlike);

            // Açık deniz: hayalet ağ ve misina (Hayalet Ağ Temizliği azaltır), parlayan plastik, ışıklı tekne
            var yapilar = new GameObject("DenizYapilari").AddComponent<DenizYapilari>();
            GriKumsalKur.Bagla(yapilar, "ayarlar", tehlike);
            GriKumsalKur.Bagla(yapilar, "kumsal", kumsal);
            GriKumsalKur.Bagla(yapilar, "agKalibi", HayaletAgKalibi());
            GriKumsalKur.Bagla(yapilar, "misinaKalibi", MisinaKalibi());
            Vector3[] agYerleri = { new Vector3(-6f, -3f, -45f), new Vector3(8f, -4f, -70f), new Vector3(-4f, -8f, -125f), new Vector3(10f, -6f, -140f), new Vector3(-8f, -5f, -160f), new Vector3(5f, -3f, -180f) };
            var so = new SerializedObject(yapilar);
            SerializedProperty noktalar = so.FindProperty("noktalar");
            noktalar.arraySize = agYerleri.Length;
            for (int i = 0; i < agYerleri.Length; i++)
                noktalar.GetArrayElementAtIndex(i).objectReferenceValue = Nokta($"AgYeri_{i}", agYerleri[i], Quaternion.Euler(0f, i * 37f, 0f), yapilar.transform);
            so.ApplyModifiedPropertiesWithoutUndo();

            PlastikKur(new Vector3(10f, -3f, -130f), tehlike, kumsal);
            PlastikKur(new Vector3(-12f, -2f, -150f), tehlike, kumsal);
            PlastikKur(new Vector3(4f, -4f, -170f), tehlike, kumsal);
            TekneKur("IsikliTekne", -150f, true, tehlike, kumsal);

            BolumKur(AdimTuru.IlkYuzus, ayarlar, yavru, baslangic, true);
            GriKumsalKur.Kaydet(sahne, Bolum5Sahnesi);
        }

        // ---- Ortak ----

        private static void Hazirla()
        {
            su = GriKumsalKur.SaydamMalzeme("SuYuzeyi", new Color(0.2f, 0.45f, 0.6f, 0.6f));
            // Su altından da görünsün: iki yüzlü
            su.SetFloat("_Cull", 0f);
            taban = GriKumsalKur.Malzeme("DenizTabani", new Color(0.55f, 0.5f, 0.4f));
            kaya = GriKumsalKur.Malzeme("Kaya", new Color(0.35f, 0.35f, 0.38f));
            cayir = GriKumsalKur.SaydamMalzeme("DenizCayiri", new Color(0.2f, 0.6f, 0.3f, 0.5f));
            ag = GriKumsalKur.Malzeme("HayaletAg", new Color(0.85f, 0.95f, 1f), "Universal Render Pipeline/Unlit");
            misina = GriKumsalKur.Malzeme("Misina", new Color(0.95f, 0.95f, 0.8f), "Universal Render Pipeline/Unlit");
            denizanasi = GriKumsalKur.SaydamMalzeme("Denizanasi", new Color(1f, 0.6f, 0.85f, 0.7f));
            poset = GriKumsalKur.SaydamMalzeme("Poset", new Color(0.95f, 0.95f, 0.95f, 0.7f));
            tekne = GriKumsalKur.Malzeme("Tekne", new Color(0.9f, 0.9f, 0.9f));
            avci = GriKumsalKur.Malzeme("AvciBalik", new Color(0.15f, 0.18f, 0.22f));
            sicakSu = GriKumsalKur.SaydamMalzeme("SicakSu", new Color(0.5f, 0.15f, 0.1f, 0.35f));
            akinti = GriKumsalKur.SaydamMalzeme("Akinti", new Color(0.8f, 0.9f, 1f, 0.25f));
            yosun = GriKumsalKur.Malzeme("YosunAdasi", new Color(0.45f, 0.4f, 0.15f));
            isaret = GriKumsalKur.Malzeme("KontrolIsareti", new Color(1f, 0.85f, 0.2f));
            plastik = GriKumsalKur.Malzeme("ParlayanPlastik", new Color(1f, 1f, 0.7f), "Universal Render Pipeline/Unlit");
            dalga = GriKumsalKur.SaydamMalzeme("Dalga", new Color(0.9f, 0.95f, 1f, 0.6f));
            agKenari = GriKumsalKur.Malzeme("AgKenari", new Color(0.7f, 1f, 1f), "Universal Render Pipeline/Unlit");
            tekneGolgesi = GriKumsalKur.SaydamMalzeme("TekneGolgesi", new Color(0f, 0.02f, 0.05f, 0.55f));
            kopuk = GriKumsalKur.SaydamMalzeme("PervaneKopugu", new Color(1f, 1f, 1f, 0.75f));

            // Su altı sisi
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.025f;
            RenderSettings.fogColor = new Color(0.1f, 0.35f, 0.45f);

            var deniz = new GameObject("Deniz").transform;
            GriKumsalKur.SuYuzeyiKur("SuYuzeyi", deniz, new Vector3(0f, 0f, -DenizUzunlugu / 2f), new Vector2(DenizGenisligi, DenizUzunlugu), su);

            // Taban üç basamak: açık deniz, sığ kıyı suları, kumsalın önü; kıyıda kumsal
            GriKumsalKur.Kutu("AcikDeniz", deniz, new Vector3(0f, -15.5f, -160f), new Vector3(DenizGenisligi, 1f, 80f), taban, "Ground");
            GriKumsalKur.Kutu("SigSular", deniz, new Vector3(0f, -6.5f, -85f), new Vector3(DenizGenisligi, 1f, 70f), taban, "Ground");
            GriKumsalKur.Kutu("KumsalOnu", deniz, new Vector3(0f, -3f, -25f), new Vector3(DenizGenisligi, 1f, 50f), taban, "Ground");
            GriKumsalKur.Kutu("Kumsal", deniz, new Vector3(0f, 0f, 15f), new Vector3(DenizGenisligi, 1f, 30f), taban, "Ground");

            // Görünmez duvarlar
            foreach (float x in new[] { -DenizGenisligi / 2f, DenizGenisligi / 2f })
                Object.DestroyImmediate(GriKumsalKur.Kutu("Duvar", deniz, new Vector3(x, -10f, -DenizUzunlugu / 2f), new Vector3(1f, 40f, DenizUzunlugu), su, "Default").GetComponent<MeshRenderer>());
            Object.DestroyImmediate(GriKumsalKur.Kutu("Duvar", deniz, new Vector3(0f, -10f, -DenizUzunlugu - 1f), new Vector3(DenizGenisligi, 40f, 1f), su, "Default").GetComponent<MeshRenderer>());
        }

        private static DenizHareketi YuzucuKur(string ad, DenizAyarlari ayarlar, DenizTehlikeAyarlari tehlike, Vector3 govdeOlcegi, float yaricap, Vector3 gozYeri)
        {
            var nesne = new GameObject(ad);
            nesne.layer = LayerMask.NameToLayer("Turtle");
            var kontrol = nesne.AddComponent<CharacterController>();
            kontrol.radius = yaricap;
            kontrol.height = yaricap * 2f;
            kontrol.center = Vector3.zero;
            kontrol.skinWidth = yaricap * 0.1f;
            kontrol.stepOffset = 0f;
            nesne.AddComponent<Enerji>();
            var yuzucu = nesne.AddComponent<DenizHareketi>();

            GriKumsalKur.Govde(nesne.transform, Vector3.zero, govdeOlcegi);
            BirinciSahisKamera goz = GriKumsalKur.KameraKur(nesne.transform, gozYeri, nesne.transform);
            GriKumsalKur.Bagla(goz.gameObject.AddComponent<SuAltiGorunumu>(), "ayarlar", GriKumsalKur.Ayar<OrtamAyarlari>("OrtamAyarlari"));
            GriKumsalKur.Bagla(goz, "ayarlar", AyarAdla<KameraAyarlari>("DenizKameraAyarlari", so =>
            {
                so.FindProperty("asagiSinir").floatValue = -75f;
                so.FindProperty("yukariSinir").floatValue = 75f;
            }));

            GriKumsalKur.Bagla(yuzucu, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(yuzucu, "tehlikeAyarlari", tehlike);
            GriKumsalKur.Bagla(yuzucu, "goz", goz.transform);

            // Göstergeler: enerji, nefes, ağdan kurtulma
            Transform tuval = GriKumsalKur.TuvalKur("DenizArayuzu", null);
            Cubuk enerji = GriKumsalKur.CubukKur("EnerjiCubugu", tuval, new Vector2(0.03f, 0.04f), new Vector2(0.28f, 0.065f), new Color(0.4f, 0.8f, 0.45f), false);
            var enerjiGostergesi = enerji.gameObject.AddComponent<EnerjiCubugu>();
            GriKumsalKur.Bagla(enerjiGostergesi, "enerji", nesne.GetComponent<Enerji>());
            GriKumsalKur.Bagla(enerjiGostergesi, "dolgu", enerji.transform.GetChild(0));
            Cubuk nefes = GriKumsalKur.CubukKur("NefesCubugu", tuval, new Vector2(0.03f, 0.08f), new Vector2(0.28f, 0.1f), new Color(0.5f, 0.8f, 1f), false);
            Cubuk kurtulma = GriKumsalKur.CubukKur("KurtulmaCubugu", tuval, new Vector2(0.42f, 0.42f), new Vector2(0.58f, 0.445f), Color.white, false);
            var gosterge = tuval.gameObject.AddComponent<DenizGostergesi>();
            GriKumsalKur.Bagla(gosterge, "yuzucu", yuzucu);
            GriKumsalKur.Bagla(gosterge, "nefes", nefes);
            GriKumsalKur.Bagla(gosterge, "kurtulma", kurtulma);
            return yuzucu;
        }

        private static void BolumKur(AdimTuru adim, DenizAyarlari ayarlar, DenizHareketi yuzucu, Transform baslangic, bool final)
        {
            yuzucu.transform.SetPositionAndRotation(baslangic.position, baslangic.rotation);
            var bolum = new GameObject("DenizBolumu").AddComponent<DenizBolumu>();
            var so = new SerializedObject(bolum);
            so.FindProperty("adim").enumValueIndex = (int)adim;
            so.FindProperty("finalKamerasi").boolValue = final;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(bolum, "ayarlar", ayarlar);
            GriKumsalKur.Bagla(bolum, "yuzucu", yuzucu);
            GriKumsalKur.Bagla(bolum, "baslangic", baslangic);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SistemlerKalibi));
        }

        private static Transform Nokta(string ad, Vector3 yer, Quaternion yon, Transform ebeveyn = null)
        {
            Transform nokta = GriKumsalKur.Bos(ad, ebeveyn);
            nokta.SetPositionAndRotation(yer, yon);
            return nokta;
        }

        /// <summary>Kıyıya paralel, denizi boydan boya kesen tetikleyici; yüzeyde sarı şamandıra.</summary>
        private static void Kontrol(string ad, float z, Quaternion yon, bool son)
        {
            GameObject alan = GriKumsalKur.Kutu(ad, null, new Vector3(0f, -10f, z), new Vector3(DenizGenisligi, 40f, 4f), isaret, "Default");
            alan.transform.rotation = yon;
            Object.DestroyImmediate(alan.GetComponent<MeshRenderer>());
            alan.GetComponent<Collider>().isTrigger = true;
            var nokta = alan.AddComponent<KontrolNoktasi>();
            var so = new SerializedObject(nokta);
            so.FindProperty("son").boolValue = son;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject samandira = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            samandira.name = "Samandira";
            Object.DestroyImmediate(samandira.GetComponent<Collider>());
            samandira.transform.position = new Vector3(4f, 0.2f, z);
            samandira.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            samandira.GetComponent<Renderer>().sharedMaterial = isaret;
        }

        private static void AkintiKur(Vector3 yer, Vector3 boyut, float yon, DenizTehlikeAyarlari tehlike)
        {
            GameObject alan = GriKumsalKur.Kutu("Akinti", null, yer, boyut, akinti, "Default");
            alan.transform.rotation = Quaternion.Euler(0f, yon, 0f);
            alan.GetComponent<Collider>().isTrigger = true;
            alan.isStatic = false;
            GriKumsalKur.Bagla(alan.AddComponent<Akinti>(), "ayarlar", tehlike);

            // Soluk oklar: akış yönünde ince çizgiler
            for (int i = -1; i <= 1; i++)
            {
                GameObject ok = GriKumsalKur.Kutu("Ok", alan.transform, new Vector3(i * 0.3f, 0f, 0f), new Vector3(0.02f, 0.02f, 0.3f), akinti, "Default");
                Object.DestroyImmediate(ok.GetComponent<Collider>());
            }
        }

        private static void YiyecekKur(Yiyecek.Turu tur, Vector3 yer, DenizTehlikeAyarlari tehlike)
        {
            GameObject nesne = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nesne.name = tur.ToString();
            nesne.transform.position = yer;
            nesne.transform.localScale = tur == Yiyecek.Turu.Denizanasi ? new Vector3(0.6f, 0.45f, 0.6f) : new Vector3(0.6f, 0.4f, 0.5f);
            nesne.GetComponent<Collider>().isTrigger = true;
            nesne.GetComponent<Renderer>().sharedMaterial = tur == Yiyecek.Turu.Denizanasi ? denizanasi : poset;
            var yiyecek = nesne.AddComponent<Yiyecek>();
            var so = new SerializedObject(yiyecek);
            so.FindProperty("tur").enumValueIndex = (int)tur;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(yiyecek, "ayarlar", tehlike);
        }

        private static GameObject HayaletAgKalibi()
        {
            var kok = new GameObject("HayaletAg");
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(4f, 3f, 0.1f), ag, "Trash");
            govde.isStatic = false;
            ((BoxCollider)govde.GetComponent<Collider>()).isTrigger = true;
            var so = new SerializedObject(govde.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.HayaletAg;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Kenarları hafif parlar: uzaktan seçilsin.
            var kenarlar = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                bool yatay = i < 2;
                Vector3 yer = yatay ? new Vector3(0f, i == 0 ? 1.5f : -1.5f, 0f) : new Vector3(i == 2 ? 2f : -2f, 0f, 0f);
                Vector3 boy = yatay ? new Vector3(4.1f, 0.08f, 0.12f) : new Vector3(0.08f, 3.1f, 0.12f);
                GameObject kenar = GriKumsalKur.Kutu($"Kenar_{i}", kok.transform, yer, boy, agKenari, "Trash");
                Object.DestroyImmediate(kenar.GetComponent<Collider>());
                kenar.isStatic = false;
                kenarlar[i] = kenar.GetComponent<Renderer>();
            }
            ParildatKur(kok, new Color(0.7f, 1f, 1f), kenarlar);
            return GriKumsalKur.KalipKaydet(kok, $"{KalipKlasoru}/HayaletAg.prefab");
        }

        private static GameObject MisinaKalibi()
        {
            var kok = new GameObject("Misina");
            kok.layer = LayerMask.NameToLayer("Trash");
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(6f, 0.03f, 0.03f), misina, "Trash");
            govde.isStatic = false;
            var kutu = (BoxCollider)govde.GetComponent<Collider>();
            kutu.isTrigger = true;
            // İnce çizgi; takılmak için çevresinde biraz pay
            kutu.size = new Vector3(1f, 20f, 20f);
            var so = new SerializedObject(govde.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.Misina;
            so.ApplyModifiedPropertiesWithoutUndo();
            ParildatKur(kok, new Color(0.95f, 0.95f, 0.8f), govde.GetComponent<Renderer>());
            return GriKumsalKur.KalipKaydet(kok, $"{KalipKlasoru}/Misina.prefab");
        }

        private static void ParildatKur(GameObject kok, Color renk, params Renderer[] parcalar)
        {
            var parilti = kok.AddComponent<Parilti>();
            GriKumsalKur.Bagla(parilti, "ayarlar", GriKumsalKur.Ayar<DenizTehlikeAyarlari>("DenizTehlikeAyarlari"));
            GriKumsalKur.DiziBagla(parilti, "parcalar", parcalar);
            var so = new SerializedObject(parilti);
            so.FindProperty("renk").colorValue = renk;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TekneKur(string ad, float z, bool isikli, DenizTehlikeAyarlari tehlike, KumsalDurumu kumsal)
        {
            Transform a = Nokta($"{ad}_A", new Vector3(-DenizGenisligi / 2f + 2f, 0.3f, z), Quaternion.identity);
            Transform b = Nokta($"{ad}_B", new Vector3(DenizGenisligi / 2f - 2f, 0.3f, z), Quaternion.identity);
            var kok = new GameObject(ad);
            kok.transform.position = a.position;
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(2f, 0.8f, 5f), tekne, "Default");
            Object.DestroyImmediate(govde.GetComponent<Collider>());
            govde.isStatic = false;

            // Pervane sesi: klip Abdü'den gelecek
            var ses = kok.AddComponent<AudioSource>();
            ses.loop = true;
            ses.spatialBlend = 1f;
            ses.playOnAwake = true;

            // Uyarı: suyun hemen altında teknenin önüne düşen gölge, arkasında titreyen pervane köpüğü
            GameObject golge = GriKumsalKur.Kutu("Golge", kok.transform, new Vector3(0f, -0.5f, 0f), new Vector3(3f, 0.02f, 7f), tekneGolgesi, "Default");
            Object.DestroyImmediate(golge.GetComponent<Collider>());
            golge.isStatic = false;
            var kopukler = new Object[3];
            for (int i = 0; i < kopukler.Length; i++)
            {
                GameObject parca = GriKumsalKur.Kutu($"Kopuk_{i}", kok.transform, new Vector3(0f, -0.2f, -3f - i * 1.5f), new Vector3(1.5f + i * 0.5f, 0.05f, 1.2f), kopuk, "Default");
                Object.DestroyImmediate(parca.GetComponent<Collider>());
                parca.isStatic = false;
                kopukler[i] = parca.transform;
            }

            var bilesen = kok.AddComponent<Tekne>();
            GriKumsalKur.Bagla(bilesen, "ayarlar", tehlike);
            GriKumsalKur.Bagla(bilesen, "a", a);
            GriKumsalKur.Bagla(bilesen, "b", b);
            GriKumsalKur.Bagla(bilesen, "golge", golge.transform);
            GriKumsalKur.DiziBagla(bilesen, "kopukler", kopukler);

            if (!isikli) return;
            IsikAlaniKur(kok.transform, Vector3.down * 0.5f, 10f, 6f, kumsal);
        }

        private static void SicakSuKur(Vector3 yer, float yaricap, DenizTehlikeAyarlari tehlike)
        {
            GameObject alan = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            alan.name = "SicakSu";
            alan.layer = LayerMask.NameToLayer("Hazard");
            alan.transform.position = yer;
            alan.transform.localScale = Vector3.one * yaricap * 2f;
            alan.GetComponent<Collider>().isTrigger = true;
            alan.GetComponent<Renderer>().sharedMaterial = sicakSu;
            var bolge = alan.AddComponent<HazardZone>();
            var so = new SerializedObject(bolge);
            so.FindProperty("tur").enumValueIndex = (int)HazardZone.Turu.SicakSu;
            so.ApplyModifiedPropertiesWithoutUndo();
            GriKumsalKur.Bagla(bolge, "denizAyarlari", tehlike);
        }

        private static void CayirKur(Vector3 yer, Vector3 boyut)
        {
            GameObject alan = GriKumsalKur.Kutu("DenizCayiri", null, yer, boyut, cayir, "Cover");
            alan.GetComponent<Collider>().isTrigger = true;
            alan.AddComponent<Cover>();
        }

        private static void AvciKur(Vector3 yer, DenizTehlikeAyarlari tehlike)
        {
            var kok = new GameObject("AvciBalik");
            kok.layer = LayerMask.NameToLayer("Predator");
            kok.transform.position = yer;
            GameObject govde = GriKumsalKur.Kutu("Govde", kok.transform, Vector3.zero, new Vector3(0.6f, 0.8f, 2f), avci, "Predator");
            Object.DestroyImmediate(govde.GetComponent<Collider>());
            govde.isStatic = false;
            GriKumsalKur.Bagla(kok.AddComponent<AvciBalik>(), "ayarlar", tehlike);
        }

        /// <summary>Parlayan plastik: çevresindeki yavruyu kendine çeker, değerse yavaşlatır.</summary>
        private static void PlastikKur(Vector3 yer, DenizTehlikeAyarlari tehlike, KumsalDurumu kumsal)
        {
            var kok = new GameObject("ParlayanPlastik");
            kok.transform.position = yer;
            IsikAlaniKur(kok.transform, Vector3.zero, 6f, 2f, kumsal);

            GameObject parca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            parca.name = "Parca";
            parca.layer = LayerMask.NameToLayer("Trash");
            parca.transform.SetParent(kok.transform, false);
            parca.transform.localScale = new Vector3(0.5f, 0.2f, 0.4f);
            parca.GetComponent<Collider>().isTrigger = true;
            parca.GetComponent<Renderer>().sharedMaterial = plastik;
            var so = new SerializedObject(parca.AddComponent<TrashTrap>());
            so.FindProperty("tur").enumValueIndex = (int)TrashTrap.Turu.Plastik;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Yasakla sönmeyen bir ışık alanı (ışıklı tekne, parlayan plastik).</summary>
        private static void IsikAlaniKur(Transform ebeveyn, Vector3 yer, float yaricap, float isikMenzili, KumsalDurumu kumsal)
        {
            Transform alan = GriKumsalKur.Bos("IsikAlani", ebeveyn);
            alan.localPosition = yer;
            alan.gameObject.layer = LayerMask.NameToLayer("Hazard");
            var kure = alan.gameObject.AddComponent<SphereCollider>();
            kure.isTrigger = true;
            kure.radius = yaricap;

            var lamba = alan.gameObject.AddComponent<Light>();
            lamba.type = LightType.Point;
            lamba.range = isikMenzili;
            lamba.intensity = 3f;
            lamba.color = new Color(1f, 0.95f, 0.75f);

            var isik = alan.gameObject.AddComponent<IsikKaynagi>();
            GriKumsalKur.Bagla(isik, "ayarlar", GriKumsalKur.Ayar<KacisAyarlari>("KacisAyarlari"));
            GriKumsalKur.Bagla(isik, "kumsal", kumsal != null ? kumsal : GriKumsalKur.Ayar<KumsalDurumu>("KumsalDurumu"));
            GriKumsalKur.Bagla(isik, "isik", lamba);
            var so = new SerializedObject(isik);
            so.FindProperty("yasaklanabilir").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Ayar asset'ini sadece adıyla bulur; yoksa oluşturur ve ilk değerlerini yazar.</summary>
        private static T AyarAdla<T>(string ad, Action<SerializedObject> ilkDegerler) where T : ScriptableObject
        {
            string yol = $"{AyarKlasoru}/{ad}.asset";
            var ayar = AssetDatabase.LoadAssetAtPath<T>(yol);
            if (ayar != null) return ayar;

            ayar = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ayar, yol);
            if (ilkDegerler != null)
            {
                var so = new SerializedObject(ayar);
                ilkDegerler(so);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return ayar;
        }
    }
}
