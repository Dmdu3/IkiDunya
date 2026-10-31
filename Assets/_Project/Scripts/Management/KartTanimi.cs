using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Kartın kumsala ya da insan kısmına etkisi; Etkiler bu türe göre uygular.</summary>
    /// <remarks>Asset'lerde sayı olarak saklanır: yeni etki hep sona eklenir.</remarks>
    public enum KartEtkisi
    {
        Golgelik, TemizlikKampanyasi, KumulOnarimi, YuvaTasima, IsikYasagi, AracYasagi,
        SezlongDuzenlemesi, TilkiCiti, HayaletAgTemizligi, GonulluCagrisi, SahaArastirmasi, TemizEnerji
    }

    /// <summary>Kartın, görevin hedefi: oyuncu haritada ne seçer.</summary>
    public enum Hedef { Yok, Bolge, Yuva }

    /// <summary>Bir kart (GDD: İnsan kısmı, Kartlar). Adı ve açıklaması metinler.md'den id ile gelir.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kart", fileName = "Kart")]
    public class KartTanimi : ScriptableObject
    {
        [Tooltip("Metin id'si (ör. kart_golgelik); açıklaması id_aciklama.")]
        [SerializeField] private string id;
        [SerializeField] private KartEtkisi etki;
        [SerializeField] private Hedef hedef;
        [SerializeField] private int bedel = 2;
        [Tooltip("Etkinin büyüklüğü (Temizlik Kampanyası: kaç bölge, Gönüllü Çağrısı: kaç gönüllü).")]
        [SerializeField] private int deger = 1;
        [Tooltip("Baştan havuzda mı; değilse ilgili kanıt açılınca girer.")]
        [SerializeField] private bool bastan = true;
        [Tooltip("Kartı açan kanıt (baştan değilse) ya da 1 ucuzlatan kanıt (baştansa ve 'kanıt ucuzlatır' işaretliyse).")]
        [SerializeField] private KanitTuru kanit;
        [SerializeField] private bool kanitUcuzlatir;
        [Tooltip("Bir kez oynanınca havuzdan çıkar (kalıcı yasaklar ve Temiz Enerji).")]
        [SerializeField] private bool tekSeferlik;

        public string Id => id;
        public KartEtkisi Etki => etki;
        public Hedef Hedef => hedef;
        public int Bedel => bedel;
        public int Deger => deger;
        public bool Bastan => bastan;
        public KanitTuru Kanit => kanit;
        public bool KanitUcuzlatir => kanitUcuzlatir;
        public bool TekSeferlik => tekSeferlik;

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
    }
}
