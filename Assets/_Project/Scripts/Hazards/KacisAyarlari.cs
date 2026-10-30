using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kaçış gecesinin kumsal ayarları: gün doğumu sayacı, sıcak kum lekeleri, dalga (GDD: Bölüm 4).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kaçış Ayarları", fileName = "KacisAyarlari")]
    public class KacisAyarlari : ScriptableObject
    {
        [Header("Gün doğumu")]
        [Tooltip("Kumsal koşusunun süresi (GDD: 150 sn).")]
        [SerializeField] private float gunDogumuSuresi = 150f;

        [Header("Sıcak kum lekesi")]
        [Tooltip("Gece başında lekenin ölçek çarpanı.")]
        [SerializeField] private float lekeBaslangicOlcegi = 1f;
        [Tooltip("Gün doğarken lekenin ölçek çarpanı.")]
        [SerializeField] private float lekeSonOlcegi = 2.5f;
        [Tooltip("Gölgelikli bölgede leke bu çarpanla küçülür (GDD: gölgelik → sıcak lekeler küçük).")]
        [SerializeField] private float golgelikLekeCarpani = 0.5f;
        [Tooltip("Lekenin içindeki yavrunun saniyede kaybettiği enerji.")]
        [SerializeField] private float sicakLekeHasari = 15f;

        [Header("Dalga")]
        [Tooltip("Dalganın gelip gittiği kıyı şeridi (GDD: son 5 m).")]
        [SerializeField] private float dalgaBolgesi = 5f;
        [Tooltip("Bir dalganın gelip gitme süresi (sn).")]
        [SerializeField] private float dalgaPeriyodu = 8f;
        [Tooltip("Dalga vururken suya giren yavrunun bir kez geri savrulduğu mesafe (metre).")]
        [SerializeField] private float savrulmaMesafesi = 1f;

        public float GunDogumuSuresi => gunDogumuSuresi;
        public float LekeBaslangicOlcegi => lekeBaslangicOlcegi;
        public float LekeSonOlcegi => lekeSonOlcegi;
        public float GolgelikLekeCarpani => golgelikLekeCarpani;
        public float SicakLekeHasari => sicakLekeHasari;
        public float DalgaBolgesi => dalgaBolgesi;
        public float DalgaPeriyodu => dalgaPeriyodu;
        public float SavrulmaMesafesi => savrulmaMesafesi;
    }
}
