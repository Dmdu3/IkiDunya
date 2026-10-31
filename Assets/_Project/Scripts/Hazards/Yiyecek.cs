using System;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Denizdeki besin ya da ona benzeyen çöp. Denizanası kasılarak ilerler, poşet akıntıyla düz süzülür; yakından bakınca ayırt edilir.</summary>
    public class Yiyecek : MonoBehaviour
    {
        public enum Turu { Denizanasi, Poset }

        [SerializeField] private Turu tur = Turu.Denizanasi;
        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [Tooltip("Süzülme yönü (dünya).")]
        [SerializeField] private Vector3 yon = Vector3.right;

        private Vector3 ilkOlcek;
        private float zaman;

        public Turu Tur => tur;

        /// <summary>Bir yiyecek yenince (poşet de "yenir"); parkurlar sayar.</summary>
        public static event Action<Turu> Yendi;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => Yendi = null;

        /// <summary>Akıntıyla taşınan sürüdeki yiyeceğin yönü (poşet sürüsü kurar).</summary>
        public void YonAyarla(Vector3 yeniYon) => yon = yeniYon;

        private void Awake()
        {
            ilkOlcek = transform.localScale;
            zaman = UnityEngine.Random.value * 10f;
        }

        private void Update()
        {
            zaman += Time.deltaTime;
            if (tur == Turu.Denizanasi)
            {
                // Kasılma: kısa itişler ve nabız gibi atan gövde
                float nabiz = Mathf.Max(0f, Mathf.Sin(zaman * ayarlar.KasilmaSikligi * 2f * Mathf.PI));
                transform.position += (Vector3.up * 0.5f + yon.normalized) * nabiz * ayarlar.SuzulmeHizi * 2f * Time.deltaTime;
                transform.localScale = Vector3.Scale(ilkOlcek, new Vector3(1f + 0.15f * nabiz, 1f - 0.2f * nabiz, 1f + 0.15f * nabiz));
            }
            else
            {
                transform.position += yon.normalized * ayarlar.SuzulmeHizi * Time.deltaTime;
            }
        }

        /// <summary>Yenir ve kaybolur; ne olduğunu döner.</summary>
        public Turu Ye()
        {
            gameObject.SetActive(false);
            Yendi?.Invoke(tur);
            return tur;
        }
    }
}
