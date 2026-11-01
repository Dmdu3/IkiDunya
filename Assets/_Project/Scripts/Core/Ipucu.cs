using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncu bir şeyle ilk karşılaştığında kısa ipucu. Her ipucu bir oyunda bir kez çıkar (yeni oyunda sıfırlanır); ekranda IpucuPaneli gösterir.
    /// Kod yazıyı değil id'yi verir; yazı docs/metinler.md'den gelir.</summary>
    public static class Ipucu
    {
        private const string Anahtar = "ipuclari_gosterilen";

        /// <summary>Gösterilecek ipucunun id'si.</summary>
        public static event Action<string> Istendi;
        /// <summary>Bildirimin id'si ve yazıya konacak değerler ({0}, {1}).</summary>
        public static event Action<string, object[]> Bildirildi;

        private static HashSet<string> gosterilenler;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Istendi = null;
            Bildirildi = null;
            gosterilenler = null;
        }

        /// <summary>Oyuncuya o an olanı söyleyen kısa yazı (deneme bitti, kontrol noktasına dönüldü). İpucunun aksine her seferinde çıkar,
        /// ayarlarda kapatılmaz ve sıradaki ipucunun önüne geçer.</summary>
        public static void Bildir(string id, params object[] degerler) => Bildirildi?.Invoke(id, degerler);

        /// <summary>İpucu daha önce gösterilmediyse (ve ayarlarda kapalı değilse) gösterilmesini ister.</summary>
        public static void Goster(string id)
        {
            // Ekranda ipucu kutusu yoksa (Sistemler'siz deneme sahnesi) gösterilmiş sayılmasın.
            if (!OyunTercihleri.Ipuclari || Istendi == null) return;
            if (!Gosterilenler().Add(id)) return;
            PlayerPrefs.SetString(Anahtar, string.Join(",", gosterilenler));
            Istendi?.Invoke(id);
        }

        /// <summary>Yeni oyunda bütün ipuçları yeniden gösterilebilir.</summary>
        public static void Sifirla()
        {
            PlayerPrefs.DeleteKey(Anahtar);
            gosterilenler = new HashSet<string>();
        }

        private static HashSet<string> Gosterilenler()
        {
            if (gosterilenler != null) return gosterilenler;
            string kayitli = PlayerPrefs.GetString(Anahtar, "");
            gosterilenler = new HashSet<string>(kayitli.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            return gosterilenler;
        }
    }
}
