using UnityEngine;

namespace IkiDunya.Camera
{
    /// <summary>Birinci şahıs kameranın ayarları: fare hassasiyeti ve yukarı/aşağı bakış sınırları.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kamera Ayarları", fileName = "KameraAyarlari")]
    public class KameraAyarlari : ScriptableObject
    {
        [SerializeField] private float hassasiyet = 0.1f;
        [SerializeField] private float asagiSinir = -35f;
        [SerializeField] private float yukariSinir = 30f;

        public float Hassasiyet => hassasiyet;
        public float AsagiSinir => asagiSinir;
        public float YukariSinir => yukariSinir;
    }
}
