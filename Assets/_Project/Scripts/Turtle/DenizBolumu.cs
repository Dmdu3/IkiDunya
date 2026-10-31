using System.Collections;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Bir deniz bölümü (Bölüm 1 dönüş, Bölüm 5 ilk yüzüş): kontrol noktaları, enerji biterse ya da avcı yakalarsa son noktadan devam, sonda (Bölüm 5'te kamera uzaklaşarak) akışa geçiş.
    /// Parkur kapısına gelince bölümün ilk parçası biter; parkurdan sonra adımın Sira'sı 1'se bölüm devam noktasından sürer.</summary>
    public class DenizBolumu : MonoBehaviour
    {
        [SerializeField] private AdimTuru adim = AdimTuru.Donus;
        [SerializeField] private DenizAyarlari ayarlar;
        [SerializeField] private DenizHareketi yuzucu;
        [Tooltip("Bölümün başladığı yer.")]
        [SerializeField] private Transform baslangic;
        [Tooltip("Sonda kamera uzaklaşsın mı (Bölüm 5 finali).")]
        [SerializeField] private bool finalKamerasi;
        [Tooltip("Parkurdan sonra bölümün devam ettiği yer (adımın Sira'sı 1).")]
        [SerializeField] private Transform devamBaslangici;

        private Transform sonNokta;
        private bool bitti;
        private int parca;

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            KontrolNoktasi.Ulasildi += Ulasildi;
            yuzucu.Enerji.Bitti += YenidenBasla;
            yuzucu.Yakalandi += YenidenBasla;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            KontrolNoktasi.Ulasildi -= Ulasildi;
            // Sahne kapanırken yüzücü bizden önce yok edilmiş olabilir.
            if (yuzucu == null) return;
            yuzucu.Enerji.Bitti -= YenidenBasla;
            yuzucu.Yakalandi -= YenidenBasla;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Basla(0);
        }

        private void AdimBasladi(Adim yeni)
        {
            if (yeni.Tur == adim) Basla(yeni.Sira);
        }

        private void Basla(int yeniParca)
        {
            bitti = false;
            parca = yeniParca;
            sonNokta = parca > 0 && devamBaslangici != null ? devamBaslangici : baslangic;
            yuzucu.Baslat(sonNokta.position, sonNokta.rotation);
            if (parca > 0) return;
            // Bölüm 1 annenin denizi: besin ve nefes; Bölüm 5 yavrunun: dalga.
            if (adim == AdimTuru.Donus)
            {
                Ipucu.Goster("ipucu_poset");
                Ipucu.Goster("ipucu_nefes");
            }
            else Ipucu.Goster("ipucu_dalga");
        }

        /// <summary>GDD: enerji biterse ya da avcı yakalarsa son kontrol noktasından devam edilir.</summary>
        private void YenidenBasla()
        {
            if (bitti) return;
            Debug.Log("DenizBolumu: son kontrol noktasından devam.");
            yuzucu.Baslat(sonNokta.position, sonNokta.rotation);
        }

        private void Ulasildi(KontrolNoktasi nokta)
        {
            if (bitti) return;
            // Parkur kapısı: ilk parçada bölüm burada biter, parkur sahnesi gelir; devam parçasında sıradan kontrol noktası.
            if (nokta.Kapi && parca == 0)
            {
                bitti = true;
                yuzucu.Kilitli = true;
                Tamam();
                return;
            }
            if (!nokta.Son)
            {
                sonNokta = nokta.transform;
                Ses.Cal(SesTuru.KontrolNoktasi);
                yuzucu.Enerji.Doldur(ayarlar.EnerjiEnFazla);
                return;
            }

            bitti = true;
            yuzucu.Kilitli = true;
            Debug.Log("DenizBolumu: bölüm bitti.");
            if (finalKamerasi) StartCoroutine(Final());
            else Tamam();
        }

        /// <summary>Yavru yosunun arasına saklanır, kamera yavaşça uzaklaşır.</summary>
        private IEnumerator Final()
        {
            Transform goz = yuzucu.Goz;
            goz.SetParent(null, true);
            Vector3 ilk = goz.position;
            Vector3 son = yuzucu.transform.position - yuzucu.transform.forward * ayarlar.FinalUzakligi + Vector3.up * ayarlar.FinalUzakligi * 0.6f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / ayarlar.FinalSuresi)
            {
                goz.position = Vector3.Lerp(ilk, son, Mathf.SmoothStep(0f, 1f, t));
                goz.rotation = Quaternion.LookRotation(yuzucu.transform.position - goz.position);
                yield return null;
            }
            Tamam();
        }

        private void Tamam()
        {
            if (OyunAkisi.Var) OyunAkisi.AdimBitti();
        }
    }
}
