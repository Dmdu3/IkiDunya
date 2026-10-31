using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Bir Text'in yazısını metin tablosundan id ile doldurur; yazının kendisi koda girmez.</summary>
    [RequireComponent(typeof(Text))]
    public class MetinYazisi : MonoBehaviour
    {
        [SerializeField] private MetinTablosu tablo;
        [SerializeField] private string id;

        private void OnEnable()
        {
            MetinTablosu kullanilan = tablo != null ? tablo : MetinTablosu.Yuklu;
            var yazi = GetComponent<Text>();
            if (kullanilan == null || yazi == null)
            {
                Debug.LogWarning($"MetinYazisi: '{name}' için metin tablosu ya da Text yok (İki Dünya → Menü ve duraklatmayı kur).", this);
                return;
            }
            yazi.text = kullanilan.Al(id);
        }
    }
}
