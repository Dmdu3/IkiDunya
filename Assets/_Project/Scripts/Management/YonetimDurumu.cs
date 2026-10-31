using System;
using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Rapordaki bir satır: metin id'si ve yanındaki sayı.</summary>
    [Serializable]
    public class RaporSatiri
    {
        [SerializeField] private string id;
        [SerializeField] private int sayi;

        public RaporSatiri(string id, int sayi)
        {
            this.id = id;
            this.sayi = sayi;
        }

        public string Id => id;
        public int Sayi => sayi;
    }

    /// <summary>İnsan kısmının kendi durumu: bütçe, halk desteği, gönüllüler, açılan kanıtlar, oynanan kalıcı kartlar, bekleyen uyarı, rapor.
    /// Kumsalın durumu KumsalDurumu'nda; bu asset kayda IOyunVerisi olarak girer.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Yönetim Durumu", fileName = "YonetimDurumu")]
    public class YonetimDurumu : ScriptableObject, IOyunVerisi
    {
        [SerializeField] private YonetimAyarlari ayarlar;

        [SerializeField] private int tur;
        [SerializeField] private int butce;
        [SerializeField] private int destek;
        [SerializeField] private int ekGonullu;
        [SerializeField] private int mesgulGonullu;
        [SerializeField] private int sonrakiMesgul;
        [SerializeField] private int kullanilanGonullu;
        [SerializeField] private int sayilanYavru;
        [SerializeField] private List<KanitTuru> kanitlar = new List<KanitTuru>();
        [SerializeField] private List<string> oynananTekKartlar = new List<string>();
        [Tooltip("Gelecek tur gelecek uyarının olayı; -1 yok.")]
        [SerializeField] private int bekleyenUyari = -1;
        [SerializeField] private int sonOlay = -1;
        [SerializeField] private List<RaporSatiri> rapor = new List<RaporSatiri>();
        [SerializeField] private List<RaporSatiri> sonrakiRapor = new List<RaporSatiri>();

        public event Action Degisti;

        public YonetimAyarlari Ayarlar => ayarlar;
        /// <summary>Kaçıncı insan turu (ilki 1).</summary>
        public int Tur => tur;
        public int Butce => butce;
        public int Destek => destek;
        public int KullanilanGonullu => kullanilanGonullu;
        public IReadOnlyList<KanitTuru> Kanitlar => kanitlar;
        public IReadOnlyList<RaporSatiri> Rapor => rapor;
        public bool UyariBekliyor(OlayEtkisi olay) => bekleyenUyari == (int)olay;
        public int SonOlay => sonOlay;

        /// <summary>Bu turdaki toplam gönüllü: başlangıç + denize ulaşan yavrudan gelenler (en fazla sınırı) + çağrı − meşgul.</summary>
        public int Gonullu(KumsalDurumu kumsal)
        {
            int temel = Mathf.Min(ayarlar.GonulluEnFazla, ayarlar.BaslangicGonullu + kumsal.DenizeUlasanYavru / ayarlar.YavruBasinaGonullu);
            return Mathf.Max(0, temel + ekGonullu - mesgulGonullu);
        }

        public int BosGonullu(KumsalDurumu kumsal) => Mathf.Max(0, Gonullu(kumsal) - kullanilanGonullu);

        public bool KanitVar(KanitTuru kanit) => kanitlar.Contains(kanit);
        public bool TekKartOynandi(string id) => oynananTekKartlar.Contains(id);

        /// <summary>Kartın bu turdaki bedeli: ucuzlatan kanıt açıksa 1 eksik (en az 0).</summary>
        public int Bedel(KartTanimi kart) =>
            Mathf.Max(0, kart.Bedel - (kart.Bastan && kart.KanitUcuzlatir && KanitVar(kart.Kanit) ? 1 : 0));

        /// <summary>Kart havuzda mı: baştan ya da kanıtı açık, tek seferlikse daha oynanmamış.</summary>
        public bool Havuzda(KartTanimi kart) =>
            (kart.Bastan || KanitVar(kart.Kanit)) && !(kart.TekSeferlik && TekKartOynandi(kart.Id));

        // ---- IOyunVerisi ----

        public void YeniOyun()
        {
            tur = 0;
            butce = ayarlar.BaslangicButce;
            destek = ayarlar.BaslangicDestek;
            ekGonullu = 0;
            mesgulGonullu = 0;
            sonrakiMesgul = 0;
            kullanilanGonullu = 0;
            sayilanYavru = 0;
            kanitlar.Clear();
            oynananTekKartlar.Clear();
            bekleyenUyari = -1;
            sonOlay = -1;
            rapor.Clear();
            sonrakiRapor.Clear();
            Degisti?.Invoke();
        }

        public string Yaz() => JsonUtility.ToJson(this);

        public void Oku(string json)
        {
            // Asset bağlantısı JSON'dan gelmez (oturumlar arasında geçersiz); korunur.
            YonetimAyarlari bagli = ayarlar;
            JsonUtility.FromJsonOverwrite(json, this);
            ayarlar = bagli;
            Degisti?.Invoke();
        }

        // ---- Tur ----

        /// <summary>Yeni tur: sayaç, gelir (ilk tur hariç), gönüllüler sıfırlanır, meşguller yerine geçer; geçen turun raporu başlar.</summary>
        public void TurBasla(KumsalDurumu kumsal)
        {
            tur++;
            rapor.Clear();
            rapor.AddRange(sonrakiRapor);
            sonrakiRapor.Clear();
            if (tur > 1)
            {
                int gelir = ayarlar.TurButcesi(destek);
                butce += gelir;
                RaporEkle("rapor_gelir", gelir);
            }
            int yeniYavru = kumsal.DenizeUlasanYavru - sayilanYavru;
            if (yeniYavru > 0) RaporEkle("rapor_denize_ulasan", yeniYavru);
            sayilanYavru = kumsal.DenizeUlasanYavru;

            ekGonullu = 0;
            kullanilanGonullu = 0;
            mesgulGonullu = sonrakiMesgul;
            sonrakiMesgul = 0;
            Degisti?.Invoke();
        }

        public void RaporEkle(string id, int sayi)
        {
            rapor.Add(new RaporSatiri(id, sayi));
            Degisti?.Invoke();
        }

        /// <summary>Tur sonunda olan, gelecek turun raporunda görünecek satır.</summary>
        public void SonrakiRaporaEkle(string id, int sayi)
        {
            sonrakiRapor.Add(new RaporSatiri(id, sayi));
        }

        public void ButceDegistir(int fark)
        {
            butce = Mathf.Max(0, butce + fark);
            Degisti?.Invoke();
        }

        public void DestekDegistir(int fark)
        {
            destek = Mathf.Clamp(destek + fark, 0, ayarlar.DestekEnFazla);
            Degisti?.Invoke();
        }

        public void GonulluKullan()
        {
            kullanilanGonullu++;
            Degisti?.Invoke();
        }

        public void EkGonulluEkle(int adet)
        {
            ekGonullu += adet;
            Degisti?.Invoke();
        }

        /// <summary>Okul ziyareti: gelecek tur bu kadar gönüllü meşgul.</summary>
        public void SonrakiTurMesgul(int adet)
        {
            sonrakiMesgul += adet;
            Degisti?.Invoke();
        }

        /// <summary>Kanıtı açar; yeniyse true.</summary>
        public bool KanitEkle(KanitTuru kanit)
        {
            if (kanitlar.Contains(kanit)) return false;
            kanitlar.Add(kanit);
            Degisti?.Invoke();
            return true;
        }

        public void KartOynandi(KartTanimi kart)
        {
            if (kart.TekSeferlik && !oynananTekKartlar.Contains(kart.Id)) oynananTekKartlar.Add(kart.Id);
            Degisti?.Invoke();
        }

        public void UyariAyarla(OlayEtkisi? olay)
        {
            bekleyenUyari = olay.HasValue ? (int)olay.Value : -1;
            Degisti?.Invoke();
        }

        public void OlayGeldi(int sira)
        {
            sonOlay = sira;
            Degisti?.Invoke();
        }

#if UNITY_EDITOR
        // KumsalDurumu gibi: Play sırasındaki değişiklikler asset'e yazılmasın.
        private string playOncesi;

        private void OnEnable() => UnityEditor.EditorApplication.playModeStateChanged += OynatmaDegisti;
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OynatmaDegisti;

        private void OynatmaDegisti(UnityEditor.PlayModeStateChange durum)
        {
            if (durum == UnityEditor.PlayModeStateChange.ExitingEditMode) playOncesi = JsonUtility.ToJson(this);
            else if (durum == UnityEditor.PlayModeStateChange.EnteredEditMode && playOncesi != null)
            {
                JsonUtility.FromJsonOverwrite(playOncesi, this);
                playOncesi = null;
            }
        }
#endif
    }
}
