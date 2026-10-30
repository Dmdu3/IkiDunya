using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.UI.Night
{
    /// <summary>Kazma, yumurtlama ve örtme sırasında ekranın ortasında çıkan çubuk; aşamaya göre renk değiştirir.</summary>
    public class KazmaGostergesi : MonoBehaviour
    {
        [SerializeField] private AnneKazma kazma;
        [SerializeField] private Cubuk cubuk;
        [SerializeField] private Color kazmaRengi = new Color(0.85f, 0.7f, 0.4f);
        [SerializeField] private Color yumurtlamaRengi = new Color(1f, 1f, 0.9f);
        [SerializeField] private Color ortmeRengi = new Color(0.6f, 0.5f, 0.3f);

        private void Update()
        {
            AnneKazma.Asama asama = kazma.SuAnki;
            bool gorunsun = asama == AnneKazma.Asama.Kaziyor || asama == AnneKazma.Asama.Yumurtluyor || asama == AnneKazma.Asama.Ortuyor;
            if (cubuk.gameObject.activeSelf != gorunsun) cubuk.gameObject.SetActive(gorunsun);
            if (!gorunsun) return;

            cubuk.Ayarla(kazma.Oran);
            cubuk.Renk(asama == AnneKazma.Asama.Kaziyor ? kazmaRengi : asama == AnneKazma.Asama.Yumurtluyor ? yumurtlamaRengi : ortmeRengi);
        }
    }
}
