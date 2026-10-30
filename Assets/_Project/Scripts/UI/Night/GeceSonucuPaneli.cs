using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Gecenin sonu: yuva gecesinde yuvanın bandı ve yumurta sayısı, kaçışta kurtulan, kaybedilen, dişi ve erkek. Şimdilik sadece sayı ve renk; yazılar metinler.md'ye girince eklenecek.</summary>
    public class GeceSonucuPaneli : MonoBehaviour
    {
        [SerializeField] private YuvaGecesi yuvaGecesi;
        [SerializeField] private KacisGecesi kacisGecesi;
        [SerializeField] private TermalAyarlari termal;

        [Header("Yuva gecesi")]
        [SerializeField] private GameObject yuvaPaneli;
        [Tooltip("Yuvanın sıcaklık bandı rengi.")]
        [SerializeField] private Image yuvaBandi;
        [SerializeField] private Text yumurtaSayisi;

        [Header("Kaçış gecesi")]
        [SerializeField] private GameObject kacisPaneli;
        [SerializeField] private Text kurtulan;
        [SerializeField] private Text kaybedilen;
        [SerializeField] private Text disi;
        [SerializeField] private Text erkek;

        private void OnEnable()
        {
            if (yuvaGecesi != null)
            {
                yuvaGecesi.SonucHazir += YuvaSonucu;
                yuvaGecesi.SonucKapandi += Kapat;
            }
            if (kacisGecesi != null)
            {
                kacisGecesi.SonucHazir += KacisSonucu;
                kacisGecesi.SonucKapandi += Kapat;
            }
            Kapat();
        }

        private void OnDisable()
        {
            if (yuvaGecesi != null)
            {
                yuvaGecesi.SonucHazir -= YuvaSonucu;
                yuvaGecesi.SonucKapandi -= Kapat;
            }
            if (kacisGecesi != null)
            {
                kacisGecesi.SonucHazir -= KacisSonucu;
                kacisGecesi.SonucKapandi -= Kapat;
            }
        }

        private void YuvaSonucu(Yuva yuva)
        {
            if (yuva == null || yuvaPaneli == null) return;
            yuvaBandi.color = termal.Renk(yuva.Bant);
            yumurtaSayisi.text = yuva.Yumurta.ToString();
            yuvaPaneli.SetActive(true);
        }

        private void KacisSonucu(KacisGecesi.Sonuc sonuc)
        {
            if (kacisPaneli == null) return;
            kurtulan.text = sonuc.Kurtulan.ToString();
            kaybedilen.text = sonuc.Kaybedilen.ToString();
            disi.text = sonuc.Disi.ToString();
            erkek.text = sonuc.Erkek.ToString();
            kacisPaneli.SetActive(true);
        }

        private void Kapat()
        {
            if (yuvaPaneli != null) yuvaPaneli.SetActive(false);
            if (kacisPaneli != null) kacisPaneli.SetActive(false);
        }
    }
}
