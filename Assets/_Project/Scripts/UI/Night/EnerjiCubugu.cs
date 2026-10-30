using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.UI.Night
{
    /// <summary>Ekrandaki enerji çubuğu: dolgu objesinin genişliği enerji oranını izler.</summary>
    public class EnerjiCubugu : MonoBehaviour
    {
        [SerializeField] private Enerji enerji;
        [Tooltip("Sol kenara sabitli dolgu; sağ kenarı orana göre kayar.")]
        [SerializeField] private RectTransform dolgu;

        private void OnEnable()
        {
            enerji.Degisti += Guncelle;
            Guncelle(enerji.Oran);
        }

        private void OnDisable()
        {
            enerji.Degisti -= Guncelle;
        }

        private void Guncelle(float oran)
        {
            dolgu.anchorMax = new Vector2(oran, dolgu.anchorMax.y);
        }
    }
}
