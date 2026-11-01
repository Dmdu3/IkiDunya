using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ana menüde Parkurlar: hikâyede bitirilen minigame'ler buradan ayrıca oynanır; her birinin yanında rekoru. Bitirilmemiş olan kilitli.</summary>
    public class ParkurlarPaneli : MonoBehaviour
    {
        [SerializeField] private MetinTablosu metinler;
        [Tooltip("ParkurTuru sırasıyla: poşet sürüsü, çöp seli, çöp girdabı.")]
        [SerializeField] private Button[] dugmeler = new Button[0];
        [SerializeField] private Button geri;
        [Tooltip("Geri'ye basınca açılacak panel (menü düğmeleri).")]
        [SerializeField] private GameObject oncekiPanel;

        private MetinTablosu Tablo => metinler != null ? metinler : MetinTablosu.Yuklu;
        private string Al(string id) => Tablo != null ? Tablo.Al(id) : $"[{id}]";

        private void Awake()
        {
            DugmeSesi.Bagla(dugmeler);
            DugmeSesi.Bagla(geri);
            for (int i = 0; i < dugmeler.Length; i++)
            {
                var tur = (ParkurTuru)i;
                dugmeler[i].onClick.AddListener(() => OyunAkisi.ParkurOyna(tur));
            }
            geri.onClick.AddListener(Kapat);
        }

        private void OnEnable()
        {
            for (int i = 0; i < dugmeler.Length; i++)
            {
                var tur = (ParkurTuru)i;
                bool acik = Parkur.Acik(tur);
                float rekor = Parkur.Rekor(tur);
                dugmeler[i].interactable = acik;
                dugmeler[i].GetComponentInChildren<Text>().text = acik
                    ? $"{Al(Parkur.MetinId(tur))}   {Al("parkur_rekor")} {Parkur.Bicim(rekor)}"
                    : $"{Al(Parkur.MetinId(tur))}   {Al("parkur_kilitli")}";
            }
        }

        public void Ac()
        {
            oncekiPanel.SetActive(false);
            gameObject.SetActive(true);
            // Kilitli parkurlar atlanır; hepsi kilitliyse Geri seçilir.
            MenuSecenegi.IlkiniSec(gameObject);
        }

        private void Kapat()
        {
            gameObject.SetActive(false);
            oncekiPanel.SetActive(true);
            MenuSecenegi.IlkiniSec(oncekiPanel);
        }
    }
}
