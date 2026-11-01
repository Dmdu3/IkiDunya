using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Ekranın üstünde flaş (darbe, sevinç, poşet) ve tehlikede kararan kenarlar; tehlikede kalp atışı sesi. Sistemler'de durur.</summary>
    public class EkranEfektleri : MonoBehaviour
    {
        [SerializeField] private GeribildirimAyarlari ayarlar;
        [SerializeField] private Image flas;
        [Tooltip("Ortası saydam, kenarları koyu doku (kodla üretilir).")]
        [SerializeField] private RawImage kenar;

        private Color flasRengi;
        private float flasKalan;
        private float kalpKalan;

        private void Awake()
        {
            kenar.texture = KenarDokusu(128);
            flas.color = Color.clear;
            kenar.color = new Color(0f, 0f, 0f, 0f);
        }

        private void OnEnable()
        {
            Geribildirim.Flas += FlasYap;
            OyunAkisi.AdimBasladi += AdimBasladi;
        }

        private void OnDisable()
        {
            Geribildirim.Flas -= FlasYap;
            OyunAkisi.AdimBasladi -= AdimBasladi;
        }

        // Yeni adımda eski tehlike kalmasın (enerji uyarısı yeni oyuncuda yeniden hesaplar).
        private void AdimBasladi(Adim adim) => Geribildirim.Tehlike = 0f;

        private void FlasYap(Color renk)
        {
            flasRengi = renk;
            flasKalan = ayarlar.FlasSuresi;
        }

        private void Update()
        {
            if (flasKalan > 0f)
            {
                flasKalan -= Time.unscaledDeltaTime;
                Color c = flasRengi;
                c.a *= Mathf.Clamp01(flasKalan / ayarlar.FlasSuresi);
                flas.color = c;
            }
            else if (flas.color.a > 0f) flas.color = Color.clear;

            float tehlike = Duraklatma.Acik || OyunAkisi.MenudeMi ? 0f : Geribildirim.Tehlike;
            // Kalp gibi atan kenar: tehlikeyle koyulaşır ve hızlanır.
            float aralik = Mathf.Lerp(ayarlar.KalpYavas, ayarlar.KalpHizli, tehlike);
            float atis = tehlike > 0f ? 0.75f + 0.25f * Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f / aralik) : 0f;
            kenar.color = new Color(0f, 0f, 0f, tehlike * ayarlar.KenarKoyulugu * atis);

            if (tehlike <= 0f)
            {
                kalpKalan = 0f;
                return;
            }
            kalpKalan -= Time.unscaledDeltaTime;
            if (kalpKalan > 0f) return;
            kalpKalan = aralik;
            Ses.Cal(SesTuru.KalpAtisi);
        }

        /// <summary>Ortası saydam, kenara doğru koyulaşan kare doku.</summary>
        private static Texture2D KenarDokusu(int boy)
        {
            var doku = new Texture2D(boy, boy, TextureFormat.Alpha8, false) { wrapMode = TextureWrapMode.Clamp };
            var piksel = new Color[boy * boy];
            for (int y = 0; y < boy; y++)
                for (int x = 0; x < boy; x++)
                {
                    float dx = (x + 0.5f) / boy * 2f - 1f;
                    float dy = (y + 0.5f) / boy * 2f - 1f;
                    float uzaklik = Mathf.Sqrt(dx * dx + dy * dy) / 1.414f;
                    piksel[y * boy + x] = new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, uzaklik)));
                }
            doku.SetPixels(piksel);
            doku.Apply();
            return doku;
        }
    }
}
