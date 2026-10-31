using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyunun duraklatılması: zaman durur; birinci şahıs kamera imleci serbest bırakır.</summary>
    public static class Duraklatma
    {
        public static bool Acik { get; private set; }
        /// <summary>Duraklatmanın kapandığı kare; o karedeki tıklama oyuna gitmez.</summary>
        public static int KapanmaKaresi { get; private set; } = -1;

        /// <summary>Duraklatma açıldığında ya da kapandığında; yeni durumla.</summary>
        public static event Action<bool> Degisti;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Acik = false;
            KapanmaKaresi = -1;
            Degisti = null;
        }

        public static void Ac() => Ayarla(true);
        public static void Kapat() => Ayarla(false);

        private static void Ayarla(bool acik)
        {
            if (Acik == acik) return;
            Acik = acik;
            if (!acik) KapanmaKaresi = Time.frameCount;
            Time.timeScale = acik ? 0f : 1f;
            Degisti?.Invoke(acik);
        }
    }
}
