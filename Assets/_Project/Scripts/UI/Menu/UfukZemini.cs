using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Menü zemini, temanın renklerinden kodla çizilir: alacakaranlıkta ufuk (sağda turuncu, sola doğru gece) ve koyu deniz.
    /// Yan karartma kipinde oyunun üstüne soldan sağa açılan perde (duraklatma, ayarlar).</summary>
    [RequireComponent(typeof(RawImage))]
    public class UfukZemini : MonoBehaviour
    {
        [SerializeField] private ArayuzTemasi tema;
        [Tooltip("Açıksa ufuk yerine soldan koyu, sağa doğru yarı saydam perde.")]
        [SerializeField] private bool yanKarartma;

        private Texture2D doku;

        private void Awake()
        {
            ArayuzTemasi t = tema != null ? tema : ArayuzTemasi.Yuklu;
            if (t == null) return;
            doku = yanKarartma ? Perde(t) : Ufuk(t);
            var resim = GetComponent<RawImage>();
            resim.texture = doku;
            resim.color = Color.white;
        }

        private void OnDestroy()
        {
            if (doku != null) Destroy(doku);
        }

        private static Texture2D Yeni(int en, int boy) =>
            new Texture2D(en, boy, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "UfukZemini" };

        private static Texture2D Ufuk(ArayuzTemasi t)
        {
            const int en = 256, boy = 128;
            const float ufukHatti = 0.3f;
            Texture2D d = Yeni(en, boy);
            for (int y = 0; y < boy; y++)
                for (int x = 0; x < en; x++)
                {
                    float tx = x / (en - 1f), ty = y / (boy - 1f);
                    Color renk;
                    if (ty > ufukHatti)
                    {
                        float h = (ty - ufukHatti) / (1f - ufukHatti);
                        renk = Color.Lerp(Color.Lerp(t.Alacakaranlik, t.Ufuk, Mathf.Pow(1f - h, 3f)), t.Gece, h * 0.8f);
                    }
                    else renk = Color.Lerp(t.Deniz, Color.Lerp(t.Alacakaranlik, t.Ufuk, 0.4f), Mathf.Pow(ty / ufukHatti, 6f) * 0.5f);
                    // Sol taraf (yazıların durduğu yer) koyulaşır.
                    d.SetPixel(x, y, Color.Lerp(renk, t.Gece, Mathf.Pow(1f - tx, 1.6f) * 0.85f));
                }
            d.Apply();
            return d;
        }

        private static Texture2D Perde(ArayuzTemasi t)
        {
            const int en = 256;
            Texture2D d = Yeni(en, 1);
            for (int x = 0; x < en; x++)
            {
                Color renk = t.Perde;
                renk.a *= Mathf.Lerp(1f, 0.82f, Mathf.SmoothStep(0f, 1f, x / (en - 1f)));
                d.SetPixel(x, 0, renk);
            }
            d.Apply();
            return d;
        }
    }
}
