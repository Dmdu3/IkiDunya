using System;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Bir insan turu (GDD: Bir turun akışı): rapor → gönüllüleri ata → kart oyna → olay → tur biter (simülasyon) ve akış ilerler.
    /// Tur başında bir hafta geçer (KumsalDurumu.BirHaftaGecti), gelir gelir, geçen turun uyarısı gerçekleşir.</summary>
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
            etkiler.UyariyiUygula();
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

            if (Time.frameCount != baslangicKaresi && GameInput.ZiplaDalBasildi) Ilerle();
        }

        /// <summary>İleri düğmesi ve Space. Olay aşamasında seçim yapılmadan ilerlenmez.</summary>
        public void Ilerle()
        {
            BekleyeniBirak();
            switch (SuAnki)
            {
                case Asama.Rapor:
                    AsamayaGec(Asama.Gonulluler);
                    Ipucu.Goster("ipucu_gonullu");
                    break;
                case Asama.Gonulluler:
                    ElDagit();
                    AsamayaGec(Asama.Kartlar);
                    Ipucu.Goster("ipucu_kart");
                    break;
                case Asama.Kartlar:
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

        // ---- Gönüllüler ----

        public bool GorevVerilebilir(GorevTanimi gorev) =>
            SuAnki == Asama.Gonulluler && yonetim.BosGonullu(kumsal) > 0 && (gorev.Hedef != Hedef.Yuva || kumsal.Yuvalar.Count > 0);

        public void GorevSec(GorevTanimi gorev)
        {
            if (!GorevVerilebilir(gorev)) return;
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

        private void ElDagit()
        {
            el.Clear();
            var havuz = new List<KartTanimi>();
            foreach (KartTanimi kart in yonetim.Ayarlar.Kartlar)
                if (kart != null && yonetim.Havuzda(kart)) havuz.Add(kart);
            for (int i = 0; i < yonetim.Ayarlar.EldekiKart && havuz.Count > 0; i++)
            {
                int sec = UnityEngine.Random.Range(0, havuz.Count);
                el.Add(havuz[sec]);
                havuz.RemoveAt(sec);
            }
        }

        public bool KartOynanabilir(KartTanimi kart) =>
            SuAnki == Asama.Kartlar && el.Contains(kart) && yonetim.Butce >= yonetim.Bedel(kart)
            && (kart.Hedef != Hedef.Yuva || kumsal.Yuvalar.Count > 0);

        public void KartSec(KartTanimi kart)
        {
            if (!KartOynanabilir(kart)) return;
            if (BekleyenKart == kart)
            {
                BekleyeniBirak();
                Degisti?.Invoke();
                return;
            }
            BekleyeniBirak();
            if (kart.Hedef == Hedef.Yok) KartiOyna(kart, -1);
            else BekleyenKart = kart;
            Degisti?.Invoke();
        }

        private void KartiOyna(KartTanimi kart, int hedef)
        {
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
            // Aynı olay art arda gelmesin.
            int sira = UnityEngine.Random.Range(0, olaylar.Count);
            if (olaylar.Count > 1 && sira == yonetim.SonOlay) sira = (sira + 1) % olaylar.Count;
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
            else if (BekleyenGorev != null && yonetim.BosGonullu(kumsal) > 0) etkiler.GorevVer(BekleyenGorev, hedef);
            BekleyeniBirak();
            Degisti?.Invoke();
        }

        private void BekleyeniBirak()
        {
            BekleyenKart = null;
            BekleyenGorev = null;
        }

        private void KanitAcildi(KayipIzi iz)
        {
            if (yonetim.KanitEkle(iz.Kanit)) yonetim.RaporEkle($"kanit_{iz.Kanit.ToString().ToLowerInvariant()}", 0);
        }

        private void AsamayaGec(Asama yeni)
        {
            SuAnki = yeni;
            harita.Acik = yeni != Asama.Kapali;
            Degisti?.Invoke();
        }
    }
}
