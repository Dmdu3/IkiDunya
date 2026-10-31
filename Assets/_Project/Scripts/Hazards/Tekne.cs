using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Yüzeyde iki nokta arasında gidip gelen tekne. Uyarısı pervane sesi, önüne düşen gölgesi ve arkasında titreyen köpük; yüzeydeki kaplumbağaya çarpar.</summary>
    public class Tekne : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [SerializeField] private Transform a;
        [SerializeField] private Transform b;
        [Tooltip("Suyun hemen altındaki koyu leke; teknenin önüne taşınır.")]
        [SerializeField] private Transform golge;
        [Tooltip("Pervane köpüğü parçaları; titrer.")]
        [SerializeField] private Transform[] kopukler;

        private readonly Collider[] bulunan = new Collider[8];
        private int kaplumbagaMaskesi;
        private bool bYeGidiyor;
        private float bekleme;
        private Vector3[] kopukOlcekleri;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            transform.position = a.position;
            bYeGidiyor = true;
            if (golge != null) golge.localPosition = new Vector3(0f, golge.localPosition.y, ayarlar.TekneGolgeOnu);
            kopukOlcekleri = new Vector3[kopukler != null ? kopukler.Length : 0];
            for (int i = 0; i < kopukOlcekleri.Length; i++) kopukOlcekleri[i] = kopukler[i].localScale;
        }

        private void Start()
        {
            // Pervane sesi: kendi AudioSource'unda döngüyle; klip SesAyarlari'ndan (yoksa sessiz).
            if (!TryGetComponent(out AudioSource kaynak) || kaynak.clip != null) return;
            kaynak.clip = Ses.Klip(SesTuru.Pervane, out float duzey);
            kaynak.volume = duzey;
            kaynak.loop = true;
            if (kaynak.clip != null) kaynak.Play();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < kopukOlcekleri.Length; i++)
            {
                float titreme = 1f + ayarlar.KopukTitremeGenligi * Mathf.Sin(Time.time * ayarlar.KopukTitremeHizi + i * 1.7f);
                kopukler[i].localScale = new Vector3(kopukOlcekleri[i].x * titreme, kopukOlcekleri[i].y, kopukOlcekleri[i].z * titreme);
            }
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
