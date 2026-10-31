using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.UI.Night
{
    /// <summary>Denizde nefes çubuğu (nefes varsa) ve ağdan kurtulma çubuğu (E basışları).</summary>
    public class DenizGostergesi : MonoBehaviour
    {
        [SerializeField] private DenizHareketi yuzucu;
        [SerializeField] private Cubuk nefes;
        [SerializeField] private Cubuk kurtulma;

        private void Update()
        {
            if (nefes.gameObject.activeSelf != yuzucu.NefesVar) nefes.gameObject.SetActive(yuzucu.NefesVar);
            if (yuzucu.NefesVar) nefes.Ayarla(yuzucu.Nefes);

            if (kurtulma.gameObject.activeSelf != yuzucu.Takili) kurtulma.gameObject.SetActive(yuzucu.Takili);
            if (yuzucu.Takili) kurtulma.Ayarla(1f - (float)yuzucu.KalanBasis / yuzucu.ToplamBasis);
        }
    }
}
