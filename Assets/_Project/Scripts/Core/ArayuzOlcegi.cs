using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Core
{
    /// <summary>Tuvalin ölçeğini oyuncunun yazı boyutu ayarına göre büyütür ya da küçültür (bütün tuvallerde; kurulum ekler).
    /// Taşma koruması: tuvaldeki yazılar kutusuna sığmazsa küçülür (en çok EnKucukOran'a kadar), komşusunun üstüne taşmaz.</summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class ArayuzOlcegi : MonoBehaviour
    {
        /// <summary>En büyük yazı boyutunda (1.5) 1'de sığan yazı 1/1.5'e küçülerek yine sığar; biraz pay bırakılır.</summary>
        private const float EnKucukOran = 0.6f;

        private CanvasScaler olcek;
        private Vector2 tabanCozunurluk;

        private void Awake()
        {
            olcek = GetComponent<CanvasScaler>();
            tabanCozunurluk = olcek.referenceResolution;
            // Genişlik ya da yükseklik hangisi darsa ona uyar: 16:10, 4:3 ve geniş ekranda tuval en az referans kadar kalır, kenarlar taşmaz.
            olcek.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            foreach (Text yazi in GetComponentsInChildren<Text>(true)) Sigdir(yazi);
        }

        private void OnEnable()
        {
            OyunTercihleri.Degisti += Uygula;
            Uygula();
        }

        private void OnDisable() => OyunTercihleri.Degisti -= Uygula;

        // Referans çözünürlüğü küçüldükçe arayüz büyür.
        private void Uygula() => olcek.referenceResolution = tabanCozunurluk / Mathf.Max(0.1f, OyunTercihleri.ArayuzOlcegi);

        /// <summary>Yazı kutusuna sığmazsa küçülsün (kendi en az/en çok boyutu verilmişse ona dokunmaz).</summary>
        public static void Sigdir(Text yazi)
        {
            if (yazi == null || yazi.resizeTextForBestFit) return;
            Boyut(yazi, yazi.fontSize);
        }

        /// <summary>Yazı boyutunu verir; kutuya sığmazsa boyut * EnKucukOran'a kadar küçülür.</summary>
        public static void Boyut(Text yazi, int boyut)
        {
            yazi.fontSize = boyut;
            yazi.resizeTextMaxSize = boyut;
            yazi.resizeTextMinSize = Mathf.Max(10, Mathf.RoundToInt(boyut * EnKucukOran));
            yazi.resizeTextForBestFit = true;
        }
    }
}
