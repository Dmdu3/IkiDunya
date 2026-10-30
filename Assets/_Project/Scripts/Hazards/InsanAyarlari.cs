using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Yuva gecesinde kumsalda gezen insanların ayarları: yürüyüş ve görüş konisi (GDD: Bölüm 2, İnsanlar).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/İnsan Ayarları", fileName = "InsanAyarlari")]
    public class InsanAyarlari : ScriptableObject
    {
        [SerializeField] private float yurumeHizi = 1f;
        [Tooltip("Bir noktaya varınca bekleme (sn).")]
        [SerializeField] private float bekleme = 2f;
        [SerializeField] private float gorusMenzili = 7f;
        [Tooltip("Telefon feneri olan insanın menzili (GDD: fener koniyi uzatır).")]
        [SerializeField] private float fenerliMenzil = 12f;
        [Tooltip("Koninin toplam açısı (derece).")]
        [SerializeField] private float gorusAcisi = 70f;
        [Tooltip("Göz yüksekliği (metre); görüş çizgisi buradan çekilir.")]
        [SerializeField] private float gozYuksekligi = 1.6f;

        public float YurumeHizi => yurumeHizi;
        public float Bekleme => bekleme;
        public float GorusMenzili => gorusMenzili;
        public float FenerliMenzil => fenerliMenzil;
        public float GorusAcisi => gorusAcisi;
        public float GozYuksekligi => gozYuksekligi;
    }
}
