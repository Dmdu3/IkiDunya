using System;
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
        private Yuva sonYuva;
        private int bitisKaresi;

        public int KalanDeneme => kalanDeneme;
        /// <summary>Yuvanın şu anki bandı (bu gecenin kum ısınması dahil); sonuç paneli gösterir.</summary>
        public SicaklikBandi YuvaBandi(Yuva yuva) => kumsal.YuvaBandi(yuva);

        /// <summary>Gece bitince; kazılan yuvayla. Sonuç ekranı gösterir; oyuncu tıklayınca akış ilerler.</summary>
        public event Action<Yuva> SonucHazir;
        /// <summary>Sonuç ekranı kapanınca.</summary>
        public event Action SonucKapandi;

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
            // Sahne kapanırken anne bizden önce yok edilmiş olabilir.
            if (anne != null) anne.Enerji.Bitti -= EnerjiBitti;
            if (kazma != null) kazma.YuvaTamamlandi -= YuvaTamamlandi;
        }

        private void Start()
        {
            // Akışın olmadığı deneme sahnesinde kendiliğinden başlar.
            if (!OyunAkisi.Var) Basla();
        }

        private void AdimBasladi(Adim adim)
        {
            // Bekleyen sonuç (kendiliğinden yuva) yeni adıma taşmasın.
            CancelInvoke(nameof(GeceBitti));
            // Anne ve gecenin nesneleri AdimNesneleri ile açılıp kapanır.
            if (adim.Tur == AdimTuru.YuvaGecesi) Basla();
            else durum = Durum.Kapali;
        }

        private void Basla()
        {
            Ipucu.Goster("ipucu_termal");
            Ipucu.Goster("ipucu_kazma");
            kalanDeneme = ayarlar.DenemeHakki;
            DenemeBaslat();
        }

        private void DenemeBaslat()
        {
            durum = Durum.Deneme;
            anne.Isinla(baslangic.position, baslangic.rotation);
            anne.Enerji.Doldur(ayarlar.EnerjiEnFazla);
            kazma.Sifirla();
            kazma.enabled = true;
        }

        private void Update()
        {
            if (durum == Durum.DenizeDonus && kum.KiyidanUzaklik(anne.transform.position) < 0f) GeceBitti();
            else if (durum == Durum.Bitti && Time.frameCount != bitisKaresi && (GameInput.TiklaBasildi || GameInput.ZiplaDalBasildi)) SonucuKapat();
        }

        private void InsanGordu(Transform gorulen)
        {
            // Ürken anne denize döner; yuva örtüldüyse artık önemi yok.
            if (durum != Durum.Deneme || gorulen != anne.transform) return;
            Geribildirim.Darbe(Siddet.Orta);
            DenemeBitti("bildirim_deneme_insan");
        }

        private void EnerjiBitti()
        {
            if (durum == Durum.Deneme)
            {
                Geribildirim.Darbe(Siddet.Hafif);
                DenemeBitti("bildirim_deneme_enerji");
            }
            else if (durum == Durum.DenizeDonus) GeceBitti();
        }

        /// <summary>Deneme bitti; bildirim nedeni ve kalan hakkı söyler. Son hak da bittiyse anne yuvayı kendisi kazar; bu söylenir, kısa süre sonra sonuç gelir.</summary>
        private void DenemeBitti(string bildirim)
        {
            kalanDeneme--;
            Ses.Cal(SesTuru.YavruKaybedildi);
            if (kalanDeneme > 0)
            {
                Ipucu.Bildir(bildirim, kalanDeneme);
                DenemeBaslat();
                return;
            }

            // GDD: iki deneme de biterse anne kumsalın en yakın noktasına kendiliğinden yuva kazar.
            Vector3 yer = kum.BolgeNoktasi(kum.Bolge(anne.transform.position), ayarlar.KendiligindenYuvaUzakligi);
            YuvaKaydet(yer);
            // Bekleme sırasında yeni deneme, ürkme ya da tıklama kabul edilmez.
            durum = Durum.Kapali;
            anne.Kilitli = true;
            Ipucu.Bildir("bildirim_kendiliginden_yuva");
            Invoke(nameof(GeceBitti), ayarlar.KendiligindenYuvaBekleme);
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
            // Yuvaya ısınmasız zemin bandı yazılır; sıcak dalgası geçince yuva da serinler.
            var yuva = new Yuva(kum.Bolge(yer), yer, kum.ZeminBandi(yer), kum.DalgaAltinda(yer), yumurta, ayarlar.KuluckaHafta);
            kumsal.YuvaEkle(yuva);
            sonYuva = yuva;
            Debug.Log($"YuvaGecesi: yuva kazıldı — bölge {yuva.Bolge}, {bant}, dalga altında: {yuva.DalgaAltinda}, {yumurta} yumurta.");
        }

        private void GeceBitti()
        {
            durum = Durum.Bitti;
            bitisKaresi = Time.frameCount;
            // Yarım kalan kazma sonuç ekranının arkasında sürmesin (örtme bitince anneyi açardı).
            kazma.Sifirla();
            kazma.enabled = false;
            anne.Kilitli = true;
            Debug.Log("YuvaGecesi: gece bitti.");
            SonucHazir?.Invoke(sonYuva);
        }

        private void SonucuKapat()
        {
            // Geçiş sürerken kabul edilmezse sonuç açık kalır, sonraki tıklamada tekrar denenir.
            if (OyunAkisi.Var && !OyunAkisi.AdimBitti()) return;
            durum = Durum.Kapali;
            SonucKapandi?.Invoke();
        }
    }
}
