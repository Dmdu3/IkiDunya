using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Su altında yüzeyden aşağı inen ışık hüzmeleri. Hüzmeler dünyada sabit durur, oyuncunun çevresinde tekrarlanır
    /// (uzaklaştıkça söner, öbür taraftan belirir); oyuncu suyun üstündeyken görünmez. Malzeme sissiz, toplamalı shader (IkiDunya/Gokyuzu).</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SuIsiklari : MonoBehaviour
    {
        [Tooltip("Hüzmelerin çevresinde göründüğü obje (yüzücünün gözü).")]
        [SerializeField] private Transform hedef;
        [SerializeField] private int huzmeSayisi = 16;
        [Tooltip("Hüzmelerin tekrarlandığı karenin kenarı; hüzmeler bu alana dağılır.")]
        [SerializeField] private float alan = 40f;
        [SerializeField] private float suYuzeyi = 0f;
        [SerializeField] private float boy = 16f;
        [SerializeField] private float genislik = 1.6f;
        [Tooltip("Hüzmenin yana yatması (güneş açısı).")]
        [SerializeField] private Vector3 egim = new Vector3(0.25f, 0f, 0.15f);
        [SerializeField] private Color renk = new Color(0.75f, 0.95f, 1f, 0.09f);
        [Tooltip("Parlaklığın dalgalanma hızı.")]
        [SerializeField] private float titremeHizi = 0.6f;

        private Mesh ag;
        private Vector3[] koseler;
        private Color[] renkler;
        private Vector2[] yerler;
        private float[] evreler;
        private MeshRenderer gorunus;

        private void Awake()
        {
            gorunus = GetComponent<MeshRenderer>();
            var rastgele = new System.Random(4321);
            yerler = new Vector2[huzmeSayisi];
            evreler = new float[huzmeSayisi];
            for (int i = 0; i < huzmeSayisi; i++)
            {
                yerler[i] = new Vector2((float)rastgele.NextDouble() * alan, (float)rastgele.NextDouble() * alan);
                evreler[i] = (float)rastgele.NextDouble() * Mathf.PI * 2f;
            }

            // Her hüzme çapraz iki dikdörtgen (her yönden görünsün diye); köşeleri her kare yeniden yazılır.
            koseler = new Vector3[huzmeSayisi * 8];
            renkler = new Color[koseler.Length];
            var ucgenler = new int[huzmeSayisi * 12];
            for (int i = 0; i < huzmeSayisi * 2; i++)
            {
                int k = i * 4, t = i * 6;
                ucgenler[t] = k; ucgenler[t + 1] = k + 2; ucgenler[t + 2] = k + 1;
                ucgenler[t + 3] = k; ucgenler[t + 4] = k + 3; ucgenler[t + 5] = k + 2;
            }
            ag = new Mesh { name = "SuIsiklari" };
            ag.MarkDynamic();
            ag.vertices = koseler;
            ag.colors = renkler;
            ag.triangles = ucgenler;
            GetComponent<MeshFilter>().sharedMesh = ag;
        }

        private void OnDestroy()
        {
            if (ag != null) Destroy(ag);
        }

        private void LateUpdate()
        {
            if (hedef == null) return;
            bool suAltinda = hedef.position.y < suYuzeyi;
            gorunus.enabled = suAltinda;
            if (!suAltinda) return;

            Vector3 merkez = new Vector3(hedef.position.x, suYuzeyi, hedef.position.z);
            transform.SetPositionAndRotation(merkez, Quaternion.identity);
            float yari = alan / 2f;
            Vector3 asagi = (Vector3.down + egim).normalized * boy;
            for (int i = 0; i < huzmeSayisi; i++)
            {
                // Dünyadaki sabit yer, oyuncunun çevresindeki kareye sarılır.
                float x = Mathf.Repeat(yerler[i].x - merkez.x, alan) - yari;
                float z = Mathf.Repeat(yerler[i].y - merkez.z, alan) - yari;
                // Karenin kenarına yaklaştıkça söner: sarılırken birden belirmez.
                float uzaklik = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) / yari;
                float parlaklik = (1f - uzaklik * uzaklik) * (0.6f + 0.4f * Mathf.Sin(Time.time * titremeHizi + evreler[i]));
                Color ust = renk * new Color(1f, 1f, 1f, Mathf.Max(0f, parlaklik));
                Color alt = new Color(ust.r, ust.g, ust.b, 0f);

                var tepe = new Vector3(x, 0f, z);
                for (int yuz = 0; yuz < 2; yuz++)
                {
                    Vector3 yan = (yuz == 0 ? Vector3.right : Vector3.forward) * genislik / 2f;
                    int k = (i * 2 + yuz) * 4;
                    koseler[k] = tepe - yan;
                    koseler[k + 1] = tepe + yan;
                    koseler[k + 2] = tepe + yan * 1.8f + asagi;
                    koseler[k + 3] = tepe - yan * 1.8f + asagi;
                    renkler[k] = renkler[k + 1] = ust;
                    renkler[k + 2] = renkler[k + 3] = alt;
                }
            }
            ag.vertices = koseler;
            ag.colors = renkler;
            ag.RecalculateBounds();
        }
    }
}
