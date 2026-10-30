using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Geçici insan turu: Toprak'ın yönetim ekranı gelene kadar kumsal üstten görünür, tıklayınca bir hafta geçer ve tur biter.</summary>
    public class GeciciInsanTuru : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;

        private bool aktif;
        private int baslangicKaresi;

        private void OnEnable() => OyunAkisi.AdimBasladi += AdimBasladi;
        private void OnDisable() => OyunAkisi.AdimBasladi -= AdimBasladi;

        private void AdimBasladi(Adim adim)
        {
            aktif = adim.Tur == AdimTuru.InsanTuru;
            // Önceki adımı bitiren tıklama bu turu da bitirmesin.
            baslangicKaresi = Time.frameCount;
        }

        private void Update()
        {
            if (!aktif || Time.frameCount == baslangicKaresi || !GameInput.TiklaBasildi) return;
            aktif = false;
            kumsal.BirHaftaGecti();
            OyunAkisi.AdimBitti();
        }
    }
}
