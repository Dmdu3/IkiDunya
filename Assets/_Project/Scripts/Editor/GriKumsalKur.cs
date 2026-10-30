using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using IkiDunya.UI.Night;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IkiDunya.Editor
{
    /// <summary>Gri kutu kumsal deneme sahnesini kurar: kum, deniz, kumul, dalga çizgisi, bölgeler, birkaç engel, termal görünüm ve birinci şahıs anne.</summary>
    public static class GriKumsalKur
    {
        private const string SahneYolu = "Assets/_Project/Scenes/Prototypes/GriKumsal.unity";
        private const string MalzemeKlasoru = "Assets/_Project/Scenes/Prototypes/GriKumsal";
        private const string AyarKlasoru = "Assets/_Project/ScriptableObjects/Settings";

        // Gri kutu ölçüleri (metre). Oyun değeri değil; sadece deneme sahnesinin yerleşimi.
        // Kıyı x ekseni boyunca uzanır; z = 0 deniz kenarı, z arttıkça karaya doğru.
        private const float KumsalUzunlugu = 80f;
        private const float KumsalDerinligi = 40f;
        private const float DenizDerinligi = 30f;
        private const float DalgaCizgisiZ = 6f;
        private const int BolgeSayisi = 8;

        [MenuItem("İki Dünya/Gri kutu kumsalı kur")]
        public static void Kur()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SahneYolu) != null &&
                !EditorUtility.DisplayDialog("Gri kutu kumsal", "GriKumsal sahnesi var. Baştan kurulsun mu?", "Kur", "Vazgeç"))
                return;

            KatmanKurulumu.Kur();
            if (!AssetDatabase.IsValidFolder(MalzemeKlasoru))
                AssetDatabase.CreateFolder("Assets/_Project/Scenes/Prototypes", "GriKumsal");

            Scene sahne = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (GameObject kok in sahne.GetRootGameObjects())
                if (kok.GetComponent<UnityEngine.Camera>() != null) Object.DestroyImmediate(kok);

            Material kum = Malzeme("Kum", new Color(0.86f, 0.78f, 0.6f));
            Material kumKoyu = Malzeme("KumKoyu", new Color(0.8f, 0.72f, 0.55f));
            Material deniz = Malzeme("Deniz", new Color(0.15f, 0.35f, 0.55f));
            Material taban = Malzeme("Taban", new Color(0.45f, 0.42f, 0.35f));
            Material beyaz = Malzeme("Beyaz", Color.white);
            Material engel = Malzeme("Engel", new Color(0.55f, 0.3f, 0.25f));
            Material cop = Malzeme("Cop", new Color(0.3f, 0.5f, 0.3f));
            Material kaplumbaga = Malzeme("Kaplumbaga", new Color(0.35f, 0.3f, 0.2f));

            var dunya = new GameObject("Dunya").transform;

            Kutu("Kum", dunya, new Vector3(0f, -0.5f, KumsalDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, KumsalDerinligi), kum, "Ground");
            Kutu("DenizTabani", dunya, new Vector3(0f, -2f, -DenizDerinligi / 2f), new Vector3(KumsalUzunlugu, 1f, DenizDerinligi), taban, "Ground");
            GameObject su = Kutu("Deniz", dunya, new Vector3(0f, -0.6f, -DenizDerinligi / 2f), new Vector3(KumsalUzunlugu, 0.1f, DenizDerinligi), deniz, "Water");
            Object.DestroyImmediate(su.GetComponent<Collider>());

            // Kumul: kumsalın arkasında alçak bir sıra
            var kumul = new GameObject("Kumul").transform;
            kumul.SetParent(dunya);
            for (int i = 0; i < 8; i++)
            {
                float x = -KumsalUzunlugu / 2f + 5f + i * 10f;
                Kutu($"Kumul_{i}", kumul, new Vector3(x, 0.8f, KumsalDerinligi - 3f), new Vector3(9f, 2.5f, 6f), kumKoyu, "Ground");
            }

            // Dalga çizgisi: kıyıya paralel kesik çizgi, çarpışmasız
            var dalga = new GameObject("DalgaCizgisi").transform;
            dalga.SetParent(dunya);
            for (float x = -KumsalUzunlugu / 2f + 1f; x < KumsalUzunlugu / 2f; x += 2f)
            {
                GameObject parca = Kutu("Parca", dalga, new Vector3(x, 0.01f, DalgaCizgisiZ), new Vector3(1f, 0.02f, 0.15f), beyaz, "Default");
                Object.DestroyImmediate(parca.GetComponent<Collider>());
            }

            // Bölgeler: kıyı boyunca eşit şeritler, ince ve çarpışmasız
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

            // Engeller: birkaç şezlong ve çöp yığını
            var engeller = new GameObject("Engeller").transform;
            engeller.SetParent(dunya);
            Kutu("Sezlong_0", engeller, new Vector3(-6f, 0.25f, 12f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            Kutu("Sezlong_1", engeller, new Vector3(-4f, 0.25f, 12f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            Kutu("Sezlong_2", engeller, new Vector3(5f, 0.25f, 18f), new Vector3(0.7f, 0.5f, 2f), engel, "Obstacle");
            Kutu("CopYigini_0", engeller, new Vector3(2f, 0.2f, 9f), new Vector3(1.2f, 0.4f, 1.2f), cop, "Trash");
            Kutu("CopYigini_1", engeller, new Vector3(-10f, 0.2f, 20f), new Vector3(1.2f, 0.4f, 1.2f), cop, "Trash");

            TermalKur(Malzeme("TermalSerit", Color.white, "Universal Render Pipeline/Unlit"));

            GameObject anne = AnneKur(kaplumbaga);
            EnerjiCubuguKur(anne.GetComponent<Enerji>());

            EditorSceneManager.SaveScene(sahne, SahneYolu);
            Debug.Log($"Gri kutu kumsal kuruldu: {SahneYolu}");
        }

        private static GameObject AnneKur(Material malzeme)
        {
            var anne = new GameObject("Anne");
            anne.layer = LayerMask.NameToLayer("Turtle");
            anne.transform.position = new Vector3(0f, 0.1f, 2f);

            var kontrol = anne.AddComponent<CharacterController>();
            kontrol.radius = 0.4f;
            kontrol.height = 0.8f;
            kontrol.center = new Vector3(0f, 0.4f, 0f);
            kontrol.stepOffset = 0.2f;

            anne.AddComponent<Enerji>();
            var kara = anne.AddComponent<AnneKara>();
            Bagla(kara, "ayarlar", Ayar<AnneAyarlari>("AnneAyarlari"));

            GameObject kabuk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kabuk.name = "Kabuk";
            Object.DestroyImmediate(kabuk.GetComponent<Collider>());
            kabuk.layer = anne.layer;
            kabuk.transform.SetParent(anne.transform, false);
            kabuk.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            kabuk.transform.localScale = new Vector3(0.9f, 0.35f, 1.1f);
            kabuk.GetComponent<Renderer>().sharedMaterial = malzeme;

            var goz = new GameObject("Goz");
            goz.transform.SetParent(anne.transform, false);
            goz.transform.localPosition = new Vector3(0f, 0.35f, 0.6f);
            goz.tag = "MainCamera";
            var kamera = goz.AddComponent<UnityEngine.Camera>();
            kamera.nearClipPlane = 0.05f;
            goz.AddComponent<AudioListener>();
            var bakis = goz.AddComponent<BirinciSahisKamera>();
            Bagla(bakis, "ayarlar", Ayar<KameraAyarlari>("KameraAyarlari"));
            Bagla(bakis, "govde", anne.transform);

            return anne;
        }

        private static void TermalKur(Material serit)
        {
            // Konumu deniz kenarının ortası; +z karaya doğru.
            var nesne = new GameObject("KumSicakligi");
            var kum = nesne.AddComponent<KumSicakligi>();
            var kumsal = Ayar<KumsalDurumu>("KumsalDurumu");
            Bagla(kum, "ayarlar", Ayar<TermalAyarlari>("TermalAyarlari"));
            Bagla(kum, "kumsal", kumsal);
            var so = new SerializedObject(kum);
            so.FindProperty("uzunluk").floatValue = KumsalUzunlugu;
            so.FindProperty("derinlik").floatValue = KumsalDerinligi;
            so.FindProperty("bolgeSayisi").intValue = BolgeSayisi;
            so.ApplyModifiedPropertiesWithoutUndo();

            var termal = nesne.AddComponent<TermalGorunum>();
            Bagla(termal, "kum", kum);
            Bagla(termal, "kumsal", kumsal);
            Bagla(termal, "seritMalzemesi", serit);
        }

        private static void EnerjiCubuguKur(Enerji enerji)
        {
            int uiKatmani = LayerMask.NameToLayer("UI");

            var tuval = new GameObject("Arayuz", typeof(Canvas), typeof(CanvasScaler));
            tuval.layer = uiKatmani;
            tuval.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var olcek = tuval.GetComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = new Vector2(1920f, 1080f);

            var arka = new GameObject("EnerjiCubugu", typeof(RectTransform), typeof(Image));
            arka.layer = uiKatmani;
            arka.transform.SetParent(tuval.transform, false);
            var arkaRect = (RectTransform)arka.transform;
            arkaRect.anchorMin = new Vector2(0.03f, 0.04f);
            arkaRect.anchorMax = new Vector2(0.28f, 0.065f);
            arkaRect.offsetMin = arkaRect.offsetMax = Vector2.zero;
            arka.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var dolgu = new GameObject("Dolgu", typeof(RectTransform), typeof(Image));
            dolgu.layer = uiKatmani;
            dolgu.transform.SetParent(arka.transform, false);
            var dolguRect = (RectTransform)dolgu.transform;
            dolguRect.anchorMin = Vector2.zero;
            dolguRect.anchorMax = Vector2.one;
            dolguRect.offsetMin = dolguRect.offsetMax = Vector2.zero;
            dolgu.GetComponent<Image>().color = new Color(0.4f, 0.8f, 0.45f);

            var cubuk = arka.AddComponent<EnerjiCubugu>();
            Bagla(cubuk, "enerji", enerji);
            Bagla(cubuk, "dolgu", dolguRect);
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
