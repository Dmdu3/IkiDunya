using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kumsalı KumsalDurumu'ndan kurar: bölge bölge çöp, şezlong, ışık, insan, araç izi, gölgelik; kayıp izleri. Gece ortasında yapılar yeniden kurulmaz.</summary>
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
        [Tooltip("Kafesli yuvanın etrafına konan kafes. Boşsa kafes görünmez.")]
        [SerializeField] private GameObject yuvaKafesiKalibi;
        [SerializeField] private GameObject isikKalibi;
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

        private readonly List<GameObject> yapilar = new List<GameObject>();
        private readonly List<GameObject> izler = new List<GameObject>();

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
            if (!OyunAkisi.Var || (!OyunAkisi.MenudeMi && OyunAkisi.SuAnkiAdim.Tur == AdimTuru.InsanTuru)) YapilariKur();
        }

        private void YapilariKur()
        {
            Temizle(yapilar);
            for (int i = 0; i < kumsal.BolgeSayisi && i < kum.BolgeSayisi; i++)
            {
                Bolge bolge = kumsal.BolgeAl(i);
                // Bölge başına sabit tohum: aynı durum her kurulumda aynı yerleşimi verir.
                var rastgele = new System.Random(i * 7919 + 17);

                if (bolge.Var(BolgeYapisi.Golgelik)) Koy(golgelikKalibi, kum.BolgeNoktasi(i, golgelikUzakligi), 0f);
                if (bolge.Var(BolgeYapisi.AracIzi)) Koy(tekerlekIziKalibi, kum.BolgeNoktasi(i, tekerlekIziUzakligi), 0f);
                if (bolge.Var(BolgeYapisi.TilkiCiti)) CitiKur(i);

                for (int c = 0; c < bolge.Seviye(BolgeKatmani.Cop); c++)
                {
                    Koy(copYiginiKalibi, Nokta(rastgele, i, copAraligi), Aci(rastgele));
                    Koy(agKalibi, Nokta(rastgele, i, copAraligi), Aci(rastgele));
                    Koy(siseKalibi, Nokta(rastgele, i, copAraligi), 0f);
                }

                for (int s = 0; s < bolge.Seviye(BolgeKatmani.Sezlong); s++)
                    Koy(sezlongKalibi, Nokta(rastgele, i, sezlongAraligi), Aci(rastgele));

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
        }

        private void IzleriKur()
        {
            Temizle(izler);
            foreach (KayipIzi iz in kumsal.KayipIzleri)
                izler.Add(Instantiate(kayipIziKalibi, iz.Konum, Quaternion.identity, transform));
        }

        private void Koy(GameObject kalip, Vector3 yer, float aci)
        {
            if (kalip == null) return;
            yapilar.Add(Instantiate(kalip, yer, kum.transform.rotation * Quaternion.Euler(0f, aci, 0f), transform));
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
