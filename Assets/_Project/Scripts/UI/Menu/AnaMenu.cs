using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ana menü: Yeni oyun, Devam et (kayıt varsa), Parkurlar, Ayarlar, Çıkış.</summary>
    public class AnaMenu : MonoBehaviour
    {
        [SerializeField] private Button yeniOyun;
        [SerializeField] private Button devamEt;
        [SerializeField] private Button ayarlar;
        [SerializeField] private Button cikis;
        [SerializeField] private AyarlarPaneli ayarlarPaneli;
        [SerializeField] private Button parkurlar;
        [SerializeField] private ParkurlarPaneli parkurlarPaneli;

        private void Awake()
        {
            DugmeSesi.Bagla(yeniOyun, devamEt, ayarlar, cikis, parkurlar);
            yeniOyun.onClick.AddListener(OyunAkisi.YeniOyun);
            devamEt.onClick.AddListener(OyunAkisi.DevamEt);
            ayarlar.onClick.AddListener(ayarlarPaneli.Ac);
            cikis.onClick.AddListener(Cik);
            if (parkurlar != null && parkurlarPaneli != null) parkurlar.onClick.AddListener(parkurlarPaneli.Ac);
        }

        private void OnEnable()
        {
            devamEt.interactable = OyunAkisi.KayitVar;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static void Cik()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
