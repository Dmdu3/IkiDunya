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
            // Gece sonucu ekrandayken ipucu panelin arkasında kalmasın: gizlenir, süresi işlemez; panel kapanınca kaldığı yerden görünür.
            if (GeceSonucuPaneli.Acik)
            {
                if (kutu.activeSelf) kutu.SetActive(false);
                return;
            }

            if (kalan > 0f)
            {
                if (!kutu.activeSelf) kutu.SetActive(true);
                kalan -= Time.deltaTime;
                if (kalan <= 0f) kutu.SetActive(false);
                return;
            }
            if (sira.Count == 0) return;

            MetinTablosu tablo = ayarlar.Metinler != null ? ayarlar.Metinler : MetinTablosu.Yuklu;
            string id = sira.Dequeue();
            yazi.text = tablo != null ? tablo.Al(id) : $"[{id}]";
            Boyutla();
            kutu.SetActive(true);
            kalan = ayarlar.IpucuSuresi;
            Ses.Cal(SesTuru.Ipucu);
        }

        /// <summary>Kutu yazıyı tam alacak boyda; genişliği iki yandaki panellere (yönetimde sol panel, 400 birim) değmeyecek kadar.</summary>
        private void Boyutla()
        {
            var tuval = (RectTransform)transform;
            var alan = (RectTransform)kutu.transform;
            var yaziAlani = (RectTransform)yazi.transform;
            // Yönetim ekranında iki yanda paneller var; başka yerde kutu daha geniş olabilir.
            bool yanPaneller = !OyunAkisi.MenudeMi && OyunAkisi.SuAnkiAdim.Tur == AdimTuru.InsanTuru;
            float genislik = Mathf.Clamp(tuval.rect.width - 2f * (yanPaneller ? YanPay : DarYanPay), EnDar, EnGenis);
            float icPay = -yaziAlani.sizeDelta.x;
            TextGenerationSettings ayar = yazi.GetGenerationSettings(new Vector2(genislik - icPay, 0f));
            ayar.resizeTextForBestFit = false;
            float yukseklik = yazi.cachedTextGeneratorForLayout.GetPreferredHeight(yazi.text, ayar) / yazi.pixelsPerUnit;
            alan.sizeDelta = new Vector2(genislik, Mathf.Max(EnAzYukseklik, yukseklik - yaziAlani.sizeDelta.y));
        }

        private const float YanPay = 440f, DarYanPay = 180f, EnDar = 440f, EnGenis = 900f, EnAzYukseklik = 72f;
    }
}
