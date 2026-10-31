using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bölüm 4 minigame'i (çöp seli): fırtınadan sonraki kaçış gecesinde kumsaldaki çukurlar sırayla su alıp boşalır, yüzen kasalar kumsalda gidip gelir.
    /// Su basmış çukurdaki yavru karaya itilir, uzun kalırsa sürüklenip kaybedilir; kasa önündeki yavruyu iter. Koşu başlayınca parkur başlar, gece en az bir yavru kurtulursa biter.</summary>
    public class CopSeli : MonoBehaviour
    {
        [SerializeField] private ParkurAyarlari ayarlar;
        [SerializeField] private KacisGecesi gece;
        [SerializeField] private Suru suru;
        [Tooltip("Selin nesneleri; sadece sel gecesinde açık.")]
        [SerializeField] private GameObject icerik;
        [Tooltip("Su alan çukurlar (kutu; ölçeği çukurun alanı).")]
        [SerializeField] private Transform[] cukurlar = new Transform[0];
        [Tooltip("Her çukurun dolunca görünen suyu.")]
        [SerializeField] private GameObject[] sular = new GameObject[0];
        [Tooltip("Yüzen kasalar (kutu); kendi sağ yönlerinde gidip gelir.")]
        [SerializeField] private Transform[] kasalar = new Transform[0];
        [Tooltip("Kasaların gidip geldiği yarı mesafe (metre).")]
        [SerializeField] private float kasaGenisligi = 6f;

        private readonly Dictionary<Yavru, float> sudaKalma = new Dictionary<Yavru, float>();
        private Vector3[] kasaMerkezleri;
        private Vector3[] kasaOnceki;
        private bool aktif;

        private void Awake()
        {
            kasaMerkezleri = new Vector3[kasalar.Length];
            kasaOnceki = new Vector3[kasalar.Length];
            for (int i = 0; i < kasalar.Length; i++) kasaMerkezleri[i] = kasaOnceki[i] = kasalar[i].position;
            icerik.SetActive(false);
        }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            gece.KosuBasladi += KosuBasladi;
            gece.SonucHazir += SonucHazir;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (gece == null) return;
            gece.KosuBasladi -= KosuBasladi;
            gece.SonucHazir -= SonucHazir;
        }

        private void AdimBasladi(Adim adim)
        {
            aktif = (adim.Tur == AdimTuru.KacisGecesi && adim.Sira == ayarlar.SelGecesi)
                    || (adim.Tur == AdimTuru.Parkur && adim.Sira == (int)ParkurTuru.CopSeli);
            icerik.SetActive(aktif);
            sudaKalma.Clear();
        }

        private void KosuBasladi()
        {
            if (!aktif) return;
            Parkur.Baslat(ParkurTuru.CopSeli);
            Ipucu.Goster("ipucu_cop_seli");
        }

        private void SonucHazir(KacisGecesi.Sonuc sonuc)
        {
            if (!Parkur.SuruyorMu(ParkurTuru.CopSeli)) return;
            if (sonuc.Kurtulan > 0) Parkur.Bitir(ParkurTuru.CopSeli);
            else Parkur.Birak(ParkurTuru.CopSeli);
        }

        private void Update()
        {
            if (!aktif) return;

            // Çukurlar sırayla dolup boşalır (her biri farklı evrede): yollar açılıp kapanır.
            for (int i = 0; i < cukurlar.Length; i++)
            {
                float evre = Mathf.Repeat(Time.time / ayarlar.SelPeriyodu + i * 0.37f, 1f);
                if (i < sular.Length) sular[i].SetActive(evre < ayarlar.SelDoluOrani);
            }

            for (int i = 0; i < kasalar.Length; i++)
            {
                kasaOnceki[i] = kasalar[i].position;
                float yol = Mathf.Sin(Time.time * ayarlar.KasaHizi / Mathf.Max(0.1f, kasaGenisligi)) * kasaGenisligi;
                Vector3 hedef = kasaMerkezleri[i] + kasalar[i].right * yol;
                // Kasa kumun üstünde yüzer gibi: tabanı zeminde.
                kasalar[i].position = Zemin.Oturt(hedef) + Vector3.up * (kasalar[i].lossyScale.y / 2f);
            }

            if (!suru.Aktif) return;
            foreach (Yavru yavru in suru.Yavrular)
            {
                if (yavru.SuAnki != Yavru.Durum.Suruyor) continue;
                Vector3 yer = yavru.transform.position;

                bool suda = false;
                for (int i = 0; i < cukurlar.Length && !suda; i++)
                    suda = i < sular.Length && sular[i].activeSelf && Icinde(cukurlar[i], yer);
                if (suda)
                {
                    // Su yavruyu karaya iter; uzun kalan sürüklenir.
                    yavru.Savrul(Karaya() * ayarlar.SelItme * Time.deltaTime);
                    sudaKalma.TryGetValue(yavru, out float sure);
                    sure += Time.deltaTime;
                    sudaKalma[yavru] = sure;
                    if (sure >= ayarlar.SelBogulma) yavru.Kaybet(KayipNedeni.Dalga);
                }
                else sudaKalma.Remove(yavru);

                for (int i = 0; i < kasalar.Length; i++)
                    if (Icinde(kasalar[i], yer)) yavru.Savrul((kasalar[i].position - kasaOnceki[i]) * ayarlar.KasaItme);
            }
        }

        /// <summary>Kumsalda karaya doğru (deniz −z, kara +z).</summary>
        private Vector3 Karaya() => transform.forward;

        /// <summary>Nokta kutunun (ölçeği alanı) içinde mi, yukarıdan bakınca.</summary>
        private static bool Icinde(Transform kutu, Vector3 nokta)
        {
            Vector3 yerel = kutu.InverseTransformPoint(nokta);
            return Mathf.Abs(yerel.x) <= 0.5f && Mathf.Abs(yerel.z) <= 0.5f;
        }
    }
}
