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

        /// <summary>Bir kaplumbağa görüldüğünde; görülen kaplumbağanın kökü.</summary>
        public static event Action<Transform> Gordu;

        private readonly Collider[] bulunan = new Collider[16];
        private Vector3[] yol;
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
            GorusKonisi.Ciz(transform, Menzil, ayarlar.GorusAcisi, koniMalzemesi);
            if (yol == null && noktalar != null)
            {
                yol = new Vector3[noktalar.Length];
                for (int i = 0; i < noktalar.Length; i++) yol[i] = noktalar[i].position;
            }
        }

        /// <summary>Sahnede sonradan kurulan insanın gidip geldiği iki nokta.</summary>
        public void Kur(Vector3 a, Vector3 b)
        {
            yol = new[] { a, b };
            hedef = 1;
        }

        private void Update()
        {
            Yuru();
            Bak();
        }

        private void Yuru()
        {
            if (yol == null || yol.Length == 0) return;
            if (beklemeKalan > 0f)
            {
                beklemeKalan -= Time.deltaTime;
                return;
            }

            Vector3 fark = yol[hedef] - transform.position;
            fark.y = 0f;
            if (fark.magnitude < 0.1f)
            {
                hedef = (hedef + 1) % yol.Length;
                beklemeKalan = ayarlar.Bekleme;
                return;
            }

            Vector3 yon = fark.normalized;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(yon), 180f * Time.deltaTime);
            transform.position += yon * Mathf.Min(ayarlar.YurumeHizi * Time.deltaTime, fark.magnitude);
        }

        private void Bak()
        {
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, Menzil, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
                if (GorusKonisi.Goruyor(transform, bulunan[i].bounds.center, Menzil, ayarlar.GorusAcisi, ayarlar.GozYuksekligi, engelMaskesi))
                    Gordu?.Invoke(bulunan[i].transform);
        }
    }
}
