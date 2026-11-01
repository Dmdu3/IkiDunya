using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Bir Text'in yazısını metin tablosundan id ile doldurur; yazının kendisi koda girmez.
    /// Tuş verilirse metindeki {0}, {1}… o eylemlerin tuşuyla dolar ve cihaz ya da tuş ataması değişince yenilenir.</summary>
    [RequireComponent(typeof(Text))]
    public class MetinYazisi : MonoBehaviour
    {
        [SerializeField] private MetinTablosu tablo;
        [SerializeField] private string id;
        [Tooltip("GameInput eylem adları (ör. KazYukari); metindeki {0}, {1}… sırayla bunların tuşu olur.")]
        [SerializeField] private string[] tuslar;

        private string gosterilen;

        private void OnEnable()
        {
            gosterilen = id;
            if (tuslar != null && tuslar.Length > 0)
            {
                GameInput.CihazDegisti += Yaz;
                GameInput.BaglamalarDegisti += Yaz;
            }
            Yaz();
        }

        private void OnDisable()
        {
            GameInput.CihazDegisti -= Yaz;
            GameInput.BaglamalarDegisti -= Yaz;
        }

        /// <summary>Geçici olarak başka bir metni gösterir (ör. onay sorusu); null verilirse kendi metnine döner.</summary>
        public void Goster(string digerId)
        {
            gosterilen = digerId ?? id;
            Yaz();
        }

        private void Yaz()
        {
            MetinTablosu kullanilan = tablo != null ? tablo : MetinTablosu.Yuklu;
            var yazi = GetComponent<Text>();
            if (kullanilan == null || yazi == null)
            {
                Debug.LogWarning($"MetinYazisi: '{name}' için metin tablosu ya da Text yok (İki Dünya → Menü ve duraklatmayı kur).", this);
                return;
            }
            string metin = kullanilan.Al(gosterilen);
            if (tuslar != null && tuslar.Length > 0 && gosterilen == id)
            {
                var tusYazilari = new object[tuslar.Length];
                for (int i = 0; i < tuslar.Length; i++) tusYazilari[i] = GameInput.TusYazisi(tuslar[i]);
                metin = string.Format(metin, tusYazilari);
            }
            yazi.text = metin;
        }
    }
}
