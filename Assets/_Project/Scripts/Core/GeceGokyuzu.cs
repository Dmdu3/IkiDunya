using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Gece gökyüzü: yıldızlar (kodla kurulan tek bir ağ, rastgele ama her seferinde aynı) ve ay; etkin kamerayı izler, uzakta durur.
    /// Gece açık, gündüz kapalı (OrtamIsigi açar/kapatır). Malzeme sissiz gökyüzü shader'ı (IkiDunya/Gokyuzu).</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GeceGokyuzu : MonoBehaviour
    {
        [SerializeField] private int yildizSayisi = 600;
        [Tooltip("Gökyüzü küresinin yarıçapı (kameranın uzak düzleminden küçük olmalı).")]
        [SerializeField] private float yaricap = 180f;
        [SerializeField] private float yildizBoyu = 0.5f;
        [Tooltip("Ufkun bu kadar altına yıldız konmaz (derece).")]
        [SerializeField] private float ufukPayi = 5f;
        [Tooltip("Ay (kendi küresi; bu objenin çocuğu).")]
        [SerializeField] private Transform ay;
        [SerializeField] private Vector3 ayYonu = new Vector3(-0.35f, 0.45f, -1f);
        [Header("Bulut (ayın önüne geçer)")]
        [Tooltip("Bulut tamken ayın kalan parlaklığı (0–1).")]
        [SerializeField] private float bulutluAy = 0.15f;
        [Tooltip("Bulut parçalarının rengi; alfa tam bulutun koyuluğu.")]
        [SerializeField] private Color bulutRengi = new Color(0.06f, 0.07f, 0.1f, 0.9f);
        [SerializeField] private int bulutParcasi = 7;
        [Tooltip("Bulutun saydam (alfa karışımlı) malzemesi; gökyüzü shader'ı eklemeli olduğu için koyu bulutu çizemez. Boşsa bulut yok, ay yalnızca söner.")]
        [SerializeField] private Material bulutMalzemesi;

        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");
        private static readonly int YuvarlakId = Shader.PropertyToID("_Yuvarlak");

        /// <summary>Gerçek ay (gökyüzü açıkken); yavrunun içgüdüsü sahte aylarla karşılaştırır.</summary>
        public static Transform Ay { get; private set; }

        /// <summary>Ayın önündeki bulut (0 açık – 1 kapalı). Kaplumbağanın algısı sahte ay görünürken artırır; ay ve denizdeki yolu söner.</summary>
        public static float Bulut { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Ay = null;
            Bulut = 0f;
        }

        private Renderer ayGorunus;
        private Transform bulut;
        private Renderer[] bulutlar;
        private MaterialPropertyBlock blok;
        private Color ayRengi = Color.white;
        private float uygulanan = -1f;

        private void OnEnable() => Ay = ay;

        private void OnDisable()
        {
            if (Ay == ay) Ay = null;
        }

        private void Awake()
        {
            GetComponent<MeshFilter>().sharedMesh = YildizAgi();
            blok = new MaterialPropertyBlock();
            // Yıldızlar yuvarlak: dörtgenin kenarı erir (kare benek gibi durmasın).
            var yildizlar = GetComponent<MeshRenderer>();
            yildizlar.GetPropertyBlock(blok);
            blok.SetFloat(YuvarlakId, 1f);
            yildizlar.SetPropertyBlock(blok);
            blok.Clear();
            if (ay == null) return;
            ay.localPosition = ayYonu.normalized * yaricap * 0.9f;
            ayGorunus = ay.GetComponent<Renderer>();
            if (ayGorunus != null && ayGorunus.sharedMaterial.HasProperty(RenkId)) ayRengi = ayGorunus.sharedMaterial.GetColor(RenkId);
            BulutKur();
        }

        private void LateUpdate()
        {
            // Gökyüzü hep kameranın etrafında: yürüdükçe yıldızlar kaymasın.
            UnityEngine.Camera kamera = UnityEngine.Camera.main;
            if (kamera != null) transform.position = kamera.transform.position;
            if (!Mathf.Approximately(uygulanan, Bulut)) BulutUygula();
        }

        /// <summary>Ayın yanında bekleyen birkaç yassı, koyu küre; bulut arttıkça ayın önüne kayar.</summary>
        private void BulutKur()
        {
            if (ayGorunus == null || bulutMalzemesi == null) return;
            bulut = new GameObject("Bulut").transform;
            bulut.SetParent(transform, false);
            bulutlar = new Renderer[bulutParcasi];
            var rastgele = new System.Random(77);
            float boy = ay.localScale.x;
            for (int i = 0; i < bulutParcasi; i++)
            {
                GameObject parca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                parca.name = "BulutParcasi";
                Destroy(parca.GetComponent<Collider>());
                parca.transform.SetParent(bulut, false);
                float x = ((float)rastgele.NextDouble() - 0.5f) * 2.6f * boy;
                float y = ((float)rastgele.NextDouble() - 0.5f) * 0.9f * boy;
                parca.transform.localPosition = new Vector3(x, y, 0f);
                parca.transform.localScale = new Vector3(boy * (1.4f + (float)rastgele.NextDouble()), boy * (0.6f + 0.4f * (float)rastgele.NextDouble()), boy * 0.2f);
                var r = parca.GetComponent<Renderer>();
                r.sharedMaterial = bulutMalzemesi;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                bulutlar[i] = r;
            }
        }

        private void BulutUygula()
        {
            uygulanan = Bulut;
            if (ayGorunus == null) return;
            float b = Mathf.SmoothStep(0f, 1f, Bulut);
            Color renk = ayRengi;
            renk.a *= Mathf.Lerp(1f, bulutluAy, b);
            ayGorunus.GetPropertyBlock(blok);
            blok.SetColor(RenkId, renk);
            ayGorunus.SetPropertyBlock(blok);

            if (bulut == null) return;
            // Bulut ayla kamera arasında, ayın yanından önüne kayar.
            Vector3 ayYeri = ay.localPosition;
            Vector3 yan = Vector3.Cross(ayYeri.normalized, Vector3.up).normalized;
            bulut.localPosition = ayYeri * 0.96f + yan * (1f - b) * ay.localScale.x * 3f;
            bulut.localRotation = Quaternion.LookRotation(-ayYeri.normalized);
            Color bulutRenk = bulutRengi;
            bulutRenk.a *= Mathf.Lerp(0.15f, 1f, b);
            foreach (Renderer r in bulutlar)
            {
                r.GetPropertyBlock(blok);
                blok.SetColor(RenkId, bulutRenk);
                r.SetPropertyBlock(blok);
            }
        }

        private Mesh YildizAgi()
        {
            var rastgele = new System.Random(1234);
            var koseler = new Vector3[yildizSayisi * 4];
            var renkler = new Color[koseler.Length];
            var uv = new Vector2[koseler.Length];
            var ucgenler = new int[yildizSayisi * 6];
            float altSinir = Mathf.Sin(-ufukPayi * Mathf.Deg2Rad);
            for (int i = 0; i < yildizSayisi; i++)
            {
                // Küre üstünde düzgün dağılım; ufkun altı boş.
                float y = Mathf.Lerp(altSinir, 1f, (float)rastgele.NextDouble());
                float aci = (float)rastgele.NextDouble() * Mathf.PI * 2f;
                float r = Mathf.Sqrt(1f - y * y);
                Vector3 yon = new Vector3(Mathf.Cos(aci) * r, y, Mathf.Sin(aci) * r);
                Vector3 merkez = yon * yaricap;
                Vector3 sag = Vector3.Cross(yon, Mathf.Abs(yon.y) > 0.99f ? Vector3.forward : Vector3.up).normalized;
                Vector3 ust = Vector3.Cross(sag, yon);
                float boy = yildizBoyu * (0.4f + 0.8f * (float)rastgele.NextDouble());
                float parlaklik = 0.35f + 0.65f * (float)rastgele.NextDouble();
                var renk = new Color(0.85f + 0.15f * parlaklik, 0.9f, 1f, parlaklik);

                int k = i * 4;
                koseler[k] = merkez + (-sag - ust) * boy;
                koseler[k + 1] = merkez + (sag - ust) * boy;
                koseler[k + 2] = merkez + (sag + ust) * boy;
                koseler[k + 3] = merkez + (-sag + ust) * boy;
                for (int j = 0; j < 4; j++) renkler[k + j] = renk;
                uv[k] = new Vector2(0f, 0f); uv[k + 1] = new Vector2(1f, 0f); uv[k + 2] = new Vector2(1f, 1f); uv[k + 3] = new Vector2(0f, 1f);
                int t = i * 6;
                ucgenler[t] = k; ucgenler[t + 1] = k + 1; ucgenler[t + 2] = k + 2;
                ucgenler[t + 3] = k; ucgenler[t + 4] = k + 2; ucgenler[t + 5] = k + 3;
            }
            var mesh = new Mesh { name = "Yildizlar", vertices = koseler, colors = renkler, uv = uv, triangles = ucgenler };
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * yaricap * 2f);
            return mesh;
        }
    }
}
