using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Camera
{
    /// <summary>Deniz bölümlerinde göz su altına inince sis koyu mavi ve yoğun olur, yüzeye çıkınca açılır.</summary>
    public class SuAltiGorunumu : MonoBehaviour
    {
        [SerializeField] private OrtamAyarlari ayarlar;
        [Tooltip("Su yüzeyinin yüksekliği (dünya y).")]
        [SerializeField] private float suYuzeyi;

        private bool? altta;

        private void LateUpdate()
        {
            bool simdiAltta = transform.position.y < suYuzeyi;
            if (simdiAltta == altta) return;
            altta = simdiAltta;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = simdiAltta ? ayarlar.SuAltiSis : ayarlar.SuUstuSis;
            RenderSettings.fogDensity = simdiAltta ? ayarlar.SuAltiSisYogunlugu : ayarlar.SuUstuSisYogunlugu;
            var kamera = GetComponent<UnityEngine.Camera>();
            if (kamera == null) return;
            kamera.clearFlags = CameraClearFlags.SolidColor;
            kamera.backgroundColor = RenderSettings.fogColor;
        }
    }
}
