using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Çöp girdabı parkuru (Bölüm 5 ortasında, kendi sahnesi): yavru girdabın merkezinde başlar. İç içe dönen çöp halkalarının her birinde bir boşluk var;
    /// halkadan sadece boşluğundan geçilir. Girdap yavruyu döndürür ve hafifçe merkeze çeker; halkalardaki ağır çöpler (varil, kasa) çarparsa savurur ve enerji alır.
    /// Son halkadan çıkınca biter. Enerji biterse merkezden baştan (süre işler).</summary>
    public class CopGirdabi : MonoBehaviour
    {
        [SerializeField] private ParkurAyarlari ayarlar;
        [SerializeField] private DenizHareketi yuzucu;
        [Tooltip("İçten dışa halkalar; her halkanın boşluğu kendi ileri yönünde. Ağır çöpler adı 'AgirCop' ile başlayan çocuklar.")]
        [SerializeField] private Transform[] halkalar = new Transform[0];
        [Tooltip("Yavrunun başladığı yer (girdabın merkezi).")]
        [SerializeField] private Transform merkez;

        private readonly List<Transform> agirCopler = new List<Transform>();
        private bool aktif;
        private int halka;
        private float carpmaKalan;

        private void Awake()
        {
            foreach (Transform h in halkalar)
                foreach (Transform cocuk in h)
                    if (cocuk.name.StartsWith("AgirCop")) agirCopler.Add(cocuk);
        }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            yuzucu.Enerji.Bitti += Bastan;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (yuzucu != null) yuzucu.Enerji.Bitti -= Bastan;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Baslat();
        }

        private void AdimBasladi(Adim adim)
        {
            if (adim.Tur == AdimTuru.Parkur && adim.Sira == (int)ParkurTuru.CopGirdabi) Baslat();
        }

        private void Baslat()
        {
            Bastan();
            aktif = true;
            Parkur.Baslat(ParkurTuru.CopGirdabi);
            Ipucu.Goster("ipucu_girdap");
        }

        private void Bastan()
        {
            halka = 0;
            yuzucu.Baslat(merkez.position, merkez.rotation);
            Bilgi();
        }

        private void Update()
        {
            for (int i = 0; i < halkalar.Length && i < ayarlar.HalkaSayisi; i++)
                halkalar[i].Rotate(0f, ayarlar.HalkaHizi(i) * Time.deltaTime, 0f, Space.World);
            if (!aktif) return;

            Vector3 fark = yuzucu.Konum - merkez.position;
            fark.y = 0f;
            float uzaklik = fark.magnitude;
            Vector3 disari = uzaklik > 0.01f ? fark / uzaklik : Vector3.forward;

            // Girdap: halkanın döndüğü yönde sürükler, hafifçe merkeze çeker.
            float yon = Mathf.Sign(ayarlar.HalkaHizi(Mathf.Min(halka, ayarlar.HalkaSayisi - 1)));
            Vector3 akim = Vector3.Cross(Vector3.up, disari) * yon * ayarlar.GirdapDonme - disari * ayarlar.GirdapCekim;
            yuzucu.Savrul(akim * Time.deltaTime);

            Carpma();
            if (halka >= ayarlar.HalkaSayisi) return;

            // Şu anki halka: boşluğu dışında duvar gibi, geçilemez.
            float yaricap = ayarlar.HalkaYaricapi(halka);
            bool boslukta = Mathf.Abs(Vector3.SignedAngle(halkalar[halka].forward, disari, Vector3.up)) < ayarlar.CikisAcisi / 2f;
            if (!boslukta && uzaklik > yaricap - 0.4f) yuzucu.Savrul(-disari * (uzaklik - (yaricap - 0.4f)));
            else if (uzaklik > yaricap + 0.4f)
            {
                halka++;
                Ses.Cal(SesTuru.ParkurAsama);
                Bilgi();
                if (halka < ayarlar.HalkaSayisi) return;
                aktif = false;
                yuzucu.Kilitli = true;
                Parkur.Bitir(ParkurTuru.CopGirdabi);
            }
        }

        /// <summary>Halkalardaki ağır çöplerden biri yavruya çarparsa savurur ve enerji alır (kısa bir aralıkla).</summary>
        private void Carpma()
        {
            if (carpmaKalan > 0f)
            {
                carpmaKalan -= Time.deltaTime;
                return;
            }
            foreach (Transform cop in agirCopler)
            {
                Vector3 fark = yuzucu.Konum - cop.position;
                if (fark.magnitude > ayarlar.CarpmaMesafesi) continue;
                yuzucu.EnerjiKaybet(ayarlar.CarpmaHasari);
                Geribildirim.Darbe(Siddet.Orta);
                yuzucu.Savrul(fark.normalized * ayarlar.CarpmaSavrulma);
                Ses.Cal(SesTuru.DalgaVurdu);
                carpmaKalan = ayarlar.CarpmaBekleme;
                return;
            }
        }

        private void Bilgi() => Parkur.Bilgi("parkur_girdap_halka", Mathf.Min(halka, ayarlar.HalkaSayisi), ayarlar.HalkaSayisi);
    }
}
