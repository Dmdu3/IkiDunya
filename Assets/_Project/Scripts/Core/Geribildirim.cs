using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncuya his veren ekran efektlerinin tek kapısı: sarsıntı (kamera), flaş (renk), sevinç, tehlike (düşük enerji: kenar kararması ve kalp atışı).
    /// Sistemler bunu çağırır; kamera ve ekran efektleri dinler.</summary>
    public static class Geribildirim
    {
        /// <summary>Kamera sarsıntısı: 0–1 güç.</summary>
        public static event Action<float> Sarsildi;
        /// <summary>Ekran bir an bu renge döner (alfa güç).</summary>
        public static event Action<Color> Flas;

        /// <summary>Oyuncunun tehlikesi (0 güvende – 1 enerji bitmek üzere); ekran kenarı kararır, kalp atar.</summary>
        public static float Tehlike { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Sarsildi = null;
            Flas = null;
            Tehlike = 0f;
        }

        public static void Sars(float guc) => Sarsildi?.Invoke(Mathf.Clamp01(guc));

        /// <summary>Darbe: sarsıntı ve kırmızı flaş.</summary>
        public static void Darbe(float guc)
        {
            Sars(guc);
            Flas?.Invoke(new Color(0.85f, 0.1f, 0.05f, 0.35f * Mathf.Clamp01(guc + 0.3f)));
        }

        /// <summary>İyi bir şey oldu (yavru denize ulaştı): kısa, yumuşak yeşil parıltı.</summary>
        public static void Sevinc() => Flas?.Invoke(new Color(0.4f, 0.95f, 0.6f, 0.12f));

        /// <summary>Kötü ama darbe olmayan şey (poşet yuttun): kısa kahverengi bulanıklık.</summary>
        public static void Bulanma() => Flas?.Invoke(new Color(0.45f, 0.35f, 0.2f, 0.3f));
    }
}
