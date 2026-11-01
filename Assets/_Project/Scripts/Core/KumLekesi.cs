using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Kuma serilen yumuşak kenarlı leke (finaldeki kızgın kum): düzensiz bir daire, kumulun biçimine oturur, kenarı kuma karışır.
    /// Düz kutu yokuşta havada ya da kumun içinde kalıyordu.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class KumLekesi : MonoBehaviour
    {
        private static readonly int DokuId = Shader.PropertyToID("_BaseMap");
        private const int Halka = 6;
        private const int Dilim = 24;

        [SerializeField] private float yaricap = 2f;
        [Tooltip("Kenarın ne kadar düzensiz olduğu (yarıçapın oranı).")]
        [SerializeField, Range(0f, 0.5f)] private float duzensizlik = 0.3f;
        [Tooltip("Lekenin kumun ne kadar üstünde durduğu (titreşmesin diye).")]
        [SerializeField] private float yukseklik = 0.03f;

        private static Texture2D doku;
        private Mesh mesh;

        private void OnEnable()
        {
            // Kumsal sahnesinde zemin yüklendikten sonra kurulur; her açılışta yeniden serilir (kumul aynı ama ucuz).
            if (mesh == null) mesh = new Mesh { name = "KumLekesi" };
            Ser();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            var gorunus = GetComponent<MeshRenderer>();
            var blok = new MaterialPropertyBlock();
            gorunus.GetPropertyBlock(blok);
            blok.SetTexture(DokuId, Doku());
            gorunus.SetPropertyBlock(blok);
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        private void Ser()
        {
            // Her leke kendi biçiminde: konumdan türeyen tohum.
            float tohum = transform.position.x * 1.7f + transform.position.z * 3.1f;
            var koseler = new Vector3[1 + Halka * Dilim];
            var uv = new Vector2[koseler.Length];
            koseler[0] = Yerel(Vector3.zero);
            uv[0] = new Vector2(0.5f, 0.5f);
            for (int h = 1; h <= Halka; h++)
                for (int d = 0; d < Dilim; d++)
                {
                    float aci = d * Mathf.PI * 2f / Dilim;
                    float kenar = 1f + duzensizlik * (Mathf.PerlinNoise(tohum + Mathf.Cos(aci) * 1.3f, tohum + Mathf.Sin(aci) * 1.3f) - 0.5f) * 2f;
                    float oran = (float)h / Halka;
                    var yon = new Vector3(Mathf.Cos(aci), 0f, Mathf.Sin(aci));
                    int i = 1 + (h - 1) * Dilim + d;
                    koseler[i] = Yerel(yon * (yaricap * kenar * oran));
                    uv[i] = new Vector2(0.5f + yon.x * oran * 0.5f, 0.5f + yon.z * oran * 0.5f);
                }

            var ucgenler = new int[Dilim * 3 + (Halka - 1) * Dilim * 6];
            int u = 0;
            for (int d = 0; d < Dilim; d++)
            {
                ucgenler[u++] = 0;
                ucgenler[u++] = 1 + (d + 1) % Dilim;
                ucgenler[u++] = 1 + d;
            }
            for (int h = 1; h < Halka; h++)
                for (int d = 0; d < Dilim; d++)
                {
                    int ic = 1 + (h - 1) * Dilim, dis = 1 + h * Dilim;
                    int a = ic + d, b = ic + (d + 1) % Dilim, c = dis + d, e = dis + (d + 1) % Dilim;
                    ucgenler[u++] = a; ucgenler[u++] = b; ucgenler[u++] = c;
                    ucgenler[u++] = b; ucgenler[u++] = e; ucgenler[u++] = c;
                }

            mesh.Clear();
            mesh.vertices = koseler;
            mesh.uv = uv;
            mesh.triangles = ucgenler;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        /// <summary>Yatay ofsetteki kum noktası, nesnenin yerel uzayında.</summary>
        private Vector3 Yerel(Vector3 yatay)
        {
            Vector3 dunya = transform.position + yatay;
            dunya.y = Zemin.Yukseklik(dunya, transform.position.y) + yukseklik;
            return transform.InverseTransformPoint(dunya);
        }

        /// <summary>Ortası dolu, kenara doğru eriyen yuvarlak doku (bütün lekeler paylaşır).</summary>
        private static Texture2D Doku()
        {
            if (doku != null) return doku;
            const int boy = 64;
            doku = new Texture2D(boy, boy, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "KumLekesi" };
            var piksel = new Color[boy * boy];
            for (int y = 0; y < boy; y++)
                for (int x = 0; x < boy; x++)
                {
                    float dx = (x + 0.5f) / boy * 2f - 1f;
                    float dy = (y + 0.5f) / boy * 2f - 1f;
                    float uzaklik = Mathf.Sqrt(dx * dx + dy * dy);
                    piksel[y * boy + x] = new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 1f, uzaklik)));
                }
            doku.SetPixels(piksel);
            doku.Apply();
            return doku;
        }
    }
}
