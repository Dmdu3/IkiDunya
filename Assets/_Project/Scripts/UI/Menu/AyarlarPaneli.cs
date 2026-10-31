using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ayarlar: ses ve fare hassasiyeti kaydırıcıları; Geri ile kapanır. Değerler bu bilgisayarda saklanır.</summary>
    public class AyarlarPaneli : MonoBehaviour
    {
        [SerializeField] private Slider ses;
        [SerializeField] private Slider hassasiyet;
        [SerializeField] private Button geri;
        [Tooltip("Geri'ye basınca açılacak panel (menü düğmeleri).")]
        [SerializeField] private GameObject oncekiPanel;

        private void Awake()
        {
            ses.onValueChanged.AddListener(deger => OyunTercihleri.Ses = deger);
            hassasiyet.onValueChanged.AddListener(deger => OyunTercihleri.HassasiyetCarpani = deger);
            geri.onClick.AddListener(Kapat);
        }

        private void OnEnable()
        {
            ses.SetValueWithoutNotify(OyunTercihleri.Ses);
            hassasiyet.SetValueWithoutNotify(OyunTercihleri.HassasiyetCarpani);
        }

        public void Ac()
        {
            oncekiPanel.SetActive(false);
            gameObject.SetActive(true);
        }

        private void Kapat()
        {
            gameObject.SetActive(false);
            oncekiPanel.SetActive(true);
        }
    }
}
