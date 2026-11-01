using IkiDunya.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Kutusuz menü seçeneği: seçiliyken yazı vurgu renginde ve solunda kısa çizgi; fare üstüne gelince seçilir (fare ve oyun kolu aynı görünür).
    /// Basılamazken soluk.</summary>
    [RequireComponent(typeof(Selectable))]
    public class MenuSecenegi : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private ArayuzTemasi tema;
        [SerializeField] private Text yazi;
        [Tooltip("Seçiliyken görünen kısa çizgi.")]
        [SerializeField] private Graphic isaret;

        private Selectable secilebilir;

        private void Awake() => secilebilir = GetComponent<Selectable>();

        public void OnPointerEnter(PointerEventData veri)
        {
            if (secilebilir.IsInteractable() && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        /// <summary>Paneldeki ilk basılabilir seçeneği seçer (panel değişince oyun koluyla gezinme kopmasın).</summary>
        public static void IlkiniSec(GameObject panel)
        {
            if (EventSystem.current == null || panel == null) return;
            foreach (Selectable s in panel.GetComponentsInChildren<Selectable>())
                if (s.IsInteractable())
                {
                    EventSystem.current.SetSelectedGameObject(s.gameObject);
                    return;
                }
        }

        private void LateUpdate()
        {
            ArayuzTemasi t = tema != null ? tema : ArayuzTemasi.Yuklu;
            if (t == null || yazi == null) return;
            bool secili = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
            bool acik = secilebilir.IsInteractable();
            Color renk = !acik ? t.SolukYazi * new Color(1f, 1f, 1f, 0.5f) : secili ? t.Vurgu : t.Yazi;
            if (yazi.color != renk) yazi.color = renk;
            if (isaret != null && isaret.enabled != (secili && acik)) isaret.enabled = secili && acik;
        }
    }
}
