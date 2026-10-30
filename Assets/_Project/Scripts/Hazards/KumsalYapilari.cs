using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalı KumsalDurumu'ndan kurar: gölgelikli bölgelere gölgelik, kayıp izlerinin yerine iz. Durum değişince yeniden kurar.</summary>
    public class KumsalYapilari : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private KumSicakligi kum;
        [Tooltip("Structure layer'ında gölgelik kalıbı.")]
        [SerializeField] private GameObject golgelikKalibi;
        [Tooltip("Kayıp izinin kumdaki işareti.")]
        [SerializeField] private GameObject kayipIziKalibi;
        [Tooltip("Gölgeliğin bölge içinde deniz kenarından uzaklığı (metre).")]
        [SerializeField] private float golgelikUzakligi = 20f;

        private readonly List<GameObject> kurulan = new List<GameObject>();

        private void OnEnable()
        {
            kumsal.Degisti += Kur;
            Kur();
        }

        private void OnDisable()
        {
            kumsal.Degisti -= Kur;
        }

        private void Kur()
        {
            foreach (GameObject nesne in kurulan) Destroy(nesne);
            kurulan.Clear();

            for (int i = 0; i < kumsal.BolgeSayisi && i < kum.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.Golgelik))
                    kurulan.Add(Instantiate(golgelikKalibi, kum.BolgeNoktasi(i, golgelikUzakligi), kum.transform.rotation, transform));

            foreach (KayipIzi iz in kumsal.KayipIzleri)
                kurulan.Add(Instantiate(kayipIziKalibi, iz.Konum, Quaternion.identity, transform));
        }
    }
}
