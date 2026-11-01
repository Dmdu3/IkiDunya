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
        [Tooltip("Kumsal dışında kayda giren veriler (ör. Management/YonetimDurumu); IOyunVerisi olmalı.")]
        [SerializeField] private ScriptableObject[] ekVeriler = new ScriptableObject[0];

        /// <summary>Yeni adım başladığında; sahne yüklendiyse sahnedeki objeler OnEnable'da abone olmuş olur.</summary>
        public static event Action<Adim> AdimBasladi;
        /// <summary>Menüye dönüldüğünde.</summary>
        public static event Action MenuyeDonuldu;

        public static Adim SuAnkiAdim { get; private set; }
        public static bool MenudeMi => ornek == null || (ornek.sira < 0 && !ornek.parkur);
        /// <summary>Menüden bir parkur oynanıyor (hikâyenin dışında; kayıt yazılmaz, bitince menüye dönülür).</summary>
        public static bool ParkurModunda => ornek != null && ornek.parkur;
        /// <summary>Sahnede oyun akışı var mı. Deneme sahnelerinde yoktur; gece kendi kendine başlar. Awake'te kurulur, Start'larda güvenle okunur.</summary>
        public static bool Var => ornek != null;
        /// <summary>Devam edilebilecek bir kayıt var mı.</summary>
        public static bool KayitVar => Kayit.Var;
        /// <summary>Ekran kararıp açılırken (sahne yükleniyor, bölüm başlığı) true; ipuçları bu sırada beklemeye alınır.</summary>
        public static bool GecisSuruyor => ornek != null && ornek.yukleniyor;

        private static OyunAkisi ornek;

        private List<Adim> akis;
        private readonly List<IOyunVerisi> veriler = new List<IOyunVerisi>();
        private int sira = -1;
        private bool parkur;
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
                // Sahnenin kendi Sistemler kopyası: hemen kapanır (EventSystem ve kulak iki tane olmasın), sonra silinir.
                gameObject.SetActive(false);
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
            foreach (ScriptableObject ek in ekVeriler)
            {
                if (ek is IOyunVerisi veri) veriler.Add(veri);
                else if (ek != null) Debug.LogError($"OyunAkisi: '{ek.name}' IOyunVerisi değil, kayda girmez.", this);
            }
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
            Ipucu.Sifirla();
            ornek.KumsaliSifirla();
            ornek.AdimaGec(0);
        }

        /// <summary>Menüden devam: kayıtlı kumsal ve adımdan sürer; kayıt yoksa yeni oyun.</summary>
        public static void DevamEt()
        {
            if (ornek == null || ornek.yukleniyor) return;
            if (Kayit.Yukle(ornek.kumsal, ornek.veriler, out int kayitliSira) && kayitliSira >= 0 && kayitliSira < ornek.akis.Count)
                ornek.AdimaGec(kayitliSira);
            else
                YeniOyun();
        }

        /// <summary>Şu anki adım bitti (gece bitti, tur bitti); sıradakine geçilir. Son adımdan sonra kayıt silinir, menüye dönülür.
        /// Geçiş sürerken kabul edilmez ve false döner; çağıran kendini bitmiş saymadan sonra tekrar dener.</summary>
        public static bool AdimBitti()
        {
            if (ornek == null || ornek.yukleniyor) return false;
            if (ornek.parkur)
            {
                MenuyeDon();
                return true;
            }
            if (ornek.sira + 1 < ornek.akis.Count) ornek.AdimaGec(ornek.sira + 1);
            else
            {
                Kayit.Sil();
                MenuyeDon();
            }
            return true;
        }

        /// <summary>Menüden parkur: kumsal ilk haline gelir (kayıt değişmez), parkurun sahnesi yüklenir, parkur başından başlar.</summary>
        public static void ParkurOyna(ParkurTuru tur)
        {
            if (ornek == null || ornek.yukleniyor) return;
            ornek.KumsaliSifirla();
            ornek.StartCoroutine(ornek.ParkuraGec(tur));
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
            int bulunan = akis.FindIndex(a => ayarlar.SahneAdi(a) == sahne);
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

            // Yarıda bırakılan parkurun süresi menüde ve sonraki adımda işlemesin.
            if (Parkur.Aktif.HasValue) Parkur.Birak(Parkur.Aktif.Value);
            parkur = false;
            sira = yeniSira;
            string sahne = sira >= 0 ? ayarlar.SahneAdi(akis[sira]) : ayarlar.MenuSahnesi;
            if (sira >= 0)
            {
                SuAnkiAdim = akis[sira];
                Kayit.Kaydet(sira, kumsal, veriler);
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

        private IEnumerator ParkuraGec(ParkurTuru tur)
        {
            yukleniyor = true;
            Duraklatma.Kapat();
            yield return gecis.Solustur(1f, ayarlar.GecisSuresi);

            parkur = true;
            sira = -1;
            int bolum = tur == ParkurTuru.PosetSurusu ? 1 : tur == ParkurTuru.CopSeli ? 4 : 5;
            SuAnkiAdim = new Adim(AdimTuru.Parkur, bolum, (int)tur);
            AsyncOperation yukleme = SceneManager.LoadSceneAsync(ayarlar.ParkurSahnesi(tur));
            while (!yukleme.isDone) yield return null;
            AdimBasladi?.Invoke(SuAnkiAdim);

            yield return gecis.Solustur(0f, ayarlar.GecisSuresi);
            yukleniyor = false;
        }

        private void KumsaliSifirla()
        {
            ayarlar.KumsaliSifirla(kumsal);
            foreach (IOyunVerisi veri in veriler) veri.YeniOyun();
        }
    }
}
