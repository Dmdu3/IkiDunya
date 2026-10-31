using System;
using UnityEngine;

namespace IkiDunya.Management
{
    /// <summary>Kumsalın paydaşları (GDD: Paydaşlar). Asset'lerde sayı olarak saklanır: yeni paydaş hep sona eklenir.</summary>
    public enum Paydas { Otel, Belediye, Balikci }

    /// <summary>Bir kartın ya da olay seçeneğinin paydaş ilişkilerine etkisi.</summary>
    [Serializable]
    public struct PaydasFarki
    {
        [SerializeField] private int otel;
        [SerializeField] private int belediye;
        [SerializeField] private int balikci;

        public PaydasFarki(int otel, int belediye, int balikci)
        {
            this.otel = otel;
            this.belediye = belediye;
            this.balikci = balikci;
        }

        public int Al(Paydas paydas) => paydas == Paydas.Otel ? otel : paydas == Paydas.Belediye ? belediye : balikci;
    }
}
