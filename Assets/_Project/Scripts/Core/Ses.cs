using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Ses olaylarının tek kapısı: sistemler sesi türüyle ister (Ses.Cal), SesYoneticisi çalar. Yönetici ya da klip yoksa sessizce geçer.</summary>
    public static class Ses
    {
        private static SesYoneticisi yonetici;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => yonetici = null;

        /// <summary>İlk yönetici kalır; sonraki sahnelerin Sistemler kopyası (yok edilecek olan) kabul edilmez.</summary>
        internal static bool YoneticiAyarla(SesYoneticisi yeni)
        {
            if (yonetici != null && yonetici != yeni) return false;
            yonetici = yeni;
            return true;
        }

        internal static void YoneticiBirak(SesYoneticisi eski)
        {
            if (yonetici == eski) yonetici = null;
        }

        /// <summary>Ekranın sesi (konumsuz): kontrol noktası, kurtulan yavru, ipucu.</summary>
        public static void Cal(SesTuru tur)
        {
            if (yonetici != null) yonetici.Cal(tur, null);
        }

        /// <summary>Bir yerden gelen ses: yengeç, martı, kazma.</summary>
        public static void Cal(SesTuru tur, Vector3 konum)
        {
            if (yonetici != null) yonetici.Cal(tur, konum);
        }

        /// <summary>Sürekli çalan sesler için klip (ör. teknenin pervanesi kendi AudioSource'unda döngüyle çalar).</summary>
        public static AudioClip Klip(SesTuru tur, out float duzey)
        {
            duzey = 1f;
            if (yonetici == null) return null;
            return yonetici.Klip(tur, out duzey);
        }
    }
}
