using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın bir nesneyi başka bir şey sanması: poşet denizanası, lamba ay, plastik yosun gibi görünür.
    /// Yaklaştıkça yanılgı arada bir "göz kırpar" (gerçek bir an görünür); çok yaklaşınca, (türüne göre) uzun bakınca ya da ışık sönünce kırılır.
    /// Kırılma kısa bir andır (denizanası içine çöker, ay lambaya düşer) ve kalıcıdır. Görünüşleri kurulum komutu hazırlar. Tasarım: docs/kaplumbaga-algisi.md.</summary>
    public class AlgiNesnesi : MonoBehaviour
    {
        private enum Durum { Yanilgi, Kiriliyor, Kirildi }

        private static readonly List<AlgiNesnesi> kayitlar = new List<AlgiNesnesi>();
        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => kayitlar.Clear();

        [SerializeField] private AlgiTuru tur;
        [Tooltip("Gerçek görünüş; yanılgı açıkken renderer'ları kapanır (lambada direk).")]
        [SerializeField] private Transform gercekGorunus;
        [Tooltip("Yanılgı görünüşü (kapalı başlar).")]
        [SerializeField] private GameObject yanilgiGorunus;
        [Tooltip("Uzaklığın ve bakışın ölçüldüğü nokta (lambanın başı). Boşsa bu obje.")]
        [SerializeField] private Transform odak;
        [Tooltip("Doluysa Işık Yasağı yanılgıyı söndürür (lamba yanmıyorsa ay da yoktur).")]
        [SerializeField] private KumsalDurumu isikYasagiKumsali;

        private readonly List<Renderer> gercekRendererlari = new List<Renderer>();
        private Vector3 gercekOlcek;
        private Vector3 yanilgiOlcek;
        private Renderer ayGorunus;
        private MaterialPropertyBlock blok;
        private Durum durum;
        private bool gorunur;
        private bool sonuyor;
        private float kirilma;
        private Vector3 dusmeYeri;
        private Vector3 dusmeOlcegi;
        private float bakilanSure;
        private float sonrakiKirpma;
        private float kirpmaKalan;
        private float titremeTohumu;

        /// <summary>Kaplumbağanın gözünde şu an yanılgı mı görünüyor.</summary>
        public bool YanilgiGorunuyor => durum == Durum.Yanilgi && gorunur;
        /// <summary>Yanılgı kalıcı olarak kırıldı mı.</summary>
        public bool Kirildi => durum == Durum.Kirildi;
        public AlgiTuru Tur => tur;
        /// <summary>Yanılgı görünüşünün dünyadaki yeri (sahte ay gökte).</summary>
        public Vector3 YanilgiKonumu => yanilgiGorunus.transform.position;

        /// <summary>Açık (sahnede etkin) yanılgılar; içgüdü sahte ayları buradan bulur.</summary>
        public static IReadOnlyList<AlgiNesnesi> Kayitlar => kayitlar;

        private void Awake()
        {
            if (odak == null) odak = transform;
            if (gercekGorunus != null)
            {
                gercekOlcek = gercekGorunus.localScale;
                // Sadece açık renderer'lar yönetilir: modeli olan nesnede gizli yedek küre açılmasın.
                foreach (Renderer r in gercekGorunus.GetComponentsInChildren<Renderer>(true))
                    if (r.enabled) gercekRendererlari.Add(r);
            }
            if (yanilgiGorunus != null)
            {
                yanilgiOlcek = yanilgiGorunus.transform.localScale;
                if (tur == AlgiTuru.LambaAy) ayGorunus = yanilgiGorunus.GetComponent<Renderer>();
            }
            blok = new MaterialPropertyBlock();
            titremeTohumu = Random.value * 100f;
            sonrakiKirpma = Random.Range(0.5f, 2f);
        }

        private void OnEnable() => kayitlar.Add(this);

        private void OnDisable()
        {
            kayitlar.Remove(this);
            if (durum == Durum.Kiriliyor) KirilmayiBitir();
            else if (durum == Durum.Yanilgi) Goster(false);
        }

        /// <summary>Kayıtlı bütün yanılgıları gözün o anki yerine ve bakışına göre günceller.</summary>
        public static void HepsiniGuncelle(Transform goz, AlgiAyarlari ayarlar)
        {
            for (int i = kayitlar.Count - 1; i >= 0; i--) kayitlar[i].Guncelle(goz, ayarlar);
        }

        private void Guncelle(Transform goz, AlgiAyarlari ayarlar)
        {
            if (yanilgiGorunus == null || durum == Durum.Kirildi) return;
            AlgiAyarlari.Profil profil = ayarlar.ProfilAl(tur);
            if (durum == Durum.Kiriliyor)
            {
                KirilmaAdimi(ayarlar, profil);
                return;
            }

            // Işık sönükse sahte ay yoktur: gece başında yasak varsa hiç görünmez, gece içinde sönerse ay erir.
            if (isikYasagiKumsali != null && isikYasagiKumsali.IsikYasagi)
            {
                Kir(profil, false);
                return;
            }
            if (tur == AlgiTuru.LambaAy && !OrtamIsigi.GeceMi)
            {
                Goster(false);
                return;
            }

            Vector3 fark = odak.position - goz.position;
            float mesafe = fark.magnitude;
            if (mesafe <= profil.KirilmaYaricapi)
            {
                Kir(profil, true);
                return;
            }

            float aci = Vector3.Angle(goz.forward, fark);
            bool bakiyor = aci <= ayarlar.BakmaAcisi;
            if (profil.BakmakKirar)
            {
                bool yakindan = bakiyor && mesafe <= profil.BakmaYaricapi;
                bakilanSure = yakindan ? bakilanSure + Time.deltaTime : Mathf.Max(0f, bakilanSure - Time.deltaTime * 0.5f);
                if (bakilanSure >= profil.BakmaSuresi)
                {
                    Kir(profil, true);
                    return;
                }
            }

            Goster(true);

            // Şüphe: yaklaştıkça gerçek görünüş daha sık bir an görünür; doğrudan bakmak kırpmaları hızlandırır.
            if (mesafe < profil.SupheYaricapi)
            {
                sonrakiKirpma -= Time.deltaTime * (bakiyor ? 2f : 1f);
                if (sonrakiKirpma <= 0f)
                {
                    float oran = Mathf.InverseLerp(profil.KirilmaYaricapi, profil.SupheYaricapi, mesafe);
                    kirpmaKalan = ayarlar.KirpmaSuresi;
                    sonrakiKirpma = Mathf.Lerp(ayarlar.KirpmaAraligiYakin, ayarlar.KirpmaAraligiUzak, oran) * Random.Range(0.7f, 1.3f);
                }
            }
            kirpmaKalan -= Time.deltaTime;
            bool kirpiyor = kirpmaKalan > 0f;

            if (tur == AlgiTuru.LambaAy)
            {
                GokteYerlestir(goz, ayarlar);
                AyRengi(ayarlar, Titreme(ayarlar) * (kirpiyor ? 0.3f : 1f));
            }
            else
            {
                yanilgiGorunus.SetActive(!kirpiyor);
                GercekGorunsun(kirpiyor);
            }
        }

        /// <summary>Yanılgıyı kırar. Görünmüyorsa (gündüz, ışık hiç yanmadı) sessizce biter; göründüyse kısa bir an oynar ve oyuncuya hissettirir.</summary>
        private void Kir(AlgiAyarlari.Profil profil, bool gercegiGordu)
        {
            if (!gorunur)
            {
                durum = Durum.Kirildi;
                return;
            }
            durum = Durum.Kiriliyor;
            sonuyor = !gercegiGordu;
            kirilma = 0f;
            dusmeYeri = yanilgiGorunus.transform.position;
            dusmeOlcegi = yanilgiGorunus.transform.localScale;
            yanilgiGorunus.SetActive(true);
            if (!gercegiGordu) return;

            Ses.Cal(profil.Ses, odak.position);
            Geribildirim.Uyanis();
            if (!string.IsNullOrEmpty(profil.Ipucu)) Ipucu.Goster(profil.Ipucu);
        }

        private void KirilmaAdimi(AlgiAyarlari ayarlar, AlgiAyarlari.Profil profil)
        {
            kirilma = Mathf.Min(1f, kirilma + Time.deltaTime / Mathf.Max(0.01f, profil.KirilmaSuresi));
            Transform yanilgi = yanilgiGorunus.transform;

            if (sonuyor)
            {
                // Lamba söndü: ay olduğu yerde erir.
                if (ayGorunus != null) AyRengi(ayarlar, 1f - kirilma);
                else yanilgi.localScale = yanilgiOlcek * (1f - kirilma);
            }
            else if (tur == AlgiTuru.LambaAy)
            {
                // Ay gökten lambanın başına düşer ve ampul boyuna küçülür; düşüşün sonunda direk görünür.
                float e = kirilma * kirilma;
                yanilgi.position = Vector3.Lerp(dusmeYeri, odak.position, e);
                yanilgi.localScale = Vector3.Lerp(dusmeOlcegi, Vector3.one * YerelBoy(ayarlar.AmpulBoyu), e);
                AyRengi(ayarlar, 1f);
                GercekGorunsun(kirilma >= 0.75f);
            }
            else
            {
                // Sahte görünüş içine çöker (önce boyu), gerçek görünüş küçükten hafif taşarak belirir.
                float s = Mathf.SmoothStep(0f, 1f, kirilma);
                yanilgi.localScale = Vector3.Scale(yanilgiOlcek, new Vector3(1f - 0.7f * s, 1f - s, 1f - 0.7f * s));
                GercekGorunsun(true);
                if (gercekGorunus != null) gercekGorunus.localScale = gercekOlcek * Mathf.LerpUnclamped(0.5f, 1f, GeriTasma(kirilma));
            }

            if (kirilma >= 1f) KirilmayiBitir();
        }

        private void KirilmayiBitir()
        {
            durum = Durum.Kirildi;
            gorunur = false;
            yanilgiGorunus.SetActive(false);
            yanilgiGorunus.transform.localScale = yanilgiOlcek;
            if (gercekGorunus != null) gercekGorunus.localScale = gercekOlcek;
            GercekGorunsun(true);
        }

        private void Goster(bool acik)
        {
            if (gorunur == acik) return;
            gorunur = acik;
            kirpmaKalan = 0f;
            if (yanilgiGorunus != null) yanilgiGorunus.SetActive(acik);
            GercekGorunsun(!acik);
        }

        private void GercekGorunsun(bool acik)
        {
            foreach (Renderer r in gercekRendererlari)
                if (r != null) r.enabled = acik;
        }

        /// <summary>Sahte ay: lambanın yönünde, biraz yukarıda, uzakta ve hep aynı görünür çapta; gerçek ay gibi gökte durur.</summary>
        private void GokteYerlestir(Transform goz, AlgiAyarlari ayarlar)
        {
            Vector3 yon = odak.position - goz.position;
            Vector3 yatay = new Vector3(yon.x, 0f, yon.z);
            if (yatay.sqrMagnitude < 0.0001f) return;
            float yukseklik = (Mathf.Atan2(yon.y, yatay.magnitude) * Mathf.Rad2Deg + ayarlar.AyYukselisi) * Mathf.Deg2Rad;
            Vector3 gok = yatay.normalized * Mathf.Cos(yukseklik) + Vector3.up * Mathf.Sin(yukseklik);
            Transform ay = yanilgiGorunus.transform;
            ay.position = goz.position + gok * ayarlar.AyMesafesi;
            ay.localScale = Vector3.one * YerelBoy(2f * ayarlar.AyMesafesi * Mathf.Tan(ayarlar.AyAcisalCap * 0.5f * Mathf.Deg2Rad));
        }

        /// <summary>Dünya boyunu ebeveynin ölçeğine göre yerel ölçeğe çevirir.</summary>
        private float YerelBoy(float dunyaBoyu)
        {
            Transform ebeveyn = yanilgiGorunus.transform.parent;
            return dunyaBoyu / Mathf.Max(0.0001f, ebeveyn != null ? ebeveyn.lossyScale.x : 1f);
        }

        /// <summary>Elektrik titremesi: ara sıra kısa sönükleşmeler (gerçek ay titremez).</summary>
        private float Titreme(AlgiAyarlari ayarlar)
        {
            float n = Mathf.PerlinNoise(Time.time * 9f, titremeTohumu);
            float esik = 0.25f + ayarlar.AyTitremeSikligi;
            return 1f - ayarlar.AyTitremesi * Mathf.Clamp01((esik - n) * 8f);
        }

        private void AyRengi(AlgiAyarlari ayarlar, float gorunurluk)
        {
            if (ayGorunus == null) return;
            Color renk = ayarlar.AyRengi;
            renk.a = Mathf.Clamp01(gorunurluk);
            ayGorunus.GetPropertyBlock(blok);
            blok.SetColor(RenkId, renk);
            ayGorunus.SetPropertyBlock(blok);
        }

        /// <summary>0→1 giderken hedefi biraz aşıp geri gelen eğri (beliren nesne "oturur").</summary>
        private static float GeriTasma(float t)
        {
            const float c = 1.7f;
            float u = t - 1f;
            return 1f + (c + 1f) * u * u * u + c * u * u;
        }
    }
}
