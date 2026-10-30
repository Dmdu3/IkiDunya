using System;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalda noktalar arasında gezen insan. Önündeki görüş konisine giren kaplumbağayı Gordu ile bildirir; koni yerde görünür.</summary>
    public class Insan : MonoBehaviour
    {
        [SerializeField] private InsanAyarlari ayarlar;
        [Tooltip("Sırayla gidilen noktalar.")]
        [SerializeField] private Transform[] noktalar;
        [SerializeField] private bool fenerli;
        [Tooltip("Yerdeki koninin malzemesi (yarı saydam).")]
        [SerializeField] private Material koniMalzemesi;

        private const int KoniDilimi = 16;

        /// <summary>Bir kaplumbağa görüldüğünde; görülen kaplumbağanın kökü.</summary>
        public static event Action<Transform> Gordu;

        private readonly Collider[] bulunan = new Collider[16];
        private int hedef;
        private float beklemeKalan;
        private int kaplumbagaMaskesi;
        private int engelMaskesi;

        private float Menzil => fenerli ? ayarlar.FenerliMenzil : ayarlar.GorusMenzili;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => Gordu = null;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            engelMaskesi = LayerMask.GetMask("Obstacle", "Structure");
            KoniKur();
        }

        private void Update()
        {
            Yuru();
            Bak();
        }

        private void Yuru()
        {
            if (noktalar == null || noktalar.Length == 0) return;
            if (beklemeKalan > 0f)
            {
                beklemeKalan -= Time.deltaTime;
                return;
            }

            Vector3 fark = noktalar[hedef].position - transform.position;
            fark.y = 0f;
            if (fark.magnitude < 0.1f)
            {
                hedef = (hedef + 1) % noktalar.Length;
                beklemeKalan = ayarlar.Bekleme;
                return;
            }

            Vector3 yon = fark.normalized;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(yon), 180f * Time.deltaTime);
            transform.position += yon * Mathf.Min(ayarlar.YurumeHizi * Time.deltaTime, fark.magnitude);
        }

        private void Bak()
        {
            Vector3 goz = transform.position + Vector3.up * ayarlar.GozYuksekligi;
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, Menzil, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                Vector3 hedefNokta = bulunan[i].bounds.center;
                Vector3 duz = hedefNokta - transform.position;
                duz.y = 0f;
                if (Vector3.Angle(transform.forward, duz) > ayarlar.GorusAcisi / 2f) continue;
                if (Physics.Linecast(goz, hedefNokta, engelMaskesi, QueryTriggerInteraction.Ignore)) continue;
                Gordu?.Invoke(bulunan[i].transform);
            }
        }

        private void KoniKur()
        {
            var koni = new GameObject("GorusKonisi", typeof(MeshFilter), typeof(MeshRenderer));
            koni.transform.SetParent(transform, false);
            koni.transform.localPosition = Vector3.up * 0.03f;

            var koseler = new Vector3[KoniDilimi + 2];
            var ucgenler = new int[KoniDilimi * 3];
            float yariAci = ayarlar.GorusAcisi / 2f;
            for (int i = 0; i <= KoniDilimi; i++)
            {
                float aci = Mathf.Deg2Rad * Mathf.Lerp(-yariAci, yariAci, (float)i / KoniDilimi);
                koseler[i + 1] = new Vector3(Mathf.Sin(aci), 0f, Mathf.Cos(aci)) * Menzil;
            }
            for (int i = 0; i < KoniDilimi; i++)
            {
                ucgenler[i * 3] = 0;
                ucgenler[i * 3 + 1] = i + 1;
                ucgenler[i * 3 + 2] = i + 2;
            }

            var mesh = new Mesh { name = "GorusKonisi", vertices = koseler, triangles = ucgenler };
            mesh.RecalculateNormals();
            koni.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = koni.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = koniMalzemesi;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
