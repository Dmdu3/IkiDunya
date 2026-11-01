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

    /// <summary>İnsan kısmının kendi durumu: bütçe, halk desteği, paydaşlar, gönüllüler, açılan kanıtlar, oynanan kalıcı kartlar ve bakımları, uyarılar, olay zinciri, rapor.
    /// Kumsalın durumu KumsalDurumu'nda; bu asset kayda IOyunVerisi olarak girer.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Yönetim Durumu", fileName = "YonetimDurumu")]
    public class YonetimDurumu : ScriptableObject, IOyunVerisi, IKorumaDurumu
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
        [Tooltip("Paydaş sırasıyla (Otel, Belediye, Balıkçılar) ilişki.")]
        [SerializeField] private List<int> iliskiler = new List<int>();
        [Tooltip("Kanıt türü sırasıyla kaç kez açıldı (ikincisi kartı güçlendirir).")]
        [SerializeField] private List<int> kanitSayilari = new List<int>();
        [Tooltip("Oynanan kartların her tur gelirden düşen toplam bakımı.")]
        [SerializeField] private int bakim;
        [Tooltip("Üniversite Ortaklığı: her tur fazladan gelen kart.")]
        [SerializeField] private int ekKart;
        [SerializeField] private int sonrakiEk;
        [SerializeField] private List<int> korunanBolgeler = new List<int>();
        [Tooltip("Bu turun sonunda gerçekleşecek uyarının olayı; -1 yok.")]
        [SerializeField] private int aktifUyari = -1;
        [Tooltip("Sıradaki turun olayı (zincir), olay listesindeki sırası; -1 yok.")]
        [SerializeField] private int zincir = -1;
        [Tooltip("Köpeklere izin verildi: bu tur avcı ihtimali artar.")]
        [SerializeField] private bool kopek;
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
        /// <summary>Bu turun sonunda gerçekleşecek uyarı (geçen tur geldi).</summary>
        public bool UyariAktif(OlayEtkisi olay) => aktifUyari == (int)olay;
        public bool UyariVar => aktifUyari >= 0;
        public OlayEtkisi AktifUyari => (OlayEtkisi)aktifUyari;
        public int SonOlay => sonOlay;
        public int Zincir => zincir;
        public int Bakim => bakim;
        public int EkKart => ekKart;
        public bool Kopek => kopek;
        public bool Korunuyor(int bolge) => korunanBolgeler.Contains(bolge);
        public bool KorumaAlaniVar => korunanBolgeler.Count > 0;

        // ---- Paydaşlar ----

        public int Iliski(Paydas paydas)
        {
            Doldur(iliskiler, Enum.GetValues(typeof(Paydas)).Length);
            return iliskiler[(int)paydas];
        }

        public bool Dost(Paydas paydas) => Iliski(paydas) >= ayarlar.DostEsigi;
        public bool Kus(Paydas paydas) => Iliski(paydas) <= ayarlar.KusEsigi;

        public void IliskiDegistir(PaydasFarki fark)
        {
            foreach (Paydas paydas in Enum.GetValues(typeof(Paydas)))
            {
                int eski = Iliski(paydas);
                iliskiler[(int)paydas] = Mathf.Clamp(eski + fark.Al(paydas), ayarlar.IliskiEnAz, ayarlar.IliskiEnCok);
            }
            Degisti?.Invoke();
        }

        /// <summary>Eski kayıtta liste kısa olabilir; eksikleri 0 ile doldurur.</summary>
        private static void Doldur(List<int> liste, int adet)
        {
            while (liste.Count < adet) liste.Add(0);
        }

        /// <summary>Bu turdaki toplam gönüllü: başlangıç + denize ulaşan yavrudan gelenler (en fazla sınırı) + çağrı − meşgul.</summary>
        public int Gonullu(KumsalDurumu kumsal)
        {
            int temel = Mathf.Min(ayarlar.GonulluEnFazla, ayarlar.BaslangicGonullu + kumsal.DenizeUlasanYavru / ayarlar.YavruBasinaGonullu);
            return Mathf.Max(0, temel + ekGonullu - mesgulGonullu);
        }

        public int BosGonullu(KumsalDurumu kumsal) => Mathf.Max(0, Gonullu(kumsal) - kullanilanGonullu);

        public bool KanitVar(KanitTuru kanit) => kanitlar.Contains(kanit);
        public bool TekKartOynandi(string id) => oynananTekKartlar.Contains(id);

        public int KanitSayisi(KanitTuru kanit)
        {
            Doldur(kanitSayilari, Enum.GetValues(typeof(KanitTuru)).Length);
            // Eski kayıtta sayı yok ama kanıt açık: en az 1.
            return Mathf.Max(kanitSayilari[(int)kanit], KanitVar(kanit) ? 1 : 0);
        }

        /// <summary>Kartın bu turdaki bedeli: ucuzlatan kanıt açıksa 1 eksik, Belediye dostsa politikalar ucuz (en az 0).</summary>
        public int Bedel(KartTanimi kart)
        {
            int indirim = kart.Bastan && kart.KanitUcuzlatir && KanitVar(kart.Kanit) ? 1 : 0;
            if (kart.Tur == KartTuru.Politika && Dost(Paydas.Belediye)) indirim += ayarlar.PolitikaIndirimi;
            return Mathf.Max(0, kart.Bedel - indirim);
        }

        /// <summary>Aynı türden yeterince kanıt açıldıysa kart güçlüdür (GDD: Kanıtlar, İkinci kanıt).</summary>
        public bool Guclu(KartTanimi kart) => kart.GucluDeger > 0 && KanitSayisi(kart.Kanit) >= ayarlar.GucluKanit;

        /// <summary>Etkinin bu turdaki büyüklüğü.</summary>
        public int Deger(KartTanimi kart) => Guclu(kart) ? kart.GucluDeger : kart.Deger;

        /// <summary>Paydaş koşulu sağlanıyor mu (ör. Koruma Alanı: Belediye +2).</summary>
        public bool KosulTamam(KartTanimi kart) => !kart.KosulVar || Iliski(kart.KosulPaydas) >= kart.KosulEnAz;

        /// <summary>Kart havuzda mı: baştan ya da kanıtı açık, tek seferlikse daha oynanmamış. Acil kartlar sadece kendi uyarısı varken.</summary>
        public bool Havuzda(KartTanimi kart)
        {
            if (kart.TekSeferlik && TekKartOynandi(kart.Id)) return false;
            if (kart.Tur == KartTuru.Acil) return UyariAktif(kart.AcilUyari);
            return kart.Bastan || KanitVar(kart.Kanit);
        }

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
            iliskiler.Clear();
            kanitSayilari.Clear();
            bakim = 0;
            ekKart = 0;
            sonrakiEk = 0;
            korunanBolgeler.Clear();
            aktifUyari = -1;
            zincir = -1;
            kopek = false;
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

        /// <summary>Yeni tur: sayaç, gelir (ilk tur hariç; paydaşlar ve bakım dahil), gönüllüler sıfırlanır, meşguller ve ekler yerine geçer,
        /// geçen turun uyarısı bu turun sonuna kurulur; geçen turun raporu başlar.</summary>
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
                if (Dost(Paydas.Otel))
                {
                    butce += ayarlar.OtelSponsoru;
                    RaporEkle("rapor_sponsor", ayarlar.OtelSponsoru);
                }
                // Bütçe sıfırın altına inmez: raporda gerçekte alınan yazılır, yetmeyen kısım ayrı satırda.
                int odenemeyen = 0;
                if (Kus(Paydas.Belediye)) odenemeyen += Ode("rapor_kesinti", ayarlar.BelediyeKesintisi);
                if (bakim > 0) odenemeyen += Ode("rapor_bakim", bakim);
                if (odenemeyen > 0) RaporEkle("rapor_odenemeyen", odenemeyen);
            }
            aktifUyari = bekleyenUyari;
            bekleyenUyari = -1;
            kopek = false;
            int yeniYavru = kumsal.DenizeUlasanYavru - sayilanYavru;
            if (yeniYavru > 0) RaporEkle("rapor_denize_ulasan", yeniYavru);
            sayilanYavru = kumsal.DenizeUlasanYavru;

            ekGonullu = sonrakiEk;
            sonrakiEk = 0;
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

        /// <summary>Bütçeden yettiği kadarını düşer, alınanı rapora yazar (hiç alınamadıysa satır yok); ödenemeyen kısmı döner.</summary>
        private int Ode(string raporId, int tutar)
        {
            int alinan = Mathf.Min(tutar, butce);
            butce -= alinan;
            if (alinan > 0) RaporEkle(raporId, -alinan);
            return tutar - alinan;
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

        public void GonulluIadeEt()
        {
            if (kullanilanGonullu <= 0) return;
            kullanilanGonullu--;
            Degisti?.Invoke();
        }

        public void EkGonulluEkle(int adet)
        {
            ekGonullu += adet;
            Degisti?.Invoke();
        }

        /// <summary>Gelecek turun gönüllüsü: artı ek gönüllü (Gönüllü Çağrısı, stajyer), eksi meşgul (okul ziyareti).</summary>
        public void SonrakiTurGonullu(int fark)
        {
            if (fark > 0) sonrakiEk += fark;
            else sonrakiMesgul -= fark;
            Degisti?.Invoke();
        }

        /// <summary>Kanıtı açar ya da sayısını artırır; tür yeniyse true.</summary>
        public bool KanitEkle(KanitTuru kanit)
        {
            int sayi = KanitSayisi(kanit);
            kanitSayilari[(int)kanit] = sayi + 1;
            bool yeni = !kanitlar.Contains(kanit);
            if (yeni) kanitlar.Add(kanit);
            Degisti?.Invoke();
            return yeni;
        }

        public void KartOynandi(KartTanimi kart)
        {
            if (kart.TekSeferlik && !oynananTekKartlar.Contains(kart.Id)) oynananTekKartlar.Add(kart.Id);
            bakim += kart.Bakim;
            Degisti?.Invoke();
        }

        public void EkKartEkle(int adet)
        {
            ekKart += adet;
            Degisti?.Invoke();
        }

        /// <summary>Koruma Alanı: otel ve festival bu bölgeye giremez.</summary>
        public void KorumaEkle(int bolge)
        {
            if (!korunanBolgeler.Contains(bolge)) korunanBolgeler.Add(bolge);
            Degisti?.Invoke();
        }

        /// <summary>Sıradaki turun olayı (olay listesindeki sırası; -1 yok).</summary>
        public void ZincirAyarla(int sira)
        {
            zincir = sira;
            Degisti?.Invoke();
        }

        public void KopekAyarla(bool var)
        {
            kopek = var;
            Degisti?.Invoke();
        }

        /// <summary>Tur sonunda uyarı gerçekleşti.</summary>
        public void UyariBitti()
        {
            aktifUyari = -1;
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
