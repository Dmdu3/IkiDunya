using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Yüzeyde iki nokta arasında gidip gelen tekne. Uyarısı pervane sesi ve suyun üstündeki gölgesi; yüzeydeki kaplumbağaya çarpar.</summary>
    public class Tekne : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [SerializeField] private Transform a;
        [SerializeField] private Transform b;

        private readonly Collider[] bulunan = new Collider[8];
        private int kaplumbagaMaskesi;
        private bool bYeGidiyor;
        private float bekleme;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            transform.position = a.position;
            bYeGidiyor = true;
        }

        private void Update()
        {
            Vector3 hedef = bYeGidiyor ? b.position : a.position;
            Vector3 fark = hedef - transform.position;
            if (fark.magnitude < 0.5f) bYeGidiyor = !bYeGidiyor;
            else
            {
                transform.rotation = Quaternion.LookRotation(fark);
                transform.position += fark.normalized * Mathf.Min(ayarlar.TekneHizi * Time.deltaTime, fark.magnitude);
            }

            if (bekleme > 0f)
            {
                bekleme -= Time.deltaTime;
                return;
            }
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.TekneYaricapi, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IYuzucu yuzucu) || !yuzucu.Yuzeyde) continue;
                yuzucu.EnerjiKaybet(ayarlar.TekneHasari);
                bekleme = ayarlar.TekneBekleme;
            }
        }
    }
}
