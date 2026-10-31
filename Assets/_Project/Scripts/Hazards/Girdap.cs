using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Bölüm 5 minigame'i (çöp girdabı): yüzücüyü yakalayıp döndürerek merkeze çeker. Dönen çöp halkasının bir yerinde boşluk var;
    /// sadece orada çekim yok, oradan dışarı yüzen kurtulur ve parkur biter. Halka boşluğu kendi yönünde (forward) taşır.</summary>
    public class Girdap : MonoBehaviour
    {
        [SerializeField] private ParkurAyarlari ayarlar;
        [Tooltip("Dönen çöp halkası; boşluk bu objenin ileri yönünde.")]
        [SerializeField] private Transform halka;

        private readonly Collider[] bulunan = new Collider[8];
        private int kaplumbagaMaskesi;
        private IYuzucu yuzucu;
        private bool iceride;
        private bool kurtuldu;

        private bool Suruyor => Parkur.SuruyorMu(ParkurTuru.CopGirdabi);

        private void Awake() => kaplumbagaMaskesi = LayerMask.GetMask("Turtle");

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;
        private void OnDisable() => OyunAkisi.AdimBasladi -= AdimBasladi;

        private void AdimBasladi(Adim adim)
        {
            kurtuldu = false;
            iceride = false;
        }

        private void Update()
        {
            halka.Rotate(0f, ayarlar.HalkaDonmeHizi * Time.deltaTime, 0f, Space.World);
            if (kurtuldu) return;

            if (yuzucu == null)
            {
                int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.GirdapCekimYaricapi, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < sayi && yuzucu == null; i++) bulunan[i].TryGetComponent(out yuzucu);
                if (yuzucu == null) return;
            }

            Vector3 fark = yuzucu.Konum - transform.position;
            fark.y = 0f;
            float uzaklik = fark.magnitude;

            // Yakalama alanının çok dışına çıktıysa (kontrol noktasına döndü) parkur yarıda kalır.
            if (uzaklik > ayarlar.GirdapCekimYaricapi * 1.5f)
            {
                if (Suruyor) Parkur.Birak(ParkurTuru.CopGirdabi);
                iceride = false;
                yuzucu = null;
                return;
            }
            if (uzaklik > ayarlar.GirdapCekimYaricapi) return;

            if (!Suruyor)
            {
                Parkur.Baslat(ParkurTuru.CopGirdabi);
                Ipucu.Goster("ipucu_girdap");
            }
            if (uzaklik < ayarlar.GirdapHalkaYaricapi) iceride = true;
            else if (iceride)
            {
                // Halkanın içinden dışına çıktı: sadece boşluktan çıkılabilir (başka yerde çekim yüzmekten güçlü).
                kurtuldu = true;
                iceride = false;
                Parkur.Bitir(ParkurTuru.CopGirdabi);
                return;
            }

            Vector3 disari = uzaklik > 0.01f ? fark / uzaklik : Vector3.forward;
            Vector3 donme = Vector3.Cross(Vector3.up, disari);
            bool boslukta = Mathf.Abs(Vector3.SignedAngle(halka.forward, disari, Vector3.up)) < ayarlar.CikisAcisi / 2f;
            Vector3 akim = donme * ayarlar.GirdapDonme + (boslukta ? Vector3.zero : -disari * ayarlar.GirdapCekim);
            yuzucu.Savrul(akim * Time.deltaTime);
        }
    }
}
