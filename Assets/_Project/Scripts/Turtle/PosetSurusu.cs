using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Poşet sürüsü parkuru (Bölüm 1 sonunda, kendi sahnesi): anne kıyıya doğru bir akıntı koridorundan geçer. Her aşamada akıntı yandan denizanası ve poşet
    /// karışık bir sürü taşır; aşamanın hedefi kadar denizanası yenince sonundaki akıntı duvarı açılır. Enerji sürekli azalır, sadece denizanası doldurur;
    /// art arda denizanası kombo yapar, poşet yavaşlatır ve komboyu bozar. Son aşama karanlık: denizanası parlar, poşet görünmez. Enerji biterse aşamanın başından.</summary>
    public class PosetSurusu : MonoBehaviour
    {
        [SerializeField] private ParkurAyarlari ayarlar;
        [SerializeField] private DenizHareketi yuzucu;
        [SerializeField] private Yiyecek denizanasiKalibi;
        [SerializeField] private Yiyecek posetKalibi;
        [Tooltip("Her aşamanın alanı (sürü burada akar) ve başlangıç noktası, sonundaki akıntı duvarı.")]
        [SerializeField] private BoxCollider[] alanlar = new BoxCollider[0];
        [SerializeField] private Transform[] baslangiclar = new Transform[0];
        [SerializeField] private GameObject[] duvarlar = new GameObject[0];
        [Tooltip("Parkurun bittiği yer (kıyıya doğru +z).")]
        [SerializeField] private Transform bitis;
        [Tooltip("Karanlık aşamada kısılan güneş.")]
        [SerializeField] private Light gunes;

        private readonly List<Yiyecek> surudekiler = new List<Yiyecek>();
        private bool aktif;
        private int asama;
        private int yenen;
        private int kombo;
        private float dogmaKalan;
        private float gunesIsigi;
        private Color ortamIsigi;

        private void Awake()
        {
            if (gunes != null) gunesIsigi = gunes.intensity;
            ortamIsigi = RenderSettings.ambientLight;
        }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            Yiyecek.Yendi += Yendi;
            yuzucu.Enerji.Bitti += EnerjiBitti;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            Yiyecek.Yendi -= Yendi;
            if (yuzucu != null) yuzucu.Enerji.Bitti -= EnerjiBitti;
            RenderSettings.ambientLight = ortamIsigi;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Baslat();
        }

        private void AdimBasladi(Adim adim)
        {
            if (adim.Tur == AdimTuru.Parkur && adim.Sira == (int)ParkurTuru.PosetSurusu) Baslat();
        }

        private void Baslat()
        {
            Temizle();
            asama = 0;
            foreach (GameObject duvar in duvarlar) duvar.SetActive(true);
            AsamaBasi();
            aktif = true;
            Parkur.Baslat(ParkurTuru.PosetSurusu);
            Bilgi();
            Ipucu.Goster("ipucu_poset_surusu");
        }

        private void AsamaBasi()
        {
            yenen = 0;
            kombo = 0;
            dogmaKalan = 0f;
            Transform nokta = baslangiclar[Mathf.Min(asama, baslangiclar.Length - 1)];
            yuzucu.Baslat(nokta.position, nokta.rotation);
        }

        private void Update()
        {
            if (!aktif) return;
            int a = Mathf.Min(asama, ayarlar.PosetAsamaSayisi - 1);
            ParkurAyarlari.PosetAsamasi ayar = ayarlar.PosetAsama(a);

            yuzucu.EnerjiKaybet(ayarlar.PosetEnerjiKaybi * Time.deltaTime);
            Karart(ayar.Karanlik);
            Akit(ayar.AkisHizi);

            // Bütün duvarlar açıldı ve kıyıya varıldı: parkur biter.
            if (asama >= ayarlar.PosetAsamaSayisi)
            {
                if (yuzucu.Konum.z < bitis.position.z) return;
                aktif = false;
                yuzucu.Kilitli = true;
                Parkur.Bitir(ParkurTuru.PosetSurusu);
                return;
            }

            dogmaKalan -= Time.deltaTime;
            if (dogmaKalan > 0f || surudekiler.Count >= ayarlar.PosetEnFazla) return;
            dogmaKalan = ayar.DogmaAraligi;
            Bounds b = alanlar[a].bounds;
            Yiyecek kalip = Random.value < ayar.PosetOrani ? posetKalibi : denizanasiKalibi;
            var yer = new Vector3(b.min.x, Random.Range(b.min.y, b.max.y), Random.Range(b.min.z, b.max.z));
            Yiyecek yeni = Instantiate(kalip, yer, Quaternion.identity, transform);
            surudekiler.Add(yeni);
        }

        /// <summary>Sürü akıntıyla yandan geçer; alandan çıkan ya da yenen silinir.</summary>
        private void Akit(float hiz)
        {
            float sinir = alanlar.Length > 0 ? alanlar[0].bounds.max.x : 20f;
            for (int i = surudekiler.Count - 1; i >= 0; i--)
            {
                Yiyecek y = surudekiler[i];
                if (y == null || !y.gameObject.activeSelf || y.transform.position.x > sinir)
                {
                    if (y != null) Destroy(y.gameObject);
                    surudekiler.RemoveAt(i);
                    continue;
                }
                y.transform.position += Vector3.right * hiz * Time.deltaTime;
            }
        }

        private void Karart(float karanlik)
        {
            if (gunes != null) gunes.intensity = Mathf.MoveTowards(gunes.intensity, gunesIsigi * (1f - karanlik), Time.deltaTime);
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, ortamIsigi * (1f - karanlik * 0.9f), Time.deltaTime);
        }

        private void Yendi(Yiyecek.Turu tur)
        {
            if (!aktif || asama >= ayarlar.PosetAsamaSayisi) return;
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
            if (yenen >= ayarlar.PosetAsama(asama).Hedef)
            {
                // Aşamanın duvarı açılır; sıradaki aşamanın sürüsü başlar.
                if (asama < duvarlar.Length) duvarlar[asama].SetActive(false);
                Ses.Cal(SesTuru.ParkurAsama);
                asama++;
                yenen = 0;
                kombo = 0;
            }
            Bilgi();
        }

        private void EnerjiBitti()
        {
            if (!aktif) return;
            // Aşamanın başından (süre işlemeye devam eder).
            Geribildirim.Darbe(Siddet.Hafif);
            Ses.Cal(SesTuru.YavruKaybedildi);
            Ipucu.Bildir("bildirim_poset_enerji");
            AsamaBasi();
            Bilgi();
        }

        private void Bilgi()
        {
            if (asama >= ayarlar.PosetAsamaSayisi) Parkur.Bilgi("parkur_poset_kiyiya", 0, 0);
            else Parkur.Bilgi($"parkur_poset_asama_{asama + 1}", yenen, ayarlar.PosetAsama(asama).Hedef);
        }

        private void Temizle()
        {
            foreach (Yiyecek y in surudekiler)
                if (y != null) Destroy(y.gameObject);
            surudekiler.Clear();
        }
    }
}
