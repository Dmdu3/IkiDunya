using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncunun ayarları (ses, fare hassasiyeti, ipuçları); bu bilgisayarda saklanır, kayıttan bağımsızdır.</summary>
    public static class OyunTercihleri
    {
        private const string SesAnahtari = "ses";
        private const string HassasiyetAnahtari = "fare_hassasiyeti";
        private const string IpuclariAnahtari = "ipuclari";

        /// <summary>Ana ses (0–1).</summary>
        public static float Ses
        {
            get => PlayerPrefs.GetFloat(SesAnahtari, 1f);
            set
            {
                PlayerPrefs.SetFloat(SesAnahtari, Mathf.Clamp01(value));
                AudioListener.volume = Ses;
            }
        }

        /// <summary>Fare hassasiyeti çarpanı; kamera ayarındaki hassasiyetle çarpılır.</summary>
        public static float HassasiyetCarpani
        {
            get => PlayerPrefs.GetFloat(HassasiyetAnahtari, 1f);
            set => PlayerPrefs.SetFloat(HassasiyetAnahtari, value);
        }

        /// <summary>İlk karşılaşmada ipuçları gösterilsin mi.</summary>
        public static bool Ipuclari
        {
            get => PlayerPrefs.GetInt(IpuclariAnahtari, 1) == 1;
            set => PlayerPrefs.SetInt(IpuclariAnahtari, value ? 1 : 0);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Uygula() => AudioListener.volume = Ses;
    }
}
