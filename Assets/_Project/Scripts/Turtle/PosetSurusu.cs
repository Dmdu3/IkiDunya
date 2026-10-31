using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bölüm 1 minigame'i (poşet sürüsü): akıntı alanın bir yanından denizanası ve poşet karışık bir sürü taşır. Anne alana girince parkur başlar;
    /// hedef kadar denizanası yiyince biter. Art arda denizanası kombo yapar (enerji), poşet komboyu bozar ve yavaşlatır.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PosetSurusu : MonoBehaviour
    {
        [SerializeField] private ParkurAyarlari ayarlar;
        [SerializeField] private DenizHareketi yuzucu;
        [SerializeField] private Yiyecek denizanasiKalibi;
        [SerializeField] private Yiyecek posetKalibi;

        private readonly List<Yiyecek> surudekiler = new List<Yiyecek>();
        private BoxCollider alan;
        private float dogmaKalan;
        private int yenen;
        private int kombo;
        private bool bitti;

        private bool Suruyor => Parkur.SuruyorMu(ParkurTuru.PosetSurusu);

        private void Awake()
        {
            alan = GetComponent<BoxCollider>();
            alan.isTrigger = true;
        }

        private void OnEnable()
        {
            Yiyecek.Yendi += Yendi;
            yuzucu.Enerji.Bitti += Kaldi;
            yuzucu.Yakalandi += Kaldi;
            OyunAkisi.AdimBasladi += AdimBasladi;
        }

        private void OnDisable()
        {
            Yiyecek.Yendi -= Yendi;
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (yuzucu == null) return;
            yuzucu.Enerji.Bitti -= Kaldi;
            yuzucu.Yakalandi -= Kaldi;
        }

        private void AdimBasladi(Adim adim)
        {
            bitti = false;
            Temizle();
        }

        private void OnTriggerEnter(Collider diger)
        {
            if (bitti || Suruyor || !diger.TryGetComponent(out DenizHareketi _)) return;
            yenen = 0;
            kombo = 0;
            dogmaKalan = 0f;
            Parkur.Baslat(ParkurTuru.PosetSurusu);
            Parkur.Bilgi("parkur_denizanasi", yenen, ayarlar.PosetHedef);
            Ipucu.Goster("ipucu_poset_surusu");
        }

        private void OnTriggerExit(Collider diger)
        {
            // Hedefe ulaşmadan alandan çıkan (kıyıya kaçan) parkuru bırakır.
            if (Suruyor && diger.TryGetComponent(out DenizHareketi _)) Kaldi();
        }

        private void Update()
        {
            // Sürü hep akar (dekor da olsun); parkur sürerken daha çok doğar.
            Bounds b = alan.bounds;
            for (int i = surudekiler.Count - 1; i >= 0; i--)
            {
                Yiyecek y = surudekiler[i];
                if (y == null || !y.gameObject.activeSelf || y.transform.position.x > b.max.x)
                {
                    if (y != null) Destroy(y.gameObject);
                    surudekiler.RemoveAt(i);
                    continue;
                }
                y.transform.position += Vector3.right * ayarlar.PosetAkisHizi * Time.deltaTime;
            }

            if (!Suruyor) return;
            dogmaKalan -= Time.deltaTime;
            if (dogmaKalan > 0f || surudekiler.Count >= ayarlar.PosetEnFazla) return;
            dogmaKalan = ayarlar.PosetDogmaAraligi;

            Yiyecek kalip = Random.value < ayarlar.PosetOrani ? posetKalibi : denizanasiKalibi;
            var yer = new Vector3(b.min.x, Random.Range(b.min.y, b.max.y), Random.Range(b.min.z, b.max.z));
            Yiyecek yeni = Instantiate(kalip, yer, Quaternion.identity, transform);
            yeni.YonAyarla(Vector3.right);
            surudekiler.Add(yeni);
        }

        private void Yendi(Yiyecek.Turu tur)
        {
            if (!Suruyor) return;
            if (tur == Yiyecek.Turu.Poset)
            {
                kombo = 0;
                return;
            }

            yenen++;
            kombo++;
            if (kombo % ayarlar.KomboEsigi == 0)
            {
                yuzucu.Enerji.Ekle(ayarlar.KomboEnerjisi);
                Ses.Cal(SesTuru.Kombo);
            }
            Parkur.Bilgi("parkur_denizanasi", yenen, ayarlar.PosetHedef);
            if (yenen < ayarlar.PosetHedef) return;
            bitti = true;
            Parkur.Bitir(ParkurTuru.PosetSurusu);
        }

        /// <summary>Enerji bitti, avlandı ya da alandan çıktı: parkur yarıda kalır, yeniden girince baştan başlar.</summary>
        private void Kaldi()
        {
            if (!Suruyor) return;
            Parkur.Birak(ParkurTuru.PosetSurusu);
            Temizle();
        }

        private void Temizle()
        {
            foreach (Yiyecek y in surudekiler)
                if (y != null) Destroy(y.gameObject);
            surudekiler.Clear();
        }
    }
}
