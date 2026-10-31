using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Sahnede hep tek bir kulak (Audio Listener) açık olsun: kumsal sahnesinde her bölümün kendi kamerası var (anne, sürü, yönetim, final).
    /// Açılan kamera kulağı alır, öncekinin kulağı kapanır.</summary>
    [RequireComponent(typeof(AudioListener))]
    public class TekKulak : MonoBehaviour
    {
        private static TekKulak aktif;
        private AudioListener kulak;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => aktif = null;

        private void Awake() => kulak = GetComponent<AudioListener>();

        private void OnEnable()
        {
            if (aktif != null && aktif != this) aktif.kulak.enabled = false;
            kulak.enabled = true;
            aktif = this;
        }

        private void OnDisable()
        {
            if (aktif == this) aktif = null;
        }
    }
}
