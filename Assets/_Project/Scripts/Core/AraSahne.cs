using System.Collections;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Bölümün başında kısa ara sahne: kamera geniş bir açıdan oyuncunun gözüne doğru süzülür, bu sırada oyuncu kıpırdamaz.
    /// Ekran açılırken (geçiş bitene kadar) başta bekler. Belirtilen adım türlerinde (ve istenirse sadece belli bir sırada) oynar; sahnede birden çok olabilir.</summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class AraSahne : MonoBehaviour
    {
        [SerializeField] private AdimTuru[] turler = new AdimTuru[0];
        [Tooltip("Sadece bu sıradaki adımda (−1 hepsinde); ör. Bölüm 5'in devam parçasında oynamasın.")]
        [SerializeField] private int sira = -1;
        [Tooltip("Kameranın oyuncunun gözüne varma süresi (sn). Başladığı yer ve yön bu objenin kendisidir.")]
        [SerializeField] private float sure = 3.5f;
        [Tooltip("Geçiş bittikten sonra hareket başlamadan bekleme (sn).")]
        [SerializeField] private float bekleme = 0.4f;

        private UnityEngine.Camera kamera;
        private Vector3 ilkYer;
        private Quaternion ilkDonus;
        private Coroutine oynuyor;

        private void Awake()
        {
            kamera = GetComponent<UnityEngine.Camera>();
            kamera.enabled = false;
            ilkYer = transform.position;
            ilkDonus = transform.rotation;
        }

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (oynuyor != null) Bitir(null);
        }

        private void AdimBasladi(Adim adim)
        {
            if (System.Array.IndexOf(turler, adim.Tur) < 0 || (sira >= 0 && adim.Sira != sira)) return;
            if (oynuyor != null) StopCoroutine(oynuyor);
            oynuyor = StartCoroutine(Oyna());
        }

        private IEnumerator Oyna()
        {
            // Adımın nesneleri bu karede açılıyor; oyuncunun gözü bir kare sonra belli olur.
            yield return null;
            UnityEngine.Camera hedef = UnityEngine.Camera.main;
            if (hedef == null || hedef == kamera)
            {
                oynuyor = null;
                yield break;
            }

            GameInput.Kilitli = true;
            transform.SetPositionAndRotation(ilkYer, ilkDonus);
            kamera.fieldOfView = hedef.fieldOfView;
            kamera.enabled = true;
            hedef.enabled = false;

            while (OyunAkisi.GecisSuruyor) yield return null;
            for (float t = 0f; t < bekleme; t += Time.deltaTime) yield return null;

            for (float t = 0f; t < 1f; t += Time.deltaTime / sure)
            {
                float y = Mathf.SmoothStep(0f, 1f, t);
                transform.SetPositionAndRotation(Vector3.Lerp(ilkYer, hedef.transform.position, y), Quaternion.Slerp(ilkDonus, hedef.transform.rotation, y));
                yield return null;
            }
            Bitir(hedef);
        }

        private void Bitir(UnityEngine.Camera hedef)
        {
            if (hedef != null) hedef.enabled = true;
            kamera.enabled = false;
            GameInput.Kilitli = false;
            oynuyor = null;
        }
    }
}
