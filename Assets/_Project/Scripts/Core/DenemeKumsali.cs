using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Akışın olmadığı deneme sahnelerinde kumsalı Play başında ilk haline getirir. Oyun sahnelerinde bunu OyunAkisi yapar.</summary>
    public class DenemeKumsali : MonoBehaviour
    {
        [SerializeField] private OyunAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;

        private void Awake()
        {
            if (!OyunAkisi.Var) ayarlar.KumsaliSifirla(kumsal);
        }
    }
}
