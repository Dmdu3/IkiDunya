using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kaçış gecesindeki doğal avcıların ayarları: yengeç, martı, tilki (GDD: Bölüm 4, Avcılar).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Avcı Ayarları", fileName = "AvciAyarlari")]
    public class AvciAyarlari : ScriptableObject
    {
        [Header("Yengeç")]
        [SerializeField] private float yengecBeklemeEnAz = 4f;
        [SerializeField] private float yengecBeklemeEnCok = 9f;
        [Tooltip("Deliğin kumu saldırıdan önce bu kadar titrer (GDD: 2 sn).")]
        [SerializeField] private float yengecUyari = 2f;
        [SerializeField] private float yengecYaricap = 0.8f;
        [SerializeField] private float titremeGenligi = 0.03f;

        [Header("Martı")]
        [SerializeField] private float martiBeklemeEnAz = 12f;
        [SerializeField] private float martiBeklemeEnCok = 20f;
        [Tooltip("Gölge dalıştan önce bu kadar kuma düşer (GDD: 1,5 sn).")]
        [SerializeField] private float martiUyari = 1.5f;
        [SerializeField] private float martiDalisYaricapi = 1.5f;
        [Tooltip("Dalış alanında bu kadar ya da daha çok yavru varsa sürü toplu sayılır.")]
        [SerializeField] private int martiTopluEsigi = 3;
        [Tooltip("Toplu sürüden alınan yavru (GDD: 3); dağınıktan 1.")]
        [SerializeField] private int martiTopluAv = 3;
        [Tooltip("Martının yavru aradığı alan (metre).")]
        [SerializeField] private float martiAramaYaricapi = 60f;

        [Header("Tilki")]
        [Tooltip("Bir gecede tilkinin gelme ihtimali.")]
        [Range(0f, 1f)] [SerializeField] private float tilkiIhtimali = 0.5f;
        [Tooltip("Tilki Çiti varsa ihtimal bu çarpanla azalır (GDD: yarıya iner).")]
        [SerializeField] private float tilkiCitiCarpani = 0.5f;
        [SerializeField] private float tilkiYurumeHizi = 1.2f;
        [SerializeField] private float tilkiKosuHizi = 4f;
        [SerializeField] private float tilkiMenzil = 8f;
        [SerializeField] private float tilkiAci = 60f;
        [SerializeField] private float tilkiBekleme = 2f;

        public float YengecBekleme => Random.Range(yengecBeklemeEnAz, yengecBeklemeEnCok);
        public float YengecUyari => yengecUyari;
        public float YengecYaricap => yengecYaricap;
        public float TitremeGenligi => titremeGenligi;
        public float MartiBekleme => Random.Range(martiBeklemeEnAz, martiBeklemeEnCok);
        public float MartiUyari => martiUyari;
        public float MartiDalisYaricapi => martiDalisYaricapi;
        public int MartiTopluEsigi => martiTopluEsigi;
        public int MartiTopluAv => martiTopluAv;
        public float MartiAramaYaricapi => martiAramaYaricapi;
        public float TilkiIhtimali => tilkiIhtimali;
        public float TilkiCitiCarpani => tilkiCitiCarpani;
        public float TilkiYurumeHizi => tilkiYurumeHizi;
        public float TilkiKosuHizi => tilkiKosuHizi;
        public float TilkiMenzil => tilkiMenzil;
        public float TilkiAci => tilkiAci;
        public float TilkiBekleme => tilkiBekleme;
    }
}
