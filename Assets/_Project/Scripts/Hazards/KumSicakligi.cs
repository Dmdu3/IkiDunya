using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalın herhangi bir noktasındaki sıcaklık bandını verir: denizden uzaklık, bölgedeki gölgelik ve genel ısınma.</summary>
    public class KumSicakligi : MonoBehaviour
    {
        [SerializeField] private TermalAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;

        [Header("Kumsalın yerleşimi (bu objenin konumu deniz kenarının ortası; +z karaya doğru)")]
        [SerializeField] private float uzunluk = 80f;
        [SerializeField] private float derinlik = 40f;
        [SerializeField] private int bolgeSayisi = 8;

        public TermalAyarlari Ayarlar => ayarlar;
        public float Uzunluk => uzunluk;
        public float Derinlik => derinlik;
        public int BolgeSayisi => bolgeSayisi;
        public float BolgeGenisligi => uzunluk / bolgeSayisi;

        /// <summary>Dünya konumunun kumsaldaki bölgesi (kumsal dışındaysa en yakın bölge).</summary>
        public int Bolge(Vector3 konum)
        {
            float x = transform.InverseTransformPoint(konum).x + uzunluk / 2f;
            return Mathf.Clamp(Mathf.FloorToInt(x / BolgeGenisligi), 0, bolgeSayisi - 1);
        }

        /// <summary>Deniz kenarından karaya doğru uzaklık (metre).</summary>
        public float KiyidanUzaklik(Vector3 konum) => transform.InverseTransformPoint(konum).z;

        public SicaklikBandi Bant(Vector3 konum) => Bant(Bolge(konum), KiyidanUzaklik(konum));

        public SicaklikBandi Bant(int bolge, float kiyidanUzaklik)
        {
            int bant = 0;
            foreach (float sinir in ayarlar.BantSinirlari)
                if (kiyidanUzaklik >= sinir) bant++;

            if (bolge < kumsal.BolgeSayisi && kumsal.BolgeAl(bolge).Var(BolgeYapisi.Golgelik))
                bant -= ayarlar.GolgelikSerinletme;
            bant += kumsal.KumIsinmasi;

            return (SicaklikBandi)Mathf.Clamp(bant, (int)SicaklikBandi.Mavi, (int)SicaklikBandi.Kirmizi);
        }
    }
}
