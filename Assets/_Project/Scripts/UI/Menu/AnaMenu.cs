using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Menu
{
    /// <summary>Ana menü: Yeni oyun, Devam et (kayıt varsa), Ayarlar, Çıkış.</summary>
    public class AnaMenu : MonoBehaviour
    {
        [SerializeField] private Button yeniOyun;
        [SerializeField] private Button devamEt;
        [SerializeField] private Button ayarlar;
        [SerializeField] private Button cikis;
        [SerializeField] private AyarlarPaneli ayarlarPaneli;

        private void Awake()
        {
            yeniOyun.onClick.AddListener(OyunAkisi.YeniOyun);
            devamEt.onClick.AddListener(OyunAkisi.DevamEt);
            ayarlar.onClick.AddListener(ayarlarPaneli.Ac);
            cikis.onClick.AddListener(Cik);
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
