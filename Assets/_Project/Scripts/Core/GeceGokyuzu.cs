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

        private void Awake()
        {
            GetComponent<MeshFilter>().sharedMesh = YildizAgi();
            if (ay != null) ay.localPosition = ayYonu.normalized * yaricap * 0.9f;
        }

        private void LateUpdate()
        {
            // Gökyüzü hep kameranın etrafında: yürüdükçe yıldızlar kaymasın.
            UnityEngine.Camera kamera = UnityEngine.Camera.main;
            if (kamera != null) transform.position = kamera.transform.position;
        }

        private Mesh YildizAgi()
        {
            var rastgele = new System.Random(1234);
            var koseler = new Vector3[yildizSayisi * 4];
            var renkler = new Color[koseler.Length];
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
                int t = i * 6;
                ucgenler[t] = k; ucgenler[t + 1] = k + 1; ucgenler[t + 2] = k + 2;
                ucgenler[t + 3] = k; ucgenler[t + 4] = k + 2; ucgenler[t + 5] = k + 3;
            }
            var mesh = new Mesh { name = "Yildizlar", vertices = koseler, colors = renkler, triangles = ucgenler };
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * yaricap * 2f);
            return mesh;
        }
    }
}
