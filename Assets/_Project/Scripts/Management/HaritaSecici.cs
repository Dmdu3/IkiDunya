using System;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IkiDunya.Management
{
    /// <summary>İnsan turunda kumsal haritası: imlecin altındaki bölgeyi vurgular, tıklanınca bölgeyi ya da en yakın yuvayı bildirir. Arayüzün üstündeki tıklamaları yok sayar.</summary>
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

        /// <summary>İmlecin altındaki bölge; kumsal dışındaysa -1.</summary>
        public int Ustundeki { get; private set; } = -1;
        /// <summary>İnsan turu açıkken true; kapalıyken vurgu gizlenir, tıklama dinlenmez.</summary>
        public bool Acik { get; set; }

        public event Action<int> BolgeSecildi;
        public event Action<int> YuvaSecildi;

        private void Update()
        {
            Ustundeki = -1;
            Vector3 nokta = Vector3.zero;
            bool arayuzde = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Acik && !arayuzde && Isaret(out nokta)) Ustundeki = kum.Bolge(nokta);

            if (vurgu != null)
            {
                vurgu.gameObject.SetActive(Ustundeki >= 0);
                if (Ustundeki >= 0)
                {
                    Vector3 orta = kum.transform.TransformPoint(new Vector3(-kum.Uzunluk / 2f + kum.BolgeGenisligi * (Ustundeki + 0.5f), vurguYuksekligi, kum.Derinlik / 2f));
                    vurgu.position = orta;
                    vurgu.rotation = kum.transform.rotation;
                    vurgu.localScale = new Vector3(kum.BolgeGenisligi, vurgu.localScale.y, kum.Derinlik);
                }
            }

            if (Ustundeki < 0 || !GameInput.TiklaBasildi) return;
            BolgeSecildi?.Invoke(Ustundeki);
            int yuva = EnYakinYuva(nokta);
            if (yuva >= 0) YuvaSecildi?.Invoke(yuva);
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
