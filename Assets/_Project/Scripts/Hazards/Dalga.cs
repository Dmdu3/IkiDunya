using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kıyının son metrelerinde gelip giden dalga. Çekilirken suya giren yavru kurtulur, vururken giren geri savrulur.</summary>
    public class Dalga : MonoBehaviour
    {
        [SerializeField] private KacisAyarlari ayarlar;
        [Tooltip("Kıyıya göre konumlanan su şeridi (çarpışmasız); kara kenarı dalgayla ileri geri gider.")]
        [SerializeField] private Transform suSeridi;
        [Tooltip("Su şeridinin deniz tarafında kıyıdan ne kadar içeri uzandığı (metre).")]
        [SerializeField] private float denizPayi = 1f;

        private float zaman;

        /// <summary>Suyun karadaki kenarının kıyıdan uzaklığı (0 – dalga bölgesi).</summary>
        public float SuKenari => (0.5f - 0.5f * Mathf.Cos(Faz)) * ayarlar.DalgaBolgesi;
        /// <summary>Dalga denize çekiliyor mu (kenar kıyıya doğru geliyor).</summary>
        public bool Cekiliyor => Mathf.Sin(Faz) < 0f;

        private float Faz => 2f * Mathf.PI * zaman / ayarlar.DalgaPeriyodu;

        private void Update()
        {
            zaman += Time.deltaTime;
            if (suSeridi == null) return;

            float boy = SuKenari + denizPayi;
            Vector3 olcek = suSeridi.localScale;
            suSeridi.localScale = new Vector3(olcek.x, olcek.y, boy);
            suSeridi.localPosition = new Vector3(suSeridi.localPosition.x, suSeridi.localPosition.y, SuKenari - boy / 2f);
        }
    }
}
