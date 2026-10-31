using System.Collections;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Çöp seli parkuru (Bölüm 4'ün fırtına gecesi, kendi sahnesi): yavrular yuvadan çıkıp denize koşar; deniz dalga dalga kumsala basar.
    /// Önce ufukta köpük ve uğultu gelir (uyarı), sonra su yükselir, bekler, çekilir. Su gelirken kum tepelerinin (SelTepesi) üstündeki yavru güvende;
    /// açıkta yakalanan karaya itilir, uzun kalırsa sürüklenip kaybedilir. Su çekilirken ya da durgunken denize giren kurtulur; dalgalar gittikçe uzağa ulaşır.
    /// Yüzen kasalar suyla gelip yavruları iter. Kıyı z = 0, kara +z (bu objenin yerel ekseni).</summary>
    public class CopSeli : MonoBehaviour
    {
        private enum Evre { Uyari, Yukselme, Bekleme, Cekilme, Sakin }

        [SerializeField] private ParkurAyarlari ayarlar;
        [SerializeField] private OyunAyarlari oyunAyarlari;
        [SerializeField] private YavruAyarlari yavruAyarlari;
        [SerializeField] private KumsalDurumu kumsal;
        [SerializeField] private Suru suru;
        [Tooltip("Yavruların çıktığı yuva (sahnede).")]
        [SerializeField] private Transform yuva;
        [Tooltip("Kumsala basan su (ölçeği kodda değişir; kıyıdan karaya uzanır).")]
        [SerializeField] private Transform su;
        [Tooltip("Dalganın önündeki köpük çizgisi; uyarıda ufukta görünür.")]
        [SerializeField] private Transform kopuk;
        [Tooltip("Suyun deniz tarafında kıyıdan ne kadar içeri uzandığı (metre).")]
        [SerializeField] private float denizPayi = 30f;
        [Tooltip("Uyarıda köpüğün göründüğü uzaklık (kıyıdan denize, metre).")]
        [SerializeField] private float ufuk = 18f;
        [Tooltip("Bu yükseklikten yukarıdaki yavru (kum tepesinin üstü) suya kapılmaz.")]
        [SerializeField] private float tepeYuksekligi = 0.12f;
        [SerializeField] private Transform[] kasalar = new Transform[0];

        private readonly Dictionary<Yavru, float> sudaKalma = new Dictionary<Yavru, float>();
        private float[] kasaZ;
        private bool aktif;
        private float zaman;
        private int sonDalga = -1;
        private Evre evre;
        private float kenar;
        private int kurtulan;

        private void Awake()
        {
            kasaZ = new float[kasalar.Length];
            for (int i = 0; i < kasalar.Length; i++) kasaZ[i] = transform.InverseTransformPoint(kasalar[i].position).z;
        }

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            suru.YavruKurtuldu += Kurtuldu;
            suru.YavruKaybedildi += Kaybedildi;
            suru.Bitti += Bitti;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            if (suru == null) return;
            suru.YavruKurtuldu -= Kurtuldu;
            suru.YavruKaybedildi -= Kaybedildi;
            suru.Bitti -= Bitti;
        }

        private void Start()
        {
            if (!OyunAkisi.Var) Baslat();
        }

        private void AdimBasladi(Adim adim)
        {
            if (adim.Tur == AdimTuru.Parkur && adim.Sira == (int)ParkurTuru.CopSeli) Baslat();
        }

        /// <summary>Hikâyede o gecenin yuvasının yumurtası kadar yavru; menüden ya da yuva yoksa varsayılan sayı.</summary>
        private int YavruSayisi()
        {
            int gece = oyunAyarlari.SelGecesi;
            if (!OyunAkisi.ParkurModunda && gece >= 1 && gece <= kumsal.Yuvalar.Count) return kumsal.Yuvalar[gece - 1].Yumurta;
            return yavruAyarlari.VarsayilanYavruSayisi;
        }

        private void Baslat()
        {
            zaman = 0f;
            sonDalga = -1;
            kurtulan = 0;
            kenar = 0f;
            sudaKalma.Clear();
            int sayi = YavruSayisi();
            suru.Kur(yuva.position, Quaternion.LookRotation(-transform.forward), sayi);
            aktif = sayi > 0;
            Parkur.Baslat(ParkurTuru.CopSeli);
            Ipucu.Goster("ipucu_cop_seli");
            Bilgi();
            if (!aktif) Bitti();
        }

        private void Update()
        {
            if (!aktif) return;
            zaman += Time.deltaTime;
            if (zaman >= ayarlar.SelSuresi)
            {
                // Gün doğdu: kumda kalanlar kaybedilir.
                foreach (Yavru yavru in suru.Yavrular)
                    if (yavru.SuAnki == Yavru.Durum.Suruyor) yavru.Kaybet(yavru.GunDogumuNedeni);
                return;
            }

            Dalga();
            Kasalar();
            Yavrular();
        }

        /// <summary>Dalga döngüsü: uyarı (köpük ufuktan gelir) → yükselme → bekleme → çekilme → sakin. Her dalga daha uzağa ulaşır.</summary>
        private void Dalga()
        {
            int dalga = Mathf.FloorToInt(zaman / ayarlar.SelDongusu);
            float t = zaman - dalga * ayarlar.SelDongusu;
            float erisim = ayarlar.SelErisim(dalga);
            if (dalga != sonDalga)
            {
                sonDalga = dalga;
                Ses.Cal(SesTuru.SelGeliyor);
            }

            float bas = 0f;
            if (t < (bas += ayarlar.SelUyari))
            {
                evre = Evre.Uyari;
                kenar = 0f;
                KopukKoy(Mathf.Lerp(-ufuk, 0f, t / ayarlar.SelUyari), true);
            }
            else if (t < (bas += ayarlar.SelYukselme))
            {
                evre = Evre.Yukselme;
                kenar = Mathf.Lerp(0f, erisim, 1f - (bas - t) / ayarlar.SelYukselme);
                KopukKoy(kenar, true);
            }
            else if (t < (bas += ayarlar.SelBekleme))
            {
                evre = Evre.Bekleme;
                kenar = erisim;
                KopukKoy(kenar, true);
            }
            else if (t < (bas += ayarlar.SelCekilme))
            {
                evre = Evre.Cekilme;
                kenar = Mathf.Lerp(erisim, 0f, 1f - (bas - t) / ayarlar.SelCekilme);
                KopukKoy(kenar, false);
            }
            else
            {
                evre = Evre.Sakin;
                kenar = 0f;
                KopukKoy(0f, false);
            }

            // Su: deniz tarafından kenara kadar.
            float boy = kenar + denizPayi;
            Vector3 olcek = su.localScale;
            su.localScale = new Vector3(olcek.x, olcek.y, boy);
            su.localPosition = new Vector3(su.localPosition.x, su.localPosition.y, kenar - boy / 2f);
        }

        private void KopukKoy(float z, bool gorunur)
        {
            kopuk.gameObject.SetActive(gorunur);
            kopuk.localPosition = new Vector3(kopuk.localPosition.x, kopuk.localPosition.y, z);
        }

        /// <summary>Kasalar su gelince kenarla birlikte karaya sürüklenir, çekilince yerine döner; önündeki yavruyu iter.</summary>
        private void Kasalar()
        {
            for (int i = 0; i < kasalar.Length; i++)
            {
                Vector3 yerel = transform.InverseTransformPoint(kasalar[i].position);
                float yeniZ = Mathf.Max(kasaZ[i], kenar - 1f);
                float itme = yeniZ - yerel.z;
                yerel.z = yeniZ;
                kasalar[i].position = transform.TransformPoint(yerel);
                if (itme <= 0f) continue;
                foreach (Yavru yavru in suru.Yavrular)
                {
                    if (yavru.SuAnki != Yavru.Durum.Suruyor) continue;
                    Vector3 fark = kasalar[i].InverseTransformPoint(yavru.transform.position);
                    if (Mathf.Abs(fark.x) <= 0.6f && Mathf.Abs(fark.z) <= 0.8f)
                        yavru.Savrul(transform.forward * itme * ayarlar.KasaItme);
                }
            }
        }

        private void Yavrular()
        {
            bool denizAcik = evre == Evre.Cekilme || evre == Evre.Sakin;
            foreach (Yavru yavru in suru.Yavrular)
            {
                if (yavru.SuAnki != Yavru.Durum.Suruyor) continue;
                Vector3 yerel = transform.InverseTransformPoint(yavru.transform.position);

                // Denize ulaştı: su çekilirken ya da durgunken kurtulur, dalga gelirken geri itilir.
                if (yerel.z < 0f)
                {
                    if (denizAcik && yavru.EngelDurumu == Yavru.Engel.Yok) yavru.Kurtar();
                    else yavru.Savrul(transform.forward * ayarlar.SelItme * Time.deltaTime);
                    continue;
                }

                bool suda = yerel.z < kenar && yerel.y < tepeYuksekligi;
                if (!suda)
                {
                    sudaKalma.Remove(yavru);
                    continue;
                }
                yavru.Savrul(transform.forward * ayarlar.SelItme * Time.deltaTime);
                sudaKalma.TryGetValue(yavru, out float sure);
                sure += Time.deltaTime;
                sudaKalma[yavru] = sure;
                if (sure >= ayarlar.SelBogulma) yavru.Kaybet(KayipNedeni.Dalga);
            }
        }

        private void Kurtuldu(Yavru yavru)
        {
            kurtulan++;
            if (!OyunAkisi.ParkurModunda) kumsal.DenizeUlasti(1);
            Bilgi();
        }

        private void Kaybedildi(Yavru yavru, KayipNedeni neden)
        {
            sudaKalma.Remove(yavru);
            // Hikâyede kayıp, o gecenin yuvasının yerinde iz bırakır (insan kısmı kanıt olarak okur).
            int gece = oyunAyarlari.SelGecesi;
            if (OyunAkisi.ParkurModunda || gece < 1 || gece > kumsal.Yuvalar.Count) return;
            Yuva y = kumsal.Yuvalar[gece - 1];
            kumsal.KayipIziEkle(new KayipIzi(y.Bolge, y.Konum, neden));
        }

        private void Bitti()
        {
            if (!aktif && zaman > 0f) return;
            aktif = false;
            suru.Durdur();
            if (kurtulan > 0)
            {
                Parkur.Bitir(ParkurTuru.CopSeli);
                return;
            }
            // Kimse kurtulamadı: rekor yok, gece yine de biter.
            Parkur.Birak(ParkurTuru.CopSeli);
            StartCoroutine(Ilerle());
        }

        private IEnumerator Ilerle()
        {
            yield return new WaitForSeconds(2f);
            while (OyunAkisi.Var && !OyunAkisi.MenudeMi && !OyunAkisi.AdimBitti()) yield return null;
        }

        private void Bilgi() => Parkur.Bilgi("parkur_sel_kurtulan", kurtulan, suru.Yavrular.Count);
    }
}
