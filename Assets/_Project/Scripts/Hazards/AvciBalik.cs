using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Avcı balık: bir merkezin çevresinde döner, gizli olmayan kaplumbağayı görünce kovalar; yakalarsa kontrol noktasına dönülür. Kaya arası ve deniz çayırı korur.</summary>
    public class AvciBalik : MonoBehaviour
    {
        private enum Durum { Geziyor, Kovaliyor, Bekliyor }

        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [Tooltip("Gezinme çemberinin yarıçapı (metre).")]
        [SerializeField] private float gezinmeYaricapi = 8f;

        private readonly Collider[] bulunan = new Collider[8];
        private int kaplumbagaMaskesi;
        private Vector3 merkez;
        private float aci;
        private float bekleme;
        private Durum durum;
        private IYuzucu av;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            merkez = transform.position;
        }

        private void Update()
        {
            switch (durum)
            {
                case Durum.Geziyor:
                    aci += ayarlar.AvciHizi / gezinmeYaricapi * Time.deltaTime;
                    Git(merkez + new Vector3(Mathf.Cos(aci), 0f, Mathf.Sin(aci)) * gezinmeYaricapi, ayarlar.AvciHizi);
                    Bak();
                    break;
                case Durum.Kovaliyor:
                    if (av == null || av.Gizli || Vector3.Distance(av.Konum, transform.position) > ayarlar.AvciMenzil * 1.5f)
                    {
                        Bekle();
                        break;
                    }
                    Git(av.Konum, ayarlar.AvciKosuHizi);
                    if (Vector3.Distance(av.Konum, transform.position) > ayarlar.AvciYakalamaMesafesi) break;
                    av.Yakala();
                    Geribildirim.Darbe(1f);
                    Bekle();
                    break;
                case Durum.Bekliyor:
                    bekleme -= Time.deltaTime;
                    Git(merkez, ayarlar.AvciHizi);
                    if (bekleme <= 0f) durum = Durum.Geziyor;
                    break;
            }
        }

        private void Bak()
        {
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.AvciMenzil, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IYuzucu aday) || aday.Gizli) continue;
                av = aday;
                durum = Durum.Kovaliyor;
                return;
            }
        }

        private void Bekle()
        {
            av = null;
            bekleme = ayarlar.AvciBekleme;
            durum = Durum.Bekliyor;
        }

        private void Git(Vector3 hedef, float hiz)
        {
            Vector3 fark = hedef - transform.position;
            if (fark.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(fark);
            transform.position += fark.normalized * Mathf.Min(hiz * Time.deltaTime, fark.magnitude);
        }
    }
}
