using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IkiDunya.Core
{
    /// <summary>Oyunun adım adım akışı ve bölüm geçişi. Her oyun sahnesinde bir tane durur; ilki sahneler arasında yaşar.</summary>
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

        private static OyunAkisi ornek;

        private List<Adim> akis;
        private int sira = -1;
        private bool yukleniyor;

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
            if (ornek != this || !Debug.isDebugBuild || yukleniyor) return;

            if (GameInput.GelistiriciBolum1Basildi) IlkAdimaGec(AdimTuru.Donus);
            else if (GameInput.GelistiriciKumsalBasildi) IlkAdimaGec(AdimTuru.YuvaGecesi);
            else if (GameInput.GelistiriciBolum5Basildi) IlkAdimaGec(AdimTuru.IlkYuzus);
            else if (GameInput.GelistiriciMenuBasildi) MenuyeDon();
        }

        // ---- Diğer sistemlerin çağırdığı metotlar ----

        /// <summary>Menüden yeni oyun: kumsal sıfırlanır, Bölüm 1 başlar.</summary>
        public static void YeniOyun()
        {
            if (ornek == null) return;
            ornek.KumsaliSifirla();
            ornek.AdimaGec(0);
        }

        /// <summary>Şu anki adım bitti (gece bitti, tur bitti); sıradakine geçilir.</summary>
        public static void AdimBitti()
        {
            if (ornek == null || ornek.yukleniyor) return;
            if (ornek.sira + 1 < ornek.akis.Count) ornek.AdimaGec(ornek.sira + 1);
            else MenuyeDon();
        }

        public static void MenuyeDon()
        {
            if (ornek == null) return;
            ornek.sira = -1;
            ornek.SahneYukle(ornek.ayarlar.MenuSahnesi, () => MenuyeDonuldu?.Invoke());
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

        private void AdimaGec(int yeniSira)
        {
            string oncekiSahne = sira >= 0 ? ayarlar.SahneAdi(akis[sira].Tur) : ayarlar.MenuSahnesi;
            sira = yeniSira;
            SuAnkiAdim = akis[sira];
            string sahne = ayarlar.SahneAdi(SuAnkiAdim.Tur);

            // Aynı sahnede kalınıyorsa (ör. kumsalda gece → insan turu) yükleme yok.
            if (sahne == oncekiSahne && SceneManager.GetActiveScene().name == sahne)
                AdimBasladi?.Invoke(SuAnkiAdim);
            else
                SahneYukle(sahne, () => AdimBasladi?.Invoke(SuAnkiAdim));
        }

        private void SahneYukle(string sahne, Action yuklenince)
        {
            yukleniyor = true;

            void Yuklendi(Scene yuklenen, LoadSceneMode mod)
            {
                SceneManager.sceneLoaded -= Yuklendi;
                yukleniyor = false;
                yuklenince();
            }

            SceneManager.sceneLoaded += Yuklendi;
            SceneManager.LoadScene(sahne);
        }

        private void KumsaliSifirla()
        {
            ayarlar.KumsaliSifirla(kumsal);
        }
    }
}
