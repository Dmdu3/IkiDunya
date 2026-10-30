using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Martı: bir yavrunun üstüne gölgesi düşer (uyarı), sonra dalar. Toplu sürüden çok, dağınık sürüden bir yavru alır.</summary>
    public class Marti : MonoBehaviour
    {
        [SerializeField] private AvciAyarlari ayarlar;
        [Tooltip("Kuma düşen gölge; dalış yeri burasıdır.")]
        [SerializeField] private Transform golge;

        private readonly Collider[] bulunan = new Collider[64];
        private readonly List<IAvlanabilir> adaylar = new List<IAvlanabilir>();
        private int kaplumbagaMaskesi;
        private bool golgeVar;
        private float sayac;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
        }

        private void OnEnable()
        {
            golgeVar = false;
            golge.gameObject.SetActive(false);
            sayac = ayarlar.MartiBekleme;
        }

        private void Update()
        {
            sayac -= Time.deltaTime;
            if (sayac > 0f) return;

            if (!golgeVar) GolgeyiDusur();
            else Dal();
        }

        private void GolgeyiDusur()
        {
            Adaylar(transform.position, ayarlar.MartiAramaYaricapi);
            if (adaylar.Count == 0)
            {
                sayac = 1f;
                return;
            }

            Vector3 hedef = adaylar[Random.Range(0, adaylar.Count)].Konum;
            golge.position = new Vector3(hedef.x, hedef.y + 0.02f, hedef.z);
            golge.gameObject.SetActive(true);
            golgeVar = true;
            sayac = ayarlar.MartiUyari;
        }

        private void Dal()
        {
            golgeVar = false;
            golge.gameObject.SetActive(false);
            sayac = ayarlar.MartiBekleme;

            Adaylar(golge.position, ayarlar.MartiDalisYaricapi);
            int alinacak = adaylar.Count >= ayarlar.MartiTopluEsigi ? ayarlar.MartiTopluAv : 1;
            for (int i = 0; i < adaylar.Count && i < alinacak; i++) adaylar[i].Yakala(KayipNedeni.Marti);
        }

        private void Adaylar(Vector3 merkez, float yaricap)
        {
            adaylar.Clear();
            int sayi = Physics.OverlapSphereNonAlloc(merkez, yaricap, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
                if (bulunan[i].TryGetComponent(out IAvlanabilir av) && av.Yakalanabilir) adaylar.Add(av);
        }
    }
}
