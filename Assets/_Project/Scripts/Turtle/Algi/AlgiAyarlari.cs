using System;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın algı yanılgılarının sayıları: tür başına ne zaman kırıldığı, şüphe anlarının sıklığı, sahte ayın görünüşü ve yavrunun içgüdüsü.
    /// Tasarım: docs/kaplumbaga-algisi.md.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Algı Ayarları", fileName = "AlgiAyarlari")]
    public class AlgiAyarlari : ScriptableObject
    {
        /// <summary>Bir yanılgı türünün kuralları.</summary>
        [Serializable]
        public class Profil
        {
            [SerializeField] private AlgiTuru tur;
            [Tooltip("Bu uzaklığa girince yanılgı kalıcı olarak kırılır (metre).")]
            [SerializeField] private float kirilmaYaricapi;
            [Tooltip("Bu uzaklığın içinde yanılgı arada bir 'göz kırpar': gerçek görünüş bir an görünür (metre).")]
            [SerializeField] private float supheYaricapi;
            [Tooltip("Doğrudan bakmak yanılgıyı kırar mı. Lambada kırmaz: yavru içgüdüsünü dikkatle yenemez.")]
            [SerializeField] private bool bakmakKirar;
            [Tooltip("Bu uzaklık içinde doğrudan bakılırsa yanılgı kırılır (metre).")]
            [SerializeField] private float bakmaYaricapi;
            [Tooltip("Bu kadar doğrudan bakınca yanılgı kırılır (saniye).")]
            [SerializeField] private float bakmaSuresi;
            [Tooltip("Kırılma anının süresi: yanılgı söner, gerçek belirir (saniye).")]
            [SerializeField] private float kirilmaSuresi;
            [SerializeField] private SesTuru ses;
            [Tooltip("İlk kırılmada gösterilen ipucunun id'si (docs/metinler.md).")]
            [SerializeField] private string ipucu;

            public Profil(AlgiTuru tur, float kirilma, float suphe, bool bakmakKirar, float bakmaYaricapi, float bakmaSuresi, float kirilmaSuresi, SesTuru ses, string ipucu)
            {
                this.tur = tur;
                kirilmaYaricapi = kirilma;
                supheYaricapi = suphe;
                this.bakmakKirar = bakmakKirar;
                this.bakmaYaricapi = bakmaYaricapi;
                this.bakmaSuresi = bakmaSuresi;
                this.kirilmaSuresi = kirilmaSuresi;
                this.ses = ses;
                this.ipucu = ipucu;
            }

            public AlgiTuru Tur => tur;
            public float KirilmaYaricapi => kirilmaYaricapi;
            public float SupheYaricapi => supheYaricapi;
            public bool BakmakKirar => bakmakKirar;
            public float BakmaYaricapi => bakmaYaricapi;
            public float BakmaSuresi => bakmaSuresi;
            public float KirilmaSuresi => kirilmaSuresi;
            public SesTuru Ses => ses;
            public string Ipucu => ipucu;
        }

        [SerializeField] private Profil[] profiller =
        {
            new Profil(AlgiTuru.PosetDenizanasi, 2.4f, 9f, true, 6f, 1.5f, 0.5f, SesTuru.YanilgiPoset, "ipucu_yanilgi_poset"),
            new Profil(AlgiTuru.LambaAy, 8f, 30f, false, 0f, 0f, 1.4f, SesTuru.YanilgiIsik, "ipucu_yanilgi_isik"),
            new Profil(AlgiTuru.PlastikYosun, 3f, 10f, true, 7f, 1.5f, 0.6f, SesTuru.YanilgiPoset, "ipucu_yanilgi_plastik"),
        };

        [Header("Şüphe (göz kırpma)")]
        [Tooltip("Ekranın ortasından bu açının içi 'doğrudan bakış' sayılır (derece).")]
        [SerializeField] private float bakmaAcisi = 8f;
        [Tooltip("Şüphe alanının dış kenarında iki göz kırpma arası (saniye).")]
        [SerializeField] private float kirpmaAraligiUzak = 3f;
        [Tooltip("Kırılmanın eşiğinde iki göz kırpma arası (saniye).")]
        [SerializeField] private float kirpmaAraligiYakin = 0.6f;
        [Tooltip("Gerçek görünüşün bir an görünme süresi (saniye).")]
        [SerializeField] private float kirpmaSuresi = 0.09f;

        [Header("Sahte ay (lamba)")]
        [Tooltip("Sahte ayın gökteki görünür çapı (derece); gerçek ay ~3,5°. Biraz büyük: daha çekici.")]
        [SerializeField] private float ayAcisalCap = 4.2f;
        [Tooltip("Sahte ayın gözden uzaklığı (metre); gökyüzü küresinin (180 m) içinde.")]
        [SerializeField] private float ayMesafesi = 150f;
        [Tooltip("Sahte ay lambanın başından bu kadar yukarıda görünür (derece).")]
        [SerializeField] private float ayYukselisi = 7f;
        [Tooltip("Sodyum lamba sarısı, gerçek aydan parlak (1'in üstü ışıma taşırır). Gerçek ay soğuk beyazdır: renk farkı bir ipucu.")]
        [ColorUsage(false, true)]
        [SerializeField] private Color ayRengi = new Color(1.5f, 1.2f, 0.7f, 1f);
        [Tooltip("Elektrik titremesinde ayın sönükleşme oranı (0–1).")]
        [SerializeField] private float ayTitremesi = 0.35f;
        [Tooltip("Titreme sıklığı: büyüdükçe daha sık söner.")]
        [SerializeField] private float ayTitremeSikligi = 0.12f;
        [Tooltip("Kırılırken ay bu boya küçülerek lambanın başına düşer (metre).")]
        [SerializeField] private float ampulBoyu = 0.35f;

        [Header("İçgüdü (yavru en parlak aya döner)")]
        [Tooltip("Bakış bu kadar oynatılmayınca içgüdü başlar (saniye).")]
        [SerializeField] private float icguduGecikmesi = 0.8f;
        [Tooltip("İçgüdünün tam güce çıkma süresi (saniye).")]
        [SerializeField] private float icguduYukselmesi = 1.5f;
        [Tooltip("Bakışın en parlak aya dönme hızı (derece/saniye). 0 kapatır.")]
        [SerializeField] private float icguduHizi = 22f;
        [Tooltip("Sahte ayın çekiciliği (gerçek ay 1).")]
        [SerializeField] private float sahteAyCekiciligi = 2f;
        [Tooltip("Arkada kalan ay daha az çeker: bu açıda çekicilik yarıya iner (derece). Önde gerçek ay varken yandaki lamba yine baskın çıkar, arkadaki çıkmaz.")]
        [SerializeField] private float cekicilikYariAcisi = 100f;

        public float BakmaAcisi => bakmaAcisi;
        public float KirpmaAraligiUzak => kirpmaAraligiUzak;
        public float KirpmaAraligiYakin => kirpmaAraligiYakin;
        public float KirpmaSuresi => kirpmaSuresi;
        public float AyAcisalCap => ayAcisalCap;
        public float AyMesafesi => ayMesafesi;
        public float AyYukselisi => ayYukselisi;
        public Color AyRengi => ayRengi;
        public float AyTitremesi => ayTitremesi;
        public float AyTitremeSikligi => ayTitremeSikligi;
        public float AmpulBoyu => ampulBoyu;
        public float IcguduGecikmesi => icguduGecikmesi;
        public float IcguduYukselmesi => icguduYukselmesi;
        public float IcguduHizi => icguduHizi;
        public float SahteAyCekiciligi => sahteAyCekiciligi;
        public float CekicilikYariAcisi => cekicilikYariAcisi;

        /// <summary>Türün profili (yoksa ilki).</summary>
        public Profil ProfilAl(AlgiTuru tur)
        {
            foreach (Profil profil in profiller)
                if (profil.Tur == tur) return profil;
            return profiller[0];
        }
    }
}
