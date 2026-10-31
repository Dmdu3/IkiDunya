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
        [SerializeField] private float yukseklik = 0.03f;

        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");
        private static readonly int DokuId = Shader.PropertyToID("_BaseMap");
        private static readonly int DokuOlcekId = Shader.PropertyToID("_BaseMap_ST");
        private static Texture2D[] desenler;

        private Transform kok;
        private Renderer[,] seritler;
        private float[,] seritOrtaZ;
        private MaterialPropertyBlock blok;

        public bool Acik => kok != null && kok.gameObject.activeSelf;

        private void Awake()
        {
            blok = new MaterialPropertyBlock();
            SeritleriKur();
            kok.gameObject.SetActive(false);
        }

        private void OnEnable() => kumsal.Degisti += Renklendir;
        private void OnDisable() => kumsal.Degisti -= Renklendir;

        private void Update()
        {
            if (!GameInput.TermalBasildi) return;
            kok.gameObject.SetActive(!Acik);
            if (Acik) Renklendir();
        }

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
                    GameObject serit = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    serit.name = $"Bolge{i}_Bant{b}";
                    Destroy(serit.GetComponent<Collider>());
                    serit.transform.SetParent(kok, false);
                    float x = -kum.Uzunluk / 2f + kum.BolgeGenisligi * (i + 0.5f);
                    serit.transform.localPosition = new Vector3(x, yukseklik, (zBas + zSon) / 2f);
                    serit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    serit.transform.localScale = new Vector3(kum.BolgeGenisligi, zSon - zBas, 1f);

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
