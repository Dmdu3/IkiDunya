using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncunun ayarları (ses, fare hassasiyeti, ipuçları, tam ekran, çözünürlük, grafik kalitesi, yazı boyutu); bu bilgisayarda saklanır, kayıttan bağımsızdır.</summary>
    public static class OyunTercihleri
    {
        private const string SesAnahtari = "ses";
        private const string HassasiyetAnahtari = "fare_hassasiyeti";
        private const string IpuclariAnahtari = "ipuclari";
        private const string TamEkranAnahtari = "tam_ekran";
        private const string CozunurlukAnahtari = "cozunurluk";
        private const string KaliteAnahtari = "kalite";
        private const string ArayuzAnahtari = "arayuz_olcegi";

        /// <summary>Ekranla ilgili bir ayar değişti (arayüz ölçeği dinler).</summary>
        public static event Action Degisti;

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

        public static bool TamEkran
        {
            get => PlayerPrefs.GetInt(TamEkranAnahtari, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(TamEkranAnahtari, value ? 1 : 0);
                EkraniUygula();
            }
        }

        /// <summary>Ekranın desteklediği çözünürlüklerden seçilenin sırası (-1: ekranın kendi çözünürlüğü).</summary>
        public static int Cozunurluk
        {
            get => PlayerPrefs.GetInt(CozunurlukAnahtari, -1);
            set
            {
                PlayerPrefs.SetInt(CozunurlukAnahtari, value);
                EkraniUygula();
            }
        }

        /// <summary>Grafik kalitesi: 0 düşük, 1 orta, 2 yüksek (projedeki kalite seviyelerine yayılır).</summary>
        public static int Kalite
        {
            get => PlayerPrefs.GetInt(KaliteAnahtari, 2);
            set
            {
                PlayerPrefs.SetInt(KaliteAnahtari, Mathf.Clamp(value, 0, 2));
                KaliteyiUygula();
            }
        }

        /// <summary>Arayüz ve yazı boyutu çarpanı.</summary>
        public static float ArayuzOlcegi
        {
            get => PlayerPrefs.GetFloat(ArayuzAnahtari, 1f);
            set
            {
                PlayerPrefs.SetFloat(ArayuzAnahtari, Mathf.Clamp(value, 0.75f, 1.5f));
                Degisti?.Invoke();
            }
        }

        /// <summary>Seçilebilen çözünürlükler (aynı boyutun farklı yenileme hızları tek sayılır).</summary>
        public static Resolution[] Cozunurlukler()
        {
            var liste = new System.Collections.Generic.List<Resolution>();
            foreach (Resolution r in Screen.resolutions)
                if (!liste.Exists(x => x.width == r.width && x.height == r.height)) liste.Add(r);
            return liste.ToArray();
        }

        private static void EkraniUygula()
        {
            Resolution[] liste = Cozunurlukler();
            int i = Cozunurluk;
            if (i >= 0 && i < liste.Length) Screen.SetResolution(liste[i].width, liste[i].height, TamEkran);
            else Screen.fullScreen = TamEkran;
            Degisti?.Invoke();
        }

        private static void KaliteyiUygula()
        {
            int seviye = QualitySettings.names.Length;
            if (seviye == 0) return;
            QualitySettings.SetQualityLevel(Mathf.RoundToInt(Kalite / 2f * (seviye - 1)), true);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Uygula()
        {
            Degisti = null;
            AudioListener.volume = Ses;
            KaliteyiUygula();
#if !UNITY_EDITOR
            EkraniUygula();
#endif
        }
    }
}
