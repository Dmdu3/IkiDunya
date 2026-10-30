using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Çocuklarını sadece belirtilen adım türlerinde açık tutar (ör. kumsal sahnesinde yuva gecesi nesneleri). Akış yoksa dokunmaz.</summary>
    public class AdimNesneleri : MonoBehaviour
    {
        [SerializeField] private AdimTuru[] turler;

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;
        private void OnDisable() => OyunAkisi.AdimBasladi -= AdimBasladi;

        private void AdimBasladi(Adim adim)
        {
            bool acik = Array.IndexOf(turler, adim.Tur) >= 0;
            foreach (Transform cocuk in transform) cocuk.gameObject.SetActive(acik);
        }
    }
}
