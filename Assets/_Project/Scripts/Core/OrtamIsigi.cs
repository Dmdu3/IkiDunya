using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Kumsal sahnesinde gece ile gündüz arası: kaplumbağa gecelerinde ay ışığı ve koyu gökyüzü, insan turunda ve finalde gündüz.</summary>
    public class OrtamIsigi : MonoBehaviour
    {
        [SerializeField] private OrtamAyarlari ayarlar;
        [Tooltip("Sahnenin yönlü ışığı (gece ay, gündüz güneş).")]
        [SerializeField] private Light isik;
        [Tooltip("Gökyüzü rengi verilecek kameralar.")]
        [SerializeField] private UnityEngine.Camera[] kameralar;
        [Tooltip("Akışın olmadığı deneme sahnesinde gece mi başlasın.")]
        [SerializeField] private bool akisYoksaGece = true;
        [Tooltip("İsteğe bağlı: gece açılan gökyüzü (yıldızlar, ay).")]
        [SerializeField] private GameObject gokyuzu;

        /// <summary>Sahne şu an gece mi; kaplumbağanın algısı (sahte ay) bunu okur.</summary>
        public static bool GeceMi { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => GeceMi = false;

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;
        private void OnDisable() => OyunAkisi.AdimBasladi -= AdimBasladi;

        private void Start()
        {
            if (!OyunAkisi.Var) Uygula(akisYoksaGece);
        }

        // Finalde güneşin rengini FinalSahnesi verir (Temiz Enerji'ye göre); burada dokunulmaz.
        private void AdimBasladi(Adim adim) =>
            Uygula(adim.Tur == AdimTuru.YuvaGecesi || adim.Tur == AdimTuru.KacisGecesi
                   || (adim.Tur == AdimTuru.Parkur && adim.Sira == (int)ParkurTuru.CopSeli), adim.Tur != AdimTuru.Final);

        private void Uygula(bool gece, bool renkVer = true)
        {
            GeceMi = gece;
            if (gokyuzu != null) gokyuzu.SetActive(gece);
            if (isik != null)
            {
                if (renkVer) isik.color = gece ? ayarlar.GeceIsigi : ayarlar.GunduzIsigi;
                isik.intensity = gece ? ayarlar.GeceIsikGucu : ayarlar.GunduzIsikGucu;
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = gece ? ayarlar.GeceOrtamIsigi : ayarlar.GunduzOrtamIsigi;
            RenderSettings.fog = gece;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = ayarlar.GeceGokyuzu;
            RenderSettings.fogDensity = ayarlar.GeceSis;

            Color gok = gece ? ayarlar.GeceGokyuzu : ayarlar.GunduzGokyuzu;
            foreach (UnityEngine.Camera kamera in kameralar)
            {
                if (kamera == null) continue;
                kamera.clearFlags = CameraClearFlags.SolidColor;
                kamera.backgroundColor = gok;
            }
        }
    }
}
