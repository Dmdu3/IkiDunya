using System;
using System.Collections.Generic;
using IkiDunya.Camera;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Yavru sürüsü: oyuncu öncüyü yönetir, diğerleri kısa bir gecikmeyle izler. Shift dağıtır, Space zıplatır, E takılan yavruyu kurtarır (bu sırada sürü bekler).
    /// Öncü kendisi takılırsa E'yi basılı tutup kendini kurtarır.</summary>
    public class Suru : MonoBehaviour
    {
        [SerializeField] private YavruAyarlari ayarlar;
        [SerializeField] private Yavru yavruKalibi;
        [Tooltip("Öncünün gözü; öncü değişince yeni öncüye taşınır.")]
        [SerializeField] private BirinciSahisKamera kamera;
        [SerializeField] private float gozYuksekligi = 0.12f;
        [SerializeField] private float gozOnu = 0.12f;

        private readonly List<Yavru> yavrular = new List<Yavru>();
        private readonly List<Vector2> yerler = new List<Vector2>();
        private Yavru oncu;
        private Vector3 karaYonu;
        private float panikKalan;
        private Yavru yardimEdilen;
        private float yardimGecen;

        public IReadOnlyList<Yavru> Yavrular => yavrular;
        public Yavru Oncu => oncu;
        public bool Daginik { get; private set; }
        public bool Aktif { get; private set; }
        public int Kurtulan { get; private set; }
        public int Kaybedilen { get; private set; }
        public int Kalan => yavrular.Count - Kurtulan - Kaybedilen;
        /// <summary>Öncü şu an E ile bir yavruya yardım ediyor mu (sürü bekler).</summary>
        public bool YardimEdiyor => yardimEdilen != null;
        /// <summary>Yardımın doluluğu (0–1).</summary>
        public float YardimOrani => yardimEdilen != null ? yardimGecen / yardimEdilen.YardimSuresi : 0f;

        /// <summary>Bir yavru kaybedildiğinde; yeri ve nedeniyle.</summary>
        public event Action<Yavru, KayipNedeni> YavruKaybedildi;
        public event Action<Yavru> YavruKurtuldu;
        /// <summary>Hiç yavru kalmadığında (hepsi kurtuldu ya da kaybedildi).</summary>
        public event Action Bitti;

        private void Awake()
        {
            int katman = LayerMask.NameToLayer("Turtle");
            Physics.IgnoreLayerCollision(katman, katman, true);
        }

        private void OnEnable() => Tilki.SuruyuKorkuttu += Korktu;
        private void OnDisable() => Tilki.SuruyuKorkuttu -= Korktu;

        /// <summary>Sürüyü bir noktada kurar; ilk yavru öncü olur.</summary>
        public void Kur(Vector3 cikis, Quaternion yon, int sayi)
        {
            Temizle();
            karaYonu = yon * Vector3.back;
            for (int i = 0; i < sayi; i++)
            {
                Vector2 yer = i == 0 ? Vector2.zero : UnityEngine.Random.insideUnitCircle;
                Vector3 konum = cikis + yon * new Vector3(yer.x, 0f, yer.y) * ayarlar.TopluYaricap;
                Yavru yavru = Instantiate(yavruKalibi, konum, yon, transform);
                yavru.gameObject.SetActive(true);
                yavru.Kur(ayarlar);
                yavru.Kaybedildi += Kaybedildi;
                yavru.Kurtuldu += KurtulduOlunca;
                yavrular.Add(yavru);
                yerler.Add(yer);
            }
            Kurtulan = 0;
            Kaybedilen = 0;
            panikKalan = 0f;
            yardimEdilen = null;
            Aktif = sayi > 0;
            if (Aktif) OncuYap(yavrular[0]);
        }

        public void Durdur() => Aktif = false;

        private void Korktu(Vector3 tilkiYeri) => panikKalan = ayarlar.PanikSuresi;

        private void Update()
        {
            if (!Aktif || oncu == null) return;

            if (panikKalan > 0f) panikKalan -= Time.deltaTime;
            Daginik = GameInput.HizliBasili || panikKalan > 0f;

            Yardim();

            Transform o = oncu.transform;
            // Takılı ya da ters dönmüş öncü yürüyemez; E ile kendini kurtarır.
            bool oncuSerbest = oncu.EngelDurumu == Yavru.Engel.Yok;
            Vector2 girdi = YardimEdiyor || !oncuSerbest ? Vector2.zero : Vector2.ClampMagnitude(GameInput.Hareket, 1f);
            oncu.Yuru(oncuSerbest ? (o.forward * girdi.y + o.right * girdi.x) * ayarlar.YurumeHizi + oncu.IsikCekimi : Vector3.zero);

            if (GameInput.ZiplaDalBasildi)
                foreach (Yavru yavru in yavrular) yavru.Zipla();

            float yaricap = Daginik ? ayarlar.DaginikYaricap : ayarlar.TopluYaricap;
            Vector3 arka = o.position - o.forward * ayarlar.ArkaMesafe;
            for (int i = 0; i < yavrular.Count; i++)
            {
                Yavru yavru = yavrular[i];
                if (yavru == oncu || yavru.SuAnki != Yavru.Durum.Suruyor) continue;

                switch (yavru.EngelDurumu)
                {
                    case Yavru.Engel.Takili:
                    case Yavru.Engel.Ters:
                        yavru.Yuru(Vector3.zero);
                        continue;
                    case Yavru.Engel.YonKaybetti:
                        yavru.transform.rotation = Quaternion.LookRotation(karaYonu);
                        yavru.Yuru(karaYonu * ayarlar.YurumeHizi * ayarlar.YonKaybedenHizCarpani);
                        continue;
                }

                // Öncü yardım ederken sürü bekler.
                if (YardimEdiyor)
                {
                    yavru.Yuru(yavru.IsikCekimi);
                    continue;
                }

                Vector3 hedef = arka + o.rotation * new Vector3(yerler[i].x, 0f, yerler[i].y) * yaricap;
                Vector3 fark = hedef - yavru.transform.position;
                fark.y = 0f;
                float mesafe = fark.magnitude;
                if (mesafe < 0.05f)
                {
                    yavru.Yuru(yavru.IsikCekimi);
                    continue;
                }
                float hiz = ayarlar.YurumeHizi * ayarlar.TakipHizCarpani * Mathf.Clamp01(mesafe);
                yavru.transform.rotation = Quaternion.LookRotation(fark / mesafe);
                yavru.Yuru(fark / mesafe * hiz + yavru.IsikCekimi);
            }
        }

        /// <summary>E basılıyken önce öncünün kendisine (takıldıysa), yoksa yanındaki, yardım bekleyen en yakın yavruya yardım eder.</summary>
        private void Yardim()
        {
            Yavru aday = null;
            if (GameInput.EtkilesBasili) aday = oncu.EngelDurumu != Yavru.Engel.Yok ? oncu : YardimBekleyenEnYakin();
            if (aday != yardimEdilen)
            {
                yardimEdilen = aday;
                yardimGecen = 0f;
            }
            if (yardimEdilen == null) return;

            yardimGecen += Time.deltaTime;
            if (yardimGecen < yardimEdilen.YardimSuresi) return;
            yardimEdilen.YardimEt();
            yardimEdilen = null;
            yardimGecen = 0f;
        }

        private Yavru YardimBekleyenEnYakin()
        {
            Yavru enYakin = null;
            float enKisa = ayarlar.KurtarmaMesafesi;
            foreach (Yavru yavru in yavrular)
            {
                if (yavru == oncu || yavru.SuAnki != Yavru.Durum.Suruyor || yavru.EngelDurumu == Yavru.Engel.Yok) continue;
                float mesafe = Vector3.Distance(yavru.transform.position, oncu.transform.position);
                if (mesafe > enKisa) continue;
                enKisa = mesafe;
                enYakin = yavru;
            }
            return enYakin;
        }

        private void Kaybedildi(Yavru yavru, KayipNedeni neden)
        {
            Kaybedilen++;
            YavruKaybedildi?.Invoke(yavru, neden);
            YavruBitti(yavru);
        }

        private void KurtulduOlunca(Yavru yavru)
        {
            Kurtulan++;
            YavruKurtuldu?.Invoke(yavru);
            YavruBitti(yavru);
        }

        private void YavruBitti(Yavru yavru)
        {
            if (yavru == yardimEdilen) yardimEdilen = null;
            if (Kalan == 0)
            {
                Aktif = false;
                kamera.transform.SetParent(transform, true);
                Bitti?.Invoke();
                return;
            }
            if (yavru != oncu) return;

            // Öncü gitti: sıradaki serbest yavru öncü olur; serbest yoksa herhangi biri.
            Yavru yeni = null;
            foreach (Yavru aday in yavrular)
            {
                if (aday.SuAnki != Yavru.Durum.Suruyor) continue;
                if (yeni == null || (yeni.EngelDurumu != Yavru.Engel.Yok && aday.EngelDurumu == Yavru.Engel.Yok)) yeni = aday;
            }
            if (yeni == null) return;
            OncuYap(yeni);
            // Kamera başka yavruya geçti: oyuncu nedenini bilsin (öncü denize mi ulaştı, kaybedildi mi).
            Ipucu.Bildir(yavru.SuAnki == Yavru.Durum.Kurtuldu ? "bildirim_oncu_kurtuldu" : "bildirim_oncu_kayip", Kalan);
        }

        private void OncuYap(Yavru yeni)
        {
            if (oncu != null) oncu.Oncu = false;
            oncu = yeni;
            oncu.Oncu = true;
            kamera.transform.SetParent(yeni.transform, false);
            kamera.transform.localPosition = new Vector3(0f, gozYuksekligi, gozOnu);
            kamera.transform.localRotation = Quaternion.identity;
            kamera.GovdeAyarla(yeni.transform);
        }

        private void Temizle()
        {
            if (kamera.transform.parent != transform) kamera.transform.SetParent(transform, false);
            foreach (Yavru yavru in yavrular)
                if (yavru != null) Destroy(yavru.gameObject);
            yavrular.Clear();
            yerler.Clear();
            oncu = null;
        }
    }
}
