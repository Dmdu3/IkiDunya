using System;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>A-D sırayla kazma, yumurtlama ve örtme. Doğru sıra çubuğu doldurur, aynı tuşa iki kez basmak kumu geri döktürür.</summary>
    [RequireComponent(typeof(AnneKara))]
    public class AnneKazma : MonoBehaviour
    {
        public enum Asama { Yok, Kaziyor, Yumurtluyor, Ortuyor, Bitti }

        [SerializeField] private AnneAyarlari ayarlar;

        private AnneKara kara;
        private int sonTus;
        private float yumurtlamaKalan;
        private Vector3 yuvaKonumu;

        public Asama SuAnki { get; private set; }
        public int Vurus { get; private set; }
        public int Hedef { get; private set; }

        /// <summary>Çubuğun doluluğu (0–1); yumurtlarken geçen süre.</summary>
        public float Oran => SuAnki == Asama.Yumurtluyor
            ? 1f - yumurtlamaKalan / ayarlar.YumurtlamaSuresi
            : Hedef > 0 ? (float)Vurus / Hedef : 0f;

        /// <summary>Yuva örtülünce; yuvanın konumuyla.</summary>
        public event Action<Vector3> YuvaTamamlandi;

        private void Awake()
        {
            kara = GetComponent<AnneKara>();
        }

        public void Sifirla()
        {
            SuAnki = Asama.Yok;
            Vurus = 0;
            Hedef = 0;
            sonTus = 0;
            kara.Kilitli = false;
        }

        private void Update()
        {
            if (SuAnki == Asama.Bitti || kara.Enerji.BittiMi) return;

            if (SuAnki == Asama.Yumurtluyor)
            {
                yumurtlamaKalan -= Time.deltaTime;
                if (yumurtlamaKalan <= 0f) AsamayaGec(Asama.Ortuyor, ayarlar.OrtmeVurusu);
                return;
            }

            int tus = GameInput.KazSolBasildi ? 1 : GameInput.KazSagBasildi ? 2 : 0;
            if (tus == 0) return;

            if (SuAnki == Asama.Yok)
            {
                // Yerde dururken başlar; yürürken A/D bir şey yapmaz.
                if (!kara.Yerde || Mathf.Abs(GameInput.Hareket.y) > 0.01f) return;
                yuvaKonumu = transform.position;
                kara.Kilitli = true;
                AsamayaGec(Asama.Kaziyor, ayarlar.KazmaVurusu);
            }

            if (tus == sonTus) Vurus = Mathf.Max(0, Vurus - ayarlar.YanlisVurusGeri);
            else
            {
                Vurus++;
                Ses.Cal(SesTuru.Kazma, transform.position);
            }
            sonTus = tus;
            // Aşamayı tamamlayan vuruş enerji harcamaz: son vuruşta enerji biterse biten yuva kaybolmasın.
            if (Vurus < Hedef)
            {
                kara.Enerji.Harca(ayarlar.VurusEnerjisi);
                return;
            }

            if (SuAnki == Asama.Kaziyor)
            {
                SuAnki = Asama.Yumurtluyor;
                yumurtlamaKalan = ayarlar.YumurtlamaSuresi;
            }
            else if (SuAnki == Asama.Ortuyor)
            {
                SuAnki = Asama.Bitti;
                kara.Kilitli = false;
                Ses.Cal(SesTuru.YuvaTamam);
                YuvaTamamlandi?.Invoke(yuvaKonumu);
            }
        }

        private void AsamayaGec(Asama asama, int hedef)
        {
            SuAnki = asama;
            Vurus = 0;
            Hedef = hedef;
            sonTus = 0;
        }
    }
}
