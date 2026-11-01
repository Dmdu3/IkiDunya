using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Denizdeki tehlikelerin ve canlıların ayarları: akıntı, sıcak su, tekne, besin, avcı balık, Bölüm 5 dalgaları, ağ yoğunluğu (GDD: Bölüm 1 ve 5).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Deniz Tehlike Ayarları", fileName = "DenizTehlikeAyarlari")]
    public class DenizTehlikeAyarlari : ScriptableObject
    {
        [Header("Akıntı ve sıcak su")]
        [SerializeField] private float akintiHizi = 1.5f;
        [Tooltip("Sıcak su lekesinde saniyede kaybedilen enerji (GDD: 3).")]
        [SerializeField] private float sicakSuHasari = 3f;

        [Header("Tekne")]
        [SerializeField] private float tekneHizi = 6f;
        [Tooltip("Yüzeydeki kaplumbağaya çarpınca kaybedilen enerji (GDD: 25).")]
        [SerializeField] private float tekneHasari = 25f;
        [SerializeField] private float tekneYaricapi = 2.5f;
        [Tooltip("Aynı tekne art arda kaç saniyede bir çarpabilir.")]
        [SerializeField] private float tekneBekleme = 2f;
        [Tooltip("Teknenin suya düşen gölgesi teknenin bu kadar önünde (metre): su altındaki oyuncu tekneyi gelmeden görür.")]
        [SerializeField] private float tekneGolgeOnu = 6f;
        [Tooltip("Pervane köpüğünün titreme hızı ve genliği (ölçek oranı).")]
        [SerializeField] private float kopukTitremeHizi = 18f;
        [SerializeField] private float kopukTitremeGenligi = 0.35f;

        [Header("Besin")]
        [Tooltip("Denizanası yenince kazanılan enerji (GDD: 15).")]
        [SerializeField] private float denizanasiEnerjisi = 15f;
        [Tooltip("Poşet yutunca kaybedilen enerji (yavaşlamaya ek ceza).")]
        [SerializeField] private float posetEnerjiKaybi = 10f;
        [Tooltip("Denizanası ve poşetin süzülme hızı.")]
        [SerializeField] private float suzulmeHizi = 0.4f;
        [Tooltip("Denizanasının kasılma sıklığı (saniyede).")]
        [SerializeField] private float kasilmaSikligi = 1.5f;

        [Header("Avcı balık")]
        [SerializeField] private float avciMenzil = 10f;
        [SerializeField] private float avciHizi = 2f;
        [SerializeField] private float avciKosuHizi = 5f;
        [SerializeField] private float avciYakalamaMesafesi = 0.8f;
        [Tooltip("Yakaladıktan ya da kaybettikten sonra bekleme.")]
        [SerializeField] private float avciBekleme = 4f;

        [Header("Bölüm 5 dalgaları")]
        [SerializeField] private float dalgaAraligi = 6f;
        [SerializeField] private float dalgaHizi = 4f;
        [Tooltip("Dalganın denizde başladığı yer (kıyıdan metre).")]
        [SerializeField] private float dalgaBaslangici = 30f;
        [Tooltip("Bu derinliğin altındaki kaplumbağayı dalga savurmaz.")]
        [SerializeField] private float dalgaDerinligi = 1.2f;
        [SerializeField] private float dalgaSavrulmasi = 4f;

        [Header("Ağ ve misina")]
        [Tooltip("KumsalDurumu'ndaki denizdeki ağ seviyesinin her biri için kaç ağ ya da misina.")]
        [SerializeField] private int seviyeBasinaAg = 2;
        [Tooltip("Ağ kenarları ve misinanın parıldama hızı (saniyede) ve en sönük hali (0–1).")]
        [SerializeField] private float parildamaHizi = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float parildamaEnAz = 0.35f;

        public float AkintiHizi => akintiHizi;
        public float SicakSuHasari => sicakSuHasari;
        public float TekneHizi => tekneHizi;
        public float TekneHasari => tekneHasari;
        public float TekneYaricapi => tekneYaricapi;
        public float TekneBekleme => tekneBekleme;
        public float TekneGolgeOnu => tekneGolgeOnu;
        public float KopukTitremeHizi => kopukTitremeHizi;
        public float KopukTitremeGenligi => kopukTitremeGenligi;
        public float DenizanasiEnerjisi => denizanasiEnerjisi;
        public float PosetEnerjiKaybi => posetEnerjiKaybi;
        public float SuzulmeHizi => suzulmeHizi;
        public float KasilmaSikligi => kasilmaSikligi;
        public float AvciMenzil => avciMenzil;
        public float AvciHizi => avciHizi;
        public float AvciKosuHizi => avciKosuHizi;
        public float AvciYakalamaMesafesi => avciYakalamaMesafesi;
        public float AvciBekleme => avciBekleme;
        public float DalgaAraligi => dalgaAraligi;
        public float DalgaHizi => dalgaHizi;
        public float DalgaBaslangici => dalgaBaslangici;
        public float DalgaDerinligi => dalgaDerinligi;
        public float DalgaSavrulmasi => dalgaSavrulmasi;
        public int SeviyeBasinaAg => seviyeBasinaAg;
        public float ParildamaHizi => parildamaHizi;
        public float ParildamaEnAz => parildamaEnAz;
    }
}
