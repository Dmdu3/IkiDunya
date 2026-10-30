using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Yengeç deliği: ara ara kumu titretir (uyarı), sonra deliğin yanındaki bir yavruyu yakalar. Uzak geçen ya da üstünden atlayan kurtulur.</summary>
    public class Yengec : MonoBehaviour
    {
        [SerializeField] private AvciAyarlari ayarlar;
        [Tooltip("Uyarıda titreyen kum işareti.")]
        [SerializeField] private Transform titreyenKum;

        private readonly Collider[] bulunan = new Collider[16];
        private int kaplumbagaMaskesi;
        private bool uyariyor;
        private float sayac;
        private Vector3 kumYeri;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            kumYeri = titreyenKum.localPosition;
        }

        private void OnEnable()
        {
            uyariyor = false;
            sayac = ayarlar.YengecBekleme;
        }

        private void Update()
        {
            sayac -= Time.deltaTime;
            if (uyariyor)
                titreyenKum.localPosition = kumYeri + Random.insideUnitSphere * ayarlar.TitremeGenligi;

            if (sayac > 0f) return;

            if (!uyariyor)
            {
                uyariyor = true;
                sayac = ayarlar.YengecUyari;
                return;
            }

            uyariyor = false;
            titreyenKum.localPosition = kumYeri;
            sayac = ayarlar.YengecBekleme;
            Saldir();
        }

        private void Saldir()
        {
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.YengecYaricap, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IAvlanabilir av) || !av.Yakalanabilir || av.Havada) continue;
                av.Yakala(KayipNedeni.Yengec);
                return;
            }
        }
    }
}
