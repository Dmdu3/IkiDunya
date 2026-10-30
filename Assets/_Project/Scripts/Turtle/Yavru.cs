using System;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Tek bir yavru: yürür, tehlike bölgesinde enerji kaybeder, enerjisi biterse kaybedilir, denize girince kurtulur.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Enerji))]
    public class Yavru : MonoBehaviour
    {
        public enum Durum { Suruyor, Kurtuldu, Kayip }

        private CharacterController kontrol;
        private Enerji enerji;
        private YavruAyarlari ayarlar;
        private float dikeyHiz;
        private readonly List<HazardZone> icindeOldugu = new List<HazardZone>();

        public Durum SuAnki { get; private set; }
        /// <summary>Bu dalga vururken bir kez geri savruldu mu; dalga çekilince sıfırlanır.</summary>
        public bool BuDalgadaSavruldu { get; set; }

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
        }

        /// <summary>Verilen yatay hızla bir kare yürür (yer çekimi dahil).</summary>
        public void Yuru(Vector3 yatayHiz)
        {
            if (SuAnki != Durum.Suruyor) return;
            if (kontrol.isGrounded && dikeyHiz < 0f) dikeyHiz = -1f;
            dikeyHiz -= ayarlar.YerCekimi * Time.deltaTime;
            kontrol.Move((yatayHiz + Vector3.up * dikeyHiz) * Time.deltaTime);
        }

        public void Savrul(Vector3 yer)
        {
            if (SuAnki == Durum.Suruyor) kontrol.Move(yer);
        }

        public void Kaybet(KayipNedeni neden)
        {
            if (SuAnki != Durum.Suruyor) return;
            SuAnki = Durum.Kayip;
            gameObject.SetActive(false);
            Kaybedildi?.Invoke(this, neden);
        }

        public void Kurtar()
        {
            if (SuAnki != Durum.Suruyor) return;
            SuAnki = Durum.Kurtuldu;
            gameObject.SetActive(false);
            Kurtuldu?.Invoke(this);
        }

        private void Update()
        {
            if (SuAnki != Durum.Suruyor) return;
            foreach (HazardZone bolge in icindeOldugu)
                if (bolge.Tur == HazardZone.Turu.SicakLeke) enerji.Harca(bolge.SaniyeHasari * Time.deltaTime);
            if (enerji.BittiMi) Kaybet(KayipNedeni.SicakKum);
        }

        private void OnTriggerEnter(Collider diger)
        {
            if (diger.TryGetComponent(out HazardZone bolge) && !icindeOldugu.Contains(bolge)) icindeOldugu.Add(bolge);
        }

        private void OnTriggerExit(Collider diger)
        {
            if (diger.TryGetComponent(out HazardZone bolge)) icindeOldugu.Remove(bolge);
        }
    }
}
