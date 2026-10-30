using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Otel, yol lambası ya da telefon feneri (Hazard layer, Is Trigger alan). Alanın içindeki yavruyu yan rüzgâr gibi kendine çeker; Işık Yasağı otel ve yol ışıklarını söndürür.</summary>
    [RequireComponent(typeof(Collider))]
    public class IsikKaynagi : MonoBehaviour
    {
        [SerializeField] private KacisAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Otel ve yol ışıkları yasakla söner; telefon feneri sönmez.")]
        [SerializeField] private bool yasaklanabilir = true;
        [Tooltip("Görünen ışık (isteğe bağlı).")]
        [SerializeField] private Light isik;

        public bool Acik => !(yasaklanabilir && kumsal.IsikYasagi);

        private void OnEnable()
        {
            kumsal.Degisti += Guncelle;
            Guncelle();
        }

        private void OnDisable() => kumsal.Degisti -= Guncelle;

        private void Guncelle()
        {
            if (isik != null) isik.enabled = Acik;
        }

        /// <summary>Bu noktadaki yavruya uygulanan çekim hızı (yatay).</summary>
        public Vector3 Cekim(Vector3 konum)
        {
            if (!Acik) return Vector3.zero;
            Vector3 yon = transform.position - konum;
            yon.y = 0f;
            return yon.sqrMagnitude > 0.01f ? yon.normalized * ayarlar.IsikCekimGucu : Vector3.zero;
        }
    }
}
