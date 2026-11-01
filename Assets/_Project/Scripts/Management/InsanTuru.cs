using System;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Bir insan turu (GDD: Bir turun akışı): rapor → gönüllüleri ata → kart oyna → olay → tur biter (simülasyon) ve akış ilerler.
    /// Tur başında bir hafta geçer (KumsalDurumu.BirHaftaGecti) ve gelir gelir; geçen turun uyarısı bu turun sonunda gerçekleşir, arada hazırlanılır.</summary>
    public class InsanTuru : MonoBehaviour
    {
        public enum Asama { Kapali, Rapor, Gonulluler, Kartlar, Olay, Bitiyor }

        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private YonetimDurumu yonetim;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private HaritaSecici harita;

        private Etkiler etkiler;
        private readonly List<KartTanimi> el = new List<KartTanimi>();
        private int baslangicKaresi;

        public Asama SuAnki { get; private set; }
        public IReadOnlyList<KartTanimi> El => el;
        public OlayTanimi Olay { get; private set; }
        public bool OlaySecildi { get; private set; }
        /// <summary>Haritada hedef bekleyen kart ya da görev (yoksa null).</summary>
        public KartTanimi BekleyenKart { get; private set; }
        public GorevTanimi BekleyenGorev { get; private set; }
        public Hedef BeklenenHedef => BekleyenKart != null ? BekleyenKart.Hedef : BekleyenGorev != null ? BekleyenGorev.Hedef : Hedef.Yok;
        public YonetimDurumu Yonetim => yonetim;
        public KumsalDurumu Kumsal => kumsal;
        /// <summary>Son seçilen hedefte görev neden işe yaramadı (metin id'si); arayüz hedef şeridinde gösterir. Yeni seçimde silinir.</summary>
        public string HedefEngeli { get; private set; }
        /// <summary>Boşta gönüllü ya da oynanabilir kart varken İleri'ye bir kez basıldı; ikinci basış geçer.</summary>
        public bool IlerlemeOnayiBekliyor { get; private set; }

        /// <summary>Aşama, el ya da bekleyen hedef değişince; arayüz yenilenir.</summary>
        public event Action Degisti;

        private void Awake() => etkiler = new Etkiler(kumsal, yonetim, kum);

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            kumsal.KanitAcildi += KanitAcildi;
            harita.BolgeSecildi += BolgeSecildi;
            harita.YuvaSecildi += YuvaSecildi;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (kumsal != null) kumsal.KanitAcildi -= KanitAcildi;
            if (harita == null) return;
            harita.BolgeSecildi -= BolgeSecildi;
            harita.YuvaSecildi -= YuvaSecildi;
        }

        private void AdimBasladi(Adim adim)
        {
            if (adim.Tur != AdimTuru.InsanTuru)
            {
                AsamayaGec(Asama.Kapali);
                return;
            }

            // Önceki adımı bitiren tıklama ya da Space bu turu ilerletmesin.
            baslangicKaresi = Time.frameCount;
            kumsal.BirHaftaGecti();
            yonetim.TurBasla(kumsal);
            el.Clear();
            Olay = null;
            OlaySecildi = false;
            Ipucu.Goster("ipucu_kanit");
            AsamayaGec(Asama.Rapor);
        }

        private void Update()
        {
            if (SuAnki == Asama.Kapali) return;

            // Tur bitti: akış geçişteyse kabul etmez, sonraki karede tekrar denenir (simülasyon bir kez çalıştı).
            if (SuAnki == Asama.Bitiyor)
            {
                if (OyunAkisi.AdimBitti()) AsamayaGec(Asama.Kapali);
                return;
            }

            harita.Hedef = BeklenenHedef;
            // Hedef seçilirken İleri düğmesi gizli; Space de bekleyen kartı ya da görevi bırakıp aşamayı geçmesin. B / sağ tık vazgeçer.
            if (BeklenenHedef != Hedef.Yok)
            {
                if (GameInput.VazgecBasildi) HedeftenVazgec();
                return;
            }
            if (Time.frameCount != baslangicKaresi && GameInput.IleriBasildi) Ilerle();
        }

        /// <summary>İleri düğmesi ve Space. Olay aşamasında seçim yapılmadan ilerlenmez.</summary>
        public void Ilerle()
        {
            BekleyeniBirak();
            // Kullanılmayan gönüllü ya da kart varken ilk basış uyarır, ikincisi geçer (Space'e yanlışlıkla basılabilir).
            if (!IlerlemeOnayiBekliyor && BosKaynakVar())
            {
                IlerlemeOnayiBekliyor = true;
                Ses.Cal(SesTuru.DugmeTikla);
                if (SuAnki == Asama.Gonulluler) Ipucu.Bildir("bildirim_bos_gonullu", yonetim.BosGonullu(kumsal));
                else Ipucu.Bildir("bildirim_oynanabilir_kart");
                Degisti?.Invoke();
                return;
            }
            switch (SuAnki)
            {
                case Asama.Rapor:
                    Ses.Cal(SesTuru.DugmeTikla);
                    AsamayaGec(Asama.Gonulluler);
                    Ipucu.Goster("ipucu_gonullu");
                    break;
                case Asama.Gonulluler:
                    ElDagit();
                    AsamayaGec(Asama.Kartlar);
                    Ipucu.Goster("ipucu_kart");
                    break;
                case Asama.Kartlar:
                    Ses.Cal(SesTuru.DugmeTikla);
                    OlayCek();
                    AsamayaGec(Asama.Olay);
                    break;
                case Asama.Olay:
                    if (!OlaySecildi) return;
                    etkiler.TurSonu();
                    AsamayaGec(Asama.Bitiyor);
                    break;
            }
        }

        private bool BosKaynakVar()
        {
            if (SuAnki == Asama.Gonulluler) return yonetim.BosGonullu(kumsal) > 0;
            if (SuAnki == Asama.Kartlar)
                foreach (KartTanimi kart in el)
                    if (KartOynanabilir(kart)) return true;
            return false;
        }

        // ---- Gönüllüler ----

        public bool GorevVerilebilir(GorevTanimi gorev) => SuAnki == Asama.Gonulluler && GorevEngeli(gorev) == null;

        /// <summary>Görev neden verilemez (metin id'si); verilebilirse null.</summary>
        public string GorevEngeli(GorevTanimi gorev)
        {
            if (yonetim.BosGonullu(kumsal) <= 0) return "engel_gonullu_yok";
            if (gorev.Hedef == Hedef.Yuva && kumsal.Yuvalar.Count == 0) return "engel_yuva_yok";
            return null;
        }

        public void GorevSec(GorevTanimi gorev)
        {
            if (!GorevVerilebilir(gorev)) return;
            // Oyuncu kaynağı kullanmaya döndü; sonraki İleri yeniden uyarır.
            IlerlemeOnayiBekliyor = false;
            if (BekleyenGorev == gorev)
            {
                BekleyeniBirak();
                Degisti?.Invoke();
                return;
            }
            BekleyeniBirak();
            if (gorev.Hedef == Hedef.Yok) etkiler.GorevVer(gorev, -1);
            else BekleyenGorev = gorev;
            Degisti?.Invoke();
        }

        // ---- Kartlar ----

        /// <summary>Havuzdan rastgele el (Üniversite Ortaklığı fazladan verir); uyarı varken o uyarının Acil kartları ek olarak gelir.</summary>
        private void ElDagit()
        {
            el.Clear();
            var havuz = new List<KartTanimi>();
            foreach (KartTanimi kart in yonetim.Ayarlar.Kartlar)
            {
                if (kart == null || !yonetim.Havuzda(kart)) continue;
                if (kart.Tur == KartTuru.Acil) el.Add(kart);
                else havuz.Add(kart);
            }
            int adet = yonetim.Ayarlar.EldekiKart + yonetim.EkKart;
            for (int i = 0; i < adet && havuz.Count > 0; i++)
            {
                int sec = UnityEngine.Random.Range(0, havuz.Count);
                el.Add(havuz[sec]);
                havuz.RemoveAt(sec);
            }
            Ses.Cal(el.Count > 0 ? SesTuru.KartDagit : SesTuru.DugmeTikla);
        }

        public bool KartOynanabilir(KartTanimi kart) => SuAnki == Asama.Kartlar && el.Contains(kart) && KartEngeli(kart) == null;

        /// <summary>Kart neden oynanamaz (metin id'si); oynanabilirse null.</summary>
        public string KartEngeli(KartTanimi kart)
        {
            if (yonetim.Butce < yonetim.Bedel(kart)) return "engel_butce";
            if (!yonetim.KosulTamam(kart)) return "engel_kosul";
            if (kart.Hedef == Hedef.Yuva && kumsal.Yuvalar.Count == 0) return "engel_yuva_yok";
            return null;
        }

        /// <summary>Yuvaya bu tur sonunda avcı gelme ihtimali (0–1); arayüz gösterir.</summary>
        public float AvciRiski(int yuva) => etkiler.AvciRiski(yuva);

        public void KartSec(KartTanimi kart)
        {
            if (!KartOynanabilir(kart)) return;
            IlerlemeOnayiBekliyor = false;
            if (BekleyenKart == kart)
            {
                Ses.Cal(SesTuru.DugmeTikla);
                BekleyeniBirak();
                Degisti?.Invoke();
                return;
            }
            BekleyeniBirak();
            if (kart.Hedef == Hedef.Yok)
            {
                KartiOyna(kart, -1);
            }
            else
            {
                Ses.Cal(SesTuru.DugmeTikla);
                BekleyenKart = kart;
            }
            Degisti?.Invoke();
        }

        private void KartiOyna(KartTanimi kart, int hedef)
        {
            Ses.Cal(SesTuru.KartOyna);
            etkiler.KartOyna(kart, hedef);
            // Oynanan kart elden çıkar; oynanmayanlar tur sonunda havuza döner.
            el.Remove(kart);
        }

        // ---- Olay ----

        private void OlayCek()
        {
            IReadOnlyList<OlayTanimi> olaylar = yonetim.Ayarlar.Olaylar;
            OlaySecildi = false;
            Olay = null;
            if (olaylar.Count == 0)
            {
                OlaySecildi = true;
                return;
            }
            // Geçen turun seçimi bu turun olayını belirlediyse o gelir (zincir).
            int sira = yonetim.Zincir;
            if (sira < 0 || sira >= olaylar.Count || olaylar[sira] == null)
            {
                // Koşulu sağlayanlardan (paydaş küs, en az tur); aynı olay art arda gelmesin.
                var uygun = new List<int>();
                for (int i = 0; i < olaylar.Count; i++)
                    if (olaylar[i] != null && etkiler.OlayUygun(olaylar[i]) && i != yonetim.SonOlay) uygun.Add(i);
                if (uygun.Count == 0 && yonetim.SonOlay >= 0 && yonetim.SonOlay < olaylar.Count) uygun.Add(yonetim.SonOlay);
                if (uygun.Count == 0)
                {
                    OlaySecildi = true;
                    return;
                }
                sira = uygun[UnityEngine.Random.Range(0, uygun.Count)];
            }
            yonetim.ZincirAyarla(-1);
            yonetim.OlayGeldi(sira);
            Olay = olaylar[sira];
        }

        public bool SecenekAcik(bool a) => Olay != null && !OlaySecildi && etkiler.SecenekAcik(Olay, a);

        public void Sec(bool a)
        {
            if (SuAnki != Asama.Olay || !SecenekAcik(a)) return;
            etkiler.OlaySec(Olay, a);
            OlaySecildi = true;
            Ilerle();
        }

        // ---- Harita ----

        private void BolgeSecildi(int bolge)
        {
            if (BeklenenHedef != Hedef.Bolge) return;
            Uygula(bolge);
        }

        private void YuvaSecildi(int yuva)
        {
            if (BeklenenHedef != Hedef.Yuva) return;
            Uygula(yuva);
        }

        private void Uygula(int hedef)
        {
            if (BekleyenKart != null) KartiOyna(BekleyenKart, hedef);
            else if (BekleyenGorev != null && yonetim.BosGonullu(kumsal) > 0)
            {
                // İşe yaramayacak hedefte gönüllü harcanmaz; görev bekler, şerit nedenini söyler, oyuncu başka hedef seçer.
                string engel = etkiler.GorevHedefEngeli(BekleyenGorev, hedef);
                if (engel != null)
                {
                    HedefEngeli = engel;
                    Degisti?.Invoke();
                    return;
                }
                Ses.Cal(SesTuru.DugmeTikla);
                etkiler.GorevVer(BekleyenGorev, hedef);
            }
            BekleyeniBirak();
            Degisti?.Invoke();
        }

        /// <summary>Hedef bekleyen kart ya da görev bırakılır (Geri düğmesi, B, sağ tık).</summary>
        public void HedeftenVazgec()
        {
            if (BeklenenHedef == Hedef.Yok) return;
            Ses.Cal(SesTuru.DugmeTikla);
            BekleyeniBirak();
            Degisti?.Invoke();
        }

        private void BekleyeniBirak()
        {
            BekleyenKart = null;
            BekleyenGorev = null;
            HedefEngeli = null;
        }

        private void KanitAcildi(KayipIzi iz)
        {
            if (yonetim.KanitEkle(iz.Kanit)) yonetim.RaporEkle($"kanit_{iz.Kanit.ToString().ToLowerInvariant()}", 0);
        }

        private void AsamayaGec(Asama yeni)
        {
            SuAnki = yeni;
            IlerlemeOnayiBekliyor = false;
            harita.Acik = yeni != Asama.Kapali;
            Degisti?.Invoke();
        }
    }
}
