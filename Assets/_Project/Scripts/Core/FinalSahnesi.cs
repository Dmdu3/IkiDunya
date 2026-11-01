using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Core
{
    /// <summary>"30 yıl sonra": büyümüş dişi kaplumbağa aynı kumsala yumurtlamaya döner. Temiz Enerji oynandıysa kumsal serin, oynanmadıysa sıcak ve kızgın. Kamera yavaşça kayar, yazılar belirir, sonra jenerik akar; tıklayınca oyun biter.</summary>
    public class FinalSahnesi : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private MetinTablosu metinler;
        [Tooltip("Denizden kumsala yürüyen yetişkin kaplumbağa.")]
        [SerializeField] private Transform kaplumbaga;
        [SerializeField] private Transform yuruyusSonu;
        [SerializeField] private float yuruyusSuresi = 12f;
        [Tooltip("Sahnenin güneşi; rengi kumsalın haline göre değişir.")]
        [SerializeField] private Light gunes;
        [SerializeField] private Color serinRenk = new Color(1f, 0.92f, 0.8f);
        [SerializeField] private Color sicakRenk = new Color(1f, 0.55f, 0.3f);
        [Tooltip("Sadece Temiz Enerji oynanmadıysa görünen sıcak kum izleri.")]
        [SerializeField] private GameObject sicakIzler;
        [SerializeField] private Text baslik;
        [SerializeField] private Text kurtarilan;
        [Tooltip("Yürüyüş boyunca kayan kamera; boşsa kamera sabit kalır.")]
        [SerializeField] private Transform kamera;
        [SerializeField] private Transform kameraSon;
        [Tooltip("Başlık ve kurtarılan yazısı bununla belirir.")]
        [SerializeField] private CanvasGroup yazilar;
        [SerializeField] private float yaziBelirmeSuresi = 3f;
        [Tooltip("Yürüyüşten sonra aşağıdan yukarı akan jenerik.")]
        [SerializeField] private RectTransform jenerik;
        [SerializeField] private float jenerikSuresi = 20f;

        private Vector3 baslangic;
        private Color gunesinIlkRengi;
        private Vector3 kameraIlkYer;
        private Quaternion kameraIlkDonus;
        private float jenerikBaslangicY;
        private float gecen;
        private bool aktif;
        private Image finalGorseli;
        private Sprite finalSprite;

        private void Awake()
        {
            baslangic = kaplumbaga.position;
            gunesinIlkRengi = gunes.color;
            if (kamera != null) { kameraIlkYer = kamera.position; kameraIlkDonus = kamera.rotation; }
            if (jenerik != null) jenerikBaslangicY = jenerik.anchoredPosition.y;
        }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            gecen = 0f;
            kaplumbaga.position = baslangic;
            aktif = true;

            gunes.color = kumsal.TemizEnerji ? serinRenk : sicakRenk;
            sicakIzler.SetActive(!kumsal.TemizEnerji);
            baslik.text = metinler.Al("final_30_yil");
            kurtarilan.text = $"{metinler.Al("final_kurtarilan")} {kumsal.DenizeUlasanYavru}";
            GorseliKur();

            if (kamera != null) kamera.SetPositionAndRotation(kameraIlkYer, kameraIlkDonus);
            if (yazilar != null) yazilar.alpha = 0f;
            if (jenerik != null)
            {
                jenerik.anchoredPosition = new Vector2(jenerik.anchoredPosition.x, jenerikBaslangicY);
                if (jenerik.TryGetComponent(out Text jenerikYazisi)) jenerikYazisi.text = metinler.Al("jenerik");
                jenerik.gameObject.SetActive(false);
            }
        }

        private void GorseliKur()
        {
            if (yazilar == null) return;
            if (finalGorseli == null)
            {
                var nesne = new GameObject("FinalGorseli", typeof(RectTransform), typeof(Image));
                nesne.layer = yazilar.gameObject.layer;
                nesne.transform.SetParent(yazilar.transform, false);
                nesne.transform.SetAsFirstSibling();
                var rect = (RectTransform)nesne.transform;
                rect.anchorMin = new Vector2(.53f, .22f); rect.anchorMax = new Vector2(.96f, .70f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                finalGorseli = nesne.GetComponent<Image>();
                finalGorseli.preserveAspect = true;
                finalGorseli.raycastTarget = false;
            }
            string kaynak = kumsal.TemizEnerji ? "Final_Temiz" : "Final_Sicak";
            if (OyunAkisi.KorumaAlaniVar) kaynak += "_Koruma";
            Texture2D doku = Resources.Load<Texture2D>(kaynak);
            if (finalSprite != null) Destroy(finalSprite);
            finalSprite = doku != null ? Sprite.Create(doku, new Rect(0, 0, doku.width, doku.height), new Vector2(.5f, .5f)) : null;
            finalGorseli.sprite = finalSprite;
            finalGorseli.enabled = finalSprite != null;
        }

        private void OnDestroy()
        {
            if (finalSprite != null) Destroy(finalSprite);
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            // Sahne kapanırken güneş bizden önce yok edilmiş olabilir.
            if (gunes != null) gunes.color = gunesinIlkRengi;
        }

        private void AdimBasladi(Adim adim) => aktif = adim.Tur == AdimTuru.Final;

        private void Update()
        {
            if (!aktif) return;
            gecen += Time.deltaTime;
            // Kumsal düz değilse kaplumbağa kumun altında kalmaz, yokuşu çıkar.
            Vector3 yer = Vector3.Lerp(baslangic, yuruyusSonu.position, Mathf.Clamp01(gecen / yuruyusSuresi));
            yer.y = Mathf.Max(yer.y, Zemin.Yukseklik(yer, yer.y));
            kaplumbaga.position = yer;

            float oran = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(gecen / yuruyusSuresi));
            if (kamera != null && kameraSon != null)
                kamera.SetPositionAndRotation(Vector3.Lerp(kameraIlkYer, kameraSon.position, oran),
                    Quaternion.Slerp(kameraIlkDonus, kameraSon.rotation, oran));
            // Yazılar yürüyüşün ortasından itibaren belirir.
            if (yazilar != null) yazilar.alpha = Mathf.Clamp01((gecen - yuruyusSuresi * 0.5f) / yaziBelirmeSuresi);

            // Kaplumbağa yürürken tıklama atlanmaz; sonra jenerik akar, tıklayınca ya da jenerik bitince oyun biter.
            if (gecen < yuruyusSuresi) return;
            float jenerikOrani = 1f;
            if (jenerik != null)
            {
                if (!jenerik.gameObject.activeSelf)
                {
                    jenerik.gameObject.SetActive(true);
                    // Jenerik başlarken bir kez: nasıl geçileceği (son kullanılan cihazın tuşuyla).
                    Ipucu.Bildir("bildirim_jenerik_gec", GameInput.TusYazisi("ZiplaDal"));
                }
                jenerikOrani = Mathf.Clamp01((gecen - yuruyusSuresi) / jenerikSuresi);
                // Jenerik başlığın üstünden geçmesin diye yazılar söner.
                if (yazilar != null) yazilar.alpha = Mathf.Min(yazilar.alpha, 1f - Mathf.Clamp01((gecen - yuruyusSuresi) / yaziBelirmeSuresi));
                float yol = jenerik.rect.height + ((RectTransform)jenerik.parent).rect.height;
                jenerik.anchoredPosition = new Vector2(jenerik.anchoredPosition.x, jenerikBaslangicY + yol * jenerikOrani);
            }
            // Fareyle tıklama, Space / A ya da kolun İleri'si (Y) geçer.
            bool gec = GameInput.TiklaBasildi || GameInput.ZiplaDalBasildi || GameInput.IleriBasildi;
            if (!gec && jenerikOrani < 1f) return;
            if (OyunAkisi.AdimBitti()) aktif = false;
        }
    }
}
