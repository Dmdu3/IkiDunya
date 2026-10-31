using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Gönüllü görevinin etkisi.</summary>
    /// <remarks>Asset'lerde sayı olarak saklanır: yeni etki hep sona eklenir.</remarks>
    public enum GorevEtkisi { Temizlik, Devriye, Kafes, GolgeSulama, Bilgilendirme }

    /// <summary>Bir gönüllü görevi (GDD: Gönüllü görevleri). Her atama bir gönüllü harcar.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Gönüllü Görevi", fileName = "Gorev")]
    public class GorevTanimi : ScriptableObject
    {
        [Tooltip("Metin id'si (ör. gorev_temizlik); açıklaması id_aciklama.")]
        [SerializeField] private string id;
        [SerializeField] private GorevEtkisi etki;
        [SerializeField] private Hedef hedef;

        public string Id => id;
        public GorevEtkisi Etki => etki;
        public Hedef Hedef => hedef;

        public void Kur(string yeniId, GorevEtkisi yeniEtki, Hedef yeniHedef)
        {
            id = yeniId;
            etki = yeniEtki;
            hedef = yeniHedef;
        }
    }
}
