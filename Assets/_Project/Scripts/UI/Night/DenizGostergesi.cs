using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.UI.Night
{
    /// <summary>Denizde nefes çubuğu (nefes varsa), ağdan kurtulma çubuğu (E basışları) ve yavaşlık çubuğu (poşet, plastik; biterken boşalır).</summary>
    public class DenizGostergesi : MonoBehaviour
    {
        [SerializeField] private DenizHareketi yuzucu;
        [SerializeField] private Cubuk nefes;
        [SerializeField] private Cubuk kurtulma;
        [Tooltip("İsteğe bağlı: poşet yutunca ya da plastiğe değince kalan yavaşlık.")]
        [SerializeField] private Cubuk yavaslik;

        private void Update()
        {
            if (nefes.gameObject.activeSelf != yuzucu.NefesVar) nefes.gameObject.SetActive(yuzucu.NefesVar);
            if (yuzucu.NefesVar) nefes.Ayarla(yuzucu.Nefes);

            if (kurtulma.gameObject.activeSelf != yuzucu.Takili) kurtulma.gameObject.SetActive(yuzucu.Takili);
            if (yuzucu.Takili) kurtulma.Ayarla(1f - (float)yuzucu.KalanBasis / yuzucu.ToplamBasis);

            if (yavaslik == null) return;
            bool yavas = yuzucu.Yavaslik > 0f;
            if (yavaslik.gameObject.activeSelf != yavas) yavaslik.gameObject.SetActive(yavas);
            if (yavas) yavaslik.Ayarla(yuzucu.Yavaslik);
        }
    }
}
