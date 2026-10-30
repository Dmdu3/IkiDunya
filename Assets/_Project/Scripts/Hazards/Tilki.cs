using System;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Tilki: bazı gecelerde kumsalın kenarında gezer. Konisinde siperde olmayan bir yavru görürse ona koşar, sürü panikle dağılır; yakalarsa geri döner.</summary>
    public class Tilki : MonoBehaviour
    {
        private enum Durum { Geziyor, Kosuyor, Donuyor }

        [SerializeField] private AvciAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Sırayla gezilen noktalar (kumsalın kenarı).")]
        [SerializeField] private Transform[] noktalar;
        [SerializeField] private Material koniMalzemesi;
        [Tooltip("Tilkinin görünen gövdesi; gelmediği gece kapanır.")]
        [SerializeField] private GameObject govde;
        [SerializeField] private float gozYuksekligi = 0.4f;

        /// <summary>Tilki bir yavru görüp koşmaya başladığında; tilkinin yeriyle.</summary>
        public static event Action<Vector3> SuruyuKorkuttu;

        private readonly Collider[] bulunan = new Collider[32];
        private int kaplumbagaMaskesi;
        private int engelMaskesi;
        private Durum durum;
        private bool geldi;
        private int hedefNokta;
        private float beklemeKalan;
        private IAvlanabilir av;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => SuruyuKorkuttu = null;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            engelMaskesi = LayerMask.GetMask("Obstacle", "Structure", "Ground");
            GorusKonisi.Ciz(transform, ayarlar.TilkiMenzil, ayarlar.TilkiAci, koniMalzemesi);
        }

        /// <summary>Her gece (nesne her açıldığında) tilkinin gelip gelmeyeceği yeniden belirlenir.</summary>
        private void OnEnable()
        {
            float ihtimal = ayarlar.TilkiIhtimali * (CitVar() ? ayarlar.TilkiCitiCarpani : 1f);
            geldi = UnityEngine.Random.value < ihtimal;
            govde.SetActive(geldi);
            transform.Find("GorusKonisi")?.gameObject.SetActive(geldi);
            durum = Durum.Geziyor;
            av = null;
            if (noktalar.Length > 0) transform.position = noktalar[0].position;
        }

        private bool CitVar()
        {
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.TilkiCiti)) return true;
            return false;
        }

        private void Update()
        {
            if (!geldi) return;

            switch (durum)
            {
                case Durum.Geziyor:
                    Gez();
                    Bak();
                    break;
                case Durum.Kosuyor:
                    Kos();
                    break;
                case Durum.Donuyor:
                    if (Git(noktalar[hedefNokta].position, ayarlar.TilkiYurumeHizi)) durum = Durum.Geziyor;
                    break;
            }
        }

        private void Gez()
        {
            if (beklemeKalan > 0f)
            {
                beklemeKalan -= Time.deltaTime;
                return;
            }
            if (!Git(noktalar[hedefNokta].position, ayarlar.TilkiYurumeHizi)) return;
            hedefNokta = (hedefNokta + 1) % noktalar.Length;
            beklemeKalan = ayarlar.TilkiBekleme;
        }

        private void Bak()
        {
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.TilkiMenzil, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IAvlanabilir aday) || !aday.Yakalanabilir || aday.Gizli) continue;
                if (!GorusKonisi.Goruyor(transform, bulunan[i].bounds.center, ayarlar.TilkiMenzil, ayarlar.TilkiAci, gozYuksekligi, engelMaskesi)) continue;
                av = aday;
                durum = Durum.Kosuyor;
                SuruyuKorkuttu?.Invoke(transform.position);
                return;
            }
        }

        private void Kos()
        {
            if (av == null || !av.Yakalanabilir)
            {
                durum = Durum.Donuyor;
                return;
            }
            if (!Git(av.Konum, ayarlar.TilkiKosuHizi)) return;
            av.Yakala(KayipNedeni.Tilki);
            av = null;
            durum = Durum.Donuyor;
        }

        /// <summary>Hedefe yürür; vardıysa true.</summary>
        private bool Git(Vector3 hedef, float hiz)
        {
            Vector3 fark = hedef - transform.position;
            fark.y = 0f;
            if (fark.magnitude < 0.3f) return true;
            Vector3 yon = fark.normalized;
            transform.rotation = Quaternion.LookRotation(yon);
            transform.position += yon * Mathf.Min(hiz * Time.deltaTime, fark.magnitude);
            return false;
        }
    }
}
