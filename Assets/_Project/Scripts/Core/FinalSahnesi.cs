using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.Core
{
    /// <summary>"30 yıl sonra": büyümüş dişi kaplumbağa aynı kumsala yumurtlamaya döner. Temiz Enerji oynandıysa kumsal serin, oynanmadıysa sıcak ve kızgın. Sonunda tıklayınca oyun biter.</summary>
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

        private Vector3 baslangic;
        private Color gunesinIlkRengi;
        private float gecen;
        private bool aktif;

        private void Awake()
        {
            baslangic = kaplumbaga.position;
            gunesinIlkRengi = gunes.color;
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
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            gunes.color = gunesinIlkRengi;
        }

        private void AdimBasladi(Adim adim) => aktif = adim.Tur == AdimTuru.Final;

        private void Update()
        {
            if (!aktif) return;
            gecen += Time.deltaTime;
            kaplumbaga.position = Vector3.Lerp(baslangic, yuruyusSonu.position, Mathf.Clamp01(gecen / yuruyusSuresi));

            // Kaplumbağa yürürken tıklama atlanmaz; sonra tıklayınca oyun biter.
            if (gecen < yuruyusSuresi || !GameInput.TiklaBasildi) return;
            if (OyunAkisi.AdimBitti()) aktif = false;
        }
    }
}
