using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Camera
{
    /// <summary>Kaplumbağanın gözünden bakış: fare sağa-sola gövdeyi döndürür, yukarı-aşağı sadece kamerayı eğer.
    /// Gövdenin kendi modeli göze girmesin diye görünmez olur (gölgesi kalır); gövde değişince (öncü yavru) eskisi yeniden görünür.</summary>
    public class BirinciSahisKamera : MonoBehaviour
    {
        [SerializeField] private KameraAyarlari ayarlar;
        [Tooltip("Sağa-sola dönen gövde (kaplumbağanın kökü).")]
        [SerializeField] private Transform govde;

        private float egim;
        private Transform gizlenen;

        // Sahnede birden çok göz olabilir (anne, sürü); biri kapanınca açık kalanın imleci serbest kalmasın.
        private static int acikSayisi;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => acikSayisi = 0;

        private void Start() => KendiniGizle(govde);

        private void OnEnable()
        {
            acikSayisi++;
            Duraklatma.Degisti += DuraklatmaDegisti;
            ImleciKilitle(!Duraklatma.Acik);
        }

        private void OnDisable()
        {
            acikSayisi--;
            Duraklatma.Degisti -= DuraklatmaDegisti;
            if (acikSayisi <= 0) ImleciKilitle(false);
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
            KendiniGizle(govde);
        }

        /// <summary>Gözün ait olduğu gövdenin modeli kameraya görünmez, sadece gölgesini düşürür; önceki gövde yeniden görünür.</summary>
        private void KendiniGizle(Transform yeni)
        {
            if (gizlenen == yeni) return;
            Golge(gizlenen, false);
            gizlenen = yeni;
            Golge(gizlenen, true);
        }

        private static void Golge(Transform kok, bool sadeceGolge)
        {
            if (kok == null) return;
            foreach (Renderer r in kok.GetComponentsInChildren<Renderer>(true))
                r.shadowCastingMode = sadeceGolge ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private static void ImleciKilitle(bool kilitle)
        {
            Cursor.lockState = kilitle ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !kilitle;
        }
    }
}
