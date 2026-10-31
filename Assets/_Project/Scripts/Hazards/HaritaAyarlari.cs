using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>İnsan turunda kumsalın üstünde görünen kayıp izi simgelerinin ayarları: kanıt türüne göre renk, boy, yükseklik.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Harita Ayarları", fileName = "HaritaAyarlari")]
    public class HaritaAyarlari : ScriptableObject
    {
        [Tooltip("Isı, Işık, Çöp, Avcı, Su sırasıyla.")]
        [SerializeField] private Color[] kanitRenkleri =
        {
            new Color(0.95f, 0.35f, 0.15f),
            new Color(1f, 0.9f, 0.3f),
            new Color(0.45f, 0.75f, 0.35f),
            new Color(0.6f, 0.35f, 0.75f),
            new Color(0.25f, 0.55f, 0.95f),
        };
        [Tooltip("Kanıtı açılmış simgenin rengi.")]
        [SerializeField] private Color acilmisRenk = new Color(0.4f, 0.4f, 0.4f);
        [Tooltip("Simgenin çapı (metre); tepeden görülecek kadar büyük.")]
        [SerializeField] private float simgeBoyu = 2.5f;
        [SerializeField] private float simgeYuksekligi = 0.5f;

        public Color Renk(KanitTuru kanit) => kanitRenkleri[(int)kanit];
        public Color AcilmisRenk => acilmisRenk;
        public float SimgeBoyu => simgeBoyu;
        public float SimgeYuksekligi => simgeYuksekligi;
    }
}
