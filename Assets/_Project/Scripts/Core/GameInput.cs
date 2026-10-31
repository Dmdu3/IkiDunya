using UnityEngine;
using UnityEngine.InputSystem;

namespace IkiDunya.Core
{
    /// <summary>Oyunun bütün tuşları tek yerde; başka script tuşu doğrudan okumaz.</summary>
    public static class GameInput
    {
        private static InputActionMap harita;

        /// <summary>Oyun duraklatılmamışsa (ve duraklatma bu karede kapanmadıysa) oyun girdileri okunur.</summary>
        private static bool Oyunda => !Duraklatma.Acik && Time.frameCount != Duraklatma.KapanmaKaresi;

        private static InputAction hareket;
        private static InputAction bakis;
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

        // Kaplumbağa bölümleri (birinci şahıs)
        public static Vector2 Hareket => Oyunda ? hareket.ReadValue<Vector2>() : Vector2.zero;          // WASD
        public static Vector2 Bakis => Oyunda ? bakis.ReadValue<Vector2>() : Vector2.zero;              // fare hareketi
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
        public static bool DuraklatBasildi => duraklat.WasPressedThisFrame();   // Esc

        // İnsan kısmı (yönetim ekranı)
        public static Vector2 IsaretciKonumu => isaretci.ReadValue<Vector2>();
        public static bool TiklaBasildi => Oyunda ? tikla.WasPressedThisFrame() : false;
        public static bool TiklaBirakildi => Oyunda ? tikla.WasReleasedThisFrame() : false;
        public static bool TiklaBasili => Oyunda ? tikla.IsPressed() : false;

        // Geliştirici (F1–F4)
        public static bool GelistiriciBolum1Basildi => gelistiriciBolum1.WasPressedThisFrame();
        public static bool GelistiriciKumsalBasildi => gelistiriciKumsal.WasPressedThisFrame();
        public static bool GelistiriciBolum5Basildi => gelistiriciBolum5.WasPressedThisFrame();
        public static bool GelistiriciMenuBasildi => gelistiriciMenu.WasPressedThisFrame();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Kur()
        {
            Kapat();
            harita = new InputActionMap("Oyun");

            hareket = harita.AddAction("Hareket", InputActionType.Value);
            hareket.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            bakis = harita.AddAction("Bakis", InputActionType.Value, "<Mouse>/delta");
            hizli = harita.AddAction("Hizli", InputActionType.Button, "<Keyboard>/shift");
            ziplaDal = harita.AddAction("ZiplaDal", InputActionType.Button, "<Keyboard>/space");
            etkiles = harita.AddAction("Etkiles", InputActionType.Button, "<Keyboard>/e");
            kazSol = harita.AddAction("KazSol", InputActionType.Button, "<Keyboard>/a");
            kazSag = harita.AddAction("KazSag", InputActionType.Button, "<Keyboard>/d");
            kazYukari = harita.AddAction("KazYukari", InputActionType.Button, "<Keyboard>/w");
            termal = harita.AddAction("Termal", InputActionType.Button, "<Keyboard>/t");
            duraklat = harita.AddAction("Duraklat", InputActionType.Button, "<Keyboard>/escape");

            isaretci = harita.AddAction("Isaretci", InputActionType.Value, "<Pointer>/position");
            tikla = harita.AddAction("Tikla", InputActionType.Button, "<Pointer>/press");

            gelistiriciBolum1 = harita.AddAction("GelistiriciBolum1", InputActionType.Button, "<Keyboard>/f1");
            gelistiriciKumsal = harita.AddAction("GelistiriciKumsal", InputActionType.Button, "<Keyboard>/f2");
            gelistiriciBolum5 = harita.AddAction("GelistiriciBolum5", InputActionType.Button, "<Keyboard>/f3");
            gelistiriciMenu = harita.AddAction("GelistiriciMenu", InputActionType.Button, "<Keyboard>/f4");

            harita.Enable();
            Application.quitting += Kapat;
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
