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

        [Header("Paydaşlar")]
        [SerializeField] private int iliskiEnAz = -3;
        [SerializeField] private int iliskiEnCok = 3;
        [Tooltip("İlişki bu ve üstündeyse paydaş dost.")]
        [SerializeField] private int dostEsigi = 2;
        [Tooltip("İlişki bu ve altındaysa paydaş küs; tepki olayı havuza girer.")]
        [SerializeField] private int kusEsigi = -2;
        [Tooltip("Otel dostken her tur gelen ek bütçe.")]
        [SerializeField] private int otelSponsoru = 1;
        [Tooltip("Belediye küsken her tur gelirden düşen.")]
        [SerializeField] private int belediyeKesintisi = 1;
        [Tooltip("Belediye dostken politika kartlarının indirimi.")]
        [SerializeField] private int politikaIndirimi = 1;
        [Tooltip("Balıkçılar dostken Hayalet Ağ Temizliği'nin çarpanı.")]
        [SerializeField] private int balikciAgCarpani = 2;

        [Header("Kartlar")]
        [Tooltip("Her tur havuzdan gösterilen kart.")]
        [SerializeField] private int eldekiKart = 5;
        [Tooltip("Yuva Taşıma: yuvanın taşındığı yer, deniz kenarından uzaklık (metre; yeşil bant, dalga çizgisinin üstü).")]
        [SerializeField] private float tasimaUzakligi = 12f;
        [Tooltip("Aynı türden bu kadar kanıt açılınca bağlı kart güçlenir.")]
        [SerializeField] private int gucluKanit = 2;
        [Tooltip("Otel Ortaklığı: karşılığında bir bölgeye konan şezlong.")]
        [SerializeField] private int ortaklikSezlongu = 1;

        [Header("Olaylar")]
        [Tooltip("Otel şezlongu kabul: seçilen bölgenin şezlong artışı.")]
        [SerializeField] private int otelSezlongu = 3;
        [Tooltip("Festival kabul: bir gece ışık ve insan artışı.")]
        [SerializeField] private int festivalArtisi = 2;
        [Tooltip("Fırtına: dalga altındaki yuvanın kaybettiği yumurta oranı.")]
        [Range(0f, 1f)] [SerializeField] private float firtinaKaybi = 0.5f;
        [Tooltip("Temiz Enerji oynandıysa sıcak dalgasının kumu ısıtmama ihtimali (daha zayıf gelir).")]
        [Range(0f, 1f)] [SerializeField] private float temizEnerjiKorumasi = 0.5f;
        [Tooltip("Otel barı kabul: bir bölgede kalıcı ışık ve insan artışı.")]
        [SerializeField] private int otelBarArtisi = 1;
        [Tooltip("Festivalden sonra çöp bırakılırsa çöpü artan en kalabalık bölge sayısı.")]
        [SerializeField] private int festivalCopBolgesi = 3;
        [Tooltip("Otel küsken ışıkları açarsa bütün bölgelerde ışık artışı ve kaç tur sürdüğü.")]
        [SerializeField] private int otelIsikArtisi = 1;
        [SerializeField] private int otelIsikSuresi = 2;
        [Tooltip("Köpeklere izin verilirse o tur avcı ihtimalinin çarpanı.")]
        [SerializeField] private float kopekAvciCarpani = 2f;

        [Header("Tur sonu simülasyonu")]
        [Tooltip("Her bölgede çöpün 1 artma ihtimali.")]
        [Range(0f, 1f)] [SerializeField] private float copBirikimi = 0.3f;
        [Tooltip("Bölgedeki her şezlong ve insan kademesi için çöp birikme ihtimaline eklenen.")]
        [Range(0f, 1f)] [SerializeField] private float kalabalikCopu = 0.1f;
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
        public int IliskiEnAz => iliskiEnAz;
        public int IliskiEnCok => iliskiEnCok;
        public int DostEsigi => dostEsigi;
        public int KusEsigi => kusEsigi;
        public int OtelSponsoru => otelSponsoru;
        public int BelediyeKesintisi => belediyeKesintisi;
        public int PolitikaIndirimi => politikaIndirimi;
        public int BalikciAgCarpani => balikciAgCarpani;
        public int GucluKanit => gucluKanit;
        public int OrtaklikSezlongu => ortaklikSezlongu;
        public int OtelBarArtisi => otelBarArtisi;
        public int FestivalCopBolgesi => festivalCopBolgesi;
        public int OtelIsikArtisi => otelIsikArtisi;
        public int OtelIsikSuresi => otelIsikSuresi;
        public float KopekAvciCarpani => kopekAvciCarpani;
        public float KalabalikCopu => kalabalikCopu;
        public IReadOnlyList<KartTanimi> Kartlar => kartlar;
        public IReadOnlyList<GorevTanimi> Gorevler => gorevler;
        public IReadOnlyList<OlayTanimi> Olaylar => olaylar;

        /// <summary>Olayın listedeki sırası (kayıtta zincir sırayla tutulur); yoksa -1.</summary>
        public int OlaySirasi(OlayTanimi olay) => olay != null ? olaylar.IndexOf(olay) : -1;

        /// <summary>Editör kurulumu listeleri doldurur.</summary>
        public void IcerikAyarla(List<KartTanimi> yeniKartlar, List<GorevTanimi> yeniGorevler, List<OlayTanimi> yeniOlaylar)
        {
            kartlar = yeniKartlar;
            gorevler = yeniGorevler;
            olaylar = yeniOlaylar;
        }
    }
}
