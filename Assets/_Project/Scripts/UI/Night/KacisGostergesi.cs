using IkiDunya.Core;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.UI.Night
{
    /// <summary>Kaçış gecesinin göstergeleri: kumdan çıkışta kum kesiti, yüzey sıcaklığı ve enerji; koşuda gün doğumu.</summary>
    public class KacisGostergesi : MonoBehaviour
    {
        [SerializeField] private KacisGecesi gece;
        [SerializeField] private YavruAyarlari ayarlar;
        [SerializeField] private GunDogumu gunDogumu;
        [SerializeField] private TermalAyarlari termal;

        [Header("Kumdan çıkış")]
        [Tooltip("Kum altındayken ekranı kaplayan panel.")]
        [SerializeField] private GameObject cikisPaneli;
        [Tooltip("Dikey: üstte kalan kum katı.")]
        [SerializeField] private Cubuk kumKesiti;
        [Tooltip("Dikey: yüzey sıcaklığı; rengi termal bant rengi.")]
        [SerializeField] private Cubuk yuzeySicakligi;
        [SerializeField] private Cubuk cikisEnerjisi;

        [Header("Koşu")]
        [SerializeField] private Cubuk gunDogumuCubugu;

        private void Update()
        {
            bool cikis = gece.SuAnki == KacisGecesi.Asama.Cikis;
            bool kosu = gece.SuAnki == KacisGecesi.Asama.Kosu;
            if (cikisPaneli.activeSelf != cikis) cikisPaneli.SetActive(cikis);
            if (gunDogumuCubugu.gameObject.activeSelf != kosu) gunDogumuCubugu.gameObject.SetActive(kosu);

            if (cikis)
            {
                kumKesiti.Ayarla((float)gece.Kat / ayarlar.KatSayisi);
                yuzeySicakligi.Ayarla(gece.YuzeySicakligi);
                yuzeySicakligi.Renk(termal.Renk(Bant(gece.YuzeySicakligi)));
                cikisEnerjisi.Ayarla(gece.CikisEnerjisi);
            }
            else if (kosu)
            {
                gunDogumuCubugu.Ayarla(1f - gunDogumu.Oran);
            }
        }

        /// <summary>0–1 sıcaklığı dört banda böler; ölümcül sınırın üstü kırmızı.</summary>
        private SicaklikBandi Bant(float sicaklik)
        {
            if (sicaklik >= ayarlar.OlumculSicaklik) return SicaklikBandi.Kirmizi;
            // Ölümcül sınırın altı Mavi, Yeşil, Turuncu olarak üçe bölünür.
            int ust = (int)SicaklikBandi.Turuncu;
            return (SicaklikBandi)Mathf.Min(ust, (int)(sicaklik / ayarlar.OlumculSicaklik * (ust + 1)));
        }
    }
}
