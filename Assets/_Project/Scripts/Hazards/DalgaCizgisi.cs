using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumdaki dalga çizgisinin görüntüsü: her parça kendi bölgesinin çizgisinde durur; kumul onarımı olan bölgede denize kayar.</summary>
    public class DalgaCizgisi : MonoBehaviour
    {
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private KumsalDurumu kumsal;

        private void OnEnable()
        {
            kumsal.Degisti += Diz;
            Diz();
        }

        private void OnDisable() => kumsal.Degisti -= Diz;

        private void Diz()
        {
            foreach (Transform parca in transform)
            {
                Vector3 yerel = kum.transform.InverseTransformPoint(parca.position);
                yerel.z = kum.DalgaCizgisi(kum.Bolge(parca.position));
                parca.position = kum.transform.TransformPoint(yerel);
            }
        }
    }
}
