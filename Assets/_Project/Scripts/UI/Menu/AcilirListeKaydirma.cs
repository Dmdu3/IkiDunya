using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Açılır liste açılınca seçili satır görünsün: liste uzunsa seçili satıra kaydırır (Unity hep en üstten açar).</summary>
    [RequireComponent(typeof(Dropdown))]
    public class AcilirListeKaydirma : MonoBehaviour
    {
        private Dropdown acilir;
        private GameObject sonListe;

        private void Awake() => acilir = GetComponent<Dropdown>();

        private void LateUpdate()
        {
            // Liste her açılışta yeniden kurulur ("Dropdown List" çocuğu); yeni kurulanı bir kez kaydır.
            Transform liste = transform.Find("Dropdown List");
            GameObject nesne = liste != null ? liste.gameObject : null;
            if (nesne == sonListe) return;
            sonListe = nesne;
            if (nesne == null || acilir.options.Count < 2) return;
            var kaydirma = nesne.GetComponent<ScrollRect>();
            if (kaydirma == null) return;
            Canvas.ForceUpdateCanvases();
            // 1 en üst, 0 en alt; seçili satır görünür alanın ortasına yakın gelir.
            kaydirma.verticalNormalizedPosition = 1f - acilir.value / (acilir.options.Count - 1f);
        }
    }
}
