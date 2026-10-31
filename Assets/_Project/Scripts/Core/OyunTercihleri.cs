using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncunun ayarları (ses, fare hassasiyeti); bu bilgisayarda saklanır, kayıttan bağımsızdır.</summary>
    public static class OyunTercihleri
    {
        private const string SesAnahtari = "ses";
        private const string HassasiyetAnahtari = "fare_hassasiyeti";

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Uygula() => AudioListener.volume = Ses;
    }
}
