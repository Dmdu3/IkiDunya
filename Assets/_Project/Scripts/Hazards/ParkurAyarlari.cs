using System;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Minigame'lerin (parkurların) sayıları: poşet sürüsü (aşamalar), çöp girdabı (halkalar), çöp seli (dalga döngüsü). His ayarları burada, inspector'da.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Parkur Ayarları", fileName = "ParkurAyarlari")]
    public class ParkurAyarlari : ScriptableObject
    {
        /// <summary>Poşet sürüsünün bir aşaması.</summary>
        [Serializable]
        public class PosetAsamasi
        {
            [Tooltip("Bu aşamada yenecek denizanası; olunca aşamanın sonundaki akıntı duvarı açılır.")]
            [SerializeField] private int hedef = 5;
            [SerializeField] private float dogmaAraligi = 0.6f;
            [Range(0f, 1f)] [SerializeField] private float posetOrani = 0.45f;
            [Tooltip("Sürüyü yandan taşıyan akıntının hızı (m/sn).")]
            [SerializeField] private float akisHizi = 1.4f;
            [Tooltip("0 aydınlık, 1 karanlık: denizanası parlar, poşet görünmez olur.")]
            [Range(0f, 1f)] [SerializeField] private float karanlik;

            public PosetAsamasi(int hedef, float dogmaAraligi, float posetOrani, float akisHizi, float karanlik)
            {
                this.hedef = hedef;
                this.dogmaAraligi = dogmaAraligi;
                this.posetOrani = posetOrani;
                this.akisHizi = akisHizi;
                this.karanlik = karanlik;
            }

            public int Hedef => hedef;
            public float DogmaAraligi => dogmaAraligi;
            public float PosetOrani => posetOrani;
            public float AkisHizi => akisHizi;
            public float Karanlik => karanlik;
        }

        [Header("Poşet sürüsü (Bölüm 1)")]
        [SerializeField] private PosetAsamasi[] posetAsamalari =
        {
            new PosetAsamasi(5, 0.7f, 0.4f, 1.4f, 0f),
            new PosetAsamasi(6, 0.45f, 0.55f, 2.0f, 0f),
            new PosetAsamasi(7, 0.5f, 0.6f, 1.7f, 0.85f),
        };
        [Tooltip("Parkur boyunca saniyede azalan enerji; sadece denizanası doldurur.")]
        [SerializeField] private float posetEnerjiKaybi = 2.5f;
        [Tooltip("Art arda bu kadar denizanası yenince kombo: enerji bonusu.")]
        [SerializeField] private int komboEsigi = 3;
        [SerializeField] private float komboEnerjisi = 12f;
        [Tooltip("Aynı anda en fazla nesne.")]
        [SerializeField] private int posetEnFazla = 50;

        [Header("Çöp girdabı (Bölüm 5)")]
        [Tooltip("İçten dışa halkaların yarıçapı (metre).")]
        [SerializeField] private float[] halkaYaricaplari = { 5f, 10f, 15f };
        [Tooltip("Halkaların dönme hızı (derece/sn); eksi ters yön.")]
        [SerializeField] private float[] halkaHizlari = { 28f, -20f, 14f };
        [Tooltip("Her halkadaki ağır çöp (varil, kasa) sayısı.")]
        [SerializeField] private int[] halkaCopSayilari = { 4, 7, 10 };
        [Tooltip("Halka boşluğunun genişliği (derece).")]
        [SerializeField] private float cikisAcisi = 45f;
        [Tooltip("Girdabın döndürme hızı (m/sn).")]
        [SerializeField] private float girdapDonme = 1.5f;
        [Tooltip("Merkeze hafif çekim (m/sn).")]
        [SerializeField] private float girdapCekim = 0.5f;
        [Tooltip("Ağır çöpün çarptığı mesafe, kaybettirdiği enerji ve savurma (metre).")]
        [SerializeField] private float carpmaMesafesi = 1.3f;
        [SerializeField] private float carpmaHasari = 8f;
        [SerializeField] private float carpmaSavrulma = 2.5f;
        [SerializeField] private float carpmaBekleme = 1f;

        [Header("Çöp seli (Bölüm 4)")]
        [Tooltip("Dalga döngüsü (sn): uyarı (köpük ve uğultu), yükselme, bekleme, çekilme, sakin.")]
        [SerializeField] private float selUyari = 3f;
        [SerializeField] private float selYukselme = 2.5f;
        [SerializeField] private float selBekleme = 2.5f;
        [SerializeField] private float selCekilme = 3f;
        [SerializeField] private float selSakin = 4.5f;
        [Tooltip("İlk dalganın kıyıdan ulaştığı uzaklık ve her dalgada artışı (metre).")]
        [SerializeField] private float selIlkErisim = 14f;
        [SerializeField] private float selErisimArtisi = 4f;
        [SerializeField] private float selEnFazlaErisim = 40f;
        [Tooltip("Açıkta suya yakalanan yavru bu kadar saniyede sürüklenir; su onu karaya iter (m/sn).")]
        [SerializeField] private float selBogulma = 2f;
        [SerializeField] private float selItme = 2f;
        [Tooltip("Parkurun süresi (sn); bitince kumda kalanlar kaybedilir (gün doğumu).")]
        [SerializeField] private float selSuresi = 150f;
        [Tooltip("Yüzen kasanın yavruyu ittiği hız çarpanı.")]
        [SerializeField] private float kasaItme = 1.2f;

        public int PosetAsamaSayisi => posetAsamalari.Length;
        public PosetAsamasi PosetAsama(int i) => posetAsamalari[i];
        public float PosetEnerjiKaybi => posetEnerjiKaybi;
        public int KomboEsigi => komboEsigi;
        public float KomboEnerjisi => komboEnerjisi;
        public int PosetEnFazla => posetEnFazla;

        public int HalkaSayisi => halkaYaricaplari.Length;
        public float HalkaYaricapi(int i) => halkaYaricaplari[i];
        public float HalkaHizi(int i) => halkaHizlari[i];
        public int HalkaCopSayisi(int i) => halkaCopSayilari[i];
        public float CikisAcisi => cikisAcisi;
        public float GirdapDonme => girdapDonme;
        public float GirdapCekim => girdapCekim;
        public float CarpmaMesafesi => carpmaMesafesi;
        public float CarpmaHasari => carpmaHasari;
        public float CarpmaSavrulma => carpmaSavrulma;
        public float CarpmaBekleme => carpmaBekleme;

        public float SelUyari => selUyari;
        public float SelYukselme => selYukselme;
        public float SelBekleme => selBekleme;
        public float SelCekilme => selCekilme;
        public float SelSakin => selSakin;
        public float SelDongusu => selUyari + selYukselme + selBekleme + selCekilme + selSakin;
        public float SelErisim(int dalga) => Mathf.Min(selEnFazlaErisim, selIlkErisim + dalga * selErisimArtisi);
        public float SelBogulma => selBogulma;
        public float SelItme => selItme;
        public float SelSuresi => selSuresi;
        public float KasaItme => kasaItme;
    }
}
