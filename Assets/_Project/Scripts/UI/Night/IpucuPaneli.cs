using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Ekranın altında ipucu kutusu: istenen ipuçları sırayla, her biri ayarlardaki süre kadar görünür. Bildirim hemen çıkar,
    /// görünen yazıyı keser (yarısı görünmeden kesilen ipucu bildirimden sonra yeniden çıkar, eski bildirim atılır). Geçiş ve duraklatma sırasında bekler.</summary>
    public class IpucuPaneli : MonoBehaviour
    {
        [SerializeField] private OyunAyarlari ayarlar;
        [SerializeField] private GameObject kutu;
        [SerializeField] private Text yazi;

        private struct Yazi
        {
            public string Id;
            public object[] Degerler;
            public bool Bildirim;
        }

        private readonly List<Yazi> sira = new List<Yazi>();
        private Yazi gosterilen;
        private float kalan;

        private void OnEnable()
        {
            Ipucu.Istendi += Ekle;
            Ipucu.Bildirildi += Bildir;
            OyunAkisi.MenuyeDonuldu += Temizle;
            Temizle();
        }

        private void OnDisable()
        {
            Ipucu.Istendi -= Ekle;
            Ipucu.Bildirildi -= Bildir;
            OyunAkisi.MenuyeDonuldu -= Temizle;
        }

        private void Ekle(string id) => sira.Add(new Yazi { Id = id });

        private void Bildir(string id, object[] degerler)
        {
            // Yeni olan eskisini geçersiz kılar: bekleyen bildirimler atılır, görünen yazı kesilir.
            sira.RemoveAll(y => y.Bildirim);
            // Yarısı bile görünmeden kesilen ipucu bildirimden sonra baştan gösterilir; çoğu okunduysa geri gelmez
            // (an geçmiş olur: kumdan çıkış ipucu koşuda çıkmasın).
            if (kalan > ayarlar.IpucuSuresi / 2f && !gosterilen.Bildirim) sira.Insert(0, gosterilen);
            kalan = 0f;
            sira.Insert(0, new Yazi { Id = id, Degerler = degerler, Bildirim = true });
        }

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
            gosterilen = sira[0];
            sira.RemoveAt(0);
            string metin = tablo != null ? tablo.Al(gosterilen.Id) : $"[{gosterilen.Id}]";
            if (gosterilen.Degerler != null && gosterilen.Degerler.Length > 0) metin = string.Format(metin, gosterilen.Degerler);
            yazi.text = metin;
            Boyutla();
            kutu.SetActive(true);
            kalan = gosterilen.Bildirim ? ayarlar.BildirimSuresi : ayarlar.IpucuSuresi;
            // Bildirimin sesini onu çağıran olay çalar (darbe, kayıp).
            if (!gosterilen.Bildirim) Ses.Cal(SesTuru.Ipucu);
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
