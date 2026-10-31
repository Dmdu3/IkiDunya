using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Kartın kumsala ya da insan kısmına etkisi; Etkiler bu türe göre uygular.</summary>
    /// <remarks>Asset'lerde sayı olarak saklanır: yeni etki hep sona eklenir.</remarks>
    public enum KartEtkisi
    {
        Golgelik, TemizlikKampanyasi, KumulOnarimi, YuvaTasima, IsikYasagi, AracYasagi,
        SezlongDuzenlemesi, TilkiCiti, HayaletAgTemizligi, GonulluCagrisi, SahaArastirmasi, TemizEnerji,
        Aydinlatma, GozcuKulubesi, Kuluckahane, PlajTabelalari, Bagis, BasinBulteni, Lobi, KorumaAlani,
        OtelOrtakligi, Kooperatif, KumTorbasi, YuvaTahliyesi, AcilSulama, DronTaramasi, Universite
    }

    /// <summary>Kartın türü (GDD: Kart türleri): ne kadar sürdüğü, kaç kez oynandığı, ne zaman ele geldiği.</summary>
    public enum KartTuru { Altyapi, Kampanya, Politika, Acil, Arastirma }

    /// <summary>Kartın, görevin hedefi: oyuncu haritada ne seçer.</summary>
    public enum Hedef { Yok, Bolge, Yuva }

    /// <summary>Bir kart (GDD: İnsan kısmı, Kartlar). Adı ve açıklaması metinler.md'den id ile gelir.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kart", fileName = "Kart")]
    public class KartTanimi : ScriptableObject
    {
        [Tooltip("Metin id'si (ör. kart_golgelik); açıklaması id_aciklama.")]
        [SerializeField] private string id;
        [SerializeField] private KartEtkisi etki;
        [SerializeField] private KartTuru tur;
        [SerializeField] private Hedef hedef;
        [SerializeField] private int bedel = 2;
        [Tooltip("Oynandıktan sonra her tur gelirden düşen bakım.")]
        [SerializeField] private int bakim;
        [Tooltip("Etkinin büyüklüğü (Temizlik Kampanyası: kaç bölge, Gönüllü Çağrısı: kaç gönüllü).")]
        [SerializeField] private int deger = 1;
        [Tooltip("Aynı türden ikinci kanıt açılınca etkinin büyüklüğü; 0 güçlenmez.")]
        [SerializeField] private int gucluDeger;
        [Tooltip("Baştan havuzda mı; değilse ilgili kanıt açılınca girer. Acil kartlar havuza değil, uyarı gelince ele girer.")]
        [SerializeField] private bool bastan = true;
        [Tooltip("Kartı açan kanıt (baştan değilse), 1 ucuzlatan kanıt ('kanıt ucuzlatır' işaretliyse) ve güçlendiren kanıt.")]
        [SerializeField] private KanitTuru kanit;
        [SerializeField] private bool kanitUcuzlatir;
        [Tooltip("Bir kez oynanınca havuzdan çıkar (politikalar, Kuluçkahane, Üniversite Ortaklığı).")]
        [SerializeField] private bool tekSeferlik;
        [Tooltip("Acil kartın hangi uyarıda ele geldiği.")]
        [SerializeField] private OlayEtkisi acilUyari;

        [Header("Halk ve paydaşlar")]
        [SerializeField] private int destekFarki;
        [SerializeField] private PaydasFarki paydasFarki;
        [Tooltip("Oynamak için bir paydaşla en az bu ilişki gerekir.")]
        [SerializeField] private bool kosulVar;
        [SerializeField] private Paydas kosulPaydas;
        [SerializeField] private int kosulEnAz;

        public string Id => id;
        public KartEtkisi Etki => etki;
        public KartTuru Tur => tur;
        public Hedef Hedef => hedef;
        public int Bedel => bedel;
        public int Bakim => bakim;
        public int Deger => deger;
        public int GucluDeger => gucluDeger;
        public bool Bastan => bastan;
        public KanitTuru Kanit => kanit;
        public bool KanitUcuzlatir => kanitUcuzlatir;
        public bool TekSeferlik => tekSeferlik;
        public OlayEtkisi AcilUyari => acilUyari;
        public int DestekFarki => destekFarki;
        public PaydasFarki PaydasFarki => paydasFarki;
        public bool KosulVar => kosulVar;
        public Paydas KosulPaydas => kosulPaydas;
        public int KosulEnAz => kosulEnAz;

        /// <summary>Editör kurulumu varsayılanları yazar.</summary>
        public void Kur(string yeniId, KartEtkisi yeniEtki, Hedef yeniHedef, int yeniBedel, int yeniDeger, bool yeniBastan, KanitTuru yeniKanit, bool ucuzlatir, bool tek)
        {
            id = yeniId;
            etki = yeniEtki;
            hedef = yeniHedef;
            bedel = yeniBedel;
            deger = yeniDeger;
            bastan = yeniBastan;
            kanit = yeniKanit;
            kanitUcuzlatir = ucuzlatir;
            tekSeferlik = tek;
        }

        /// <summary>Editör kurulumu: tür, bakım, güçlenme, acil uyarısı, halk ve paydaş etkileri, paydaş koşulu.</summary>
        public void Ayrinti(KartTuru yeniTur, int yeniBakim, int guclu, OlayEtkisi acil, int destek, PaydasFarki paydas, bool kosul, Paydas kimden, int enAz)
        {
            tur = yeniTur;
            bakim = yeniBakim;
            gucluDeger = guclu;
            acilUyari = acil;
            destekFarki = destek;
            paydasFarki = paydas;
            kosulVar = kosul;
            kosulPaydas = kimden;
            kosulEnAz = enAz;
        }
    }
}
