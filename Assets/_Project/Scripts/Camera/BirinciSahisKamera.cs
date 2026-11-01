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
        [Tooltip("Sarsıntının genliği ve süresi.")]
        [SerializeField] private GeribildirimAyarlari geribildirim;

        private float egim;
        private Transform gizlenen;
        private UnityEngine.Camera kamera;
        private float ilkGorus;
        private Vector3 tabanYer;
        private Vector3 oncekiGovde;
        private float sallanmaFazi;
        private float sallanma;
        private float sarsintiKalan;
        private float sarsintiGucu;

        // Sahnede birden çok göz olabilir (anne, sürü); biri kapanınca açık kalanın imleci serbest kalmasın.
        private static int acikSayisi;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => acikSayisi = 0;

        private void Awake()
        {
            kamera = GetComponent<UnityEngine.Camera>();
            if (kamera != null) ilkGorus = kamera.fieldOfView;
            tabanYer = transform.localPosition;
        }

        private void Start()
        {
            KendiniGizle(govde);
            if (govde != null) oncekiGovde = govde.position;
        }

        private void OnEnable()
        {
            acikSayisi++;
            Duraklatma.Degisti += DuraklatmaDegisti;
            Geribildirim.Sarsildi += Sars;
            // Sistemler'siz deneme sahnesinde de şiddetler ayardan gelsin.
            if (Geribildirim.Ayarlar == null && geribildirim != null) Geribildirim.Ayarlar = geribildirim;
            ImleciKilitle(!Duraklatma.Acik);
        }

        private void OnDisable()
        {
            acikSayisi--;
            Duraklatma.Degisti -= DuraklatmaDegisti;
            Geribildirim.Sarsildi -= Sars;
            if (acikSayisi <= 0) ImleciKilitle(false);
        }

        private void Sars(float guc)
        {
            sarsintiGucu = Mathf.Max(guc, sarsintiKalan > 0f ? sarsintiGucu : 0f);
            sarsintiKalan = geribildirim != null ? geribildirim.SarsintiSuresi : 0.3f;
        }

        private void DuraklatmaDegisti(bool acik) => ImleciKilitle(!acik);

        private void Update()
        {
            if (Duraklatma.Acik || govde == null) return;

            // Fare (piksel/kare) ve oyun kolunun sağ çubuğu (−1..1, saniyede derece).
            Vector2 bakis = (GameInput.Bakis * ayarlar.Hassasiyet + GameInput.BakisCubugu * ayarlar.CubukHizi * Time.deltaTime) * OyunTercihleri.HassasiyetCarpani;
            govde.Rotate(0f, bakis.x, 0f, Space.World);
            egim = Mathf.Clamp(egim - bakis.y, -ayarlar.YukariSinir, -ayarlar.AsagiSinir);
            transform.localRotation = Quaternion.Euler(egim, 0f, 0f);
        }

        /// <summary>Baş sallanması (gövde yürüdükçe), hızlanınca geniş görüş, darbede sarsıntı. Bakış açısı Update'te; bunlar üstüne eklenir.</summary>
        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            // Göz başka biri tarafından yönetiliyorsa (final kamerası gövdeden ayırdı) dokunma.
            if (transform.parent == null || govde == null || dt <= 0f) return;

            Vector3 fark = govde.position - oncekiGovde;
            oncekiGovde = govde.position;
            fark.y = 0f;
            float oran = Mathf.Clamp01(fark.magnitude / dt / Mathf.Max(0.01f, ayarlar.SallanmaTamHiz));
            sallanma = Mathf.MoveTowards(sallanma, oran, dt * 4f);
            sallanmaFazi += dt * ayarlar.SallanmaSikligi * Mathf.PI * 2f * Mathf.Max(0.3f, oran);
            Vector3 yer = tabanYer + Vector3.up * Mathf.Sin(sallanmaFazi) * ayarlar.SallanmaGenligi * sallanma;

            if (sarsintiKalan > 0f && geribildirim != null)
            {
                sarsintiKalan -= dt;
                float g = sarsintiGucu * Mathf.Clamp01(sarsintiKalan / geribildirim.SarsintiSuresi);
                yer += Random.insideUnitSphere * geribildirim.SarsintiKaymasi * g;
                transform.localRotation *= Quaternion.Euler(Random.Range(-1f, 1f) * geribildirim.SarsintiDonmesi * g, 0f, Random.Range(-1f, 1f) * geribildirim.SarsintiDonmesi * g);
            }
            transform.localPosition = yer;

            if (kamera == null) return;
            bool hizli = GameInput.HizliBasili && oran > 0.5f;
            float hedef = ilkGorus + (hizli ? ayarlar.HizGorusArtisi : 0f);
            kamera.fieldOfView = Mathf.Lerp(kamera.fieldOfView, hedef, dt * ayarlar.GorusGecisi);
        }

        /// <summary>Gövdeyi bu kadar derece sağa (eksiyse sola) döndürür; oyuncunun girdisi değil, içgüdü (yavru en parlak aya döner) çağırır.</summary>
        public void Dondur(float derece)
        {
            if (govde != null && transform.parent != null) govde.Rotate(0f, derece, 0f, Space.World);
        }

        /// <summary>Gövdeyi değiştirir (ör. öncü yavru değişince).</summary>
        public void GovdeAyarla(Transform yeniGovde)
        {
            govde = yeniGovde;
            egim = 0f;
            // Göz yeni gövdeye taşındı (öncü değişti): sallanmanın tabanı yeni yer.
            tabanYer = transform.localPosition;
            if (govde != null) oncekiGovde = govde.position;
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
