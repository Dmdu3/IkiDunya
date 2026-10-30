using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Camera
{
    /// <summary>Kaplumbağanın gözünden bakış: fare sağa-sola gövdeyi döndürür, yukarı-aşağı sadece kamerayı eğer.</summary>
    public class BirinciSahisKamera : MonoBehaviour
    {
        [SerializeField] private KameraAyarlari ayarlar;
        [Tooltip("Sağa-sola dönen gövde (kaplumbağanın kökü).")]
        [SerializeField] private Transform govde;

        private float egim;

        private void OnEnable()
        {
            ImleciKilitle(true);
        }

        private void OnDisable()
        {
            ImleciKilitle(false);
        }

        private void Update()
        {
            // Duraklatma menüsü gelene kadar Esc imleci serbest bırakır, tekrar Esc kilitler.
            if (GameInput.DuraklatBasildi) ImleciKilitle(Cursor.lockState != CursorLockMode.Locked);
            if (Cursor.lockState != CursorLockMode.Locked) return;

            Vector2 bakis = GameInput.Bakis * ayarlar.Hassasiyet;
            govde.Rotate(0f, bakis.x, 0f, Space.World);
            egim = Mathf.Clamp(egim - bakis.y, -ayarlar.YukariSinir, -ayarlar.AsagiSinir);
            transform.localRotation = Quaternion.Euler(egim, 0f, 0f);
        }

        private static void ImleciKilitle(bool kilitle)
        {
            Cursor.lockState = kilitle ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !kilitle;
        }
    }
}
