using System;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IkiDunya.Management
{
    /// <summary>İnsan turunda kumsal haritası: imlecin altındaki bölgeyi vurgular, tıklanınca bölgeyi ya da en yakın yuvayı bildirir. Arayüzün üstündeki tıklamaları yok sayar.
    /// Hedef beklenirken oyun kolu (sol çubuk) ya da A/D ile bölgeler ya da yuvalar soldan sağa gezilir, A / Space seçer; fare kıpırdayınca imlece döner.</summary>
    public class HaritaSecici : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera kamera;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Bölgenin üstüne serilen yarı saydam vurgu (çarpışmasız).")]
        [SerializeField] private Transform vurgu;
        [Tooltip("Tıklanan noktaya bu kadar yakın yuva seçilir (metre).")]
        [SerializeField] private float yuvaSecmeMesafesi = 5f;
        [Tooltip("Vurgunun kumsalın üstünde durduğu yükseklik (metre); tepeden bakılır, kumulların üstünde kalsın.")]
        [SerializeField] private float vurguYuksekligi = 6f;
        [Tooltip("Çubukla yuva seçerken vurgunun kenarı (metre); bölge yerine sadece yuvanın üstü vurgulanır.")]
        [SerializeField] private float yuvaVurgusu = 5f;
        [Tooltip("Çubuğun gezinmeyi başlatan eğimi (0–1).")]
        [SerializeField] private float cubukEsigi = 0.5f;

        /// <summary>İmlecin altındaki bölge; kumsal dışındaysa -1.</summary>
        public int Ustundeki { get; private set; } = -1;
        /// <summary>İnsan turu açıkken true; kapalıyken vurgu gizlenir, tıklama dinlenmez.</summary>
        public bool Acik { get; set; }
        /// <summary>Beklenen hedef (InsanTuru her kare yazar); bekleniyorsa çubukla gezilebilir.</summary>
        public Hedef Hedef { get; set; }

        public event Action<int> BolgeSecildi;
        public event Action<int> YuvaSecildi;

        // Çubukla gezinme: seçili bölge ya da yuva (yuvalar soldan sağa sırada).
        private bool cubukta;
        private int cubukBolge;
        private int cubukYuva = -1;
        private int sonYon;

        private void Update()
        {
            if (!Acik || Hedef == Hedef.Yok) cubukta = false;
            Gezin();

            Ustundeki = -1;
            Vector3 nokta = Vector3.zero;
            bool yuvaSecimi = cubukta && Hedef == Hedef.Yuva && cubukYuva >= 0 && cubukYuva < kumsal.Yuvalar.Count;
            if (yuvaSecimi) Ustundeki = kumsal.Yuvalar[cubukYuva].Bolge;
            else if (cubukta) Ustundeki = cubukBolge;
            else
            {
                bool arayuzde = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (Acik && !arayuzde && Isaret(out nokta)) Ustundeki = kum.Bolge(nokta);
            }

            if (vurgu != null)
            {
                vurgu.gameObject.SetActive(Ustundeki >= 0);
                if (yuvaSecimi)
                {
                    Vector3 konum = kumsal.Yuvalar[cubukYuva].Konum;
                    vurgu.position = new Vector3(konum.x, kum.transform.position.y + vurguYuksekligi, konum.z);
                    vurgu.rotation = kum.transform.rotation;
                    vurgu.localScale = new Vector3(yuvaVurgusu, vurgu.localScale.y, yuvaVurgusu);
                }
                else if (Ustundeki >= 0)
                {
                    Vector3 orta = kum.transform.TransformPoint(new Vector3(-kum.Uzunluk / 2f + kum.BolgeGenisligi * (Ustundeki + 0.5f), vurguYuksekligi, kum.Derinlik / 2f));
                    vurgu.position = orta;
                    vurgu.rotation = kum.transform.rotation;
                    vurgu.localScale = new Vector3(kum.BolgeGenisligi, vurgu.localScale.y, kum.Derinlik);
                }
            }

            if (Ustundeki < 0) return;
            if (cubukta)
            {
                if (!GameInput.ZiplaDalBasildi) return;
                if (yuvaSecimi) YuvaSecildi?.Invoke(cubukYuva);
                else BolgeSecildi?.Invoke(cubukBolge);
                return;
            }
            if (!GameInput.TiklaBasildi) return;
            BolgeSecildi?.Invoke(Ustundeki);
            int yuva = EnYakinYuva(nokta);
            if (yuva >= 0) YuvaSecildi?.Invoke(yuva);
        }

        /// <summary>Hedef beklenirken çubuk ya da A/D bir adım sola/sağa götürür (basılı tutmak tek adım); fare kıpırdayınca imlece dönülür.</summary>
        private void Gezin()
        {
            // Fare kıpırdadı mı: konum değil hareket (editörde imleç konumu cihazlar arasında gidip gelebiliyor).
            if (GameInput.FareKipirdadi) cubukta = false;

            float x = GameInput.Hareket.x;
            int yon = x > cubukEsigi ? 1 : x < -cubukEsigi ? -1 : 0;
            bool yeni = yon != 0 && yon != sonYon;
            sonYon = yon;
            if (!Acik || Hedef == Hedef.Yok || !yeni) return;

            if (Hedef == Hedef.Yuva)
            {
                if (kumsal.Yuvalar.Count == 0) return;
                // İlk adımda imlecin bölgesine en yakın yuvadan başla.
                cubukYuva = cubukta ? KomsuYuva(cubukYuva, yon) : KomsuYuva(-1, yon);
            }
            else
            {
                int sayi = kumsal.BolgeSayisi;
                cubukBolge = cubukta ? (cubukBolge + yon + sayi) % sayi : Mathf.Max(0, Ustundeki);
            }
            cubukta = true;
        }

        /// <summary>Yuvalar kumsal boyunca soldan sağa; şimdikinin bir sağındaki ya da solundaki (uçta başa döner). Şimdiki yoksa en soldaki/sağdaki.</summary>
        private int KomsuYuva(int simdiki, int yon)
        {
            int n = kumsal.Yuvalar.Count;
            var sira = new int[n];
            var x = new float[n];
            for (int i = 0; i < n; i++)
            {
                sira[i] = i;
                x[i] = kum.transform.InverseTransformPoint(kumsal.Yuvalar[i].Konum).x;
            }
            Array.Sort(x, sira);
            int yer = Array.IndexOf(sira, simdiki);
            if (yer < 0) return sira[yon > 0 ? 0 : n - 1];
            return sira[(yer + yon + n) % n];
        }

        /// <summary>İmlecin kumsal düzlemindeki noktası; kumsalın dışındaysa false.</summary>
        private bool Isaret(out Vector3 nokta)
        {
            nokta = Vector3.zero;
            Ray isin = kamera.ScreenPointToRay(GameInput.IsaretciKonumu);
            // Önce zemine (kumsal arazisi engebeli olabilir), olmazsa kumsalın düzlemine.
            if (Physics.Raycast(isin, out RaycastHit carpan, kamera.farClipPlane, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                nokta = carpan.point;
            else
            {
                var duzlem = new Plane(kum.transform.up, kum.transform.position);
                if (!duzlem.Raycast(isin, out float uzaklik)) return false;
                nokta = isin.GetPoint(uzaklik);
            }
            Vector3 yerel = kum.transform.InverseTransformPoint(nokta);
            return Mathf.Abs(yerel.x) <= kum.Uzunluk / 2f && yerel.z >= 0f && yerel.z <= kum.Derinlik;
        }

        private int EnYakinYuva(Vector3 nokta)
        {
            int enYakin = -1;
            float enKisa = yuvaSecmeMesafesi;
            for (int i = 0; i < kumsal.Yuvalar.Count; i++)
            {
                Vector3 fark = kumsal.Yuvalar[i].Konum - nokta;
                fark.y = 0f;
                if (fark.magnitude > enKisa) continue;
                enKisa = fark.magnitude;
                enYakin = i;
            }
            return enYakin;
        }
    }
}
