using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Deniz akıntısı (Is Trigger alan). Yönü objenin ileri yönüdür; içindeki yüzücüyü sürükler, karşı yüzmek daha çok enerji harcatır.
    /// Görünüşü alanın içinde akış yönünde süzülen ince çizgiler; alanın sonuna gelen çizgi başa döner.</summary>
    [RequireComponent(typeof(Collider))]
    public class Akinti : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [Tooltip("Akış yönünde süzülen çizgiler (alanın çocuğu olmayan, ölçeksiz nesneler).")]
        [SerializeField] private Transform[] cizgiler = new Transform[0];
        [Tooltip("Çizgilerin akıntıya göre hız çarpanı (görünsün diye biraz hızlı).")]
        [SerializeField] private float cizgiHizCarpani = 2f;

        private Collider alan;

        /// <summary>Akıntının sürükleme hızı (dünya yönünde).</summary>
        public Vector3 Hiz => transform.forward * ayarlar.AkintiHizi;

        private void Awake() => alan = GetComponent<Collider>();

        private void Update()
        {
            if (cizgiler.Length == 0) return;
            Vector3 adim = Hiz * cizgiHizCarpani * Time.deltaTime;
            Bounds b = alan.bounds;
            foreach (Transform cizgi in cizgiler)
            {
                if (cizgi == null) continue;
                cizgi.position += adim;
                if (b.Contains(cizgi.position)) continue;
                // Alanın akış yönündeki ucundan çıktı: karşı uca, rastgele bir yere.
                Vector3 yerel = transform.InverseTransformPoint(cizgi.position);
                yerel.z = -0.5f;
                yerel.x = Random.Range(-0.5f, 0.5f);
                yerel.y = Random.Range(-0.5f, 0.5f);
                cizgi.position = transform.TransformPoint(yerel);
            }
        }
    }
}
