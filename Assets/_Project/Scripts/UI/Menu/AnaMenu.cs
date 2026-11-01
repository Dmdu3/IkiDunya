using IkiDunya.Core;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private bool onayBekliyor;

        private void Awake()
        {
            DugmeSesi.Bagla(yeniOyun, devamEt, ayarlar, cikis, parkurlar);
            yeniOyun.onClick.AddListener(YeniOyunaBasildi);
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
            OnayiBirak();
            // Oyun koluyla gezilebilsin: kayıt varsa Devam et, yoksa Yeni oyun seçili (tek basış kaydı silmesin).
            Button ilk = devamEt.interactable ? devamEt : yeniOyun;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(ilk.gameObject);
        }

        private void Update()
        {
            // Onay beklerken başka düğmeye geçilir ya da boşluğa tıklanırsa soru kalkar.
            if (onayBekliyor && (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != yeniOyun.gameObject)) OnayiBirak();
        }

        /// <summary>Kayıt varken ilk basış sorar (kayıt silinecek), ikinci basış yeni oyunu başlatır.</summary>
        private void YeniOyunaBasildi()
        {
            if (OyunAkisi.KayitVar && !onayBekliyor)
            {
                onayBekliyor = true;
                YeniOyunYazisi()?.Goster("menu_yeni_oyun_onay");
                return;
            }
            OnayiBirak();
            OyunAkisi.YeniOyun();
        }

        private void OnayiBirak()
        {
            if (!onayBekliyor) return;
            onayBekliyor = false;
            YeniOyunYazisi()?.Goster(null);
        }

        private MetinYazisi YeniOyunYazisi() => yeniOyun.GetComponentInChildren<MetinYazisi>(true);

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
