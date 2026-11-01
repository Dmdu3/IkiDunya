using IkiDunya.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ayarlar: ses, fare hassasiyeti, ipuçları, tam ekran, çözünürlük, grafik kalitesi, yazı boyutu, tuş atama satırları; Geri ile kapanır. Değerler bu bilgisayarda saklanır.</summary>
    public class AyarlarPaneli : MonoBehaviour
    {
        [SerializeField] private Slider ses;
        [SerializeField] private Slider hassasiyet;
        [SerializeField] private Button geri;
        [Tooltip("Tuşları varsayılana döndürür (isteğe bağlı).")]
        [SerializeField] private Button varsayilanTuslar;
        [Tooltip("Geri'ye basınca açılacak panel (menü düğmeleri).")]
        [SerializeField] private GameObject oncekiPanel;

        [Header("Ekran ve erişilebilirlik (isteğe bağlı)")]
        [SerializeField] private MetinTablosu metinler;
        [SerializeField] private Toggle ipuclari;
        [SerializeField] private Toggle tamEkran;
        [SerializeField] private Dropdown cozunurluk;
        [SerializeField] private Dropdown kalite;
        [SerializeField] private Slider arayuzOlcegi;

        private void Awake()
        {
            ses.onValueChanged.AddListener(deger => OyunTercihleri.Ses = deger);
            hassasiyet.onValueChanged.AddListener(deger => OyunTercihleri.HassasiyetCarpani = deger);
            geri.onClick.AddListener(Kapat);
            if (varsayilanTuslar != null) varsayilanTuslar.onClick.AddListener(GameInput.VarsayilanaDon);
            if (ipuclari != null) ipuclari.onValueChanged.AddListener(acik => OyunTercihleri.Ipuclari = acik);
            if (tamEkran != null) tamEkran.onValueChanged.AddListener(acik => OyunTercihleri.TamEkran = acik);
            if (cozunurluk != null) cozunurluk.onValueChanged.AddListener(i => OyunTercihleri.Cozunurluk = i);
            if (kalite != null) kalite.onValueChanged.AddListener(i => OyunTercihleri.Kalite = i);
            if (arayuzOlcegi != null) arayuzOlcegi.onValueChanged.AddListener(deger => OyunTercihleri.ArayuzOlcegi = deger);
        }

        private void OnEnable()
        {
            ses.SetValueWithoutNotify(OyunTercihleri.Ses);
            hassasiyet.SetValueWithoutNotify(OyunTercihleri.HassasiyetCarpani);
            if (ipuclari != null) ipuclari.SetIsOnWithoutNotify(OyunTercihleri.Ipuclari);
            if (tamEkran != null) tamEkran.SetIsOnWithoutNotify(OyunTercihleri.TamEkran);
            if (arayuzOlcegi != null) arayuzOlcegi.SetValueWithoutNotify(OyunTercihleri.ArayuzOlcegi);

            if (cozunurluk != null)
            {
                // Çözünürlükler sayı (yazı değil); seçili olan şu anki ekran.
                var secenekler = new List<string>();
                Resolution[] liste = OyunTercihleri.Cozunurlukler();
                int secili = liste.Length - 1;
                for (int i = 0; i < liste.Length; i++)
                {
                    secenekler.Add($"{liste[i].width} × {liste[i].height}");
                    if (liste[i].width == Screen.width && liste[i].height == Screen.height) secili = i;
                }
                cozunurluk.ClearOptions();
                cozunurluk.AddOptions(secenekler);
                cozunurluk.SetValueWithoutNotify(OyunTercihleri.Cozunurluk >= 0 ? OyunTercihleri.Cozunurluk : secili);
            }

            if (kalite != null)
            {
                MetinTablosu tablo = metinler != null ? metinler : MetinTablosu.Yuklu;
                kalite.ClearOptions();
                foreach (string id in new[] { "ayar_kalite_dusuk", "ayar_kalite_orta", "ayar_kalite_yuksek" })
                    kalite.options.Add(new Dropdown.OptionData(tablo != null ? tablo.Al(id) : id));
                kalite.SetValueWithoutNotify(OyunTercihleri.Kalite);
                kalite.RefreshShownValue();
            }
        }

        public void Ac()
        {
            oncekiPanel.SetActive(false);
            gameObject.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(ses.gameObject);
        }

        private void Kapat()
        {
            gameObject.SetActive(false);
            oncekiPanel.SetActive(true);
        }
    }
}
