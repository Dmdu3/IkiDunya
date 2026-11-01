using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın algısı: gözünden bakarak kayıtlı yanılgıları her karede günceller. Sahnedeki kamera bunu taşır;
    /// hangi nesnenin neye benzediğini AlgiNesnesi, sayıları AlgiAyarlari söyler.</summary>
    public class KaplumbagaAlgisi : MonoBehaviour
    {
        [SerializeField] private AlgiAyarlari ayarlar;
        [Tooltip("Kaplumbağanın gözü. Boşsa bu objenin kendisi.")]
        [SerializeField] private Transform goz;

        private static KaplumbagaAlgisi denetleyici;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => denetleyici = null;

        /// <summary>Algıyı şu an bu bileşen mi güncelliyor.</summary>
        public bool DenetleyiciMi => denetleyici == this;

        private void Awake()
        {
            if (goz == null) goz = transform;
        }

        private void OnEnable()
        {
            // Aynı sahnede iki göz varsa (anne ve öncü yavru) son açılan günceller.
            denetleyici = this;
        }

        private void OnDisable()
        {
            if (denetleyici == this) denetleyici = null;
        }

        private void Update()
        {
            // Gruplar sırayla kapanıp açılırken denetleyici boş kalmasın.
            if (denetleyici == null) denetleyici = this;
            if (denetleyici != this || ayarlar == null) return;
            if (GameInput.Kilitli || Duraklatma.Acik) return;

            AlgiDunyasi.Guncelle(goz, ayarlar, OrtamIsigi.GeceMi);
        }

        /// <summary>Bütün yanılgıları kalıcı olarak kırar.</summary>
        [ContextMenu("Bütün yanılgıları kır")]
        public void HepsiniKir() => AlgiDunyasi.HepsiniKir();

        /// <summary>Bütün yanılgıları başlangıç hâline döndürür.</summary>
        [ContextMenu("Yanılgıları sıfırla")]
        public void Sifirla() => AlgiDunyasi.Sifirla();
    }
}
