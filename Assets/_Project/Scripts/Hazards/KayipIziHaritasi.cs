using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IkiDunya.Hazards
{
    /// <summary>İnsan turunda kayıp izlerini kumsalın üstünde kanıt rengiyle gösterir; simgeye tıklanınca kanıt açılır (KumsalDurumu.KanitAc).</summary>
    public class KayipIziHaritasi : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private HaritaAyarlari ayarlar;
        [Tooltip("Kumsala tepeden bakan kamera.")]
        [SerializeField] private UnityEngine.Camera kamera;
        [Tooltip("Simgenin malzemesi (ışıksız); rengi simge başına verilir.")]
        [SerializeField] private Material simgeMalzemesi;

        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        private readonly List<Collider> simgeler = new List<Collider>();
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
            foreach (Collider simge in simgeler)
                if (simge != null) Destroy(simge.gameObject);
            simgeler.Clear();

            IReadOnlyList<KayipIzi> izler = kumsal.KayipIzleri;
            for (int i = 0; i < izler.Count; i++)
            {
                GameObject simge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                simge.name = $"Iz_{i}_{izler[i].Kanit}";
                simge.transform.SetParent(transform, false);
                simge.transform.position = izler[i].Konum + Vector3.up * ayarlar.SimgeYuksekligi;
                simge.transform.localScale = new Vector3(ayarlar.SimgeBoyu, 0.05f, ayarlar.SimgeBoyu);

                var renderer = simge.GetComponent<Renderer>();
                renderer.sharedMaterial = simgeMalzemesi;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                blok.SetColor(RenkId, izler[i].KanitAcildi ? ayarlar.AcilmisRenk : ayarlar.Renk(izler[i].Kanit));
                renderer.SetPropertyBlock(blok);
                simgeler.Add(simge.GetComponent<Collider>());
            }
        }

        private void Update()
        {
            if (!GameInput.TiklaBasildi) return;
            // Kart ya da olay paneline tıklanınca altındaki iz açılmasın.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Ray isin = kamera.ScreenPointToRay(GameInput.IsaretciKonumu);
            // Lambanın çekim alanı gibi trigger'lar ışını kesmesin; üst üste izlerde açılmamış en yakını açılır.
            RaycastHit[] carpanlar = Physics.RaycastAll(isin, kamera.farClipPlane, ~0, QueryTriggerInteraction.Ignore);
            int sira = -1;
            float enYakin = float.MaxValue;
            foreach (RaycastHit carpan in carpanlar)
            {
                int s = simgeler.IndexOf(carpan.collider);
                if (s < 0 || kumsal.KayipIzleri[s].KanitAcildi || carpan.distance >= enYakin) continue;
                enYakin = carpan.distance;
                sira = s;
            }
            if (sira < 0) return;
            Debug.Log($"KayipIziHaritasi: kanıt açıldı — {kumsal.KayipIzleri[sira].Kanit}.");
            Ses.Cal(SesTuru.KanitBulundu);
            kumsal.KanitAc(sira);
        }
    }
}
