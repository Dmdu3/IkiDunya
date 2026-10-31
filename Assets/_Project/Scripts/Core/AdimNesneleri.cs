using System;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Çocuklarını sadece belirtilen adım türlerinde açık tutar (ör. kumsal sahnesinde yuva gecesi nesneleri). Akış yoksa dokunmaz.</summary>
    public class AdimNesneleri : MonoBehaviour
    {
        [SerializeField] private AdimTuru[] turler;
        [Tooltip("Menüden oynanırken de açık olacağı parkurlar (ör. kaçış gecesi grubu çöp selinde).")]
        [SerializeField] private ParkurTuru[] parkurlar = new ParkurTuru[0];

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;
        private void OnDisable() => OyunAkisi.AdimBasladi -= AdimBasladi;

        private void AdimBasladi(Adim adim)
        {
            bool acik = Array.IndexOf(turler, adim.Tur) >= 0
                || (adim.Tur == AdimTuru.Parkur && Array.IndexOf(parkurlar, (ParkurTuru)adim.Sira) >= 0);
            foreach (Transform cocuk in transform) cocuk.gameObject.SetActive(acik);
        }
    }
}
