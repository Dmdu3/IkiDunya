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
        [SerializeField] private bool isikYasagi;
        [SerializeField] private bool temizEnerji;
        [SerializeField] private int denizeUlasanYavru;
        [SerializeField] private List<GeciciEtki> geciciEtkiler = new List<GeciciEtki>();

        public event Action Degisti;
        /// <summary>İnsan kısmında bir kayıp izine tıklanıp kanıt açıldığında; kartı açacak olan insan kısmı dinler.</summary>
        public event Action<KayipIzi> KanitAcildi;

        public int BolgeSayisi => bolgeler.Count;
        public IReadOnlyList<Yuva> Yuvalar => yuvalar;
        public IReadOnlyList<KayipIzi> KayipIzleri => kayipIzleri;
        public int DenizdekiAg => denizdekiAg;
        /// <summary>Bütün kumun kaç bant ısındığı (sıcak dalgası +1); kum sıcaklığına eklenir.</summary>
        public int KumIsinmasi => kumIsinmasi;
        /// <summary>Işık Yasağı kartı: otel ve yol ışıkları geceleri kapalı.</summary>
        public bool IsikYasagi => isikYasagi;
        /// <summary>Temiz Enerji kartı oynandı mı: kısa vadede etkisi yok, final sahnesi değişir.</summary>
        public bool TemizEnerji => temizEnerji;
        public int DenizeUlasanYavru => denizeUlasanYavru;

        public Bolge BolgeAl(int bolge) => bolgeler[bolge];

#if UNITY_EDITOR
        // Editörde Play sırasındaki değişiklikler asset'e yazılmasın (git'te sürekli değişmiş görünmesin):
        // Play başında hal saklanır, Play bitince geri yüklenir.
        private string playOncesi;

        private void OnEnable() => UnityEditor.EditorApplication.playModeStateChanged += OynatmaDegisti;
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OynatmaDegisti;

        private void OynatmaDegisti(UnityEditor.PlayModeStateChange durum)
        {
            if (durum == UnityEditor.PlayModeStateChange.ExitingEditMode) playOncesi = JsonUtility.ToJson(this);
            else if (durum == UnityEditor.PlayModeStateChange.EnteredEditMode && playOncesi != null)
            {
                JsonUtility.FromJsonOverwrite(playOncesi, this);
                playOncesi = null;
            }
        }
#endif

        /// <summary>Yeni oyunda kumsalı boşaltır. Oyun akışı çağırır.</summary>
        public void Sifirla(int bolgeSayisi, int katmanEnFazla)
        {
            Bosalt(bolgeSayisi, katmanEnFazla);
            Degisti?.Invoke();
        }

        /// <summary>Yeni oyunda kumsalı ilk haliyle kurar (çöp, ışık, şezlong, insan, araç izi, denizdeki ağ).</summary>
        public void Sifirla(KumsalBaslangici baslangic, int katmanEnFazla)
        {
            Bosalt(baslangic.Bolgeler.Count, katmanEnFazla);
            for (int i = 0; i < bolgeler.Count; i++)
            {
                KumsalBaslangici.BolgeBaslangici ilk = baslangic.Bolgeler[i];
                foreach (BolgeKatmani katman in Enum.GetValues(typeof(BolgeKatmani)))
                    bolgeler[i].KatmanEkle(katman, ilk.Seviye(katman));
                if (ilk.AracIzi) bolgeler[i].YapiAyarla(BolgeYapisi.AracIzi, true);
            }
            denizdekiAg = baslangic.DenizdekiAg;
            Degisti?.Invoke();
        }

        private void Bosalt(int bolgeSayisi, int katmanEnFazla)
        {
            bolgeler.Clear();
            for (int i = 0; i < bolgeSayisi; i++) bolgeler.Add(new Bolge(katmanEnFazla));
            yuvalar.Clear();
            kayipIzleri.Clear();
            denizdekiAg = 0;
            kumIsinmasi = 0;
            isikYasagi = false;
            temizEnerji = false;
            denizeUlasanYavru = 0;
            geciciEtkiler.Clear();
        }

        // ---- Bölgeler (çoğunlukla insan kısmı) ----

        public void KatmanDegistir(int bolge, BolgeKatmani katman, int fark)
        {
            int once = bolgeler[bolge].Seviye(katman);
            bolgeler[bolge].KatmanEkle(katman, fark);
            // Sınırda kırpılan kısım, aynı yöndeki süreli etkinin kapladığı yerdir: o kadarı kalıcıya geçer, süre bitince geri alınmaz.
            int kirpilan = fark - (bolgeler[bolge].Seviye(katman) - once);
            if (kirpilan != 0) GeciciKatmaniKalicilastir(bolge, katman, kirpilan);
            Degisti?.Invoke();
        }

        /// <summary>Bölgede insanı süreli azaltan bir etki sürüyor mu (ör. Plaj Tabelaları); kumsalda tabela olarak görünür.</summary>
        public bool InsanSureliAzaldi(int bolge) =>
            geciciEtkiler.Exists(e => e.Tur == GeciciEtki.Turu.Katman && e.Hedef == bolge && e.Alt == (int)BolgeKatmani.Insan && e.Fark < 0);

        /// <summary>Bölgenin katmanını kalıcı olarak sıfırlar; bekleyen süreli etkileri de düşer (ör. koruma alanı: süre bitince insan geri gelmez).</summary>
        public void KatmanSifirla(int bolge, BolgeKatmani katman)
        {
            bolgeler[bolge].KatmanEkle(katman, -bolgeler[bolge].Seviye(katman));
            geciciEtkiler.RemoveAll(e => e.Tur == GeciciEtki.Turu.Katman && e.Hedef == bolge && e.Alt == (int)katman);
            Degisti?.Invoke();
        }

        private void GeciciKatmaniKalicilastir(int bolge, BolgeKatmani katman, int kalan)
        {
            for (int i = geciciEtkiler.Count - 1; i >= 0 && kalan != 0; i--)
            {
                GeciciEtki etki = geciciEtkiler[i];
                if (etki.Tur != GeciciEtki.Turu.Katman || etki.Hedef != bolge || etki.Alt != (int)katman) continue;
                if (Math.Sign(etki.Fark) != Math.Sign(kalan)) continue;
                int alinan = Math.Sign(kalan) * Math.Min(Math.Abs(kalan), Math.Abs(etki.Fark));
                etki.Fark -= alinan;
                kalan -= alinan;
                if (etki.Fark == 0) geciciEtkiler.RemoveAt(i);
            }
        }

        public void YapiKoy(int bolge, BolgeYapisi yapi)
        {
            bolgeler[bolge].YapiAyarla(yapi, true);
            // Süreli olarak konmuşsa artık kalıcı: süre bitince kaldırılmaz.
            YapiEtkileriniSil(bolge, yapi);
            Degisti?.Invoke();
        }

        public void YapiKaldir(int bolge, BolgeYapisi yapi)
        {
            bolgeler[bolge].YapiAyarla(yapi, false);
            YapiEtkileriniSil(bolge, yapi);
            Degisti?.Invoke();
        }

        private void YapiEtkileriniSil(int bolge, BolgeYapisi yapi) =>
            geciciEtkiler.RemoveAll(e => e.Tur == GeciciEtki.Turu.Yapi && e.Hedef == bolge && e.Alt == (int)yapi);

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

        public void IsikYasagiAyarla(bool var)
        {
            isikYasagi = var;
            Degisti?.Invoke();
        }

        public void TemizEnerjiAyarla(bool var)
        {
            temizEnerji = var;
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

        /// <summary>Yuvayı taşır. Bant yeni yerin zemininden (ısınma hariç) baştan kurulur; süresi bitmemiş etkiler (gölge, sulama) sürer.</summary>
        public void YuvaTasi(int yuva, int bolge, Vector3 konum, bool dalgaAltinda, SicaklikBandi zeminBandi)
        {
            int gecici = 0;
            foreach (GeciciEtki etki in geciciEtkiler)
                if (etki.Tur == GeciciEtki.Turu.YuvaBant && etki.Hedef == yuva) gecici += etki.Fark;
            yuvalar[yuva].Tasi(bolge, konum, dalgaAltinda, zeminBandi, gecici);
            Degisti?.Invoke();
        }

        /// <summary>Yuvanın şu anki bandı: kazıldığı zemin, sonradan gelen farklar ve bütün kumun ısınması. Kaçış gecesi ve harita bunu okur.</summary>
        public SicaklikBandi YuvaBandi(int yuva) => YuvaBandi(yuvalar[yuva]);

        public SicaklikBandi YuvaBandi(Yuva yuva) =>
            (SicaklikBandi)Mathf.Clamp((int)yuva.Bant + kumIsinmasi, (int)SicaklikBandi.Mavi, (int)SicaklikBandi.Kirmizi);

        public void YuvaYumurtaKaybet(int yuva, int adet)
        {
            yuvalar[yuva].Yumurta = Mathf.Max(0, yuvalar[yuva].Yumurta - adet);
            Degisti?.Invoke();
        }

        /// <summary>Yuvanın sıcaklığını kalıcı olarak bant bant değiştirir (ör. bölgesine gölgelik kondu).</summary>
        public void YuvaBantDegistir(int yuva, int fark)
        {
            yuvalar[yuva].BantFarki += fark;
            Degisti?.Invoke();
        }

        /// <summary>İnsan turu sonunda: bütün yuvaların çıkışına bir hafta yaklaşır, süreli etkilerin süresi azalır, biten geri alınır.</summary>
        public void BirHaftaGecti()
        {
            foreach (var yuva in yuvalar) yuva.CikisinaHafta = Mathf.Max(0, yuva.CikisinaHafta - 1);
            for (int i = geciciEtkiler.Count - 1; i >= 0; i--)
            {
                GeciciEtki etki = geciciEtkiler[i];
                if (--etki.KalanTur > 0) continue;
                Uygula(etki, -1);
                geciciEtkiler.RemoveAt(i);
            }
            Degisti?.Invoke();
        }

        // ---- Süreli etkiler: "o tur", "bir gece" ----
        // Süre tur sayısıdır; 1 = sıradaki BirHaftaGecti'de geri alınır, yani bu turun ardından gelen gece boyunca sürer.

        /// <summary>Bir bölgenin katmanını süreli değiştirir (ör. festival: bir gece ışık ve insan).</summary>
        public void GeciciKatman(int bolge, BolgeKatmani katman, int fark, int tur)
        {
            // Sınırda kırpılırsa geri alınacak olan, gerçekten uygulanan fark.
            int once = bolgeler[bolge].Seviye(katman);
            bolgeler[bolge].KatmanEkle(katman, fark);
            Ekle(new GeciciEtki(GeciciEtki.Turu.Katman, bolge, (int)katman, bolgeler[bolge].Seviye(katman) - once, tur), false);
        }

        /// <summary>Bir bölgeye süreli yapı koyar (ör. gece devriyesi). Yapı zaten varsa süresi sonunda kalkmaz.</summary>
        public void GeciciYapi(int bolge, BolgeYapisi yapi, int tur)
        {
            if (bolgeler[bolge].Var(yapi)) return;
            Ekle(new GeciciEtki(GeciciEtki.Turu.Yapi, bolge, (int)yapi, 1, tur), true);
        }

        /// <summary>Bütün kumu süreli ısıtır (ör. sıcak dalgası: gelecek tur 1 bant).</summary>
        public void GeciciKumIsinmasi(int fark, int tur) =>
            Ekle(new GeciciEtki(GeciciEtki.Turu.KumIsinmasi, -1, 0, fark, tur), true);

        /// <summary>Bir yuvanın sıcaklığını süreli değiştirir (ör. gölge ve sulama: o tur 1 bant serin).</summary>
        public void GeciciYuvaBant(int yuva, int fark, int tur) =>
            Ekle(new GeciciEtki(GeciciEtki.Turu.YuvaBant, yuva, 0, fark, tur), true);

        private void Ekle(GeciciEtki etki, bool uygula)
        {
            if (uygula) Uygula(etki, 1);
            geciciEtkiler.Add(etki);
            Degisti?.Invoke();
        }

        /// <summary>Etkiyi uygular (yon 1) ya da geri alır (yon -1).</summary>
        private void Uygula(GeciciEtki etki, int yon)
        {
            switch (etki.Tur)
            {
                case GeciciEtki.Turu.Katman:
                    bolgeler[etki.Hedef].KatmanEkle((BolgeKatmani)etki.Alt, etki.Fark * yon);
                    break;
                case GeciciEtki.Turu.Yapi:
                    bolgeler[etki.Hedef].YapiAyarla((BolgeYapisi)etki.Alt, yon > 0);
                    break;
                case GeciciEtki.Turu.KumIsinmasi:
                    kumIsinmasi += etki.Fark * yon;
                    break;
                case GeciciEtki.Turu.YuvaBant:
                    if (etki.Hedef < yuvalar.Count) yuvalar[etki.Hedef].BantFarki += etki.Fark * yon;
                    break;
            }
        }

        // ---- Kaplumbağadan insana ----

        public void KayipIziEkle(KayipIzi iz)
        {
            kayipIzleri.Add(iz);
            Degisti?.Invoke();
        }

        public void KanitAc(int iz)
        {
            if (kayipIzleri[iz].KanitAcildi) return;
            kayipIzleri[iz].KanitAcildi = true;
            KanitAcildi?.Invoke(kayipIzleri[iz]);
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
    /// <remarks>Kayıtta sayı olarak saklanır: yeni yapı hep sona eklenir.</remarks>
    public enum BolgeYapisi { Golgelik, KumulOnarimi, TilkiCiti, AracIzi, Devriye }

    /// <summary>Termal görünümdeki kum sıcaklığı bandı.</summary>
    public enum SicaklikBandi { Mavi, Yesil, Turuncu, Kirmizi }

    /// <summary>Yavrunun kaybedilme nedeni; kanıtı belirler.</summary>
    public enum KayipNedeni { SicakKum, Cop, Ag, Isik, Yengec, Marti, Tilki, Dalga }

    /// <summary>Kayıp izinden açılan kanıt (GDD: İnsan kısmı, Kanıtlar).</summary>
    public enum KanitTuru { Isi, Isik, Cop, Avci, Su }

    /// <summary>"O tur", "bir gece" süren etki; süresi bitince BirHaftaGecti geri alır.</summary>
    [Serializable]
    public class GeciciEtki
    {
        public enum Turu { Katman, Yapi, KumIsinmasi, YuvaBant }

        [SerializeField] private Turu tur;
        [Tooltip("Bölge ya da yuva sırası.")]
        [SerializeField] private int hedef;
        [Tooltip("Katman ya da yapı (enum sırası).")]
        [SerializeField] private int alt;
        [SerializeField] private int fark;
        [SerializeField] private int kalanTur;

        public GeciciEtki(Turu tur, int hedef, int alt, int fark, int kalanTur)
        {
            this.tur = tur;
            this.hedef = hedef;
            this.alt = alt;
            this.fark = fark;
            this.kalanTur = kalanTur;
        }

        public Turu Tur => tur;
        public int Hedef => hedef;
        public int Alt => alt;
        public int Fark { get => fark; internal set => fark = value; }
        public int KalanTur { get => kalanTur; internal set => kalanTur = value; }
    }

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
        [SerializeField] private int bantFarki;

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
        /// <summary>Zemin bandı (kazıldığı ya da taşındığı yer, ısınma hariç) ve sonradan gelen farklar (gölgelik, gölge, sulama).
        /// Bütün kumun ısınması dahil değil; güncel bant için KumsalDurumu.YuvaBandi.</summary>
        public SicaklikBandi Bant => (SicaklikBandi)Mathf.Clamp((int)bant + bantFarki, (int)SicaklikBandi.Mavi, (int)SicaklikBandi.Kirmizi);
        public SicaklikBandi KazildigiBant => bant;
        public int BantFarki { get => bantFarki; internal set => bantFarki = value; }
        public bool DalgaAltinda => dalgaAltinda;
        public int Yumurta { get => yumurta; internal set => yumurta = value; }
        public int CikisinaHafta { get => cikisinaHafta; internal set => cikisinaHafta = value; }
        public bool Kafesli { get => kafesli; internal set => kafesli = value; }

        internal void Tasi(int yeniBolge, Vector3 yeniKonum, bool yeniDalgaAltinda, SicaklikBandi zeminBandi, int geciciFark)
        {
            bolge = yeniBolge;
            konum = yeniKonum;
            dalgaAltinda = yeniDalgaAltinda;
            bant = zeminBandi;
            bantFarki = geciciFark;
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

        /// <summary>Bu izden açılacak kanıt (GDD: Isı, Işık, Çöp, Avcı, Su).</summary>
        public KanitTuru Kanit
        {
            get
            {
                switch (neden)
                {
                    case KayipNedeni.SicakKum: return KanitTuru.Isi;
                    case KayipNedeni.Isik: return KanitTuru.Isik;
                    case KayipNedeni.Cop:
                    case KayipNedeni.Ag: return KanitTuru.Cop;
                    case KayipNedeni.Dalga: return KanitTuru.Su;
                    default: return KanitTuru.Avci;
                }
            }
        }
        public bool KanitAcildi { get => kanitAcildi; internal set => kanitAcildi = value; }
    }
}
