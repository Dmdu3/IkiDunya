using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IkiDunya.Core
{
    /// <summary>Oyunun bütün tuşları tek yerde; başka script tuşu doğrudan okumaz.</summary>
    public static class GameInput
    {
        private const string TuslarAnahtari = "tuslar";

        private static InputActionMap harita;

        /// <summary>Ayarlarda bir tuş yeniden atanırken true; o sırada Esc duraklatmayı açıp kapatmaz.</summary>
        public static bool TusAtaniyor { get; set; }

        /// <summary>Tuş atamaları değişince (kaydedildi ya da varsayılana dönüldü).</summary>
        public static event Action BaglamalarDegisti;

        /// <summary>Ara sahne oynarken true: oyuncu kıpırdamaz.</summary>
        public static bool Kilitli { get; set; }

        /// <summary>Son kullanılan cihaz oyun kolu mu; ekrandaki tuş yazıları buna göre.</summary>
        public static bool KolKullaniliyor { get; private set; }
        /// <summary>Oyuncu klavye/fareden oyun koluna ya da tersine geçince.</summary>
        public static event Action CihazDegisti;

        /// <summary>Oyun duraklatılmamışsa (ve duraklatma bu karede kapanmadıysa), ara sahne oynamıyorsa oyun girdileri okunur.</summary>
        private static bool Oyunda => !Kilitli && !Duraklatma.Acik && Time.frameCount != Duraklatma.KapanmaKaresi;

        private static InputAction hareket;
        private static InputAction bakis;
        private static InputAction bakisCubugu;
        private static InputAction hizli;
        private static InputAction ziplaDal;
        private static InputAction etkiles;
        private static InputAction kazSol;
        private static InputAction kazSag;
        private static InputAction kazYukari;
        private static InputAction termal;
        private static InputAction duraklat;
        private static InputAction isaretci;
        private static InputAction tikla;
        private static InputAction ileriKol;
        private static InputAction vazgec;
        private static InputAction gelistiriciBolum1;
        private static InputAction gelistiriciKumsal;
        private static InputAction gelistiriciBolum5;
        private static InputAction gelistiriciMenu;
        private static InputAction gelistiriciPerformans;
        private static InputAction gelistiriciPanel;

        /// <summary>Otomatik sürüş (fragman çekimi): atanınca kaplumbağa girdileri oyuncudan değil bundan okunur; null iken gerçek girdi.</summary>
        public static OtomatikGirdi Otomatik { get; set; }

        // Kaplumbağa bölümleri (birinci şahıs)
        public static Vector2 Hareket => Otomatik != null ? Otomatik.Hareket : Oyunda ? hareket.ReadValue<Vector2>() : Vector2.zero;          // WASD
        public static Vector2 Bakis => Otomatik != null ? Otomatik.Bakis : Oyunda ? bakis.ReadValue<Vector2>() : Vector2.zero;              // fare hareketi
        public static Vector2 BakisCubugu => Otomatik != null ? Vector2.zero : Oyunda ? bakisCubugu.ReadValue<Vector2>() : Vector2.zero;  // oyun kolu sağ çubuk (−1..1)
        public static bool HizliBasili => Otomatik != null ? Otomatik.Hizli : Oyunda && hizli.IsPressed();                    // Shift: anne hızlı, yavru sürüyü dağıtır
        public static bool ZiplaDalBasildi => Otomatik != null ? Otomatik.ZiplaDalBasildi : Oyunda && ziplaDal.WasPressedThisFrame();   // Space
        public static bool ZiplaDalBasili => Otomatik != null ? Otomatik.ZiplaDalBasili : Oyunda && ziplaDal.IsPressed();              // Space basılı (denizde yüzeye çık)
        public static bool EtkilesBasili => Otomatik != null ? Otomatik.EtkilesBasili : Oyunda && etkiles.IsPressed();                // E basılı tut
        public static bool EtkilesBasildi => Otomatik != null ? Otomatik.EtkilesBasildi : Oyunda && etkiles.WasPressedThisFrame();     // E bas (ağdan kurtul, ye)
        public static bool KazSolBasildi => Otomatik != null ? Otomatik.KazSolBasildi : Oyunda && kazSol.WasPressedThisFrame();       // A (kazma sırası)
        public static bool KazSagBasildi => Otomatik != null ? Otomatik.KazSagBasildi : Oyunda && kazSag.WasPressedThisFrame();       // D (kazma sırası)
        public static bool KazYukariBasildi => Otomatik != null ? Otomatik.KazYukariBasildi : Oyunda && kazYukari.WasPressedThisFrame(); // W (kum altında)
        public static bool TermalBasildi => Oyunda ? termal.WasPressedThisFrame() : false;       // T

        // Genel
        public static bool DuraklatBasildi => !TusAtaniyor && duraklat.WasPressedThisFrame();   // Esc

        // İnsan kısmı (yönetim ekranı)
        public static Vector2 IsaretciKonumu => isaretci.ReadValue<Vector2>();
        public static bool TiklaBasildi => Oyunda ? tikla.WasPressedThisFrame() : false;
        public static bool TiklaBirakildi => Oyunda ? tikla.WasReleasedThisFrame() : false;
        public static bool TiklaBasili => Oyunda ? tikla.IsPressed() : false;
        /// <summary>Fare bu karede belirgin kıpırdadı (haritada çubuktan imlece dönmek için).</summary>
        public static bool FareKipirdadi => Oyunda && bakis.ReadValue<Vector2>().sqrMagnitude > 4f;
        /// <summary>Aşamayı geçir: klavyede Space (ZiplaDal'ın klavye tuşu), oyun kolunda Y. Kolun A'sı arayüzde seçer, ilerletmez.</summary>
        public static bool IleriBasildi => Oyunda && (ileriKol.WasPressedThisFrame()
            || (ziplaDal.WasPressedThisFrame() && !(ziplaDal.activeControl?.device is Gamepad)));
        /// <summary>Hedef seçiminden vazgeç: oyun kolunda B, farede sağ tık.</summary>
        public static bool VazgecBasildi => Oyunda ? vazgec.WasPressedThisFrame() : false;
        /// <summary>Vazgeç, duraklatma menüsünde de (ayarlarda tuş atamadan çıkmak için).</summary>
        public static bool VazgecMenudeBasildi => vazgec.WasPressedThisFrame();

        // Geliştirici (F1–F6)
        public static bool GelistiriciBolum1Basildi => gelistiriciBolum1.WasPressedThisFrame();
        public static bool GelistiriciKumsalBasildi => gelistiriciKumsal.WasPressedThisFrame();
        public static bool GelistiriciBolum5Basildi => gelistiriciBolum5.WasPressedThisFrame();
        public static bool GelistiriciMenuBasildi => gelistiriciMenu.WasPressedThisFrame();
        public static bool GelistiriciPerformansBasildi => gelistiriciPerformans.WasPressedThisFrame();
        public static bool GelistiriciPanelBasildi => gelistiriciPanel.WasPressedThisFrame();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Kur()
        {
            Kapat();
            Kilitli = false;
            Otomatik = null;
            KolKullaniliyor = false;
            CihazDegisti = null;
            harita = new InputActionMap("Oyun");

            hareket = harita.AddAction("Hareket", InputActionType.Value);
            hareket.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            // Oyun kolu: klavye bağlamalarından sonra eklenir (ayarlardaki tuş atama klavyenin sırasını kullanır).
            hareket.AddBinding("<Gamepad>/leftStick");

            bakis = harita.AddAction("Bakis", InputActionType.Value, "<Mouse>/delta");
            bakisCubugu = harita.AddAction("BakisCubugu", InputActionType.Value, "<Gamepad>/rightStick");
            hizli = harita.AddAction("Hizli", InputActionType.Button, "<Keyboard>/shift");
            hizli.AddBinding("<Gamepad>/leftTrigger");
            ziplaDal = harita.AddAction("ZiplaDal", InputActionType.Button, "<Keyboard>/space");
            ziplaDal.AddBinding("<Gamepad>/buttonSouth");
            etkiles = harita.AddAction("Etkiles", InputActionType.Button, "<Keyboard>/e");
            etkiles.AddBinding("<Gamepad>/buttonWest");
            // Kazma: omuz tuşları sırayla (A/D gibi); kum altında yukarı: sol çubuğu ya da yön tuşunu yukarı.
            kazSol = harita.AddAction("KazSol", InputActionType.Button, "<Keyboard>/a");
            kazSol.AddBinding("<Gamepad>/leftShoulder");
            kazSag = harita.AddAction("KazSag", InputActionType.Button, "<Keyboard>/d");
            kazSag.AddBinding("<Gamepad>/rightShoulder");
            kazYukari = harita.AddAction("KazYukari", InputActionType.Button, "<Keyboard>/w");
            kazYukari.AddBinding("<Gamepad>/dpad/up");
            kazYukari.AddBinding("<Gamepad>/buttonNorth");
            termal = harita.AddAction("Termal", InputActionType.Button, "<Keyboard>/t");
            termal.AddBinding("<Gamepad>/rightStickPress");
            duraklat = harita.AddAction("Duraklat", InputActionType.Button, "<Keyboard>/escape");
            duraklat.AddBinding("<Gamepad>/start");

            isaretci = harita.AddAction("Isaretci", InputActionType.Value, "<Pointer>/position");
            tikla = harita.AddAction("Tikla", InputActionType.Button, "<Pointer>/press");
            ileriKol = harita.AddAction("IleriKol", InputActionType.Button, "<Gamepad>/buttonNorth");
            vazgec = harita.AddAction("Vazgec", InputActionType.Button, "<Gamepad>/buttonEast");
            vazgec.AddBinding("<Mouse>/rightButton");

            gelistiriciBolum1 = harita.AddAction("GelistiriciBolum1", InputActionType.Button, "<Keyboard>/f1");
            gelistiriciKumsal = harita.AddAction("GelistiriciKumsal", InputActionType.Button, "<Keyboard>/f2");
            gelistiriciBolum5 = harita.AddAction("GelistiriciBolum5", InputActionType.Button, "<Keyboard>/f3");
            gelistiriciMenu = harita.AddAction("GelistiriciMenu", InputActionType.Button, "<Keyboard>/f4");
            gelistiriciPerformans = harita.AddAction("GelistiriciPerformans", InputActionType.Button, "<Keyboard>/f5");
            gelistiriciPanel = harita.AddAction("GelistiriciPanel", InputActionType.Button, "<Keyboard>/f6");

            // Oyuncunun ayarlardan atadığı tuşlar
            string kayitli = PlayerPrefs.GetString(TuslarAnahtari, "");
            if (!string.IsNullOrEmpty(kayitli)) harita.LoadBindingOverridesFromJson(kayitli);

            harita.actionTriggered += CihazaBak;
            harita.Enable();
            Application.quitting += Kapat;
        }

        private static void CihazaBak(InputAction.CallbackContext baglam)
        {
            bool kol = baglam.control?.device is Gamepad;
            if (kol == KolKullaniliyor) return;
            KolKullaniliyor = kol;
            CihazDegisti?.Invoke();
        }

        // ---- Tuş atama (ayarlar) ----

        public static InputAction Eylem(string ad) => harita.FindAction(ad);

        /// <summary>Eylemin son kullanılan cihazdaki tuşu (ekranda göstermek için): oyun kolu ya da klavye/fare bağlaması.</summary>
        public static string TusYazisi(string ad)
        {
            InputAction e = Eylem(ad);
            if (e == null) return "";
            for (int i = 0; i < e.bindings.Count; i++)
            {
                InputBinding b = e.bindings[i];
                if (b.isPartOfComposite) continue;
                bool kol = (b.effectivePath ?? "").StartsWith("<Gamepad>");
                if (kol == KolKullaniliyor) return e.GetBindingDisplayString(i);
            }
            return e.GetBindingDisplayString(0);
        }

        /// <summary>İleri'nin ekrandaki tuşu: kolda Y, klavyede ZiplaDal'ın tuşu.</summary>
        public static string IleriTusu => KolKullaniliyor ? TusYazisi("IleriKol") : TusYazisi("ZiplaDal");

        public static void BaglamalariKaydet()
        {
            PlayerPrefs.SetString(TuslarAnahtari, harita.SaveBindingOverridesAsJson());
            BaglamalarDegisti?.Invoke();
        }

        public static void VarsayilanaDon()
        {
            harita.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(TuslarAnahtari);
            BaglamalarDegisti?.Invoke();
        }

        private static void Kapat()
        {
            Application.quitting -= Kapat;
            if (harita == null) return;
            harita.actionTriggered -= CihazaBak;
            harita.Disable();
            harita.Dispose();
            harita = null;
        }
    }

    /// <summary>Oyuncunun yerine verilen kaplumbağa girdileri (<see cref="GameInput.Otomatik"/>). "Basıldı" değerlerini atayan her kare geri kapatır.</summary>
    public class OtomatikGirdi
    {
        public Vector2 Hareket { get; set; }
        public Vector2 Bakis { get; set; }
        public bool Hizli { get; set; }
        public bool ZiplaDalBasildi { get; set; }
        public bool ZiplaDalBasili { get; set; }
        public bool EtkilesBasili { get; set; }
        public bool EtkilesBasildi { get; set; }
        public bool KazSolBasildi { get; set; }
        public bool KazSagBasildi { get; set; }
        public bool KazYukariBasildi { get; set; }
    }
}
