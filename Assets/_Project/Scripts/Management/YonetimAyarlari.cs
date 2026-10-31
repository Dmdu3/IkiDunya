using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>İnsan kısmının sayıları (GDD: Kaynaklar, Gönüllü görevleri, Kartlar, Olaylar) ve kart, görev, olay listeleri. Denge Toprak'ın denge-gunlugu.md'sine göre burada değişir.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Yönetim Ayarları", fileName = "YonetimAyarlari")]
    public class YonetimAyarlari : ScriptableObject
    {
        [Header("Kaynaklar")]
        [SerializeField] private int baslangicButce = 6;
        [SerializeField] private int turButcesi = 3;
        [Tooltip("Halk desteği bu ve üstündeyse tur bütçesi yükselir.")]
        [SerializeField] private int yuksekDestekEsigi = 7;
        [SerializeField] private int yuksekDestekButcesi = 5;
        [SerializeField] private int baslangicGonullu = 4;
        [Tooltip("Denize ulaşan her bu kadar yavru için +1 gönüllü.")]
        [SerializeField] private int yavruBasinaGonullu = 10;
        [SerializeField] private int gonulluEnFazla = 10;
        [SerializeField] private int baslangicDestek = 4;
        [SerializeField] private int destekEnFazla = 10;
        [Tooltip("Halk desteği bunun altındaysa otel ve belediye olaylarında olumlu seçenek kapanır.")]
        [SerializeField] private int dusukDestekEsigi = 3;

        [Header("Kartlar")]
        [Tooltip("Her tur havuzdan gösterilen kart.")]
        [SerializeField] private int eldekiKart = 4;
        [Tooltip("Yuva Taşıma: yuvanın taşındığı yer, deniz kenarından uzaklık (metre; yeşil bant, dalga çizgisinin üstü).")]
        [SerializeField] private float tasimaUzakligi = 12f;

        [Header("Olaylar")]
        [Tooltip("Otel şezlongu kabul: seçilen bölgenin şezlong artışı.")]
        [SerializeField] private int otelSezlongu = 3;
        [Tooltip("Festival kabul: bir gece ışık ve insan artışı.")]
        [SerializeField] private int festivalArtisi = 2;
        [Tooltip("Fırtına: dalga altındaki yuvanın kaybettiği yumurta oranı.")]
        [Range(0f, 1f)] [SerializeField] private float firtinaKaybi = 0.5f;
        [Tooltip("Temiz Enerji oynandıysa sıcak dalgasının kumu ısıtmama ihtimali (daha zayıf gelir).")]
        [Range(0f, 1f)] [SerializeField] private float temizEnerjiKorumasi = 0.5f;

        [Header("Tur sonu simülasyonu")]
        [Tooltip("Her bölgede çöpün 1 artma ihtimali.")]
        [Range(0f, 1f)] [SerializeField] private float copBirikimi = 0.3f;
        [Tooltip("Korunmayan (kafessiz, tilki çitsiz bölgedeki) yuvaya avcı gelme ihtimali.")]
        [Range(0f, 1f)] [SerializeField] private float avciIhtimali = 0.2f;
        [Range(0f, 1f)] [SerializeField] private float avciYumurtaKaybi = 0.3f;
        [Tooltip("Tilki Çiti varken avcı ihtimalinin çarpanı (GDD: yarıya iner).")]
        [Range(0f, 1f)] [SerializeField] private float citCarpani = 0.5f;

        [Header("İçerik")]
        [SerializeField] private List<KartTanimi> kartlar = new List<KartTanimi>();
        [SerializeField] private List<GorevTanimi> gorevler = new List<GorevTanimi>();
        [SerializeField] private List<OlayTanimi> olaylar = new List<OlayTanimi>();

        public int BaslangicButce => baslangicButce;
        public int TurButcesi(int destek) => destek >= yuksekDestekEsigi ? yuksekDestekButcesi : turButcesi;
        public int BaslangicGonullu => baslangicGonullu;
        public int YavruBasinaGonullu => yavruBasinaGonullu;
        public int GonulluEnFazla => gonulluEnFazla;
        public int BaslangicDestek => baslangicDestek;
        public int DestekEnFazla => destekEnFazla;
        public int DusukDestekEsigi => dusukDestekEsigi;
        public int EldekiKart => eldekiKart;
        public float TasimaUzakligi => tasimaUzakligi;
        public int OtelSezlongu => otelSezlongu;
        public int FestivalArtisi => festivalArtisi;
        public float FirtinaKaybi => firtinaKaybi;
        public float TemizEnerjiKorumasi => temizEnerjiKorumasi;
        public float CopBirikimi => copBirikimi;
        public float AvciIhtimali => avciIhtimali;
        public float AvciYumurtaKaybi => avciYumurtaKaybi;
        public float CitCarpani => citCarpani;
        public IReadOnlyList<KartTanimi> Kartlar => kartlar;
        public IReadOnlyList<GorevTanimi> Gorevler => gorevler;
        public IReadOnlyList<OlayTanimi> Olaylar => olaylar;

        /// <summary>Editör kurulumu listeleri doldurur.</summary>
        public void IcerikAyarla(List<KartTanimi> yeniKartlar, List<GorevTanimi> yeniGorevler, List<OlayTanimi> yeniOlaylar)
        {
            kartlar = yeniKartlar;
            gorevler = yeniGorevler;
            olaylar = yeniOlaylar;
        }
    }
}
