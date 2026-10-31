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
            Duraklatma.Degisti += DuraklatmaDegisti;
            ImleciKilitle(!Duraklatma.Acik);
        }

        private void OnDisable()
        {
            Duraklatma.Degisti -= DuraklatmaDegisti;
            ImleciKilitle(false);
        }

        private void DuraklatmaDegisti(bool acik) => ImleciKilitle(!acik);

        private void Update()
        {
            if (Duraklatma.Acik || govde == null) return;

            Vector2 bakis = GameInput.Bakis * ayarlar.Hassasiyet * OyunTercihleri.HassasiyetCarpani;
            govde.Rotate(0f, bakis.x, 0f, Space.World);
            egim = Mathf.Clamp(egim - bakis.y, -ayarlar.YukariSinir, -ayarlar.AsagiSinir);
            transform.localRotation = Quaternion.Euler(egim, 0f, 0f);
        }

        /// <summary>Gövdeyi değiştirir (ör. öncü yavru değişince).</summary>
        public void GovdeAyarla(Transform yeniGovde)
        {
            govde = yeniGovde;
            egim = 0f;
        }

        private static void ImleciKilitle(bool kilitle)
        {
            Cursor.lockState = kilitle ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !kilitle;
        }
    }
}
