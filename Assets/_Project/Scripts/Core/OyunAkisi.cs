using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IkiDunya.Core
{
    /// <summary>Oyunun adım adım akışı, bölüm geçişi (kararma, bölüm başlığı) ve kayıt. Her oyun sahnesinde bir tane durur; ilki sahneler arasında yaşar.</summary>
    public class OyunAkisi : MonoBehaviour
    {
        [SerializeField] private OyunAyarlari ayarlar;
        [SerializeField] private KumsalDurumu kumsal;

        /// <summary>Yeni adım başladığında; sahne yüklendiyse sahnedeki objeler OnEnable'da abone olmuş olur.</summary>
        public static event Action<Adim> AdimBasladi;
        /// <summary>Menüye dönüldüğünde.</summary>
        public static event Action MenuyeDonuldu;

        public static Adim SuAnkiAdim { get; private set; }
        public static bool MenudeMi => ornek == null || ornek.sira < 0;
        /// <summary>Sahnede oyun akışı var mı. Deneme sahnelerinde yoktur; gece kendi kendine başlar. Awake'te kurulur, Start'larda güvenle okunur.</summary>
        public static bool Var => ornek != null;
        /// <summary>Devam edilebilecek bir kayıt var mı.</summary>
        public static bool KayitVar => Kayit.Var;

        private static OyunAkisi ornek;

        private List<Adim> akis;
        private int sira = -1;
        private bool yukleniyor;
        private Gecis gecis;
        private bool performansGoster;
        private float ortalamaKare = 1f / 60f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla()
        {
            ornek = null;
            AdimBasladi = null;
            MenuyeDonuldu = null;
            SuAnkiAdim = default;
        }

        private void Awake()
        {
            if (ornek != null)
            {
                Destroy(gameObject);
                return;
            }
            if (ayarlar == null || kumsal == null)
            {
                Debug.LogError("OyunAkisi: Ayarlar ve Kumsal alanlarına ScriptableObjects/Settings'teki OyunAyarlari ve KumsalDurumu asset'lerini bağla.", this);
                enabled = false;
                return;
            }
            ornek = this;
            DontDestroyOnLoad(gameObject);
            akis = ayarlar.AkisiKur();
            gecis = Gecis.Kur(transform);
        }

        private void Start()
        {
            if (ornek != this) return;
            SahnedenBasla();
        }

        private void OnDestroy()
        {
            if (ornek == this) ornek = null;
        }

        private void Update()
        {
            if (ornek != this || !Debug.isDebugBuild) return;
            if (GameInput.GelistiriciPerformansBasildi) performansGoster = !performansGoster;
            if (performansGoster) ortalamaKare = Mathf.Lerp(ortalamaKare, Time.unscaledDeltaTime, 0.05f);
            if (yukleniyor) return;

            if (GameInput.GelistiriciBolum1Basildi) IlkAdimaGec(AdimTuru.Donus);
            else if (GameInput.GelistiriciKumsalBasildi) IlkAdimaGec(AdimTuru.YuvaGecesi);
            else if (GameInput.GelistiriciBolum5Basildi) IlkAdimaGec(AdimTuru.IlkYuzus);
            else if (GameInput.GelistiriciMenuBasildi) MenuyeDon();
        }

        /// <summary>Geliştirici sürümünde F5: sol üstte kare hızı (FPS) ve kare süresi; performans kontrolü için.</summary>
        private void OnGUI()
        {
            if (!performansGoster) return;
            GUI.Label(new Rect(10f, 10f, 260f, 24f), $"{1f / ortalamaKare:0} FPS · {ortalamaKare * 1000f:0.0} ms");
        }

        // ---- Diğer sistemlerin çağırdığı metotlar ----

        /// <summary>Menüden yeni oyun: eski kayıt silinir, kumsal sıfırlanır, Bölüm 1 başlar.</summary>
        public static void YeniOyun()
        {
            if (ornek == null || ornek.yukleniyor) return;
            Kayit.Sil();
            ornek.KumsaliSifirla();
            ornek.AdimaGec(0);
        }

        /// <summary>Menüden devam: kayıtlı kumsal ve adımdan sürer; kayıt yoksa yeni oyun.</summary>
        public static void DevamEt()
        {
            if (ornek == null || ornek.yukleniyor) return;
            if (Kayit.Yukle(ornek.kumsal, out int kayitliSira) && kayitliSira >= 0 && kayitliSira < ornek.akis.Count)
                ornek.AdimaGec(kayitliSira);
            else
                YeniOyun();
        }

        /// <summary>Şu anki adım bitti (gece bitti, tur bitti); sıradakine geçilir. Son adımdan sonra kayıt silinir, menüye dönülür.
        /// Geçiş sürerken kabul edilmez ve false döner; çağıran kendini bitmiş saymadan sonra tekrar dener.</summary>
        public static bool AdimBitti()
        {
            if (ornek == null || ornek.yukleniyor) return false;
            if (ornek.sira + 1 < ornek.akis.Count) ornek.AdimaGec(ornek.sira + 1);
            else
            {
                Kayit.Sil();
                MenuyeDon();
            }
            return true;
        }

        public static void MenuyeDon()
        {
            if (ornek == null || ornek.yukleniyor) return;
            ornek.StartCoroutine(ornek.Gec(-1));
        }

        // ---- İç işler ----

        private void SahnedenBasla()
        {
            string sahne = SceneManager.GetActiveScene().name;
            if (sahne == ayarlar.MenuSahnesi) return;

            // Bir oyun sahnesinde Play'e basıldı: o sahnenin ilk adımından başla.
            int bulunan = akis.FindIndex(a => ayarlar.SahneAdi(a.Tur) == sahne);
            if (bulunan < 0)
            {
                Debug.LogWarning($"OyunAkisi: '{sahne}' sahnesi akışta yok.");
                return;
            }
            KumsaliSifirla();
            sira = bulunan;
            SuAnkiAdim = akis[sira];
            AdimBasladi?.Invoke(SuAnkiAdim);
        }

        private void IlkAdimaGec(AdimTuru tur)
        {
            int bulunan = akis.FindIndex(a => a.Tur == tur);
            if (bulunan < 0) return;
            if (sira < 0) KumsaliSifirla();
            AdimaGec(bulunan);
        }

        private void AdimaGec(int yeniSira) => StartCoroutine(Gec(yeniSira));

        /// <summary>Ekran kararır, adım (ve gerekirse sahne) değişir, kayıt yazılır, yeni bölümse başlığı görünür, ekran açılır. -1 menü demektir.</summary>
        private IEnumerator Gec(int yeniSira)
        {
            yukleniyor = true;
            Duraklatma.Kapat();
            int oncekiBolum = sira >= 0 ? akis[sira].Bolum : 0;
            yield return gecis.Solustur(1f, ayarlar.GecisSuresi);

            sira = yeniSira;
            string sahne = sira >= 0 ? ayarlar.SahneAdi(akis[sira].Tur) : ayarlar.MenuSahnesi;
            if (sira >= 0)
            {
                SuAnkiAdim = akis[sira];
                Kayit.Kaydet(sira, kumsal);
            }

            // Aynı sahnede kalınıyorsa (ör. kumsalda gece → insan turu) yükleme yok.
            if (SceneManager.GetActiveScene().name != sahne)
            {
                AsyncOperation yukleme = SceneManager.LoadSceneAsync(sahne);
                while (!yukleme.isDone) yield return null;
            }

            if (sira < 0) MenuyeDonuldu?.Invoke();
            else
            {
                AdimBasladi?.Invoke(SuAnkiAdim);
                if (SuAnkiAdim.Bolum != oncekiBolum && ayarlar.Metinler != null)
                    yield return gecis.BaslikGoster(ayarlar.Metinler.Al($"bolum{SuAnkiAdim.Bolum}_giris"), ayarlar.BaslikSuresi);
            }

            yield return gecis.Solustur(0f, ayarlar.GecisSuresi);
            yukleniyor = false;
        }

        private void KumsaliSifirla()
        {
            ayarlar.KumsaliSifirla(kumsal);
        }
    }
}
