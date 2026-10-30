using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bir yuva gecesi: deneme hakları, insan görürse ya da enerji biterse deneme sonu, yuvanın KumsalDurumu'na yazılması ve denize dönüş.</summary>
    public class YuvaGecesi : MonoBehaviour
    {
        [SerializeField] private AnneAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private AnneKara anne;
        [SerializeField] private AnneKazma kazma;
        [Tooltip("Her denemede annenin denizden çıktığı yer.")]
        [SerializeField] private Transform baslangic;

        private enum Durum { Kapali, Deneme, DenizeDonus, Bitti }

        private Durum durum;
        private int kalanDeneme;

        public int KalanDeneme => kalanDeneme;

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            Insan.Gordu += InsanGordu;
            anne.Enerji.Bitti += EnerjiBitti;
            kazma.YuvaTamamlandi += YuvaTamamlandi;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            Insan.Gordu -= InsanGordu;
            anne.Enerji.Bitti -= EnerjiBitti;
            kazma.YuvaTamamlandi -= YuvaTamamlandi;
        }

        private void Start()
        {
            // Akışın olmadığı deneme sahnesinde kendiliğinden başlar.
            if (!OyunAkisi.Var) Basla();
        }

        private void AdimBasladi(Adim adim)
        {
            bool bizim = adim.Tur == AdimTuru.YuvaGecesi;
            anne.gameObject.SetActive(bizim);
            if (bizim) Basla();
            else durum = Durum.Kapali;
        }

        private void Basla()
        {
            kalanDeneme = ayarlar.DenemeHakki;
            DenemeBaslat();
        }

        private void DenemeBaslat()
        {
            durum = Durum.Deneme;
            anne.Isinla(baslangic.position, baslangic.rotation);
            anne.Enerji.Doldur(ayarlar.EnerjiEnFazla);
            kazma.Sifirla();
        }

        private void Update()
        {
            if (durum == Durum.DenizeDonus && kum.KiyidanUzaklik(anne.transform.position) < 0f) GeceBitti();
        }

        private void InsanGordu(Transform gorulen)
        {
            // Ürken anne denize döner; yuva örtüldüyse artık önemi yok.
            if (durum == Durum.Deneme && gorulen == anne.transform) DenemeBitti("insan gördü");
        }

        private void EnerjiBitti()
        {
            if (durum == Durum.Deneme) DenemeBitti("enerji bitti");
            else if (durum == Durum.DenizeDonus) GeceBitti();
        }

        private void DenemeBitti(string neden)
        {
            kalanDeneme--;
            Debug.Log($"YuvaGecesi: deneme bitti ({neden}), kalan {kalanDeneme}.");
            if (kalanDeneme > 0)
            {
                DenemeBaslat();
                return;
            }

            // GDD: iki deneme de biterse anne kumsalın en yakın noktasına kendiliğinden yuva kazar.
            Vector3 yer = kum.BolgeNoktasi(kum.Bolge(anne.transform.position), ayarlar.KendiligindenYuvaUzakligi);
            YuvaKaydet(yer);
            GeceBitti();
        }

        private void YuvaTamamlandi(Vector3 yer)
        {
            if (durum != Durum.Deneme) return;
            YuvaKaydet(yer);
            durum = Durum.DenizeDonus;
        }

        private void YuvaKaydet(Vector3 yer)
        {
            SicaklikBandi bant = kum.Bant(yer);
            int yumurta = bant == SicaklikBandi.Kirmizi ? ayarlar.KirmiziBantYumurta : ayarlar.YumurtaSayisi;
            var yuva = new Yuva(kum.Bolge(yer), yer, bant, kum.DalgaAltinda(yer), yumurta, ayarlar.KuluckaHafta);
            kumsal.YuvaEkle(yuva);
            Debug.Log($"YuvaGecesi: yuva kazıldı — bölge {yuva.Bolge}, {bant}, dalga altında: {yuva.DalgaAltinda}, {yumurta} yumurta.");
        }

        private void GeceBitti()
        {
            durum = Durum.Bitti;
            anne.Kilitli = true;
            Debug.Log("YuvaGecesi: gece bitti.");
            if (OyunAkisi.Var) OyunAkisi.AdimBitti();
        }
    }
}
