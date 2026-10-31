using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Su yüzeyi: kendi ızgara mesh'ini kurar ve tepelerini iki dalga dizisiyle yavaşça oynatır. Görünüş içindir, çarpışma yoktur.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SuDalgalanmasi : MonoBehaviour
    {
        [Tooltip("Yüzeyin x ve z boyu (metre).")]
        [SerializeField] private Vector2 boyut = new Vector2(60f, 200f);
        [Tooltip("Izgara aralığı (metre); küçüldükçe daha yumuşak ama daha pahalı.")]
        [SerializeField] private float aralik = 2f;
        [SerializeField] private float yukseklik = 0.15f;
        [SerializeField] private float dalgaBoyu = 7f;
        [SerializeField] private float hiz = 1.2f;

        private Mesh mesh;
        private Vector3[] ilk;
        private Vector3[] koseler;

        private void Awake()
        {
            int nx = Mathf.Max(1, Mathf.RoundToInt(boyut.x / aralik));
            int nz = Mathf.Max(1, Mathf.RoundToInt(boyut.y / aralik));
            ilk = new Vector3[(nx + 1) * (nz + 1)];
            var uv = new Vector2[ilk.Length];
            var ucgenler = new int[nx * nz * 6];
            for (int z = 0, k = 0; z <= nz; z++)
                for (int x = 0; x <= nx; x++, k++)
                {
                    ilk[k] = new Vector3(-boyut.x / 2f + boyut.x * x / nx, 0f, -boyut.y / 2f + boyut.y * z / nz);
                    uv[k] = new Vector2((float)x / nx, (float)z / nz);
                }
            for (int z = 0, t = 0; z < nz; z++)
                for (int x = 0; x < nx; x++, t += 6)
                {
                    int a = z * (nx + 1) + x;
                    int b = a + nx + 1;
                    ucgenler[t] = a; ucgenler[t + 1] = b; ucgenler[t + 2] = a + 1;
                    ucgenler[t + 3] = a + 1; ucgenler[t + 4] = b; ucgenler[t + 5] = b + 1;
                }

            koseler = (Vector3[])ilk.Clone();
            mesh = new Mesh { name = "SuYuzeyi" };
            if (ilk.Length > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = koseler;
            mesh.uv = uv;
            mesh.triangles = ucgenler;
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void Update()
        {
            float t = Time.time * hiz;
            float k = 2f * Mathf.PI / dalgaBoyu;
            for (int i = 0; i < ilk.Length; i++)
            {
                Vector3 p = ilk[i];
                p.y = yukseklik * (Mathf.Sin(p.z * k + t) + 0.5f * Mathf.Sin((p.x + p.z) * k * 0.7f + t * 1.3f));
                koseler[i] = p;
            }
            mesh.vertices = koseler;
            mesh.RecalculateNormals();
        }
    }
}
