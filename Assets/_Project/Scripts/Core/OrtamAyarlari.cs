using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Ortamın görünüşü: gece (kaplumbağa geceleri) ve gündüz (insan turu) ışığı, gökyüzü rengi, sis; denizde su üstü ve su altı sisi.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Ortam Ayarları", fileName = "OrtamAyarlari")]
    public class OrtamAyarlari : ScriptableObject
    {
        [Header("Gece (ay ışığı)")]
        [SerializeField] private Color geceIsigi = new Color(0.55f, 0.65f, 0.9f);
        [SerializeField] private float geceIsikGucu = 0.35f;
        [SerializeField] private Color geceOrtamIsigi = new Color(0.08f, 0.1f, 0.18f);
        [SerializeField] private Color geceGokyuzu = new Color(0.02f, 0.03f, 0.08f);
        [SerializeField] private float geceSis = 0.012f;

        [Header("Gündüz")]
        [SerializeField] private Color gunduzIsigi = new Color(1f, 0.96f, 0.88f);
        [SerializeField] private float gunduzIsikGucu = 1.1f;
        [SerializeField] private Color gunduzOrtamIsigi = new Color(0.45f, 0.5f, 0.55f);
        [SerializeField] private Color gunduzGokyuzu = new Color(0.55f, 0.75f, 0.95f);

        [Header("Deniz")]
        [SerializeField] private Color suUstuSis = new Color(0.55f, 0.7f, 0.8f);
        [SerializeField] private float suUstuSisYogunlugu = 0.004f;
        [SerializeField] private Color suAltiSis = new Color(0.06f, 0.28f, 0.38f);
        [SerializeField] private float suAltiSisYogunlugu = 0.035f;

        public Color GeceIsigi => geceIsigi;
        public float GeceIsikGucu => geceIsikGucu;
        public Color GeceOrtamIsigi => geceOrtamIsigi;
        public Color GeceGokyuzu => geceGokyuzu;
        public float GeceSis => geceSis;
        public Color GunduzIsigi => gunduzIsigi;
        public float GunduzIsikGucu => gunduzIsikGucu;
        public Color GunduzOrtamIsigi => gunduzOrtamIsigi;
        public Color GunduzGokyuzu => gunduzGokyuzu;
        public Color SuUstuSis => suUstuSis;
        public float SuUstuSisYogunlugu => suUstuSisYogunlugu;
        public Color SuAltiSis => suAltiSis;
        public float SuAltiSisYogunlugu => suAltiSisYogunlugu;
    }
}
