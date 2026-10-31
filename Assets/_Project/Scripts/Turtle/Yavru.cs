using System;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Tek bir yavru: yürür, zıplar; tehlikede enerji kaybeder, çöpe takılır, ışıkta yönünü kaybeder; avcıya yakalanabilir, denize girince kurtulur.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Enerji))]
    public class Yavru : MonoBehaviour, IAvlanabilir
    {
        public enum Durum { Suruyor, Kurtuldu, Kayip }
        /// <summary>Öncünün yardımını bekleyen durumlar (E basılı).</summary>
        public enum Engel { Yok, Takili, Ters, YonKaybetti }

        private CharacterController kontrol;
        private Enerji enerji;
        private YavruAyarlari ayarlar;
        private float dikeyHiz;
        private float isiktaKalma;
        private readonly List<HazardZone> tehlikeler = new List<HazardZone>();
        private readonly List<IsikKaynagi> isiklar = new List<IsikKaynagi>();
        private int siperSayisi;

        public Durum SuAnki { get; private set; }
        public Engel EngelDurumu { get; private set; }
        /// <summary>Oyuncunun sürdüğü yavru. O da takılır ve ters döner (E basılı tutup kendini kurtarır), ama yönünü kaybetmez.</summary>
        public bool Oncu { get; set; }
        /// <summary>Bu dalga vururken bir kez geri savruldu mu; dalga çekilince sıfırlanır.</summary>
        public bool BuDalgadaSavruldu { get; set; }

        public Vector3 Konum => transform.position;
        public bool Yakalanabilir => SuAnki == Durum.Suruyor;
        public bool Havada => !kontrol.isGrounded;
        public bool Gizli => siperSayisi > 0;

        /// <summary>Öncünün E'yi ne kadar basılı tutması gerektiği.</summary>
        public float YardimSuresi => EngelDurumu == Engel.Takili ? ayarlar.AgKurtarmaSuresi
            : EngelDurumu == Engel.Ters ? ayarlar.TersKurtarmaSuresi
            : ayarlar.GeriCagirmaSuresi;

        /// <summary>Gün doğduğunda kumsalda kalırsa kaybedilme nedeni.</summary>
        public KayipNedeni GunDogumuNedeni => EngelDurumu == Engel.Takili ? KayipNedeni.Ag
            : EngelDurumu == Engel.Ters ? KayipNedeni.Cop
            : EngelDurumu == Engel.YonKaybetti ? KayipNedeni.Isik
            : KayipNedeni.SicakKum;

        /// <summary>Açık ışıkların toplam çekimi (yatay hız).</summary>
        public Vector3 IsikCekimi
        {
            get
            {
                Vector3 toplam = Vector3.zero;
                foreach (IsikKaynagi isik in isiklar) toplam += isik.Cekim(transform.position);
                return toplam;
            }
        }

        /// <summary>Kaybedildiğinde; nedeniyle.</summary>
        public event Action<Yavru, KayipNedeni> Kaybedildi;
        public event Action<Yavru> Kurtuldu;

        private void Awake()
        {
            kontrol = GetComponent<CharacterController>();
            enerji = GetComponent<Enerji>();
        }

        public void Kur(YavruAyarlari yeniAyarlar)
        {
            ayarlar = yeniAyarlar;
            enerji.Doldur(ayarlar.EnerjiEnFazla);
            SuAnki = Durum.Suruyor;
            EngelDurumu = Engel.Yok;
        }

        /// <summary>Verilen yatay hızla bir kare yürür (yer çekimi dahil).</summary>
        public void Yuru(Vector3 yatayHiz)
        {
            if (SuAnki != Durum.Suruyor) return;
            if (kontrol.isGrounded && dikeyHiz < 0f) dikeyHiz = -1f;
            dikeyHiz -= ayarlar.YerCekimi * Time.deltaTime;
            kontrol.Move((yatayHiz + Vector3.up * dikeyHiz) * Time.deltaTime);
        }

        public void Zipla()
        {
            if (SuAnki == Durum.Suruyor && kontrol.isGrounded && EngelDurumu == Engel.Yok) dikeyHiz = ayarlar.ZiplamaHizi;
        }

        public void Savrul(Vector3 yer)
        {
            if (SuAnki == Durum.Suruyor) kontrol.Move(yer);
        }

        /// <summary>Öncü yardımı tamamladı: takılan kurtulur, ters dönen çevrilir, yönünü kaybeden sürüye döner.</summary>
        public void YardimEt()
        {
            Ses.Cal(SesTuru.Kurtarma, transform.position);
            if (EngelDurumu == Engel.Ters) Cevir(false);
            EngelDurumu = Engel.Yok;
            isiktaKalma = 0f;
        }

        public void Yakala(KayipNedeni neden) => Kaybet(neden);

        public void Kaybet(KayipNedeni neden)
        {
            if (SuAnki != Durum.Suruyor) return;
            SuAnki = Durum.Kayip;
            Ses.Cal(SesTuru.YavruKaybedildi, transform.position);
            gameObject.SetActive(false);
            Kaybedildi?.Invoke(this, neden);
        }

        public void Kurtar()
        {
            if (SuAnki != Durum.Suruyor) return;
            SuAnki = Durum.Kurtuldu;
            Ses.Cal(SesTuru.YavruKurtuldu);
            gameObject.SetActive(false);
            Kurtuldu?.Invoke(this);
        }

        private void Update()
        {
            if (SuAnki != Durum.Suruyor) return;

            foreach (HazardZone bolge in tehlikeler)
                if (bolge.Tur == HazardZone.Turu.SicakLeke) enerji.Harca(bolge.SaniyeHasari * Time.deltaTime);
            if (enerji.BittiMi)
            {
                Kaybet(KayipNedeni.SicakKum);
                return;
            }

            // GDD: ışığın içinde uzun kalan yavru yönünü kaybeder.
            bool isikta = IsikCekimi.sqrMagnitude > 0f;
            isiktaKalma = isikta ? isiktaKalma + Time.deltaTime : 0f;
            if (!Oncu && EngelDurumu == Engel.Yok && isiktaKalma >= ayarlar.YonKaybetmeSuresi) EngelDurumu = Engel.YonKaybetti;
        }

        private void OnTriggerEnter(Collider diger)
        {
            if (diger.TryGetComponent(out HazardZone bolge) && !tehlikeler.Contains(bolge)) tehlikeler.Add(bolge);
            if (diger.TryGetComponent(out IsikKaynagi isik) && !isiklar.Contains(isik)) isiklar.Add(isik);
            if (diger.TryGetComponent(out Cover _)) siperSayisi++;
            if (diger.TryGetComponent(out TrashTrap tuzak)) Takil(tuzak.Tur);
        }

        private void OnTriggerExit(Collider diger)
        {
            if (diger.TryGetComponent(out HazardZone bolge)) tehlikeler.Remove(bolge);
            if (diger.TryGetComponent(out IsikKaynagi isik)) isiklar.Remove(isik);
            if (diger.TryGetComponent(out Cover _)) siperSayisi = Mathf.Max(0, siperSayisi - 1);
        }

        private void Takil(TrashTrap.Turu tur)
        {
            if (EngelDurumu != Engel.Yok || tur == TrashTrap.Turu.CopYigini) return;
            Ses.Cal(SesTuru.Takilma, transform.position);
            Ipucu.Goster("ipucu_takili");
            if (tur == TrashTrap.Turu.TekerlekIzi)
            {
                EngelDurumu = Engel.Ters;
                Cevir(true);
            }
            else EngelDurumu = Engel.Takili;
        }

        /// <summary>Kabuğu görsel olarak ters çevirir.</summary>
        private void Cevir(bool ters)
        {
            if (transform.childCount == 0) return;
            transform.GetChild(0).localRotation = ters ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
        }
    }
}
