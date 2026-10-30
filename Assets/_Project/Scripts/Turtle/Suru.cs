using System;
using System.Collections.Generic;
using IkiDunya.Camera;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Yavru sürüsü: oyuncu öncüyü yönetir, diğerleri kısa bir gecikmeyle izler. Shift basılıyken dağılır, bırakınca toplanır.</summary>
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

        public IReadOnlyList<Yavru> Yavrular => yavrular;
        public Yavru Oncu => oncu;
        public bool Daginik { get; private set; }
        public bool Aktif { get; private set; }
        public int Kurtulan { get; private set; }
        public int Kaybedilen { get; private set; }
        public int Kalan => yavrular.Count - Kurtulan - Kaybedilen;

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

        /// <summary>Sürüyü bir noktada kurar; ilk yavru öncü olur.</summary>
        public void Kur(Vector3 cikis, Quaternion yon, int sayi)
        {
            Temizle();
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
            Aktif = sayi > 0;
            if (Aktif) OncuYap(yavrular[0]);
        }

        public void Durdur() => Aktif = false;

        private void Update()
        {
            if (!Aktif || oncu == null) return;

            Daginik = GameInput.HizliBasili;

            Vector2 girdi = Vector2.ClampMagnitude(GameInput.Hareket, 1f);
            Transform o = oncu.transform;
            oncu.Yuru((o.forward * girdi.y + o.right * girdi.x) * ayarlar.YurumeHizi);

            float yaricap = Daginik ? ayarlar.DaginikYaricap : ayarlar.TopluYaricap;
            Vector3 arka = o.position - o.forward * ayarlar.ArkaMesafe;
            for (int i = 0; i < yavrular.Count; i++)
            {
                Yavru yavru = yavrular[i];
                if (yavru == oncu || yavru.SuAnki != Yavru.Durum.Suruyor) continue;

                Vector3 hedef = arka + o.rotation * new Vector3(yerler[i].x, 0f, yerler[i].y) * yaricap;
                Vector3 fark = hedef - yavru.transform.position;
                fark.y = 0f;
                float mesafe = fark.magnitude;
                if (mesafe < 0.05f)
                {
                    yavru.Yuru(Vector3.zero);
                    continue;
                }
                float hiz = ayarlar.YurumeHizi * ayarlar.TakipHizCarpani * Mathf.Clamp01(mesafe);
                yavru.transform.rotation = Quaternion.LookRotation(fark / mesafe);
                yavru.Yuru(fark / mesafe * hiz);
            }
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
            if (Kalan == 0)
            {
                Aktif = false;
                kamera.transform.SetParent(transform, true);
                Bitti?.Invoke();
                return;
            }
            if (yavru != oncu) return;

            // Öncü gitti: sıradaki yaşayan yavru öncü olur.
            foreach (Yavru aday in yavrular)
                if (aday.SuAnki == Yavru.Durum.Suruyor)
                {
                    OncuYap(aday);
                    return;
                }
        }

        private void OncuYap(Yavru yeni)
        {
            oncu = yeni;
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
