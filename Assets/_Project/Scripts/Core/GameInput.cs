using UnityEngine;
using UnityEngine.InputSystem;

namespace IkiDunya.Core
{
    /// <summary>Oyunun bütün tuşları tek yerde; başka script tuşu doğrudan okumaz.</summary>
    public static class GameInput
    {
        private static InputActionMap harita;

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
        public static Vector2 Hareket => hareket.ReadValue<Vector2>();          // WASD
        public static Vector2 Bakis => bakis.ReadValue<Vector2>();              // fare hareketi
        public static bool HizliBasili => hizli.IsPressed();                    // Shift: anne hızlı, yavru sürüyü dağıtır
        public static bool ZiplaDalBasildi => ziplaDal.WasPressedThisFrame();   // Space
        public static bool ZiplaDalBasili => ziplaDal.IsPressed();              // Space basılı (denizde yüzeye çık)
        public static bool EtkilesBasili => etkiles.IsPressed();                // E basılı tut
        public static bool EtkilesBasildi => etkiles.WasPressedThisFrame();     // E bas (ağdan kurtul, ye)
        public static bool KazSolBasildi => kazSol.WasPressedThisFrame();       // A (kazma sırası)
        public static bool KazSagBasildi => kazSag.WasPressedThisFrame();       // D (kazma sırası)
        public static bool KazYukariBasildi => kazYukari.WasPressedThisFrame(); // W (kum altında)
        public static bool TermalBasildi => termal.WasPressedThisFrame();       // T

        // Genel
        public static bool DuraklatBasildi => duraklat.WasPressedThisFrame();   // Esc

        // İnsan kısmı (yönetim ekranı)
        public static Vector2 IsaretciKonumu => isaretci.ReadValue<Vector2>();
        public static bool TiklaBasildi => tikla.WasPressedThisFrame();
        public static bool TiklaBirakildi => tikla.WasReleasedThisFrame();
        public static bool TiklaBasili => tikla.IsPressed();

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
