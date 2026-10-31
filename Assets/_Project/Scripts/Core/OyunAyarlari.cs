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

        [Header("Akış (GDD: Oyun akışı, İnsan kısmı)")]
        [SerializeField] private int yuvaGecesiSayisi = 3;
        [SerializeField] private int kacisGecesiSayisi = 3;
        [SerializeField] private int gecelerArasiTur = 1;
        [SerializeField] private int bekleyisTurSayisi = 6;
        [SerializeField] private int bolum5OncesiTur = 1;

        [Header("Kumsal")]
        [SerializeField] private int bolgeSayisi = 8;
        [SerializeField] private int katmanEnFazla = 3;
        [Tooltip("Yeni oyunda kumsalın ilk hali. Boşsa kumsal bomboş başlar.")]
        [SerializeField] private KumsalBaslangici kumsalBaslangici;

        public string MenuSahnesi => menuSahnesi;
        public int BolgeSayisi => bolgeSayisi;
        public int KatmanEnFazla => katmanEnFazla;
        public KumsalBaslangici KumsalBaslangici => kumsalBaslangici;

        /// <summary>Kumsalı yeni oyunun ilk haline getirir.</summary>
        public void KumsaliSifirla(KumsalDurumu kumsal)
        {
            if (kumsalBaslangici != null) kumsal.Sifirla(kumsalBaslangici, katmanEnFazla);
            else kumsal.Sifirla(bolgeSayisi, katmanEnFazla);
        }

        public string SahneAdi(AdimTuru tur)
        {
            switch (tur)
            {
                case AdimTuru.Donus: return bolum1Sahnesi;
                case AdimTuru.IlkYuzus: return bolum5Sahnesi;
                default: return kumsalSahnesi;
            }
        }

        /// <summary>Oyunun baştan sona adımları: Bölüm 1, yuva geceleri ve turlar, bekleyiş, kaçış geceleri ve turlar, Bölüm 5.</summary>
        public List<Adim> AkisiKur()
        {
            var akis = new List<Adim>();
            int tur = 0;

            akis.Add(new Adim(AdimTuru.Donus, 1, 0));

            for (int gece = 1; gece <= yuvaGecesiSayisi; gece++)
            {
                akis.Add(new Adim(AdimTuru.YuvaGecesi, 2, gece));
                if (gece < yuvaGecesiSayisi)
                    for (int i = 0; i < gecelerArasiTur; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 2, ++tur));
            }

            for (int i = 0; i < bekleyisTurSayisi; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 3, ++tur));

            for (int gece = 1; gece <= kacisGecesiSayisi; gece++)
            {
                akis.Add(new Adim(AdimTuru.KacisGecesi, 4, gece));
                int turSayisi = gece < kacisGecesiSayisi ? gecelerArasiTur : bolum5OncesiTur;
                for (int i = 0; i < turSayisi; i++) akis.Add(new Adim(AdimTuru.InsanTuru, 4, ++tur));
            }

            akis.Add(new Adim(AdimTuru.IlkYuzus, 5, 0));
            return akis;
        }
    }

    /// <summary>Akıştaki bir adımın türü.</summary>
    public enum AdimTuru { Donus, YuvaGecesi, InsanTuru, KacisGecesi, IlkYuzus }

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
