using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Yeni oyunda kumsalın ilk hali: bölge bölge çöp, ışık, şezlong, insan ve araç izi; denizdeki ağ. Kartlar bu halden başlayarak değiştirir.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Kumsal Başlangıcı", fileName = "KumsalBaslangici")]
    public class KumsalBaslangici : ScriptableObject
    {
        /// <summary>Bir bölgenin ilk hali.</summary>
        [Serializable]
        public class BolgeBaslangici
        {
            [SerializeField] private int cop;
            [SerializeField] private int isik;
            [SerializeField] private int sezlong;
            [SerializeField] private int insan;
            [SerializeField] private bool aracIzi;

            public BolgeBaslangici(int cop, int isik, int sezlong, int insan, bool aracIzi)
            {
                this.cop = cop;
                this.isik = isik;
                this.sezlong = sezlong;
                this.insan = insan;
                this.aracIzi = aracIzi;
            }

            public int Seviye(BolgeKatmani katman)
            {
                switch (katman)
                {
                    case BolgeKatmani.Cop: return cop;
                    case BolgeKatmani.Isik: return isik;
                    case BolgeKatmani.Sezlong: return sezlong;
                    default: return insan;
                }
            }

            public bool AracIzi => aracIzi;
        }

        [Tooltip("Soldan sağa bölgeler. Sayısı kumsalın bölge sayısıdır.")]
        [SerializeField] private BolgeBaslangici[] bolgeler =
        {
            new BolgeBaslangici(1, 0, 1, 0, false),
            new BolgeBaslangici(2, 0, 2, 1, false),
            new BolgeBaslangici(1, 0, 0, 0, false),
            new BolgeBaslangici(0, 0, 0, 0, true),
            new BolgeBaslangici(2, 0, 1, 0, false),
            new BolgeBaslangici(1, 1, 2, 1, false),
            new BolgeBaslangici(3, 1, 3, 0, false),
            new BolgeBaslangici(1, 1, 1, 0, false),
        };
        [Tooltip("Bölüm 5'te denizdeki hayalet ağ ve misina seviyesi.")]
        [SerializeField] private int denizdekiAg = 3;

        public IReadOnlyList<BolgeBaslangici> Bolgeler => bolgeler;
        public int DenizdekiAg => denizdekiAg;
    }
}
