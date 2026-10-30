using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Basit doluluk çubuğu: dolgunun sağ (ya da üst) kenarı orana göre kayar; rengi değiştirilebilir.</summary>
    public class Cubuk : MonoBehaviour
    {
        [SerializeField] private RectTransform dolgu;
        [SerializeField] private Image dolguResmi;
        [Tooltip("Açıksa aşağıdan yukarı dolar.")]
        [SerializeField] private bool dikey;

        public void Ayarla(float oran)
        {
            oran = Mathf.Clamp01(oran);
            dolgu.anchorMax = dikey ? new Vector2(dolgu.anchorMax.x, oran) : new Vector2(oran, dolgu.anchorMax.y);
        }

        public void Renk(Color renk) => dolguResmi.color = renk;
    }
}
