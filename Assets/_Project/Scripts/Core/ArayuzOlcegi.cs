using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Core
{
    /// <summary>Tuvalin ölçeğini oyuncunun yazı boyutu ayarına göre büyütür ya da küçültür (bütün tuvallerde; kurulum ekler).</summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class ArayuzOlcegi : MonoBehaviour
    {
        private CanvasScaler olcek;
        private Vector2 tabanCozunurluk;

        private void Awake()
        {
            olcek = GetComponent<CanvasScaler>();
            tabanCozunurluk = olcek.referenceResolution;
        }

        private void OnEnable()
        {
            OyunTercihleri.Degisti += Uygula;
            Uygula();
        }

        private void OnDisable() => OyunTercihleri.Degisti -= Uygula;

        // Referans çözünürlüğü küçüldükçe arayüz büyür.
        private void Uygula() => olcek.referenceResolution = tabanCozunurluk / Mathf.Max(0.1f, OyunTercihleri.ArayuzOlcegi);
    }
}
