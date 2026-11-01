using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>T ile açılıp kapanan termal görünüm: kumun üstüne her bölge ve bant için yarı saydam renkli bir şerit serer; her bandın ayrı deseni vardır (renk ayırt edemeyenler için).</summary>
    public class TermalGorunum : MonoBehaviour
    {
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Işıktan etkilenmeyen düz malzeme (URP Unlit); rengi şerit başına verilir.")]
        [SerializeField] private Material seritMalzemesi;
        [Tooltip("Şeritlerin kumun ne kadar üstünde durduğu (metre).")]
        [SerializeField] private float yukseklik = 0.05f;
        [Tooltip("Şeridin kumu izleyen ızgarasının aralığı (metre); küçüldükçe kuma daha iyi oturur.")]
        [SerializeField] private float agAraligi = 1f;

        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");
        private static readonly int DokuId = Shader.PropertyToID("_BaseMap");
        private static readonly int DokuOlcekId = Shader.PropertyToID("_BaseMap_ST");
        private static Texture2D[] desenler;

        /// <summary>Termal görünüm şu an açık mı; kaplumbağanın algısı gerçeği gösterirken bunu okur.</summary>
        public static bool AcikMi { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => AcikMi = false;

        private Transform kok;
        private Renderer[,] seritler;
        private float[,] seritOrtaZ;
        private MaterialPropertyBlock blok;

        public bool Acik => kok != null && kok.gameObject.activeSelf;

        private void Awake() => blok = new MaterialPropertyBlock();

        private void OnEnable() => kumsal.Degisti += Renklendir;

        private void OnDisable()
        {
            kumsal.Degisti -= Renklendir;
            AcikMi = false;
        }

        private void Update()
        {
            if (!GameInput.TermalBasildi) return;
            // Şeritler ilk açılışta kurulur: zemin (arazi) o zaman kesin hazırdır.
            if (kok == null) SeritleriKur();
            else kok.gameObject.SetActive(!Acik);
            if (Acik) Renklendir();
            AcikMi = Acik;
        }

        private void OnDestroy() => AcikMi = false;

        private void SeritleriKur()
        {
            kok = new GameObject("TermalSeritler").transform;
            kok.SetParent(kum.transform, false);

            float[] sinirlar = kum.Ayarlar.BantSinirlari;
            int bantSayisi = sinirlar.Length + 1;
            seritler = new Renderer[kum.BolgeSayisi, bantSayisi];
            seritOrtaZ = new float[kum.BolgeSayisi, bantSayisi];

            for (int b = 0; b < bantSayisi; b++)
            {
                float zBas = b == 0 ? 0f : Mathf.Min(sinirlar[b - 1], kum.Derinlik);
                float zSon = b < sinirlar.Length ? Mathf.Min(sinirlar[b], kum.Derinlik) : kum.Derinlik;
                if (zSon <= zBas) continue;

                for (int i = 0; i < kum.BolgeSayisi; i++)
                {
                    // Kumu izleyen ızgara: kıyı kıvrımlı ve zemin engebeli olabilir.
                    var serit = new GameObject($"Bolge{i}_Bant{b}", typeof(MeshFilter), typeof(MeshRenderer));
                    serit.transform.SetParent(kok, false);
                    float xBas = -kum.Uzunluk / 2f + kum.BolgeGenisligi * i;
                    serit.GetComponent<MeshFilter>().sharedMesh = SeritAgi(xBas, xBas + kum.BolgeGenisligi, zBas, zSon);

                    var renderer = serit.GetComponent<Renderer>();
                    renderer.sharedMaterial = seritMalzemesi;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    seritler[i, b] = renderer;
                    // Desen metre başına aynı boyda tekrarlansın.
                    float d = kum.Ayarlar.DesenBoyu;
                    renderer.GetPropertyBlock(blok);
                    blok.SetVector(DokuOlcekId, new Vector4(kum.BolgeGenisligi / d, (zSon - zBas) / d, 0f, 0f));
                    renderer.SetPropertyBlock(blok);
                    seritOrtaZ[i, b] = (zBas + zSon) / 2f;
                }
            }
        }

        /// <summary>Bölge ve bant için kumun üstüne serilen ızgara (yerel koordinat; z kıyıdan uzaklık).</summary>
        private Mesh SeritAgi(float xBas, float xSon, float zBas, float zSon)
        {
            int nx = Mathf.Max(1, Mathf.CeilToInt((xSon - xBas) / agAraligi));
            int nz = Mathf.Max(1, Mathf.CeilToInt((zSon - zBas) / agAraligi));
            var koseler = new Vector3[(nx + 1) * (nz + 1)];
            var uv = new Vector2[koseler.Length];
            var ucgenler = new int[nx * nz * 6];
            for (int z = 0, k = 0; z <= nz; z++)
                for (int x = 0; x <= nx; x++, k++)
                {
                    float yx = Mathf.Lerp(xBas, xSon, (float)x / nx);
                    float yz = kum.KiyiZ(yx) + Mathf.Lerp(zBas, zSon, (float)z / nz);
                    Vector3 dunya = kum.transform.TransformPoint(new Vector3(yx, 0f, yz));
                    float zemin = Zemin.Yukseklik(dunya, kum.transform.position.y);
                    koseler[k] = kok.InverseTransformPoint(new Vector3(dunya.x, zemin, dunya.z)) + Vector3.up * yukseklik;
                    uv[k] = new Vector2((float)x / nx, (float)z / nz);
                }
            for (int z = 0, t = 0; z < nz; z++)
                for (int x = 0; x < nx; x++, t += 6)
                {
                    int a = z * (nx + 1) + x;
                    int c = a + nx + 1;
                    ucgenler[t] = a; ucgenler[t + 1] = c; ucgenler[t + 2] = a + 1;
                    ucgenler[t + 3] = a + 1; ucgenler[t + 4] = c; ucgenler[t + 5] = c + 1;
                }
            var mesh = new Mesh { name = "TermalSerit", vertices = koseler, uv = uv, triangles = ucgenler };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void Renklendir()
        {
            if (!Acik) return;
            for (int i = 0; i < seritler.GetLength(0); i++)
                for (int b = 0; b < seritler.GetLength(1); b++)
                {
                    if (seritler[i, b] == null) continue;
                    SicaklikBandi bant = kum.Bant(i, seritOrtaZ[i, b]);
                    Color renk = kum.Ayarlar.Renk(bant);
                    renk.a = kum.Ayarlar.SeritOpakligi;
                    seritler[i, b].GetPropertyBlock(blok);
                    blok.SetColor(RenkId, renk);
                    blok.SetTexture(DokuId, Desen(bant));
                    seritler[i, b].SetPropertyBlock(blok);
                }
        }

        /// <summary>Bandın deseni: Mavi yatay çizgi, Yeşil nokta, Turuncu çapraz çizgi, Kırmızı dama.</summary>
        private static Texture2D Desen(SicaklikBandi bant)
        {
            if (desenler == null)
            {
                desenler = new Texture2D[4];
                for (int i = 0; i < desenler.Length; i++) desenler[i] = DesenKur((SicaklikBandi)i);
            }
            return desenler[(int)bant];
        }

        private static Texture2D DesenKur(SicaklikBandi bant)
        {
            const int boy = 16;
            var doku = new Texture2D(boy, boy) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            var acik = Color.white;
            var koyu = new Color(0.55f, 0.55f, 0.55f);
            for (int y = 0; y < boy; y++)
                for (int x = 0; x < boy; x++)
                {
                    bool cizgi;
                    switch (bant)
                    {
                        case SicaklikBandi.Mavi: cizgi = y % 8 < 2; break;
                        case SicaklikBandi.Yesil: cizgi = x % 8 < 3 && y % 8 < 3; break;
                        case SicaklikBandi.Turuncu: cizgi = (x + y) % 8 < 2; break;
                        default: cizgi = (x / 4 + y / 4) % 2 == 0; break;
                    }
                    doku.SetPixel(x, y, cizgi ? koyu : acik);
                }
            doku.Apply();
            return doku;
        }
    }
}
