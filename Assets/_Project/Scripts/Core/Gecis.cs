using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Core
{
    /// <summary>Adımlar arası geçiş: ekran kararır, gerekirse bölüm başlığı yazar, açılır. Oyun akışı kendisi kurar; sahneler arasında yaşar.</summary>
    public class Gecis : MonoBehaviour
    {
        private CanvasGroup grup;
        private Text baslik;

        /// <summary>Oyun akışının altına siyah perde ve başlık yazısı kurar.</summary>
        public static Gecis Kur(Transform ebeveyn)
        {
            var nesne = new GameObject("Gecis", typeof(Canvas), typeof(CanvasGroup));
            nesne.transform.SetParent(ebeveyn, false);
            var tuval = nesne.GetComponent<Canvas>();
            tuval.renderMode = RenderMode.ScreenSpaceOverlay;
            tuval.sortingOrder = 100;

            var perde = new GameObject("Perde", typeof(RectTransform), typeof(Image));
            perde.transform.SetParent(nesne.transform, false);
            Yasla((RectTransform)perde.transform);
            perde.GetComponent<Image>().color = Color.black;

            var yazi = new GameObject("Baslik", typeof(RectTransform), typeof(Text));
            yazi.transform.SetParent(nesne.transform, false);
            Yasla((RectTransform)yazi.transform);
            var metin = yazi.GetComponent<Text>();
            metin.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            metin.fontSize = 48;
            metin.alignment = TextAnchor.MiddleCenter;
            metin.color = Color.white;

            var gecis = nesne.AddComponent<Gecis>();
            gecis.grup = nesne.GetComponent<CanvasGroup>();
            gecis.grup.alpha = 0f;
            gecis.grup.blocksRaycasts = false;
            gecis.baslik = metin;
            return gecis;
        }

        /// <summary>Ekranı karartır (ya da açar); oyun duraklatılmışken de çalışır.</summary>
        public IEnumerator Solustur(float hedef, float sure)
        {
            grup.blocksRaycasts = true;
            float ilk = grup.alpha;
            for (float t = 0f; t < 1f && sure > 0f; t += Time.unscaledDeltaTime / sure)
            {
                grup.alpha = Mathf.Lerp(ilk, hedef, t);
                yield return null;
            }
            grup.alpha = hedef;
            grup.blocksRaycasts = hedef > 0f;
        }

        /// <summary>Karanlıkta başlığı bir süre gösterir.</summary>
        public IEnumerator BaslikGoster(string yazi, float sure)
        {
            baslik.text = yazi;
            yield return new WaitForSecondsRealtime(sure);
            baslik.text = "";
        }

        private static void Yasla(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
