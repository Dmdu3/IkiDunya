using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyun akışının ayarları: sahne adları, gece ve tur sayıları, kumsal bölge sayısı.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Oyun Ayarları", fileName = "OyunAyarlari")]
    public class OyunAyarlari : ScriptableObject
    {
        [Header("Sahneler (Build Settings'te olmalı)")]
        [SerializeField] private string menuSahnesi = "Menu";
        [SerializeField] private string bolum1Sahnesi = "Bolum1_Donus";
        [SerializeField] private string kumsalSahnesi = "Kumsal";
        [SerializeField] private string bolum5Sahnesi = "Bolum5_IlkYuzus";
        [SerializeField] private string posetSurusuSahnesi = "Parkur_PosetSurusu";
        [SerializeField] private string copSeliSahnesi = "Parkur_CopSeli";
        [SerializeField] private string copGirdabiSahnesi = "Parkur_CopGirdabi";

        [Header("Akış (GDD: Oyun akışı, İnsan kısmı)")]
        [SerializeField] private int yuvaGecesiSayisi = 3;
        [SerializeField] private int kacisGecesiSayisi = 3;
        [SerializeField] private int gecelerArasiTur = 1;
        [SerializeField] private int bekleyisTurSayisi = 6;
        [SerializeField] private int bolum5OncesiTur = 1;
        [Tooltip("Fırtınadan sonraki kaçış gecesi: bu gece çöp seli parkurunda oynanır.")]
        [SerializeField] private int selGecesi = 2;

        [Header("Geçiş")]
        [Tooltip("Ekranın kararma ya da açılma süresi (sn).")]
        [SerializeField] private float gecisSuresi = 0.6f;
        [Tooltip("Yeni bölümün başlığının ekranda kalma süresi (sn).")]
        [SerializeField] private float baslikSuresi = 2.5f;
        [Tooltip("Bir ipucunun ekranda kaldığı süre (sn).")]
        [SerializeField] private float ipucuSuresi = 6f;
        [Tooltip("Bir bildirimin (deneme bitti, kontrol noktasına dönüldü) ekranda kaldığı süre (sn).")]
        [SerializeField] private float bildirimSuresi = 3.5f;
        [Tooltip("Gece sonuç ekranının kapatılamadan açık kaldığı en kısa süre (sn); son anda basılan tuş sonucu geçmesin.")]
        [SerializeField] private float sonucEnAzSure = 1f;
        [Tooltip("Oyuncunun gördüğü yazılar (İki Dünya → Metinleri aktar).")]
        [SerializeField] private MetinTablosu metinler;
        [Tooltip("Arayüzün yazı tipleri ve renkleri (Menü ve duraklatmayı kur bağlar).")]
        [SerializeField] private ArayuzTemasi tema;

        [Header("Kumsal")]
        [SerializeField] private int bolgeSayisi = 8;
        [SerializeField] private int katmanEnFazla = 3;
        [Tooltip("Yeni oyunda kumsalın ilk hali. Boşsa kumsal bomboş başlar.")]
        [SerializeField] private KumsalBaslangici kumsalBaslangici;

        public string MenuSahnesi => menuSahnesi;
        public float GecisSuresi => gecisSuresi;
        public float BaslikSuresi => baslikSuresi;
        public float IpucuSuresi => ipucuSuresi;
        public float BildirimSuresi => bildirimSuresi;
        public float SonucEnAzSure => sonucEnAzSure;
        public MetinTablosu Metinler => metinler;
        public ArayuzTemasi Tema => tema;
        public int BolgeSayisi => bolgeSayisi;
        public int KatmanEnFazla => katmanEnFazla;
        public int SelGecesi => selGecesi;
        public KumsalBaslangici KumsalBaslangici => kumsalBaslangici;

        /// <summary>Kumsalı yeni oyunun ilk haline getirir.</summary>
        public void KumsaliSifirla(KumsalDurumu kumsal)
        {
            if (kumsalBaslangici != null) kumsal.Sifirla(kumsalBaslangici, katmanEnFazla);
            else kumsal.Sifirla(bolgeSayisi, katmanEnFazla);
        }

        /// <summary>Parkurun (minigame) kendi sahnesi.</summary>
        public string ParkurSahnesi(ParkurTuru parkur)
        {
            switch (parkur)
            {
                case ParkurTuru.PosetSurusu: return posetSurusuSahnesi;
                case ParkurTuru.CopSeli: return copSeliSahnesi;
                default: return copGirdabiSahnesi;
            }
        }

        public string SahneAdi(Adim adim)
        {
            if (adim.Tur == AdimTuru.Parkur) return ParkurSahnesi((ParkurTuru)adim.Sira);
            switch (adim.Tur)
            {
                case AdimTuru.Donus: return bolum1Sahnesi;
                case AdimTuru.IlkYuzus: return bolum5Sahnesi;
                default: return kumsalSahnesi;
            }
        }

        /// <summary>Oyunun baştan sona adımları: Bölüm 1, yuva geceleri ve turlar, bekleyiş, kaçış geceleri ve turlar, Bölüm 5, final.</summary>
        public List<Adim> AkisiKur()
        {
            var akis = new List<Adim>();
            int tur = 0;

            // Bölüm 1 akıntı kapısında biter; poşet sürüsü parkuru zorunlu, ardından kumsal.
            akis.Add(new Adim(AdimTuru.Donus, 1, 0));
            akis.Add(new Adim(AdimTuru.Parkur, 1, (int)ParkurTuru.PosetSurusu));

            for (int gece = 1; gece <= yuvaGecesiSayisi; gece++)
            {
                akis.Add(new Adim(AdimTuru.YuvaGecesi, 2, gece));
                if (gece < yuvaGecesiSayisi)
                    for (int i = 0; i < gecelerArasiTur; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 2, ++tur));
            }

            for (int i = 0; i < bekleyisTurSayisi; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 3, ++tur));

            for (int gece = 1; gece <= kacisGecesiSayisi; gece++)
            {
                // Fırtınadan sonraki gece çöp seli parkurunda (o gecenin yuvasından).
                akis.Add(gece == selGecesi ? new Adim(AdimTuru.Parkur, 4, (int)ParkurTuru.CopSeli) : new Adim(AdimTuru.KacisGecesi, 4, gece));
                int turSayisi = gece < kacisGecesiSayisi ? gecelerArasiTur : bolum5OncesiTur;
                for (int i = 0; i < turSayisi; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 4, ++tur));
            }

            // Bölüm 5 girdapta bölünür: girdap parkuru, sonra açık denizden (Sira 1) devam.
            akis.Add(new Adim(AdimTuru.IlkYuzus, 5, 0));
            akis.Add(new Adim(AdimTuru.Parkur, 5, (int)ParkurTuru.CopGirdabi));
            akis.Add(new Adim(AdimTuru.IlkYuzus, 5, 1));
            akis.Add(new Adim(AdimTuru.Final, 5, 0));
            return akis;
        }
    }

    /// <summary>Akıştaki bir adımın türü.</summary>
    /// <remarks>Sahnelerde sayı olarak saklanır: yeni tür hep sona eklenir.</remarks>
    public enum AdimTuru { Donus, YuvaGecesi, InsanTuru, KacisGecesi, IlkYuzus, Final, Parkur }

    /// <summary>Oyun akışında bir adım: türü, bölümü ve sırası (gece numarası ya da genel tur numarası).</summary>
    public readonly struct Adim
    {
        public AdimTuru Tur { get; }
        public int Bolum { get; }
        public int Sira { get; }

        public Adim(AdimTuru tur, int bolum, int sira)
        {
            Tur = tur;
            Bolum = bolum;
            Sira = sira;
        }

        public bool KaplumbagaAdimi => Tur != AdimTuru.InsanTuru;

        public override string ToString() => $"Bölüm {Bolum} · {Tur} {Sira}";
    }
}
