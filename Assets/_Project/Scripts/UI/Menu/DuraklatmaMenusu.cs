using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Oyun sırasında Esc ile açılan menü: Oyuna dön, Ayarlar, Ana menü. Sistemler'in içinde, sahneler arasında yaşar.</summary>
    public class DuraklatmaMenusu : MonoBehaviour
    {
        [Tooltip("Açılıp kapanan panelin kökü.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject dugmeler;
        [SerializeField] private Button oyunaDon;
        [SerializeField] private Button ayarlar;
        [SerializeField] private Button anaMenu;
        [SerializeField] private AyarlarPaneli ayarlarPaneli;

        private void Awake()
        {
            DugmeSesi.Bagla(oyunaDon, ayarlar, anaMenu);
            oyunaDon.onClick.AddListener(Duraklatma.Kapat);
            ayarlar.onClick.AddListener(ayarlarPaneli.Ac);
            anaMenu.onClick.AddListener(OyunAkisi.MenuyeDon);
            panel.SetActive(false);
        }

        private void OnEnable() => Duraklatma.Degisti += Goster;
        private void OnDisable() => Duraklatma.Degisti -= Goster;

        private void Update()
        {
            // Menüde ve geçiş sırasında duraklatma yok (kararırken açılırsa sonraki sahne duraklatılmış gelir).
            if (!GameInput.DuraklatBasildi || OyunAkisi.MenudeMi || OyunAkisi.GecisSuruyor) return;
            if (Duraklatma.Acik) Duraklatma.Kapat();
            else Duraklatma.Ac();
        }

        private void Goster(bool acik)
        {
            panel.SetActive(acik);
            dugmeler.SetActive(true);
            ayarlarPaneli.gameObject.SetActive(false);
        }
    }
}
