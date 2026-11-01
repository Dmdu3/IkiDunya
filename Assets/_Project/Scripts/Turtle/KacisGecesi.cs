using System;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bir kaçış gecesi: kumdan çıkış (W ile kat kat), kumsal koşusu (sürü, gün doğumu, dalga), kayıpların KumsalDurumu'na yazılması, gecenin sonucu.</summary>
    public class KacisGecesi : MonoBehaviour
    {
        public enum Asama { Kapali, Cikis, Kosu, Bitti }

        /// <summary>Gecenin sonucu: kurtulan ve kaybedilen yavrular, kurtulanların dişi-erkek dağılımı.</summary>
        public readonly struct Sonuc
        {
            public int Kurtulan { get; }
            public int Kaybedilen { get; }
            public int Disi { get; }
            public int Erkek => Kurtulan - Disi;
            /// <summary>Kayıpların nedene göre sayısı; KayipNedeni sırasıyla.</summary>
            public int[] Nedenler { get; }

            public Sonuc(int kurtulan, int kaybedilen, int disi, int[] nedenler)
            {
                Kurtulan = kurtulan;
                Kaybedilen = kaybedilen;
                Disi = disi;
                Nedenler = nedenler;
            }
        }

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
        private SicaklikBandi yuvaBandi;
        private int yavruSayisi;
        private int kumdaKalan;
        private bool suruKuruldu;
        private float gecen;
        private readonly SonucKapisi sonucKapisi = new SonucKapisi();
        private int[] nedenler = new int[Enum.GetValues(typeof(KayipNedeni)).Length];
        private bool gunUyarildi;
        private int geceIziBaslangici;

        public Asama SuAnki { get; private set; }
        public bool TekrarGerekli => OyunAkisi.SonKacisGecesi && kumsal.DenizeUlasanYavru == 0;
        /// <summary>Yavruların üstünde kalan kum katı.</summary>
        public int Kat { get; private set; }
        /// <summary>Yüzey sıcaklığı: 1 sıcak, 0 serin.</summary>
        public float YuzeySicakligi => Mathf.Clamp01(1f - gecen / ayarlar.SogumaSuresi);
        /// <summary>Kum altındaki yavruların enerjisi (0–1).</summary>
        public float CikisEnerjisi { get; private set; }

        /// <summary>Gece bitince. Sonuç ekranı gösterir; oyuncu tıklayınca akış ilerler.</summary>
        public event Action<Sonuc> SonucHazir;
        public event Action SonucKapandi;
        /// <summary>Yavrular yüzeye çıkıp koşu başladığında (parkurlar dinler).</summary>
        public event Action KosuBasladi;

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
            // Sahne kapanırken bunlar bizden önce yok edilmiş olabilir.
            if (suru != null)
            {
                suru.YavruKaybedildi -= YavruKaybedildi;
                suru.YavruKurtuldu -= YavruKurtuldu;
                suru.Bitti -= GeceBitti;
            }
            if (gunDogumu != null) gunDogumu.Bitti -= GunDogdu;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Basla(1);
        }

        private void AdimBasladi(Adim adim)
        {
            // Sürü ve gecenin nesneleri AdimNesneleri ile açılıp kapanır.
            if (adim.Tur == AdimTuru.KacisGecesi) Basla(adim.Sira);
            else SuAnki = Asama.Kapali;
        }

        /// <summary>Gecenin yuvası Bölüm 2'deki aynı sıradaki yuvadır.</summary>
        private void Basla(int gece, bool tekrar = false)
        {
            geceIziBaslangici = kumsal.KayipIzleri.Count;
            bool yuvaVar = gece >= 1 && gece <= kumsal.Yuvalar.Count;
            yuvaKonumu = yuvaVar ? kumsal.Yuvalar[gece - 1].Konum : varsayilanYuva.position;
            yuvaBandi = yuvaVar ? kumsal.YuvaBandi(gece - 1) : kum.Bant(yuvaKonumu);
            yavruSayisi = yuvaVar ? kumsal.Yuvalar[gece - 1].Yumurta : ayarlar.VarsayilanYavruSayisi;
            // Tüm yumurtalar daha önce kaybedildiyse tekrar denemesi oynanabilsin.
            if (tekrar && yavruSayisi <= 0) yavruSayisi = ayarlar.VarsayilanYavruSayisi;
            kumdaKalan = 0;
            suruKuruldu = false;
            nedenler = new int[nedenler.Length];
            gunUyarildi = false;
            CikisiBaslat();
        }

        private void CikisiBaslat()
        {
            SuAnki = Asama.Cikis;
            Ipucu.Goster("ipucu_kumdan_cikis");
            Kat = ayarlar.KatSayisi;
            gecen = 0f;
            CikisEnerjisi = 1f;
        }

        private void Update()
        {
            if (SuAnki == Asama.Cikis) CikisGuncelle();
            else if (SuAnki == Asama.Kosu)
            {
                DalgayaBak();
                GunDogumunaBak();
            }
            else if (SuAnki == Asama.Bitti && sonucKapisi.KapatIstendi()) SonucuKapat();
        }

        private void CikisGuncelle()
        {
            gecen += Time.deltaTime;
            CikisEnerjisi -= ayarlar.KumAltiEnerjiKaybi / ayarlar.EnerjiEnFazla * Time.deltaTime;
            if (CikisEnerjisi <= 0f)
            {
                // GDD: enerji biterse son kontrol noktasından devam; burada çıkışın başı. Kumun soğuması da baştan
                // (korunsaydı enerjiyi bilerek bitirip serin kumdan çıkmak bedava olurdu).
                Geribildirim.Darbe(Siddet.Hafif);
                Ipucu.Bildir("bildirim_kum_alti_enerji");
                CikisiBaslat();
                return;
            }

            if (!GameInput.KazYukariBasildi) return;
            Kat--;
            Ses.Cal(SesTuru.KumKazma);
            if (Kat <= 0) YuzeyeCik();
        }

        private void YuzeyeCik()
        {
            // Yüzey ölümcül sıcaklığın ne kadar üstündeyse yavruların o kadarı ölür.
            float fazla = Mathf.InverseLerp(ayarlar.OlumculSicaklik, 1f, YuzeySicakligi);
            kumdaKalan = Mathf.RoundToInt(yavruSayisi * fazla);
            int bolge = kum.Bolge(yuvaKonumu);
            for (int i = 0; i < kumdaKalan; i++)
                kumsal.KayipIziEkle(new KayipIzi(bolge, yuvaKonumu, KayipNedeni.SicakKum));
            nedenler[(int)KayipNedeni.SicakKum] += kumdaKalan;

            int kalan = yavruSayisi - kumdaKalan;
            // Hepsi kaldıysa sonuç paneli nedeni söyler; bir kısmıysa koşu başlarken söylenir.
            if (kumdaKalan > 0 && kalan > 0) Ipucu.Bildir("bildirim_sicak_kum", kumdaKalan);
            if (kalan <= 0)
            {
                GeceBitti();
                return;
            }

            // Yavrular denize bakarak çıkar.
            Vector3 denize = kum.transform.TransformDirection(Vector3.back);
            suru.Kur(yuvaKonumu, Quaternion.LookRotation(denize), kalan);
            suruKuruldu = true;
            gunDogumu.Baslat();
            SuAnki = Asama.Kosu;
            Ipucu.Goster("ipucu_suru");
            KosuBasladi?.Invoke();
        }

        private void DalgayaBak()
        {
            foreach (Yavru yavru in suru.Yavrular)
            {
                if (yavru.SuAnki != Yavru.Durum.Suruyor) continue;
                if (dalga.Cekiliyor) yavru.BuDalgadaSavruldu = false;
                if (kum.KiyidanUzaklik(yavru.transform.position) >= dalga.SuKenari) continue;

                // Suyun içindeki yavru dalga çekilince kurtulur. Dalga vururken giren yavru bir kez geri savrulur,
                // sonra suda kalıp çekilmeyi bekler; her karede itilmez. Takılı ya da ters dönmüş yavru kendi kurtulamaz.
                if (yavru.EngelDurumu == Yavru.Engel.Takili || yavru.EngelDurumu == Yavru.Engel.Ters) continue;
                if (dalga.Cekiliyor) yavru.Kurtar();
                else if (!yavru.BuDalgadaSavruldu)
                {
                    yavru.Savrul(kum.transform.forward * kacisAyarlari.SavrulmaMesafesi);
                    yavru.BuDalgadaSavruldu = true;
                }
            }
        }

        /// <summary>Gün doğumu yaklaşınca bir kez uyarır (kumda kalanlar kaybedilecek).</summary>
        private void GunDogumunaBak()
        {
            if (gunUyarildi || gunDogumu.Oran < kacisAyarlari.GunDogumuUyarisi) return;
            gunUyarildi = true;
            Ipucu.Bildir("bildirim_gun_doguyor", suru.Kalan);
        }

        private void YavruKaybedildi(Yavru yavru, KayipNedeni neden)
        {
            nedenler[(int)neden]++;
            Vector3 yer = yavru.transform.position;
            kumsal.KayipIziEkle(new KayipIzi(kum.Bolge(yer), yer, neden));
        }

        private void YavruKurtuldu(Yavru yavru) => kumsal.DenizeUlasti(1);

        private void GunDogdu()
        {
            // Gün doğduğunda kumsalda kalan yavrular kaybedilir; neden, o an başına gelendir.
            foreach (Yavru yavru in suru.Yavrular)
                if (yavru.SuAnki == Yavru.Durum.Suruyor) yavru.Kaybet(yavru.GunDogumuNedeni);
        }

        private void GeceBitti()
        {
            if (SuAnki == Asama.Bitti) return;
            SuAnki = Asama.Bitti;
            sonucKapisi.Ac();
            gunDogumu.Durdur();
            suru.Durdur();

            int kurtulan = suruKuruldu ? suru.Kurtulan : 0;
            int kaybedilen = kumdaKalan + (suruKuruldu ? suru.Kaybedilen : 0);
            int disi = Mathf.RoundToInt(kurtulan * ayarlar.DisiOrani(yuvaBandi));
            var sonuc = new Sonuc(kurtulan, kaybedilen, disi, (int[])nedenler.Clone());
            if (Debug.isDebugBuild) Debug.Log($"KacisGecesi: gece bitti — kurtulan {sonuc.Kurtulan} (dişi {sonuc.Disi}, erkek {sonuc.Erkek}), kaybedilen {sonuc.Kaybedilen}.");
            SonucHazir?.Invoke(sonuc);
        }

        private void SonucuKapat()
        {
            if (TekrarGerekli)
            {
                kumsal.KayipIzleriniSonrasindanSil(geceIziBaslangici);
                SonucKapandi?.Invoke();
                Basla(OyunAkisi.SuAnkiAdim.Sira, true);
                return;
            }
            // Geçiş sürerken kabul edilmezse sonuç açık kalır, sonraki tıklamada tekrar denenir.
            if (OyunAkisi.Var && !OyunAkisi.AdimBitti()) return;
            SuAnki = Asama.Kapali;
            SonucKapandi?.Invoke();
        }
    }
}
