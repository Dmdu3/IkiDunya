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
        private static InputAction gelistiriciBolum1;
        private static InputAction gelistiriciKumsal;
        private static InputAction gelistiriciBolum5;
        private static InputAction gelistiriciMenu;
        private static InputAction gelistiriciPerformans;
        private static InputAction gelistiriciPanel;

        // Kaplumbağa bölümleri (birinci şahıs)
        public static Vector2 Hareket => Oyunda ? hareket.ReadValue<Vector2>() : Vector2.zero;          // WASD
        public static Vector2 Bakis => Oyunda ? bakis.ReadValue<Vector2>() : Vector2.zero;              // fare hareketi
        public static Vector2 BakisCubugu => Oyunda ? bakisCubugu.ReadValue<Vector2>() : Vector2.zero;  // oyun kolu sağ çubuk (−1..1)
        public static bool HizliBasili => Oyunda ? hizli.IsPressed() : false;                    // Shift: anne hızlı, yavru sürüyü dağıtır
        public static bool ZiplaDalBasildi => Oyunda ? ziplaDal.WasPressedThisFrame() : false;   // Space
        public static bool ZiplaDalBasili => Oyunda ? ziplaDal.IsPressed() : false;              // Space basılı (denizde yüzeye çık)
        public static bool EtkilesBasili => Oyunda ? etkiles.IsPressed() : false;                // E basılı tut
        public static bool EtkilesBasildi => Oyunda ? etkiles.WasPressedThisFrame() : false;     // E bas (ağdan kurtul, ye)
        public static bool KazSolBasildi => Oyunda ? kazSol.WasPressedThisFrame() : false;       // A (kazma sırası)
        public static bool KazSagBasildi => Oyunda ? kazSag.WasPressedThisFrame() : false;       // D (kazma sırası)
        public static bool KazYukariBasildi => Oyunda ? kazYukari.WasPressedThisFrame() : false; // W (kum altında)
        public static bool TermalBasildi => Oyunda ? termal.WasPressedThisFrame() : false;       // T

        // Genel
        public static bool DuraklatBasildi => !TusAtaniyor && duraklat.WasPressedThisFrame();   // Esc

        // İnsan kısmı (yönetim ekranı)
        public static Vector2 IsaretciKonumu => isaretci.ReadValue<Vector2>();
        public static bool TiklaBasildi => Oyunda ? tikla.WasPressedThisFrame() : false;
        public static bool TiklaBirakildi => Oyunda ? tikla.WasReleasedThisFrame() : false;
        public static bool TiklaBasili => Oyunda ? tikla.IsPressed() : false;

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

            gelistiriciBolum1 = harita.AddAction("GelistiriciBolum1", InputActionType.Button, "<Keyboard>/f1");
            gelistiriciKumsal = harita.AddAction("GelistiriciKumsal", InputActionType.Button, "<Keyboard>/f2");
            gelistiriciBolum5 = harita.AddAction("GelistiriciBolum5", InputActionType.Button, "<Keyboard>/f3");
            gelistiriciMenu = harita.AddAction("GelistiriciMenu", InputActionType.Button, "<Keyboard>/f4");
            gelistiriciPerformans = harita.AddAction("GelistiriciPerformans", InputActionType.Button, "<Keyboard>/f5");
            gelistiriciPanel = harita.AddAction("GelistiriciPanel", InputActionType.Button, "<Keyboard>/f6");

            // Oyuncunun ayarlardan atadığı tuşlar
            string kayitli = PlayerPrefs.GetString(TuslarAnahtari, "");
            if (!string.IsNullOrEmpty(kayitli)) harita.LoadBindingOverridesFromJson(kayitli);

            harita.Enable();
            Application.quitting += Kapat;
        }

        // ---- Tuş atama (ayarlar) ----

        public static InputAction Eylem(string ad) => harita.FindAction(ad);

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
            harita.Disable();
            harita.Dispose();
            harita = null;
        }
    }
}
