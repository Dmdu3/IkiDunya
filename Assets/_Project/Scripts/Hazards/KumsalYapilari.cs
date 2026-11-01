using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalı KumsalDurumu'ndan kurar: bölge bölge çöp, şezlong, ışık, insan, araç izi, gölgelik, çit, kulübe, sed, tabela; kayıp izleri. Gece ortasında yapılar yeniden kurulmaz.</summary>
    public class KumsalYapilari : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private KumSicakligi kum;

        [Header("Kalıplar")]
        [SerializeField] private GameObject golgelikKalibi;
        [SerializeField] private GameObject kayipIziKalibi;
        [Tooltip("Anneyi takan çöp yığını.")]
        [SerializeField] private GameObject copYiginiKalibi;
        [Tooltip("Yavruyu takan ağ ya da halka.")]
        [SerializeField] private GameObject agKalibi;
        [Tooltip("Yavrunun etrafından dolaştığı şişe (labirent).")]
        [SerializeField] private GameObject siseKalibi;
        [SerializeField] private GameObject sezlongKalibi;
        [SerializeField] private GameObject tekerlekIziKalibi;
        [Tooltip("Tilki çiti parçası (4 m); bölgenin kara tarafına dizilir. Boşsa çit görünmez.")]
        [SerializeField] private GameObject tilkiCitiKalibi;
        [Tooltip("Çitin sol ve sağ ucu; komşu bölgede çit yoksa konur. Boşsa uç konmaz.")]
        [SerializeField] private GameObject citSolUcKalibi;
        [SerializeField] private GameObject citSagUcKalibi;
        [Tooltip("Kafesli yuvanın etrafına konan kafes. Boşsa kafes görünmez.")]
        [SerializeField] private GameObject yuvaKafesiKalibi;
        [SerializeField] private GameObject isikKalibi;
        [Tooltip("Ağ yerine sırayla konan plastik halka (çöp katmanının ikinci, dördüncü... tuzağı). Boşsa hep ağ.")]
        [SerializeField] private GameObject halkaKalibi;
        [Tooltip("Şezlongların yanına sırayla konan şemsiye. Boşsa şemsiye yok.")]
        [SerializeField] private GameObject semsiyeKalibi;
        [Tooltip("Gece devriyesi olan bölgedeki gözcü kulübesi (Gözcü Kulübesi kartı, devriye görevi).")]
        [SerializeField] private GameObject gozcuKulubesiKalibi;
        [Tooltip("Kumul onarımı ya da kum torbası olan bölgenin deniz tarafına dizilen sed parçası (2 m; 2 m arayla dikişsiz).")]
        [SerializeField] private GameObject kumTorbasiSeddiKalibi;
        [Tooltip("Sedin iki ucuna konan tek kum torbası.")]
        [SerializeField] private GameObject kumTorbasiKalibi;
        [Tooltip("İnsanı süreli azaltan etki (Plaj Tabelaları) süren bölgeye konan tabelalar; sırayla kullanılır.")]
        [SerializeField] private GameObject[] tabelaKaliplari = new GameObject[0];
        [SerializeField] private Insan insanKalibi;
        [Tooltip("Farklı görünüşlü insanlar; doluysa her insan bunlardan biri (bölgeye göre sabit).")]
        [SerializeField] private Insan[] insanKaliplari = new Insan[0];
        [Tooltip("İnsanların kurulduğu yer; sadece yuva gecesinde açık olan bir grubun içinde olmalı. Boşsa insan kurulmaz.")]
        [SerializeField] private Transform insanKoku;

        [Header("Yerleşim (deniz kenarından uzaklık, metre)")]
        [SerializeField] private Vector2 copAraligi = new Vector2(8f, 30f);
        [SerializeField] private Vector2 sezlongAraligi = new Vector2(10f, 26f);
        [SerializeField] private Vector2 insanAraligi = new Vector2(14f, 30f);
        [SerializeField] private float isikUzakligi = 34f;
        [SerializeField] private float golgelikUzakligi = 20f;
        [SerializeField] private float tekerlekIziUzakligi = 17f;
        [Tooltip("Tilki çitinin deniz kenarından uzaklığı (m); kumsalın kara tarafı.")]
        [SerializeField] private float citUzakligi = 38f;
        [SerializeField] private float kulubeUzakligi = 31f;
        [Tooltip("Sedin deniz kenarından uzaklığı (m); yuvalarla deniz arasında.")]
        [SerializeField] private float sedUzakligi = 5f;
        [SerializeField] private float tabelaUzakligi = 12f;
        [Tooltip("Şemsiyenin şezlonga göre yeri (şezlongun kendi ekseninde, m).")]
        [SerializeField] private Vector3 semsiyeKaymasi = new Vector3(1.1f, 0f, 0.3f);

        private readonly List<GameObject> yapilar = new List<GameObject>();
        private readonly List<GameObject> izler = new List<GameObject>();
        private string kurulanImza;

        private void OnEnable()
        {
            kumsal.Degisti += Degisti;
            OyunAkisi.AdimBasladi += AdimBasladi;
        }

        private void OnDisable()
        {
            kumsal.Degisti -= Degisti;
            OyunAkisi.AdimBasladi -= AdimBasladi;
        }

        private void Start()
        {
            YapilariKur();
            IzleriKur();
        }

        private void AdimBasladi(Adim adim)
        {
            YapilariKur();
            IzleriKur();
        }

        private void Degisti()
        {
            IzleriKur();
            // Gece ortasında tuzaklar yeniden kurulursa içindeki yavru tekrar takılır; yapılar sadece insan turunda güncellenir.
            if (OyunAkisi.Var && (OyunAkisi.MenudeMi || OyunAkisi.SuAnkiAdim.Tur != AdimTuru.InsanTuru)) return;
            // Kayıp izi gibi bölgeleri değiştirmeyen bir değişiklikte (akışsız deneme sahnesinde gece de buradan geçer) yapılar yerinde kalır.
            if (Imza() != kurulanImza) YapilariKur();
        }

        /// <summary>Bölgelerin katman ve yapı durumu; aynıysa yapılar yeniden kurulmaz.</summary>
        private string Imza()
        {
            var imza = new System.Text.StringBuilder();
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
            {
                Bolge bolge = kumsal.BolgeAl(i);
                foreach (BolgeKatmani katman in System.Enum.GetValues(typeof(BolgeKatmani))) imza.Append(bolge.Seviye(katman)).Append(',');
                foreach (BolgeYapisi yapi in System.Enum.GetValues(typeof(BolgeYapisi))) imza.Append(bolge.Var(yapi) ? '1' : '0');
                imza.Append('|');
            }
            return imza.ToString();
        }

        private void YapilariKur()
        {
            Temizle(yapilar);
            kurulanImza = Imza();
            for (int i = 0; i < kumsal.BolgeSayisi && i < kum.BolgeSayisi; i++)
            {
                Bolge bolge = kumsal.BolgeAl(i);
                // Bölge başına sabit tohum: aynı durum her kurulumda aynı yerleşimi verir.
                var rastgele = new System.Random(i * 7919 + 17);

                if (bolge.Var(BolgeYapisi.Golgelik)) Koy(golgelikKalibi, kum.BolgeNoktasi(i, golgelikUzakligi), 0f);
                if (bolge.Var(BolgeYapisi.AracIzi)) Koy(tekerlekIziKalibi, kum.BolgeNoktasi(i, tekerlekIziUzakligi), 0f);
                if (bolge.Var(BolgeYapisi.TilkiCiti)) CitiKur(i);
                if (bolge.Var(BolgeYapisi.Devriye)) Koy(gozcuKulubesiKalibi, kum.BolgeNoktasi(i, 0.85f, kulubeUzakligi), 180f);
                if (bolge.Var(BolgeYapisi.KumulOnarimi)) SeddiKur(i);
                if (kumsal.InsanSureliAzaldi(i) && tabelaKaliplari.Length > 0)
                    Koy(tabelaKaliplari[i % tabelaKaliplari.Length], kum.BolgeNoktasi(i, 0.3f, tabelaUzakligi), 180f);

                for (int c = 0; c < bolge.Seviye(BolgeKatmani.Cop); c++)
                {
                    Koy(copYiginiKalibi, Nokta(rastgele, i, copAraligi), Aci(rastgele));
                    Koy(c % 2 == 1 && halkaKalibi != null ? halkaKalibi : agKalibi, Nokta(rastgele, i, copAraligi), Aci(rastgele));
                    Koy(siseKalibi, Nokta(rastgele, i, copAraligi), 0f);
                }

                for (int s = 0; s < bolge.Seviye(BolgeKatmani.Sezlong); s++)
                {
                    GameObject sezlong = Koy(sezlongKalibi, Nokta(rastgele, i, sezlongAraligi), Aci(rastgele));
                    if (s % 2 == 0 && sezlong != null)
                        Koy(semsiyeKalibi, sezlong.transform.TransformPoint(semsiyeKaymasi), Aci(rastgele));
                }

                int isikSayisi = bolge.Seviye(BolgeKatmani.Isik);
                for (int k = 0; k < isikSayisi; k++)
                    Koy(isikKalibi, kum.BolgeNoktasi(i, (k + 0.5f) / isikSayisi, isikUzakligi), 0f);

                // Gece devriyesi olan bölgede insan ve fener olmaz.
                if (insanKoku == null || bolge.Var(BolgeYapisi.Devriye)) continue;
                for (int n = 0; n < bolge.Seviye(BolgeKatmani.Insan); n++)
                {
                    float z = Mathf.Lerp(insanAraligi.x, insanAraligi.y, (float)rastgele.NextDouble());
                    Vector3 a = kum.BolgeNoktasi(i, 0.1f, z);
                    Vector3 b = kum.BolgeNoktasi(i, 0.9f, z);
                    Insan kalip = insanKaliplari.Length > 0 ? insanKaliplari[rastgele.Next(insanKaliplari.Length)] : insanKalibi;
                    Insan insan = Instantiate(kalip, a, Quaternion.LookRotation(b - a), insanKoku);
                    insan.Kur(a, b);
                    yapilar.Add(insan.gameObject);
                }
            }

            // Kafesli yuvaların etrafına tel kafes: kartların ve kafes görevinin kumsaldaki hali.
            foreach (Yuva yuva in kumsal.Yuvalar)
                if (yuva.Kafesli) Koy(yuvaKafesiKalibi, yuva.Konum, 0f);
        }

        /// <summary>Bölgenin kara tarafına 4 m'lik çit parçalarını dizer; parçalar bölge genişliğini kaplar.</summary>
        private void CitiKur(int bolge)
        {
            if (tilkiCitiKalibi == null) return;
            int adet = Mathf.Max(1, Mathf.CeilToInt(kum.BolgeGenisligi / 4f));
            for (int p = 0; p < adet; p++)
                Koy(tilkiCitiKalibi, kum.BolgeNoktasi(bolge, (p + 0.5f) / adet, citUzakligi), 0f);
            // Uçlar sadece çitin bittiği yere: komşu bölgede de çit varsa sıra kesintisiz sürer.
            if (bolge == 0 || !kumsal.BolgeAl(bolge - 1).Var(BolgeYapisi.TilkiCiti))
                Koy(citSolUcKalibi, kum.BolgeNoktasi(bolge, 0f, citUzakligi), 0f);
            if (bolge == kumsal.BolgeSayisi - 1 || !kumsal.BolgeAl(bolge + 1).Var(BolgeYapisi.TilkiCiti))
                Koy(citSagUcKalibi, kum.BolgeNoktasi(bolge, 1f, citUzakligi), 0f);
        }

        /// <summary>Bölgenin deniz tarafına kum torbası seddi dizer; iki ucuna tek torba.</summary>
        private void SeddiKur(int bolge)
        {
            if (kumTorbasiSeddiKalibi == null) return;
            int adet = Mathf.Max(1, Mathf.CeilToInt(kum.BolgeGenisligi * 0.8f / 2f));
            for (int p = 0; p < adet; p++)
            {
                float oran = 0.1f + 0.8f * (p + 0.5f) / adet;
                // Kıyı kıvrımlı: parça, o noktadaki kıyı çizgisine döner, yoksa sed basamak basamak görünür.
                Vector3 yon = kum.transform.InverseTransformDirection(
                    kum.BolgeNoktasi(bolge, oran + 0.02f, sedUzakligi) - kum.BolgeNoktasi(bolge, oran - 0.02f, sedUzakligi));
                Koy(kumTorbasiSeddiKalibi, kum.BolgeNoktasi(bolge, oran, sedUzakligi), -Mathf.Atan2(yon.z, yon.x) * Mathf.Rad2Deg);
            }
            Koy(kumTorbasiKalibi, kum.BolgeNoktasi(bolge, 0.08f, sedUzakligi + 0.6f), 25f);
            Koy(kumTorbasiKalibi, kum.BolgeNoktasi(bolge, 0.92f, sedUzakligi + 0.6f), -20f);
        }

        private void IzleriKur()
        {
            Temizle(izler);
            foreach (KayipIzi iz in kumsal.KayipIzleri)
                izler.Add(Instantiate(kayipIziKalibi, iz.Konum, Quaternion.identity, transform));
        }

        private GameObject Koy(GameObject kalip, Vector3 yer, float aci)
        {
            if (kalip == null) return null;
            GameObject nesne = Instantiate(kalip, yer, kum.transform.rotation * Quaternion.Euler(0f, aci, 0f), transform);
            yapilar.Add(nesne);
            return nesne;
        }

        private Vector3 Nokta(System.Random rastgele, int bolge, Vector2 aralik)
        {
            float z = Mathf.Lerp(aralik.x, aralik.y, (float)rastgele.NextDouble());
            return kum.BolgeNoktasi(bolge, 0.1f + 0.8f * (float)rastgele.NextDouble(), z);
        }

        private static float Aci(System.Random rastgele) => (float)rastgele.NextDouble() * 360f;

        private static void Temizle(List<GameObject> liste)
        {
            foreach (GameObject nesne in liste)
                if (nesne != null) Destroy(nesne);
            liste.Clear();
        }
    }
}
