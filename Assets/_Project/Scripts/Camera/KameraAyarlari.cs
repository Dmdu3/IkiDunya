using UnityEngine;

namespace IkiDunya.Camera
{
    /// <summary>Birinci şahıs kameranın ayarları: fare ve oyun kolu hassasiyeti, yukarı/aşağı bakış sınırları, yürürken baş sallanması, hızlanınca görüş açısı.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kamera Ayarları", fileName = "KameraAyarlari")]
    public class KameraAyarlari : ScriptableObject
    {
        [SerializeField] private float hassasiyet = 0.1f;
        [Tooltip("Oyun kolunun sağ çubuğuyla dönüş hızı (derece/sn).")]
        [SerializeField] private float cubukHizi = 140f;
        [SerializeField] private float asagiSinir = -35f;
        [SerializeField] private float yukariSinir = 30f;

        [Header("Baş sallanması")]
        [Tooltip("Bu hızda (m/sn) sallanma tam; dururken yok.")]
        [SerializeField] private float sallanmaTamHiz = 1.5f;
        [Tooltip("Yukarı-aşağı genlik (metre) ve saniyedeki salınım.")]
        [SerializeField] private float sallanmaGenligi = 0.015f;
        [SerializeField] private float sallanmaSikligi = 2f;

        [Header("Hızlanınca")]
        [Tooltip("Shift ile hızlanırken görüş açısının artışı (derece) ve geçiş hızı.")]
        [SerializeField] private float hizGorusArtisi = 8f;
        [SerializeField] private float gorusGecisi = 6f;

        public float Hassasiyet => hassasiyet;
        public float CubukHizi => cubukHizi;
        public float AsagiSinir => asagiSinir;
        public float YukariSinir => yukariSinir;
        public float SallanmaTamHiz => sallanmaTamHiz;
        public float SallanmaGenligi => sallanmaGenligi;
        public float SallanmaSikligi => sallanmaSikligi;
        public float HizGorusArtisi => hizGorusArtisi;
        public float GorusGecisi => gorusGecisi;
    }
}
