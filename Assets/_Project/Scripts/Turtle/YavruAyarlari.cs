using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Yavruların ve sürünün ayarları; kumdan çıkış dahil (GDD: Bölüm 4).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Yavru Ayarları", fileName = "YavruAyarlari")]
    public class YavruAyarlari : ScriptableObject
    {
        [Header("Hareket")]
        [SerializeField] private float yurumeHizi = 1.6f;
        [Tooltip("İzleyen yavruların öncüye yetişmek için hız çarpanı.")]
        [SerializeField] private float takipHizCarpani = 1.2f;
        [SerializeField] private float yerCekimi = 20f;
        [SerializeField] private float enerjiEnFazla = 100f;

        [Header("Sürü")]
        [Tooltip("İzleyenlerin öncünün ne kadar arkasında durduğu (metre).")]
        [SerializeField] private float arkaMesafe = 0.8f;
        [Tooltip("Toplu sürünün yarıçapı (metre).")]
        [SerializeField] private float topluYaricap = 0.6f;
        [Tooltip("Shift basılıyken dağınık sürünün yarıçapı (metre).")]
        [SerializeField] private float daginikYaricap = 2.5f;

        [Tooltip("Space ile zıplamanın ilk hızı (m/sn); küçük engellerin üstünden geçmeye yeter.")]
        [SerializeField] private float ziplamaHizi = 3f;

        [Header("Takılma ve kurtarma (E basılı)")]
        [Tooltip("Öncünün kurtarabilmek için yavruya en fazla uzaklığı (metre).")]
        [SerializeField] private float kurtarmaMesafesi = 1.2f;
        [Tooltip("Ağa ya da halkaya takılan yavruyu kurtarma süresi (GDD: 2 sn).")]
        [SerializeField] private float agKurtarmaSuresi = 2f;
        [Tooltip("Tekerlek izinde ters dönen yavruyu çevirme süresi (GDD: 1,5 sn).")]
        [SerializeField] private float tersKurtarmaSuresi = 1.5f;
        [Tooltip("Yönünü kaybeden yavruyu sürüye geri çağırma süresi.")]
        [SerializeField] private float geriCagirmaSuresi = 0.3f;

        [Header("Işık ve tilki")]
        [Tooltip("Işığın içinde bu kadar kalan yavru yönünü kaybeder (GDD: 5 sn).")]
        [SerializeField] private float yonKaybetmeSuresi = 5f;
        [Tooltip("Yönünü kaybeden yavru karaya bu hız çarpanıyla yürür.")]
        [SerializeField] private float yonKaybedenHizCarpani = 0.5f;
        [Tooltip("Tilki görünce sürünün panikle dağınık kaldığı süre (sn).")]
        [SerializeField] private float panikSuresi = 3f;

        [Header("Cinsiyet (yuvanın sıcaklık bandına göre dişi oranı)")]
        [Tooltip("Mavi, Yeşil, Turuncu, Kırmızı. GDD: çoğu erkek, dengeli, çoğu dişi, hepsi dişi.")]
        [SerializeField] private float[] disiOranlari = { 0.2f, 0.5f, 0.8f, 1f };

        [Header("Kumdan çıkış")]
        [Tooltip("Yavruların kumun kaç kat altında başladığı (GDD: 5).")]
        [SerializeField] private int katSayisi = 5;
        [Tooltip("Yüzeyin sıcaktan serine inme süresi (sn).")]
        [SerializeField] private float sogumaSuresi = 20f;
        [Tooltip("Yüzey sıcaklığı (0 serin – 1 sıcak) bunun üstündeyken çıkan yavruların bir kısmı ölür.")]
        [Range(0f, 1f)] [SerializeField] private float olumculSicaklik = 0.4f;
        [Tooltip("Kum altında beklerken saniyede azalan enerji.")]
        [SerializeField] private float kumAltiEnerjiKaybi = 2.5f;
        [Tooltip("Yuva bilgisi yoksa (deneme sahnesi) kaç yavru çıkar.")]
        [SerializeField] private int varsayilanYavruSayisi = 12;

        public float YurumeHizi => yurumeHizi;
        public float TakipHizCarpani => takipHizCarpani;
        public float YerCekimi => yerCekimi;
        public float EnerjiEnFazla => enerjiEnFazla;
        public float ArkaMesafe => arkaMesafe;
        public float TopluYaricap => topluYaricap;
        public float DaginikYaricap => daginikYaricap;
        public int KatSayisi => katSayisi;
        public float SogumaSuresi => sogumaSuresi;
        public float OlumculSicaklik => olumculSicaklik;
        public float KumAltiEnerjiKaybi => kumAltiEnerjiKaybi;
        public int VarsayilanYavruSayisi => varsayilanYavruSayisi;
        public float ZiplamaHizi => ziplamaHizi;
        public float KurtarmaMesafesi => kurtarmaMesafesi;
        public float AgKurtarmaSuresi => agKurtarmaSuresi;
        public float TersKurtarmaSuresi => tersKurtarmaSuresi;
        public float GeriCagirmaSuresi => geriCagirmaSuresi;
        public float YonKaybetmeSuresi => yonKaybetmeSuresi;
        public float YonKaybedenHizCarpani => yonKaybedenHizCarpani;
        public float PanikSuresi => panikSuresi;

        public float DisiOrani(SicaklikBandi bant) => disiOranlari[(int)bant];
    }
}
