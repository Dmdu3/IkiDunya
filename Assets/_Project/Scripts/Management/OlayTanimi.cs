using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Olayın türü; Etkiler seçeneğe göre uygular.</summary>
    /// <remarks>Asset'lerde sayı olarak saklanır: yeni tür hep sona eklenir.</remarks>
    public enum OlayEtkisi { Otel, Festival, Balikci, SicakDalga, Firtina, Okul }

    /// <summary>Bir olay (GDD: Olaylar): iki seçenekli ya da sadece bilgi (uyarı). Metni id, seçenekleri id_a ve id_b.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Olay", fileName = "Olay")]
    public class OlayTanimi : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private OlayEtkisi etki;
        [Tooltip("Sadece bilgi (sıcak dalgası, fırtına uyarısı): tek düğme, etki gelecek tur.")]
        [SerializeField] private bool uyari;
        [Tooltip("Halk desteği düşükken A seçeneği kapanır (otel, belediye).")]
        [SerializeField] private bool destekIster;

        [Header("A seçeneği")]
        [SerializeField] private int aButce;
        [SerializeField] private int aDestek;
        [Header("B seçeneği")]
        [SerializeField] private int bButce;
        [SerializeField] private int bDestek;

        public string Id => id;
        public OlayEtkisi Etki => etki;
        public bool Uyari => uyari;
        public bool DestekIster => destekIster;
        public int Butce(bool a) => a ? aButce : bButce;
        public int Destek(bool a) => a ? aDestek : bDestek;

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
    }
}
