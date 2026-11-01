using IkiDunya.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ayarlarda bir eylemin klavye tuşu: şu anki tuşu gösterir; düğmeye basıp yeni tuşa basınca atar, Esc / B / sağ tık iptal eder.
    /// Tuş başka satırda kullanılıyorsa yer değiştirirler (diğer satır eski tuşu alır ve kısa süre vurgulanır).</summary>
    public class TusSatiri : MonoBehaviour
    {
        [Tooltip("GameInput'taki eylemin adı (ör. Hareket, Termal).")]
        [SerializeField] private string eylem;
        [Tooltip("Eylemin kaçıncı bağlaması (Hareket: 1 ileri, 2 geri, 3 sol, 4 sağ; diğerleri 0).")]
        [SerializeField] private int baglamaSirasi;
        [SerializeField] private Text tusYazisi;
        [SerializeField] private Button dugme;

        private InputActionRebindingExtensions.RebindingOperation islem;
        private Color yaziRengi;
        private float vurguKalan;

        /// <summary>Yer değiştiren ya da sıfırlanan satırın tuşu bu kadar (sn) vurgu renginde kalır.</summary>
        private const float VurguSuresi = 1.5f;

        private void Awake()
        {
            dugme.onClick.AddListener(Baslat);
            yaziRengi = tusYazisi.color;
        }

        private void Update()
        {
            // Atama sürerken oyun kolunun B'si ya da sağ tık da vazgeçer (Esc atamanın kendi iptali).
            if (islem != null && GameInput.VazgecMenudeBasildi) islem.Cancel();
            if (vurguKalan <= 0f) return;
            // Duraklatma menüsünde zaman durur: süre gerçek zamanla işler.
            vurguKalan -= Time.unscaledDeltaTime;
            if (vurguKalan <= 0f) tusYazisi.color = yaziRengi;
        }

        /// <summary>Tuşun değiştiğini gösterir (yer değiştirme, varsayılana dönme).</summary>
        public void Vurgula()
        {
            ArayuzTemasi tema = ArayuzTemasi.Yuklu;
            tusYazisi.color = tema != null ? tema.Vurgu : Color.yellow;
            vurguKalan = VurguSuresi;
        }

        private void OnEnable()
        {
            GameInput.BaglamalarDegisti += Yaz;
            Yaz();
        }

        private void OnDisable()
        {
            GameInput.BaglamalarDegisti -= Yaz;
            islem?.Cancel();
        }

        private void Yaz()
        {
            InputAction e = GameInput.Eylem(eylem);
            if (e != null) tusYazisi.text = e.GetBindingDisplayString(baglamaSirasi);
        }

        private void Baslat()
        {
            InputAction e = GameInput.Eylem(eylem);
            if (e == null || islem != null) return;
            Ses.Cal(SesTuru.DugmeTikla);
            e.Disable();
            GameInput.TusAtaniyor = true;
            tusYazisi.text = "…";
            string eskiYol = e.bindings[baglamaSirasi].effectivePath;
            // Satırlar klavye tuşunu atar; oyun kolu tuşları sabit (yoksa kolun tuşu klavyeninkinin yerine geçerdi).
            islem = e.PerformInteractiveRebinding(baglamaSirasi)
                .WithControlsExcluding("<Mouse>")
                .WithControlsExcluding("<Gamepad>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(_ => Bitir(e, true, eskiYol))
                .OnCancel(_ => Bitir(e, false, eskiYol))
                .Start();
        }

        private void Bitir(InputAction e, bool atandi, string eskiYol)
        {
            islem.Dispose();
            islem = null;
            e.Enable();
            GameInput.TusAtaniyor = false;
            if (!atandi)
            {
                Yaz();
                return;
            }
            YerDegistir(e.bindings[baglamaSirasi].effectivePath, eskiYol);
            GameInput.BaglamalariKaydet();
        }

        /// <summary>Yeni tuş başka satırdaysa o satır bizim eski tuşumuzu alır. Yürüme ile kazma aynı tuşları bilerek paylaşır
        /// (kazarken yürünmez), onlar çakışma sayılmaz.</summary>
        private void YerDegistir(string yeniYol, string eskiYol)
        {
            if (transform.parent == null || yeniYol == eskiYol) return;
            foreach (TusSatiri diger in transform.parent.GetComponentsInChildren<TusSatiri>(true))
            {
                if (diger == this || IzinliOrtak(eylem, diger.eylem)) continue;
                InputAction de = GameInput.Eylem(diger.eylem);
                if (de == null || de.bindings[diger.baglamaSirasi].effectivePath != yeniYol) continue;
                de.ApplyBindingOverride(diger.baglamaSirasi, eskiYol);
                diger.Vurgula();
            }
        }

        private static bool IzinliOrtak(string a, string b) =>
            (a == "Hareket" && b.StartsWith("Kaz")) || (b == "Hareket" && a.StartsWith("Kaz"));
    }
}
