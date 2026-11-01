using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Denizdeki ay yolu: gerçek ayın yansıması. Ayın önüne bulut geçince (GeceGokyuzu.Bulut) söner; denizi gösteren ışık kaybolur.</summary>
    [RequireComponent(typeof(Renderer))]
    public class AyYansimasi : MonoBehaviour
    {
        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        [Tooltip("Bulut tamken yolun rengi (gece denizine karışır).")]
        [SerializeField] private Color sonukRenk = new Color(0.08f, 0.1f, 0.14f);

        private Renderer gorunus;
        private MaterialPropertyBlock blok;
        private Color acikRenk;
        private float uygulanan = -1f;

        private void Awake()
        {
            gorunus = GetComponent<Renderer>();
            blok = new MaterialPropertyBlock();
            acikRenk = gorunus.sharedMaterial.HasProperty(RenkId) ? gorunus.sharedMaterial.GetColor(RenkId) : Color.white;
        }

        private void LateUpdate()
        {
            if (Mathf.Approximately(uygulanan, GeceGokyuzu.Bulut)) return;
            uygulanan = GeceGokyuzu.Bulut;
            gorunus.GetPropertyBlock(blok);
            blok.SetColor(RenkId, Color.Lerp(acikRenk, sonukRenk, Mathf.SmoothStep(0f, 1f, uygulanan)));
            gorunus.SetPropertyBlock(blok);
        }
    }
}
