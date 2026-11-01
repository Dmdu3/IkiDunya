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
                // Kafes halkası ve sıcaklık rengi korunur; yuva kum rengiyle çizilir.
                if (yuva.Kafesli) Simge(yuva.Konum, boy * 1.4f, yukseklik - 0.24f, kafesRengi);
                Simge(yuva.Konum, boy, yukseklik - 0.12f, new Color32(178, 135, 88, 255));
                Simge(yuva.Konum, boy * 0.78f, yukseklik, new Color32(226, 188, 129, 255));
                if (yuva.Yumurta > 0)
                {
                    YumurtaSimgesi(yuva.Konum, -.14f, -.08f, -18f);
                    YumurtaSimgesi(yuva.Konum, .14f, -.08f, 18f);
                    YumurtaSimgesi(yuva.Konum, 0f, .15f, 5f);
                }
                Vector3 durum = yuva.Konum + new Vector3(boy * .35f, 0f, -boy * .33f);
                Simge(durum, boy * .38f, yukseklik + .12f, new Color32(250, 247, 240, 255));
                Simge(durum, boy * .25f, yukseklik + .24f, yuva.Yumurta > 0 ? termal.Renk(kumsal.YuvaBandi(yuva)) : Color.black);
            }
        }

        private void YumurtaSimgesi(Vector3 konum, float x, float z, float aci)
        {
            GameObject yumurta = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(yumurta.GetComponent<Collider>());
            yumurta.transform.SetParent(transform, false);
            yumurta.transform.position = konum + new Vector3(x * boy, yukseklik + boy * .075f, z * boy);
            yumurta.transform.localScale = new Vector3(boy * .17f, boy * .12f, boy * .20f);
            yumurta.transform.localRotation = Quaternion.Euler(0f, aci, 0f);
            var renderer = yumurta.GetComponent<Renderer>();
            renderer.sharedMaterial = malzeme;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blok.SetColor(RenkId, new Color32(250, 247, 240, 255));
            renderer.SetPropertyBlock(blok);
            simgeler.Add(yumurta);
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
