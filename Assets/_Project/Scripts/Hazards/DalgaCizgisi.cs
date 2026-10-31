using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumdaki dalga çizgisinin görüntüsü: her parça kendi bölgesinin çizgisinde, zeminin üstünde durur; kumul onarımı olan bölgede denize kayar.</summary>
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
                // Kıyı kıvrımlıysa çizgi de onu izler; parça zemine oturur.
                Vector3 yerel = kum.transform.InverseTransformPoint(parca.position);
                yerel.y = 0f;
                yerel.z = kum.KiyiZ(yerel.x) + kum.DalgaCizgisi(kum.Bolge(parca.position));
                parca.position = Zemin.Oturt(kum.transform.TransformPoint(yerel)) + kum.transform.up * 0.02f;
            }
        }
    }
}
