using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Algı yanılgılarının tek kaydı: nesneler kendini kaydeder, oyuncunun algısı hepsini tek yerden günceller.
    /// Sahne taranmaz; yeni bir denetleyici yazılmaz.</summary>
    public static class AlgiDunyasi
    {
        /// <summary>Bir yanılgı kalıcı olarak kırıldığında (gerçek görünüş ortaya çıktığında) haber verir.</summary>
        public static event Action<AlgiNesnesi> YanilgiKirildi;

        private static readonly List<AlgiNesnesi> nesneler = new List<AlgiNesnesi>();
        private static bool guncelleniyor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            nesneler.Clear();
            guncelleniyor = false;
            YanilgiKirildi = null;
        }

        /// <summary>Kayıtlı yanılgı sayısı.</summary>
        public static int Sayi => nesneler.Count;

        public static void Kaydet(AlgiNesnesi nesne)
        {
            if (nesne == null || nesneler.Contains(nesne)) return;
            nesneler.Add(nesne);
        }

        public static void Birak(AlgiNesnesi nesne)
        {
            if (nesne != null) nesneler.Remove(nesne);
        }

        internal static void KirildiBildir(AlgiNesnesi nesne) => YanilgiKirildi?.Invoke(nesne);

        /// <summary>Bütün yanılgıları oyuncunun gözüne ve o anki koşullara göre günceller.</summary>
        public static void Guncelle(Transform goz, AlgiAyarlari ayarlar, bool gece)
        {
            // İkinci bir denetleyici aynı karede ikinci kez güncellemesin.
            if (guncelleniyor) return;
            guncelleniyor = true;
            for (int i = nesneler.Count - 1; i >= 0; i--)
            {
                AlgiNesnesi nesne = nesneler[i];
                if (nesne == null)
                {
                    nesneler.RemoveAt(i);
                    continue;
                }
                nesne.Guncelle(goz, ayarlar, gece);
            }
            guncelleniyor = false;
        }

        /// <summary>Bütün yanılgıları kalıcı olarak kırar (ör. gece bittiğinde).</summary>
        public static void HepsiniKir()
        {
            for (int i = nesneler.Count - 1; i >= 0; i--)
                if (nesneler[i] != null) nesneler[i].Kir(true);
        }

        /// <summary>Bütün yanılgıları başlangıç hâline döndürür (yeni oyun).</summary>
        public static void Sifirla()
        {
            for (int i = 0; i < nesneler.Count; i++)
                if (nesneler[i] != null) nesneler[i].Sifirla();
        }
    }
}
