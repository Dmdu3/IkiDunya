using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bir kaçış gecesi: kumdan çıkış (W ile kat kat), kumsal koşusu (sürü, gün doğumu, dalga), kayıpların KumsalDurumu'na yazılması.</summary>
    public class KacisGecesi : MonoBehaviour
    {
        public enum Asama { Kapali, Cikis, Kosu, Bitti }

        [SerializeField] private YavruAyarlari ayarlar;
        [SerializeField] private KacisAyarlari kacisAyarlari;
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private Suru suru;
        [SerializeField] private GunDogumu gunDogumu;
        [SerializeField] private Dalga dalga;
        [Tooltip("KumsalDurumu'nda bu gecenin yuvası yoksa (deneme sahnesi) yavrular buradan çıkar.")]
        [SerializeField] private Transform varsayilanYuva;

        private Vector3 yuvaKonumu;
        private int yavruSayisi;
        private float gecen;

        public Asama SuAnki { get; private set; }
        /// <summary>Yavruların üstünde kalan kum katı.</summary>
        public int Kat { get; private set; }
        /// <summary>Yüzey sıcaklığı: 1 sıcak, 0 serin.</summary>
        public float YuzeySicakligi => Mathf.Clamp01(1f - gecen / ayarlar.SogumaSuresi);
        /// <summary>Kum altındaki yavruların enerjisi (0–1).</summary>
        public float CikisEnerjisi { get; private set; }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            suru.YavruKaybedildi += YavruKaybedildi;
            suru.YavruKurtuldu += YavruKurtuldu;
            suru.Bitti += GeceBitti;
            gunDogumu.Bitti += GunDogdu;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            suru.YavruKaybedildi -= YavruKaybedildi;
            suru.YavruKurtuldu -= YavruKurtuldu;
            suru.Bitti -= GeceBitti;
            gunDogumu.Bitti -= GunDogdu;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Basla(1);
        }

        private void AdimBasladi(Adim adim)
        {
            bool bizim = adim.Tur == AdimTuru.KacisGecesi;
            suru.gameObject.SetActive(bizim);
            if (bizim) Basla(adim.Sira);
            else SuAnki = Asama.Kapali;
        }

        /// <summary>Gecenin yuvası Bölüm 2'deki aynı sıradaki yuvadır.</summary>
        private void Basla(int gece)
        {
            bool yuvaVar = gece >= 1 && gece <= kumsal.Yuvalar.Count;
            yuvaKonumu = yuvaVar ? kumsal.Yuvalar[gece - 1].Konum : varsayilanYuva.position;
            yavruSayisi = yuvaVar ? kumsal.Yuvalar[gece - 1].Yumurta : ayarlar.VarsayilanYavruSayisi;
            CikisiBaslat();
        }

        private void CikisiBaslat()
        {
            SuAnki = Asama.Cikis;
            Kat = ayarlar.KatSayisi;
            gecen = 0f;
            CikisEnerjisi = 1f;
        }

        private void Update()
        {
            if (SuAnki == Asama.Cikis) CikisGuncelle();
            else if (SuAnki == Asama.Kosu) DalgayaBak();
        }

        private void CikisGuncelle()
        {
            gecen += Time.deltaTime;
            CikisEnerjisi -= ayarlar.KumAltiEnerjiKaybi / ayarlar.EnerjiEnFazla * Time.deltaTime;
            if (CikisEnerjisi <= 0f)
            {
                // GDD: enerji biterse son kontrol noktasından devam; burada çıkışın başı.
                Debug.Log("KacisGecesi: kum altında enerji bitti, çıkış baştan.");
                CikisiBaslat();
                return;
            }

            if (!GameInput.KazYukariBasildi) return;
            Kat--;
            if (Kat <= 0) YuzeyeCik();
        }

        private void YuzeyeCik()
        {
            // Yüzey ölümcül sıcaklığın ne kadar üstündeyse yavruların o kadarı ölür.
            float fazla = Mathf.InverseLerp(ayarlar.OlumculSicaklik, 1f, YuzeySicakligi);
            int olen = Mathf.RoundToInt(yavruSayisi * fazla);
            int bolge = kum.Bolge(yuvaKonumu);
            for (int i = 0; i < olen; i++)
                kumsal.KayipIziEkle(new KayipIzi(bolge, yuvaKonumu, KayipNedeni.SicakKum));
            Debug.Log($"KacisGecesi: yüzeye çıkıldı, {olen} yavru sıcak kumda kaldı.");

            int kalan = yavruSayisi - olen;
            if (kalan <= 0)
            {
                GeceBitti();
                return;
            }

            // Yavrular denize bakarak çıkar.
            Vector3 denize = kum.transform.TransformDirection(Vector3.back);
            suru.Kur(yuvaKonumu, Quaternion.LookRotation(denize), kalan);
            gunDogumu.Baslat();
            SuAnki = Asama.Kosu;
        }

        private void DalgayaBak()
        {
            foreach (Yavru yavru in suru.Yavrular)
            {
                if (yavru.SuAnki != Yavru.Durum.Suruyor) continue;
                if (kum.KiyidanUzaklik(yavru.transform.position) >= dalga.SuKenari) continue;

                if (dalga.Cekiliyor) yavru.Kurtar();
                else yavru.Savrul(kum.transform.forward * kacisAyarlari.SavrulmaMesafesi);
            }
        }

        private void YavruKaybedildi(Yavru yavru, KayipNedeni neden)
        {
            Vector3 yer = yavru.transform.position;
            kumsal.KayipIziEkle(new KayipIzi(kum.Bolge(yer), yer, neden));
        }

        private void YavruKurtuldu(Yavru yavru) => kumsal.DenizeUlasti(1);

        private void GunDogdu()
        {
            // Gün doğduğunda kumsalda kalan yavrular sıcak kumda kalır.
            foreach (Yavru yavru in suru.Yavrular)
                if (yavru.SuAnki == Yavru.Durum.Suruyor) yavru.Kaybet(KayipNedeni.SicakKum);
        }

        private void GeceBitti()
        {
            if (SuAnki == Asama.Bitti) return;
            SuAnki = Asama.Bitti;
            gunDogumu.Durdur();
            suru.Durdur();
            Debug.Log($"KacisGecesi: gece bitti — kurtulan {suru.Kurtulan}, kaybedilen {suru.Kaybedilen + (yavruSayisi - suru.Yavrular.Count)}.");
            if (OyunAkisi.Var) OyunAkisi.AdimBitti();
        }
    }
}
