using IkiDunya.Core;
using UnityEngine;
using UnityEngine.UI;

namespace IkiDunya.UI.Night
{
    /// <summary>Parkur sürerken sağ üstte süre, rekor ve parkurun sayacı; bitince sonuç (yeni rekor), ardından akış ilerler (hikâyede sıradaki adım, menüden oynanıyorsa menü).</summary>
    public class ParkurGostergesi : MonoBehaviour
    {
        [SerializeField] private OyunAyarlari ayarlar;
        [SerializeField] private GameObject sureKutusu;
        [SerializeField] private Text sureYazisi;
        [SerializeField] private Text bilgiYazisi;
        [SerializeField] private GameObject sonucKutusu;
        [SerializeField] private Text sonucYazisi;
        [Tooltip("Sonucun ekranda kaldığı süre (sn).")]
        [SerializeField] private float sonucSuresi = 3.5f;

        private float sonucKalan;
        private bool menuyeDon;

        private MetinTablosu Tablo => ayarlar != null && ayarlar.Metinler != null ? ayarlar.Metinler : MetinTablosu.Yuklu;
        private string Al(string id) => Tablo != null ? Tablo.Al(id) : $"[{id}]";

        private void OnEnable()
        {
            Parkur.Basladi += Basladi;
            Parkur.Bitti += Bitti;
            Parkur.Kaldi += Kaldi;
            Parkur.BilgiDegisti += BilgiDegisti;
            sureKutusu.SetActive(false);
            sonucKutusu.SetActive(false);
        }

        private void OnDisable()
        {
            Parkur.Basladi -= Basladi;
            Parkur.Bitti -= Bitti;
            Parkur.Kaldi -= Kaldi;
            Parkur.BilgiDegisti -= BilgiDegisti;
        }

        private void Basladi(ParkurTuru tur)
        {
            sureKutusu.SetActive(true);
            sonucKutusu.SetActive(false);
            bilgiYazisi.text = "";
        }

        private void Kaldi(ParkurTuru tur) => sureKutusu.SetActive(false);

        private void BilgiDegisti(string id, int sayi, int hedef) => bilgiYazisi.text = hedef > 0 ? $"{Al(id)} {sayi}/{hedef}" : Al(id);

        private void Bitti(ParkurTuru tur, float sure, float rekor, bool yeni)
        {
            sureKutusu.SetActive(false);
            sonucYazisi.text = $"{Al(Parkur.MetinId(tur))}\n{Al("parkur_bitti")} {Parkur.Bicim(sure)}"
                               + (yeni ? $"\n{Al("parkur_yeni_rekor")}" : $"\n{Al("parkur_rekor")} {Parkur.Bicim(rekor)}");
            sonucKutusu.SetActive(true);
            sonucKalan = sonucSuresi;
            // Parkur bir akış adımıysa (hikâyede ya da menüden) sonuçtan sonra akış ilerler.
            menuyeDon = !OyunAkisi.MenudeMi && OyunAkisi.SuAnkiAdim.Tur == AdimTuru.Parkur;
            Ses.Cal(SesTuru.ParkurBitti);
        }

        private void Update()
        {
            if (Parkur.Aktif.HasValue)
            {
                ParkurTuru tur = Parkur.Aktif.Value;
                float rekor = Parkur.Rekor(tur);
                sureYazisi.text = $"{Al("parkur_sure")} {Parkur.Bicim(Parkur.Gecen)}" + (rekor > 0f ? $"   {Al("parkur_rekor")} {Parkur.Bicim(rekor)}" : "");
            }

            // Duraklatma açıkken sonuç beklemesi de durur; menü açıkken akış ilerlemesin.
            if (sonucKalan <= 0f || Duraklatma.Acik) return;
            sonucKalan -= Time.unscaledDeltaTime;
            if (sonucKalan > 0f) return;
            sonucKutusu.SetActive(false);
            // Parkur adımı biter: hikâyede sıradaki adım, menüden oynanıyorsa menü (geçiş sürüyorsa sonraki karede tekrar denenir).
            if (menuyeDon && !OyunAkisi.MenudeMi && OyunAkisi.SuAnkiAdim.Tur == AdimTuru.Parkur && !OyunAkisi.AdimBitti()) sonucKalan = 0.01f;
            else menuyeDon = false;
        }
    }
}
