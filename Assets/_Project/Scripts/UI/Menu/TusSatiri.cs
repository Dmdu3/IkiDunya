using IkiDunya.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ayarlarda bir eylemin tuşu: şu anki tuşu gösterir; düğmeye basıp yeni tuşa basınca atar, Esc iptal eder.</summary>
    public class TusSatiri : MonoBehaviour
    {
        [Tooltip("GameInput'taki eylemin adı (ör. Hareket, Termal).")]
        [SerializeField] private string eylem;
        [Tooltip("Eylemin kaçıncı bağlaması (Hareket: 1 ileri, 2 geri, 3 sol, 4 sağ; diğerleri 0).")]
        [SerializeField] private int baglamaSirasi;
        [SerializeField] private Text tusYazisi;
        [SerializeField] private Button dugme;

        private InputActionRebindingExtensions.RebindingOperation islem;

        private void Awake() => dugme.onClick.AddListener(Baslat);

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
            e.Disable();
            GameInput.TusAtaniyor = true;
            tusYazisi.text = "…";
            islem = e.PerformInteractiveRebinding(baglamaSirasi)
                .WithControlsExcluding("<Mouse>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(_ => Bitir(e, true))
                .OnCancel(_ => Bitir(e, false))
                .Start();
        }

        private void Bitir(InputAction e, bool atandi)
        {
            islem.Dispose();
            islem = null;
            e.Enable();
            GameInput.TusAtaniyor = false;
            if (atandi) GameInput.BaglamalariKaydet();
            else Yaz();
        }
    }
}
