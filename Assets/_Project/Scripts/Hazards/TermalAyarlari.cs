using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kum sıcaklığı bantlarının yerleşimi, termal görünüm renkleri ve dalga çizgisi kayması (GDD: Bölüm 2).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Termal Ayarları", fileName = "TermalAyarlari")]
    public class TermalAyarlari : ScriptableObject
    {
        [Tooltip("Deniz kenarından karaya doğru bant sınırları (metre). Sınırdan önce Mavi, sonra Yeşil, Turuncu; sonuncudan sonra Kırmızı.")]
        [SerializeField] private float[] bantSinirlari = { 8f, 18f, 30f };

        [Tooltip("Mavi, Yeşil, Turuncu, Kırmızı sırasıyla.")]
        [SerializeField] private Color[] bantRenkleri =
        {
            new Color(0.2f, 0.45f, 1f),
            new Color(0.25f, 0.8f, 0.3f),
            new Color(1f, 0.55f, 0.1f),
            new Color(0.9f, 0.1f, 0.1f),
        };

        [Tooltip("Gölgelik olan bölgede kum kaç bant serinler (GDD: 1).")]
        [SerializeField] private int golgelikSerinletme = 1;

        [Header("Dalga çizgisi")]
        [Tooltip("Kumul onarımı olan bölgede dalga çizgisinin denize doğru çekildiği mesafe (GDD: 4 m).")]
        [SerializeField] private float kumulOnarimiGeriCekilme = 4f;

        public float[] BantSinirlari => bantSinirlari;
        public float KumulOnarimiGeriCekilme => kumulOnarimiGeriCekilme;
        public int GolgelikSerinletme => golgelikSerinletme;

        public Color Renk(SicaklikBandi bant) => bantRenkleri[(int)bant];
    }
}
