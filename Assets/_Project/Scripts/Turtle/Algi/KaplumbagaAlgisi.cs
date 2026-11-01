using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın gözü: kameradan bakarak sahnedeki yanılgıları (AlgiNesnesi) her karede günceller.</summary>
    public class KaplumbagaAlgisi : MonoBehaviour
    {
        [SerializeField] private AlgiAyarlari ayarlar;

        private void LateUpdate()
        {
            // Kamera bu karede yerine oturduktan sonra: sahte ay göze göre konur.
            if (ayarlar != null) AlgiNesnesi.HepsiniGuncelle(transform, ayarlar);
        }
    }
}
