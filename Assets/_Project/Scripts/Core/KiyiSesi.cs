using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Kıyının dalga uğultusu: kumsal boyunca dinleyicinin hizasında, su çizgisinden gelir. Göz yanılsa da kulak denizin yönünü doğru söyler.
    /// Klip SesAyarlari'ndan (SesTuru.Kiyi); yoksa sessiz.</summary>
    public class KiyiSesi : MonoBehaviour
    {
        [Tooltip("Su çizgisinin yeri (dünya z) ve yüksekliği.")]
        [SerializeField] private float kiyiZ = -2f;
        [SerializeField] private float yukseklik = 0.3f;
        [Tooltip("Ses bu uzaklığa kadar tam, sonra azalır (metre).")]
        [SerializeField] private float tamUzaklik = 6f;
        [SerializeField] private float enUzak = 70f;

        private AudioSource kaynak;

        private void OnEnable()
        {
            if (kaynak == null)
            {
                kaynak = gameObject.AddComponent<AudioSource>();
                kaynak.loop = true;
                kaynak.playOnAwake = false;
                kaynak.spatialBlend = 1f;
                kaynak.rolloffMode = AudioRolloffMode.Linear;
                kaynak.minDistance = tamUzaklik;
                kaynak.maxDistance = enUzak;
                kaynak.dopplerLevel = 0f;
            }
        }

        private void Update()
        {
            AudioListener kulak = TekKulak.Etkin;
            if (kulak != null) transform.position = new Vector3(kulak.transform.position.x, yukseklik, kiyiZ);

            // Ses yöneticisi sahneden sonra hazır olabilir: klip gelene kadar dene.
            if (kaynak.clip != null) return;
            kaynak.clip = Ses.Klip(SesTuru.Kiyi, out float duzey);
            kaynak.volume = duzey;
            if (kaynak.clip == null) return;
            kaynak.time = Random.Range(0f, kaynak.clip.length);
            kaynak.Play();
        }
    }
}
