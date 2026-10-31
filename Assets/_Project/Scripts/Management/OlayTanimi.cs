using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Olayın türü; Etkiler seçeneğe göre kumsala uygular. Genel: kumsala özel etkisi yok, sadece kaynak ve paydaş.</summary>
    /// <remarks>Asset'lerde sayı olarak saklanır: yeni tür hep sona eklenir.</remarks>
    public enum OlayEtkisi { Otel, Festival, Balikci, SicakDalga, Firtina, Okul, Genel, OtelBar, FestivalCop, Kopek, OtelIsik, BalikciAgAtti }

    /// <summary>Olayın havuza girme koşulu (GDD: Olaylar, Ne zaman).</summary>
    public enum OlayKosulu { Her, PaydasKus, EnAzTur, SadeceZincir }

    /// <summary>Bir olay (GDD: Olaylar): iki seçenekli ya da sadece bilgi (uyarı). Metni id, seçenekleri id_a ve id_b.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Olay", fileName = "Olay")]
    public class OlayTanimi : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private OlayEtkisi etki;
        [Tooltip("Sadece bilgi (sıcak dalgası, fırtına uyarısı): tek düğme, etki gelecek turun sonunda.")]
        [SerializeField] private bool uyari;
        [Tooltip("Halk desteği düşükken A seçeneği kapanır (otel, belediye).")]
        [SerializeField] private bool destekIster;
        [SerializeField] private OlayKosulu kosul;
        [Tooltip("Koşul PaydasKus ise hangi paydaş.")]
        [SerializeField] private Paydas kosulPaydas;
        [Tooltip("Koşul EnAzTur ise kaçıncı turdan sonra.")]
        [SerializeField] private int kosulTur;

        [Header("A seçeneği")]
        [SerializeField] private int aButce;
        [SerializeField] private int aDestek;
        [SerializeField] private PaydasFarki aPaydas;
        [Tooltip("Gelecek tur gönüllü farkı: artı ek gönüllü, eksi meşgul.")]
        [SerializeField] private int aGonullu;
        [Tooltip("Seçilirse sıradaki turun olayı (zincir).")]
        [SerializeField] private OlayTanimi aSonraki;
        [Header("B seçeneği")]
        [SerializeField] private int bButce;
        [SerializeField] private int bDestek;
        [SerializeField] private PaydasFarki bPaydas;
        [SerializeField] private int bGonullu;
        [SerializeField] private OlayTanimi bSonraki;

        public string Id => id;
        public OlayEtkisi Etki => etki;
        public bool Uyari => uyari;
        public bool DestekIster => destekIster;
        public OlayKosulu Kosul => kosul;
        public Paydas KosulPaydas => kosulPaydas;
        public int KosulTur => kosulTur;
        public int Butce(bool a) => a ? aButce : bButce;
        public int Destek(bool a) => a ? aDestek : bDestek;
        public PaydasFarki Paydaslar(bool a) => a ? aPaydas : bPaydas;
        public int Gonullu(bool a) => a ? aGonullu : bGonullu;
        public OlayTanimi Sonraki(bool a) => a ? aSonraki : bSonraki;

        public void Kur(string yeniId, OlayEtkisi yeniEtki, bool yeniUyari, bool destek, int ab, int ad, int bb, int bd)
        {
            id = yeniId;
            etki = yeniEtki;
            uyari = yeniUyari;
            destekIster = destek;
            aButce = ab;
            aDestek = ad;
            bButce = bb;
            bDestek = bd;
        }

        /// <summary>Editör kurulumu: koşul, paydaş ve gönüllü etkileri, zincir.</summary>
        public void Ayrinti(OlayKosulu yeniKosul, Paydas kimden, int enAzTur, PaydasFarki ap, int ag, PaydasFarki bp, int bg)
        {
            kosul = yeniKosul;
            kosulPaydas = kimden;
            kosulTur = enAzTur;
            aPaydas = ap;
            aGonullu = ag;
            bPaydas = bp;
            bGonullu = bg;
        }

        public void ZincirKur(OlayTanimi a, OlayTanimi b)
        {
            aSonraki = a;
            bSonraki = b;
        }
    }
}
