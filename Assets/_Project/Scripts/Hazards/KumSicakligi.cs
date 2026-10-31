using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalın yerleşimi ve bir noktanın durumu: bölgesi, sıcaklık bandı (denizden uzaklık, gölgelik, genel ısınma), dalga çizgisinin altında mı.</summary>
    public class KumSicakligi : MonoBehaviour
    {
        [SerializeField] private TermalAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;

        [Header("Kumsalın yerleşimi (bu objenin konumu deniz kenarının ortası; +z karaya doğru)")]
        [SerializeField] private float uzunluk = 80f;
        [SerializeField] private float derinlik = 40f;
        [SerializeField] private int bolgeSayisi = 8;
        [Tooltip("Dalga çizgisinin deniz kenarından uzaklığı (metre).")]
        [SerializeField] private float dalgaCizgisi = 6f;

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

        /// <summary>Nokta dalga çizgisinin altında mı (fırtına basabilir). Kumul onarımı olan bölgede çizgi denize çekilir.</summary>
        public bool DalgaAltinda(Vector3 konum) => KiyidanUzaklik(konum) < DalgaCizgisi(Bolge(konum));

        /// <summary>Bir bölgede dalga çizgisinin deniz kenarından uzaklığı; kumul onarımı varsa denize çekilmiş hali.</summary>
        public float DalgaCizgisi(int bolge)
        {
            float cizgi = dalgaCizgisi;
            if (bolge < kumsal.BolgeSayisi && kumsal.BolgeAl(bolge).Var(BolgeYapisi.KumulOnarimi))
                cizgi -= ayarlar.KumulOnarimiGeriCekilme;
            return cizgi;
        }

        /// <summary>Kumsal üstündeki bir noktanın dünya konumu (bölge ortası, kıyıdan uzaklık).</summary>
        public Vector3 BolgeNoktasi(int bolge, float kiyidanUzaklik) => BolgeNoktasi(bolge, 0.5f, kiyidanUzaklik);

        /// <summary>Bölgenin içinde soldan sağa oran (0–1) ve kıyıdan uzaklıkla bir nokta.</summary>
        public Vector3 BolgeNoktasi(int bolge, float bolgeIcinde, float kiyidanUzaklik)
        {
            float x = -uzunluk / 2f + BolgeGenisligi * (bolge + bolgeIcinde);
            return transform.TransformPoint(new Vector3(x, 0f, kiyidanUzaklik));
        }

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
