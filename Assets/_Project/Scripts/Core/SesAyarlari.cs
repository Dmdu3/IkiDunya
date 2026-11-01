using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyundaki bir ses olayı. Kod sesi bu türle ister; klibi SesAyarlari'na Abdü bağlar.</summary>
    /// <remarks>Asset'te sayı olarak saklanır: yeni tür hep sona eklenir.</remarks>
    public enum SesTuru
    {
        Kazma, KumKazma, YuvaTamam, Takilma, Kurtarma, YavruKurtuldu, YavruKaybedildi,
        YengecUyari, MartiUyari, TilkiUyari, TilkiEseliyor, Yeme, AgdanKurtulma, KontrolNoktasi, Pervane, DalgaVurdu, Ipucu,
        Kombo, ParkurBitti, ParkurAsama, SelGeliyor, PosetYuttu, KalpAtisi
    }

    /// <summary>Ses yuvaları: her ses olayının klipleri ve düzeyi; her adımın ortam sesi ve müziği. Klip boşsa o ses çalmaz, oyun bozulmaz.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Ses Ayarları", fileName = "SesAyarlari")]
    public class SesAyarlari : ScriptableObject
    {
        /// <summary>Bir ses olayının klipleri (birden çoksa rastgele biri çalar) ve düzeyi.</summary>
        [Serializable]
        public class Efekt
        {
            [SerializeField] private SesTuru tur;
            [SerializeField] private AudioClip[] klipler = new AudioClip[0];
            [Range(0f, 1f)] [SerializeField] private float duzey = 1f;

            public Efekt(SesTuru tur) => this.tur = tur;

            public SesTuru Tur => tur;
            public float Duzey => duzey;
            public AudioClip Klip => klipler != null && klipler.Length > 0 ? klipler[UnityEngine.Random.Range(0, klipler.Length)] : null;
        }

        /// <summary>Bir adımın (kumsal gecesi, deniz, insan turu, final) ortam sesi ve müziği.</summary>
        [Serializable]
        public class AdimSesi
        {
            [SerializeField] private AdimTuru adim;
            [SerializeField] private AudioClip ortam;
            [SerializeField] private AudioClip muzik;

            public AdimSesi(AdimTuru adim) => this.adim = adim;

            public AdimTuru Adim => adim;
            public AudioClip Ortam => ortam;
            public AudioClip Muzik => muzik;
        }

        [SerializeField] private List<Efekt> efektler = new List<Efekt>();
        [SerializeField] private List<AdimSesi> adimSesleri = new List<AdimSesi>();
        [SerializeField] private AudioClip menuMuzigi;

        [Header("Düzeyler")]
        [Range(0f, 1f)] [SerializeField] private float ortamDuzeyi = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float muzikDuzeyi = 0.4f;
        [Tooltip("Adım değişince ortam sesi ve müziğin geçiş süresi (sn).")]
        [SerializeField] private float gecisSuresi = 1.5f;
        [Tooltip("Uzaydaki seslerin (yengeç, martı, tekne) duyulduğu en uzak mesafe (metre).")]
        [SerializeField] private float duyulmaMesafesi = 30f;

        public AudioClip MenuMuzigi => menuMuzigi;
        public float OrtamDuzeyi => ortamDuzeyi;
        public float MuzikDuzeyi => muzikDuzeyi;
        public float GecisSuresi => gecisSuresi;
        public float DuyulmaMesafesi => duyulmaMesafesi;

        public Efekt EfektAl(SesTuru tur) => efektler.Find(e => e.Tur == tur);
        public AdimSesi AdimSesiAl(AdimTuru adim) => adimSesleri.Find(a => a.Adim == adim);

        /// <summary>Her ses türü ve her adım için boş yuva açar (editör kurulumu çağırır); dolu yuvalara dokunmaz.</summary>
        public void EksikYuvalariEkle()
        {
            foreach (SesTuru tur in Enum.GetValues(typeof(SesTuru)))
                if (EfektAl(tur) == null) efektler.Add(new Efekt(tur));
            foreach (AdimTuru adim in Enum.GetValues(typeof(AdimTuru)))
                if (AdimSesiAl(adim) == null) adimSesleri.Add(new AdimSesi(adim));
        }
    }
}
