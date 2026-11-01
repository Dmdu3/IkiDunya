using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Denizanası sanılıp yutulan poşet: bir an gözün önünde, ağızda görünür, sonra aşağı kayıp kaybolur. Kendini yok eder.</summary>
    public class AgizdakiCop : MonoBehaviour
    {
        private const float Sure = 1.2f;

        private float gecen;
        private Vector3 ilkYer;
        private Vector3 ilkOlcek;

        private void Start()
        {
            ilkYer = transform.localPosition;
            ilkOlcek = transform.localScale;
        }

        private void Update()
        {
            gecen += Time.deltaTime;
            float t = gecen / Sure;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            // Önce ağızda çırpınır, sonra aşağı kayıp küçülür (yutuldu).
            float cirpinma = Mathf.Sin(gecen * 22f) * (1f - t) * 4f;
            transform.localPosition = ilkYer + new Vector3(0f, -0.18f, -0.05f) * (t * t);
            transform.localRotation *= Quaternion.Euler(0f, cirpinma * Time.deltaTime * 30f, 0f);
            transform.localScale = ilkOlcek * Mathf.Lerp(1f, 0.4f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 1f, t)));
        }
    }
}
