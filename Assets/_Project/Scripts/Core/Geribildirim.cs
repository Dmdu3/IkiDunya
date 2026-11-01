using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Darbe ve sarsıntının şiddeti; sayıları GeribildirimAyarlari'nda.</summary>
    public enum Siddet { Hafif, Orta, Agir }

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

        /// <summary>Güç seviyelerinin sayıları; ekran efektleri ve kamera açılınca bağlar.</summary>
        public static GeribildirimAyarlari Ayarlar { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Sarsildi = null;
            Flas = null;
            Tehlike = 0f;
            Ayarlar = null;
        }

        /// <summary>Seviyenin gücü (0–1). Ayarlar bağlı değilse (efektsiz deneme sahnesi) dinleyen de yoktur; orta güç döner.</summary>
        private static float Guc(Siddet siddet) => Ayarlar != null ? Ayarlar.Guc(siddet) : 0.5f;

        public static void Sars(Siddet siddet) => Sarsildi?.Invoke(Mathf.Clamp01(Guc(siddet)));

        /// <summary>Darbe: sarsıntı, kırmızı flaş ve darbe sesi.</summary>
        public static void Darbe(Siddet siddet)
        {
            float guc = Guc(siddet);
            Sarsildi?.Invoke(Mathf.Clamp01(guc));
            Flas?.Invoke(new Color(0.85f, 0.1f, 0.05f, 0.35f * Mathf.Clamp01(guc + 0.3f)));
            Ses.Cal(SesTuru.Darbe);
        }

        /// <summary>İyi bir şey oldu (yavru denize ulaştı): kısa, yumuşak yeşil parıltı.</summary>
        public static void Sevinc() => Flas?.Invoke(new Color(0.4f, 0.95f, 0.6f, 0.12f));

        /// <summary>Kötü ama darbe olmayan şey (poşet yuttun): kısa kahverengi bulanıklık.</summary>
        public static void Bulanma() => Flas?.Invoke(new Color(0.45f, 0.35f, 0.2f, 0.3f));

        /// <summary>Yanılgı kırıldı, gerçek görüldü: soğuk, soluk bir parıltı ve hafif sarsıntı.</summary>
        public static void Uyanis()
        {
            Flas?.Invoke(new Color(0.75f, 0.82f, 0.9f, 0.22f));
            Sarsildi?.Invoke(Mathf.Clamp01(Guc(Siddet.Hafif) * 0.5f));
        }
    }
}
