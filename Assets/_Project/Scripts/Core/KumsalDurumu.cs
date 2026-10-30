using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Kumsalın iki tarafça paylaşılan durumu. Sadece açık metotlarla değişir; her değişiklik Degisti ile bildirilir.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kumsal Durumu", fileName = "KumsalDurumu")]
    public class KumsalDurumu : ScriptableObject
    {
        [SerializeField] private List<Bolge> bolgeler = new List<Bolge>();
        [SerializeField] private List<Yuva> yuvalar = new List<Yuva>();
        [SerializeField] private List<KayipIzi> kayipIzleri = new List<KayipIzi>();
        [SerializeField] private int denizdekiAg;
        [SerializeField] private int kumIsinmasi;
        [SerializeField] private int denizeUlasanYavru;

        public event Action Degisti;

        public int BolgeSayisi => bolgeler.Count;
        public IReadOnlyList<Yuva> Yuvalar => yuvalar;
        public IReadOnlyList<KayipIzi> KayipIzleri => kayipIzleri;
        public int DenizdekiAg => denizdekiAg;
        /// <summary>Bütün kumun kaç bant ısındığı (sıcak dalgası +1); kum sıcaklığına eklenir.</summary>
        public int KumIsinmasi => kumIsinmasi;
        public int DenizeUlasanYavru => denizeUlasanYavru;

        public Bolge BolgeAl(int bolge) => bolgeler[bolge];

        /// <summary>Yeni oyunda kumsalı boşaltır. Oyun akışı çağırır.</summary>
        public void Sifirla(int bolgeSayisi, int katmanEnFazla)
        {
            bolgeler.Clear();
            for (int i = 0; i < bolgeSayisi; i++) bolgeler.Add(new Bolge(katmanEnFazla));
            yuvalar.Clear();
            kayipIzleri.Clear();
            denizdekiAg = 0;
            kumIsinmasi = 0;
            denizeUlasanYavru = 0;
            Degisti?.Invoke();
        }

        // ---- Bölgeler (çoğunlukla insan kısmı) ----

        public void KatmanDegistir(int bolge, BolgeKatmani katman, int fark)
        {
            bolgeler[bolge].KatmanEkle(katman, fark);
            Degisti?.Invoke();
        }

        public void YapiKoy(int bolge, BolgeYapisi yapi)
        {
            bolgeler[bolge].YapiAyarla(yapi, true);
            Degisti?.Invoke();
        }

        public void YapiKaldir(int bolge, BolgeYapisi yapi)
        {
            bolgeler[bolge].YapiAyarla(yapi, false);
            Degisti?.Invoke();
        }

        public void DenizdekiAgDegistir(int fark)
        {
            denizdekiAg = Mathf.Max(0, denizdekiAg + fark);
            Degisti?.Invoke();
        }

        public void KumIsinmasiDegistir(int fark)
        {
            kumIsinmasi += fark;
            Degisti?.Invoke();
        }

        // ---- Yuvalar ----

        /// <summary>Yuva gecesinin sonucu (kaplumbağa kısmı). Yeni yuvanın sırasını döner.</summary>
        public int YuvaEkle(Yuva yuva)
        {
            yuvalar.Add(yuva);
            Degisti?.Invoke();
            return yuvalar.Count - 1;
        }

        public void YuvaKafesle(int yuva)
        {
            yuvalar[yuva].Kafesli = true;
            Degisti?.Invoke();
        }

        public void YuvaTasi(int yuva, int bolge, Vector3 konum, bool dalgaAltinda)
        {
            yuvalar[yuva].Tasi(bolge, konum, dalgaAltinda);
            Degisti?.Invoke();
        }

        public void YuvaYumurtaKaybet(int yuva, int adet)
        {
            yuvalar[yuva].Yumurta = Mathf.Max(0, yuvalar[yuva].Yumurta - adet);
            Degisti?.Invoke();
        }

        /// <summary>İnsan turu sonunda bütün yuvaların çıkışına bir hafta yaklaşır.</summary>
        public void BirHaftaGecti()
        {
            foreach (var yuva in yuvalar) yuva.CikisinaHafta = Mathf.Max(0, yuva.CikisinaHafta - 1);
            Degisti?.Invoke();
        }

        // ---- Kaplumbağadan insana ----

        public void KayipIziEkle(KayipIzi iz)
        {
            kayipIzleri.Add(iz);
            Degisti?.Invoke();
        }

        public void KanitAc(int iz)
        {
            kayipIzleri[iz].KanitAcildi = true;
            Degisti?.Invoke();
        }

        public void DenizeUlasti(int adet)
        {
            denizeUlasanYavru += adet;
            Degisti?.Invoke();
        }
    }

    /// <summary>Bölgede seviyesi olan şeyler (0 ile katmanEnFazla arası).</summary>
    public enum BolgeKatmani { Cop, Isik, Sezlong, Insan }

    /// <summary>Bölgede var ya da yok olan yapılar ve izler.</summary>
    public enum BolgeYapisi { Golgelik, KumulOnarimi, TilkiCiti, AracIzi }

    /// <summary>Termal görünümdeki kum sıcaklığı bandı.</summary>
    public enum SicaklikBandi { Mavi, Yesil, Turuncu, Kirmizi }

    /// <summary>Yavrunun kaybedilme nedeni; kanıtı belirler.</summary>
    public enum KayipNedeni { SicakKum, Cop, Ag, Isik, Yengec, Marti, Tilki, Dalga }

    /// <summary>Kumsalın bir bölgesi.</summary>
    [Serializable]
    public class Bolge
    {
        [SerializeField] private int[] katmanlar;
        [SerializeField] private List<BolgeYapisi> yapilar = new List<BolgeYapisi>();
        [SerializeField] private int katmanEnFazla;

        public Bolge(int katmanEnFazla)
        {
            this.katmanEnFazla = katmanEnFazla;
            katmanlar = new int[Enum.GetValues(typeof(BolgeKatmani)).Length];
        }

        public int Seviye(BolgeKatmani katman) => katmanlar[(int)katman];
        public bool Var(BolgeYapisi yapi) => yapilar.Contains(yapi);

        internal void KatmanEkle(BolgeKatmani katman, int fark)
        {
            katmanlar[(int)katman] = Mathf.Clamp(katmanlar[(int)katman] + fark, 0, katmanEnFazla);
        }

        internal void YapiAyarla(BolgeYapisi yapi, bool var)
        {
            if (var && !yapilar.Contains(yapi)) yapilar.Add(yapi);
            if (!var) yapilar.Remove(yapi);
        }
    }

    /// <summary>Annenin kazdığı bir yuva.</summary>
    [Serializable]
    public class Yuva
    {
        [SerializeField] private int bolge;
        [SerializeField] private Vector3 konum;
        [SerializeField] private SicaklikBandi bant;
        [SerializeField] private bool dalgaAltinda;
        [SerializeField] private int yumurta;
        [SerializeField] private int cikisinaHafta;
        [SerializeField] private bool kafesli;

        public Yuva(int bolge, Vector3 konum, SicaklikBandi bant, bool dalgaAltinda, int yumurta, int cikisinaHafta)
        {
            this.bolge = bolge;
            this.konum = konum;
            this.bant = bant;
            this.dalgaAltinda = dalgaAltinda;
            this.yumurta = yumurta;
            this.cikisinaHafta = cikisinaHafta;
        }

        public int Bolge => bolge;
        public Vector3 Konum => konum;
        public SicaklikBandi Bant => bant;
        public bool DalgaAltinda => dalgaAltinda;
        public int Yumurta { get => yumurta; internal set => yumurta = value; }
        public int CikisinaHafta { get => cikisinaHafta; internal set => cikisinaHafta = value; }
        public bool Kafesli { get => kafesli; internal set => kafesli = value; }

        internal void Tasi(int yeniBolge, Vector3 yeniKonum, bool yeniDalgaAltinda)
        {
            bolge = yeniBolge;
            konum = yeniKonum;
            dalgaAltinda = yeniDalgaAltinda;
        }
    }

    /// <summary>Kaybedilen bir yavrunun kumsalda bıraktığı iz; insan kısmında kanıta dönüşür.</summary>
    [Serializable]
    public class KayipIzi
    {
        [SerializeField] private int bolge;
        [SerializeField] private Vector3 konum;
        [SerializeField] private KayipNedeni neden;
        [SerializeField] private bool kanitAcildi;

        public KayipIzi(int bolge, Vector3 konum, KayipNedeni neden)
        {
            this.bolge = bolge;
            this.konum = konum;
            this.neden = neden;
        }

        public int Bolge => bolge;
        public Vector3 Konum => konum;
        public KayipNedeni Neden => neden;
        public bool KanitAcildi { get => kanitAcildi; internal set => kanitAcildi = value; }
    }
}
