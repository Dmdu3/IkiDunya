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
            GetComponent<Text>().text = tablo.Al(id);
        }
    }
}
