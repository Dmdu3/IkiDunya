using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>İnsan turunda yuvaları haritada gösterir: bandının renginde simge, kafesliyse çevresinde halka. Kumsal değişince yenilenir.</summary>
    public class YuvaHaritasi : MonoBehaviour
    {
        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private TermalAyarlari termal;
        [SerializeField] private Material malzeme;
        [SerializeField] private float boy = 3f;
        [SerializeField] private float yukseklik = 0.6f;
        [SerializeField] private Color kafesRengi = new Color(0.85f, 0.85f, 0.85f);

        private readonly List<GameObject> simgeler = new List<GameObject>();
        private MaterialPropertyBlock blok;

        private void Awake() => blok = new MaterialPropertyBlock();

        private void OnEnable()
        {
            kumsal.Degisti += Kur;
            Kur();
        }

        private void OnDisable() => kumsal.Degisti -= Kur;

        private void Kur()
        {
            foreach (GameObject simge in simgeler)
                if (simge != null) Destroy(simge);
            simgeler.Clear();

            foreach (Yuva yuva in kumsal.Yuvalar)
            {
                if (yuva.Kafesli) Simge(yuva.Konum, boy * 1.4f, yukseklik - 0.05f, kafesRengi);
                Simge(yuva.Konum, boy, yukseklik, yuva.Yumurta > 0 ? termal.Renk(yuva.Bant) : Color.black);
            }
        }

        private void Simge(Vector3 konum, float cap, float y, Color renk)
        {
            GameObject simge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(simge.GetComponent<Collider>());
            simge.transform.SetParent(transform, false);
            simge.transform.position = konum + Vector3.up * y;
            simge.transform.localScale = new Vector3(cap, 0.05f, cap);
            var renderer = simge.GetComponent<Renderer>();
            renderer.sharedMaterial = malzeme;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blok.SetColor(RenkId, renk);
            renderer.SetPropertyBlock(blok);
            simgeler.Add(simge);
        }
    }
}
