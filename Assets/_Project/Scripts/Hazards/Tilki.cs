using System;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Tilki: bazı gecelerde kumsalın kenarında gezer. Konisinde siperde olmayan bir yavru görürse ona koşar, sürü panikle dağılır; yakalarsa geri döner.
    /// Bir süre gezdikten sonra sonraki gecelerin kafessiz bir yuvasını eşeleyip bozar (yumurta kaybı, Avcı kanıtı).</summary>
    public class Tilki : MonoBehaviour
    {
        private enum Durum { Geziyor, Kosuyor, Donuyor, YuvayaGidiyor, Eseliyor }

        [SerializeField] private AvciAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Sırayla gezilen noktalar (kumsalın kenarı).")]
        [SerializeField] private Transform[] noktalar;
        [SerializeField] private Material koniMalzemesi;
        [Tooltip("Tilkinin görünen gövdesi; gelmediği gece kapanır.")]
        [SerializeField] private GameObject govde;
        [SerializeField] private float gozYuksekligi = 0.4f;

        /// <summary>Tilki bir yavru görüp koşmaya başladığında; tilkinin yeriyle.</summary>
        public static event Action<Vector3> SuruyuKorkuttu;

        private readonly Collider[] bulunan = new Collider[32];
        private int kaplumbagaMaskesi;
        private int engelMaskesi;
        private Durum durum;
        private bool geldi;
        private int hedefNokta;
        private float beklemeKalan;
        private IAvlanabilir av;
        private float gezmeSuresi;
        private bool yuvaDenendi;
        private int hedefYuva = -1;
        private float eselemeKalan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => SuruyuKorkuttu = null;

        private void Awake()
        {
            kaplumbagaMaskesi = LayerMask.GetMask("Turtle");
            engelMaskesi = LayerMask.GetMask("Obstacle", "Structure", "Ground");
            GorusKonisi.Ciz(transform, ayarlar.TilkiMenzil, ayarlar.TilkiAci, koniMalzemesi);
        }

        /// <summary>Her gece (nesne her açıldığında) tilkinin gelip gelmeyeceği yeniden belirlenir.</summary>
        private void OnEnable()
        {
            float ihtimal = ayarlar.TilkiIhtimali * (CitVar() ? ayarlar.TilkiCitiCarpani : 1f);
            geldi = UnityEngine.Random.value < ihtimal;
            govde.SetActive(geldi);
            transform.Find("GorusKonisi")?.gameObject.SetActive(geldi);
            durum = Durum.Geziyor;
            av = null;
            gezmeSuresi = 0f;
            yuvaDenendi = false;
            hedefYuva = -1;
            // Eşelerken gece biterse gövde eğik kalmasın.
            govde.transform.localRotation = Quaternion.identity;
            if (noktalar.Length > 0) transform.position = noktalar[0].position;
            // Sahne yüklenirken her grup bir an açık olur; ipucu sadece tilkinin gerçekten geldiği adımda harcanır.
            if (geldi && GercekGece()) Ipucu.Goster("ipucu_tilki");
        }

        private static bool GercekGece() =>
            !OyunAkisi.Var || OyunAkisi.SuAnkiAdim.Tur == AdimTuru.KacisGecesi || OyunAkisi.SuAnkiAdim.Tur == AdimTuru.Parkur;

        /// <summary>Kovalamaca bitince: yuvaya gidiyorduysa yarım kalan baskına döner, yoksa gezdiği yola.</summary>
        private void IseDon() => durum = hedefYuva >= 0 ? Durum.YuvayaGidiyor : Durum.Donuyor;

        private bool CitVar()
        {
            for (int i = 0; i < kumsal.BolgeSayisi; i++)
                if (kumsal.BolgeAl(i).Var(BolgeYapisi.TilkiCiti)) return true;
            return false;
        }

        private void Update()
        {
            if (!geldi) return;

            switch (durum)
            {
                case Durum.Geziyor:
                    Gez();
                    Bak();
                    YuvayaBak();
                    break;
                case Durum.YuvayaGidiyor:
                    if (Git(kumsal.Yuvalar[hedefYuva].Konum, ayarlar.TilkiYurumeHizi))
                    {
                        durum = Durum.Eseliyor;
                        eselemeKalan = ayarlar.TilkiEselemeSuresi;
                        Ses.Cal(SesTuru.TilkiEseliyor, transform.position);
                    }
                    else Bak();
                    break;
                case Durum.Eseliyor:
                    Esele();
                    break;
                case Durum.Kosuyor:
                    Kos();
                    break;
                case Durum.Donuyor:
                    if (Git(noktalar[hedefNokta].position, ayarlar.TilkiYurumeHizi)) durum = Durum.Geziyor;
                    break;
            }
        }

        private void Gez()
        {
            if (beklemeKalan > 0f)
            {
                beklemeKalan -= Time.deltaTime;
                return;
            }
            if (!Git(noktalar[hedefNokta].position, ayarlar.TilkiYurumeHizi)) return;
            hedefNokta = (hedefNokta + 1) % noktalar.Length;
            beklemeKalan = ayarlar.TilkiBekleme;
        }

        private void Bak()
        {
            int sayi = Physics.OverlapSphereNonAlloc(transform.position, ayarlar.TilkiMenzil, bulunan, kaplumbagaMaskesi, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out IAvlanabilir aday) || !aday.Yakalanabilir || aday.Gizli) continue;
                if (!GorusKonisi.Goruyor(transform, bulunan[i].bounds.center, ayarlar.TilkiMenzil, ayarlar.TilkiAci, gozYuksekligi, engelMaskesi)) continue;
                av = aday;
                durum = Durum.Kosuyor;
                Ses.Cal(SesTuru.TilkiUyari, transform.position);
                SuruyuKorkuttu?.Invoke(transform.position);
                return;
            }
        }

        /// <summary>Bir süre gezdikten sonra, kafessiz ve yumurtası olan sonraki gecelerin yuvasına bir kez gider.</summary>
        private void YuvayaBak()
        {
            if (yuvaDenendi) return;
            gezmeSuresi += Time.deltaTime;
            if (gezmeSuresi < ayarlar.TilkiYuvayaGitme) return;
            yuvaDenendi = true;

            // Bu gecenin yuvası çoktan açıldı; bozulabilecek olanlar sonraki gecelerin yuvaları.
            int gece = OyunAkisi.Var && !OyunAkisi.MenudeMi ? OyunAkisi.SuAnkiAdim.Sira : 1;
            for (int i = gece; i < kumsal.Yuvalar.Count; i++)
            {
                Yuva yuva = kumsal.Yuvalar[i];
                if (yuva.Kafesli || yuva.Yumurta <= 0) continue;
                hedefYuva = i;
                durum = Durum.YuvayaGidiyor;
                return;
            }
        }

        private void Esele()
        {
            // Eşelerken gövde sallanır: oyuncu görürse ne olduğunu anlar.
            eselemeKalan -= Time.deltaTime;
            govde.transform.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 20f) * 10f, 0f, 0f);
            if (eselemeKalan > 0f) return;

            govde.transform.localRotation = Quaternion.identity;
            Yuva yuva = kumsal.Yuvalar[hedefYuva];
            int kayip = Mathf.CeilToInt(yuva.Yumurta * ayarlar.TilkiYumurtaKaybi);
            kumsal.YuvaYumurtaKaybet(hedefYuva, kayip);
            kumsal.KayipIziEkle(new KayipIzi(yuva.Bolge, yuva.Konum, KayipNedeni.Tilki));
            if (Debug.isDebugBuild) Debug.Log($"Tilki: {hedefYuva + 1}. yuvayı bozdu, {kayip} yumurta kaybedildi.");
            hedefYuva = -1;
            durum = Durum.Donuyor;
        }

        private void Kos()
        {
            if (av == null || !av.Yakalanabilir)
            {
                IseDon();
                return;
            }
            if (!Git(av.Konum, ayarlar.TilkiKosuHizi)) return;
            av.Yakala(KayipNedeni.Tilki);
            av = null;
            IseDon();
        }

        /// <summary>Hedefe yürür; vardıysa true.</summary>
        private bool Git(Vector3 hedef, float hiz)
        {
            Vector3 fark = hedef - transform.position;
            fark.y = 0f;
            if (fark.magnitude < 0.3f) return true;
            Vector3 yon = fark.normalized;
            transform.rotation = Quaternion.LookRotation(yon);
            transform.position = Zemin.Oturt(transform.position + yon * Mathf.Min(hiz * Time.deltaTime, fark.magnitude));
            return false;
        }
    }
}
