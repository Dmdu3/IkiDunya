using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Bölüm 5'in dalga hattı: denizden kıyıya ara ara dalga gelir. Yüzeye yakın yakaladığı yavruyu kıyıya savurur; altından dalan geçer.</summary>
    public class DenizDalgasi : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [Tooltip("Dalganın görünen tepesi (kıyıya paralel uzun kutu, çarpışmasız).")]
        [SerializeField] private Transform tepe;
        [Tooltip("Dalga hattının genişliği (metre).")]
        [SerializeField] private float genislik = 60f;

        private readonly Collider[] bulunan = new Collider[8];
        private readonly HashSet<IYuzucu> savrulanlar = new HashSet<IYuzucu>();
        private int kaplumbagaMaskesi;
        private float kiyidanUzaklik;
        private float bekleme;
        private bool geliyor;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            tepe.gameObject.SetActive(false);
            bekleme = ayarlar.DalgaAraligi;
        }

        private void Update()
        {
            if (!geliyor)
            {
                bekleme -= Time.deltaTime;
                if (bekleme > 0f) return;
                geliyor = true;
                kiyidanUzaklik = ayarlar.DalgaBaslangici;
                savrulanlar.Clear();
                tepe.gameObject.SetActive(true);
            }

            // Bu nesnenin konumu kıyı çizgisi; -z denize doğru.
            kiyidanUzaklik -= ayarlar.DalgaHizi * Time.deltaTime;
            tepe.localPosition = new Vector3(0f, 0f, -kiyidanUzaklik);
            if (kiyidanUzaklik <= 0f)
            {
                geliyor = false;
                bekleme = ayarlar.DalgaAraligi;
                tepe.gameObject.SetActive(false);
                return;
            }

            Vector3 merkez = transform.TransformPoint(new Vector3(0f, -ayarlar.DalgaDerinligi / 2f, -kiyidanUzaklik));
            Vector3 yari = new Vector3(genislik / 2f, ayarlar.DalgaDerinligi / 2f, 1f);
            int sayi = Physics.OverlapBoxNonAlloc(merkez, yari, bulunan, transform.rotation, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IYuzucu yuzucu) || !savrulanlar.Add(yuzucu)) continue;
                yuzucu.Savrul(transform.forward * ayarlar.DalgaSavrulmasi);
            }
        }
    }
}
