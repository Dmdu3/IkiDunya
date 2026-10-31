using System;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Canlıların hareketine göre görünüşün nasıl salındığı (iskeletsiz modeller için kodla animasyon).</summary>
    /// <remarks>Sahnelerde sayı olarak saklanır: yeni tür hep sona eklenir.</remarks>
    public enum SalinimTuru { Kara, Deniz, Kus, Balik }

    /// <summary>Salınım ayarları: her tür için tam hızda adım sıklığı, zıplama, yana ve öne yatma; kazma sarsıntısı. His ayarları inspector'da.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Animasyon Ayarları", fileName = "AnimasyonAyarlari")]
    public class AnimasyonAyarlari : ScriptableObject
    {
        /// <summary>Bir türün salınımı.</summary>
        [Serializable]
        public class Profil
        {
            [SerializeField] private SalinimTuru tur;
            [Tooltip("Bu hızda (m/sn) salınım tam büyüklükte.")]
            [SerializeField] private float tamHiz = 1f;
            [Tooltip("Tam hızda saniyedeki adım ya da kulaç.")]
            [SerializeField] private float siklik = 2f;
            [Tooltip("Dururken de kalan salınım oranı (0–1): nefes, süzülme.")]
            [Range(0f, 1f)] [SerializeField] private float durgun;
            [Tooltip("Yukarı-aşağı (metre), yana yatma ve öne yatma (derece), sağa-sola kıvrılma (derece).")]
            [SerializeField] private float ziplama = 0.03f;
            [SerializeField] private float yanaYatma = 6f;
            [SerializeField] private float oneYatma = 3f;
            [SerializeField] private float kivrilma;

            public Profil(SalinimTuru tur, float tamHiz, float siklik, float durgun, float ziplama, float yanaYatma, float oneYatma, float kivrilma)
            {
                this.tur = tur;
                this.tamHiz = tamHiz;
                this.siklik = siklik;
                this.durgun = durgun;
                this.ziplama = ziplama;
                this.yanaYatma = yanaYatma;
                this.oneYatma = oneYatma;
                this.kivrilma = kivrilma;
            }

            public SalinimTuru Tur => tur;
            public float TamHiz => tamHiz;
            public float Siklik => siklik;
            public float Durgun => durgun;
            public float Ziplama => ziplama;
            public float YanaYatma => yanaYatma;
            public float OneYatma => oneYatma;
            public float Kivrilma => kivrilma;
        }

        [SerializeField] private Profil[] profiller =
        {
            new Profil(SalinimTuru.Kara, 1.2f, 2.2f, 0f, 0.025f, 7f, 3f, 0f),
            new Profil(SalinimTuru.Deniz, 2.5f, 0.9f, 0.35f, 0.06f, 5f, 4f, 0f),
            new Profil(SalinimTuru.Kus, 1f, 0.35f, 1f, 0.4f, 12f, 4f, 0f),
            new Profil(SalinimTuru.Balik, 2f, 1.6f, 0.4f, 0.02f, 0f, 0f, 12f),
        };

        [Header("Kazma (anne)")]
        [Tooltip("Kazarken geriye yatma (derece) ve her vuruşta sarsıntı (derece), sarsıntının sönme hızı.")]
        [SerializeField] private float kazmaEgimi = 8f;
        [SerializeField] private float kazmaSarsintisi = 6f;
        [SerializeField] private float sarsintiSonmesi = 10f;

        public float KazmaEgimi => kazmaEgimi;
        public float KazmaSarsintisi => kazmaSarsintisi;
        public float SarsintiSonmesi => sarsintiSonmesi;

        public Profil ProfilAl(SalinimTuru tur)
        {
            foreach (Profil p in profiller)
                if (p.Tur == tur) return p;
            return profiller[0];
        }
    }
}
