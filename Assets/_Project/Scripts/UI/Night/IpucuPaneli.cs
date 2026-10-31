using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Ekranın altında ipucu kutusu: istenen ipuçları sırayla, her biri ayarlardaki süre kadar görünür. Geçiş ve duraklatma sırasında bekler.</summary>
    public class IpucuPaneli : MonoBehaviour
    {
        [SerializeField] private OyunAyarlari ayarlar;
        [SerializeField] private GameObject kutu;
        [SerializeField] private Text yazi;

        private readonly Queue<string> sira = new Queue<string>();
        private float kalan;

        private void OnEnable()
        {
            Ipucu.Istendi += Ekle;
            OyunAkisi.MenuyeDonuldu += Temizle;
            Temizle();
        }

        private void OnDisable()
        {
            Ipucu.Istendi -= Ekle;
            OyunAkisi.MenuyeDonuldu -= Temizle;
        }

        private void Ekle(string id) => sira.Enqueue(id);

        private void Temizle()
        {
            sira.Clear();
            kalan = 0f;
            if (kutu != null) kutu.SetActive(false);
        }

        private void Update()
        {
            // Duraklatmada zaman durur (deltaTime 0); geçişte ekran karanlık, ipucu beklesin.
            if (OyunAkisi.GecisSuruyor) return;

            if (kalan > 0f)
            {
                kalan -= Time.deltaTime;
                if (kalan <= 0f) kutu.SetActive(false);
                return;
            }
            if (sira.Count == 0) return;

            MetinTablosu tablo = ayarlar.Metinler != null ? ayarlar.Metinler : MetinTablosu.Yuklu;
            string id = sira.Dequeue();
            yazi.text = tablo != null ? tablo.Al(id) : $"[{id}]";
            kutu.SetActive(true);
            kalan = ayarlar.IpucuSuresi;
            Ses.Cal(SesTuru.Ipucu);
        }
    }
}
