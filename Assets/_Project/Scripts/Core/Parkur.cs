using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyunun minigame'leri (parkurlar).</summary>
    /// <remarks>Kayıtta ve sahnelerde sayı olarak saklanır: yeni parkur hep sona eklenir.</remarks>
    public enum ParkurTuru { PosetSurusu, CopSeli, CopGirdabi }

    /// <summary>Parkurların süresi ve rekoru: parkur başlayınca süre işler, bitince rekor bu bilgisayarda saklanır ve parkur menüde açılır.
    /// Ekranda ParkurGostergesi gösterir. Süre oyun zamanıyla işler (duraklatmada durur).</summary>
    public static class Parkur
    {
        private const string AcikAnahtari = "parkur_acik_";
        private const string RekorAnahtari = "parkur_rekor_";

        /// <summary>Parkur başladı.</summary>
        public static event Action<ParkurTuru> Basladi;
        /// <summary>Parkur bitti: süre, rekor (bu süre dahil), yeni rekor mu.</summary>
        public static event Action<ParkurTuru, float, float, bool> Bitti;
        /// <summary>Parkur yarıda kaldı (kontrol noktasına dönüldü, alandan çıkıldı).</summary>
        public static event Action<ParkurTuru> Kaldi;
        /// <summary>Parkurun kendi sayacı değişti (ör. yenen denizanası): metin id'si, sayı, hedef.</summary>
        public static event Action<string, int, int> BilgiDegisti;

        private static ParkurTuru? aktif;
        private static float baslangic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            Basladi = null;
            Bitti = null;
            Kaldi = null;
            BilgiDegisti = null;
            aktif = null;
        }

        public static bool Suruyor => aktif.HasValue;
        public static bool SuruyorMu(ParkurTuru tur) => aktif == tur;
        public static float Gecen => aktif.HasValue ? Time.time - baslangic : 0f;
        public static ParkurTuru? Aktif => aktif;

        /// <summary>Parkurun adının metin id'si.</summary>
        public static string MetinId(ParkurTuru tur)
        {
            switch (tur)
            {
                case ParkurTuru.PosetSurusu: return "parkur_poset_surusu";
                case ParkurTuru.CopSeli: return "parkur_cop_seli";
                default: return "parkur_cop_girdabi";
            }
        }

        /// <summary>Süre "dk:sn.d" biçiminde (sayı, yazı değil).</summary>
        public static string Bicim(float saniye) => $"{(int)(saniye / 60f)}:{saniye % 60f:00.0}";

        public static bool Acik(ParkurTuru tur) => PlayerPrefs.GetInt(AcikAnahtari + (int)tur, 0) == 1;

        /// <summary>En iyi süre; hiç bitirilmediyse 0.</summary>
        public static float Rekor(ParkurTuru tur) => PlayerPrefs.GetFloat(RekorAnahtari + (int)tur, 0f);

        public static void Baslat(ParkurTuru tur)
        {
            aktif = tur;
            baslangic = Time.time;
            Basladi?.Invoke(tur);
        }

        public static void Bilgi(string id, int sayi, int hedef) => BilgiDegisti?.Invoke(id, sayi, hedef);

        /// <summary>Parkur bitti: rekor ve açılma kaydedilir.</summary>
        public static void Bitir(ParkurTuru tur)
        {
            if (aktif != tur) return;
            float sure = Gecen;
            aktif = null;
            float eski = Rekor(tur);
            bool yeni = eski <= 0f || sure < eski;
            if (yeni) PlayerPrefs.SetFloat(RekorAnahtari + (int)tur, sure);
            PlayerPrefs.SetInt(AcikAnahtari + (int)tur, 1);
            PlayerPrefs.Save();
            Bitti?.Invoke(tur, sure, yeni ? sure : eski, yeni);
        }

        public static void Birak(ParkurTuru tur)
        {
            if (aktif != tur) return;
            aktif = null;
            Kaldi?.Invoke(tur);
        }
    }
}
