using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Minigame'lerin (parkurların) sayıları: Bölüm 1 poşet sürüsü, Bölüm 4 çöp seli, Bölüm 5 çöp girdabı.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Parkur Ayarları", fileName = "ParkurAyarlari")]
    public class ParkurAyarlari : ScriptableObject
    {
        [Header("Poşet sürüsü (Bölüm 1)")]
        [Tooltip("Parkuru bitirmek için yenecek denizanası.")]
        [SerializeField] private int posetHedef = 8;
        [Tooltip("Akıntının sürüye kattığı yeni nesne aralığı (sn).")]
        [SerializeField] private float posetDogmaAraligi = 0.5f;
        [Tooltip("Sürüdeki poşet oranı (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float posetOrani = 0.55f;
        [Tooltip("Sürüyü taşıyan akıntının hızı (m/sn).")]
        [SerializeField] private float posetAkisHizi = 1.8f;
        [Tooltip("Art arda bu kadar denizanası yenince kombo: enerji bonusu.")]
        [SerializeField] private int komboEsigi = 3;
        [SerializeField] private float komboEnerjisi = 12f;
        [Tooltip("Alanda aynı anda en fazla nesne.")]
        [SerializeField] private int posetEnFazla = 40;

        [Header("Çöp girdabı (Bölüm 5)")]
        [Tooltip("Girdabın yüzücüyü yakaladığı yarıçap (metre).")]
        [SerializeField] private float girdapCekimYaricapi = 24f;
        [Tooltip("Çöp halkasının yarıçapı; bunun dışına çıkan kurtulur.")]
        [SerializeField] private float girdapHalkaYaricapi = 9f;
        [Tooltip("Merkeze çekme hızı (m/sn); çıkış boşluğunda yok.")]
        [SerializeField] private float girdapCekim = 2.8f;
        [Tooltip("Dönme yönünde sürükleme hızı (m/sn).")]
        [SerializeField] private float girdapDonme = 2.2f;
        [Tooltip("Çöp halkasının dönme hızı (derece/sn).")]
        [SerializeField] private float halkaDonmeHizi = 20f;
        [Tooltip("Çıkış boşluğunun genişliği (derece).")]
        [SerializeField] private float cikisAcisi = 40f;

        [Header("Çöp seli (Bölüm 4)")]
        [Tooltip("Selin geldiği kaçış gecesi (hikâyede).")]
        [SerializeField] private int selGecesi = 2;
        [Tooltip("Bir çukurun dolup boşalma süresi (sn).")]
        [SerializeField] private float selPeriyodu = 7f;
        [Tooltip("Periyodun ne kadarında çukur su altında (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float selDoluOrani = 0.5f;
        [Tooltip("Su basmış çukurdaki yavru bu kadar saniye içinde kalırsa sürüklenip kaybedilir.")]
        [SerializeField] private float selBogulma = 2.5f;
        [Tooltip("Suyun yavruyu karaya iten hızı (m/sn).")]
        [SerializeField] private float selItme = 1.5f;
        [Tooltip("Yüzen kasaların hızı (m/sn) ve iterken uygulanan savrulma (metre).")]
        [SerializeField] private float kasaHizi = 1.2f;
        [SerializeField] private float kasaItme = 0.8f;

        public int PosetHedef => posetHedef;
        public float PosetDogmaAraligi => posetDogmaAraligi;
        public float PosetOrani => posetOrani;
        public float PosetAkisHizi => posetAkisHizi;
        public int KomboEsigi => komboEsigi;
        public float KomboEnerjisi => komboEnerjisi;
        public int PosetEnFazla => posetEnFazla;
        public float GirdapCekimYaricapi => girdapCekimYaricapi;
        public float GirdapHalkaYaricapi => girdapHalkaYaricapi;
        public float GirdapCekim => girdapCekim;
        public float GirdapDonme => girdapDonme;
        public float HalkaDonmeHizi => halkaDonmeHizi;
        public float CikisAcisi => cikisAcisi;
        public int SelGecesi => selGecesi;
        public float SelPeriyodu => selPeriyodu;
        public float SelDoluOrani => selDoluOrani;
        public float SelBogulma => selBogulma;
        public float SelItme => selItme;
        public float KasaHizi => kasaHizi;
        public float KasaItme => kasaItme;
    }
}
