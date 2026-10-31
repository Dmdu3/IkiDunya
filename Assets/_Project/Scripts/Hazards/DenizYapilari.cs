using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Denizdeki hayalet ağ ve misinayı KumsalDurumu'ndaki ağ seviyesine göre kurar (Hayalet Ağ Temizliği azaltır).</summary>
    public class DenizYapilari : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private GameObject agKalibi;
        [SerializeField] private GameObject misinaKalibi;
        [Tooltip("Ağ ve misinanın konabileceği yerler; seviye düştükçe sondakiler boş kalır.")]
        [SerializeField] private Transform[] noktalar;

        private void Start()
        {
            int sayi = Mathf.Min(noktalar.Length, kumsal.DenizdekiAg * ayarlar.SeviyeBasinaAg);
            for (int i = 0; i < sayi; i++)
                Instantiate(i % 2 == 0 ? agKalibi : misinaKalibi, noktalar[i].position, noktalar[i].rotation, transform);
        }
    }
}
