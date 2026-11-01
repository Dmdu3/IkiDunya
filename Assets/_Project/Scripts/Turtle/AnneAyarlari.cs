using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Annenin karadaki hareket, enerji, kazma ve yuva gecesi ayarları (GDD: Kaplumbağa kısmı ortak temel, Bölüm 2).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Anne Ayarları", fileName = "AnneAyarlari")]
    public class AnneAyarlari : ScriptableObject
    {
        [Header("Karada hareket")]
        [SerializeField] private float yurumeHizi = 1.2f;
        [SerializeField] private float hizliCarpani = 1.8f;
        [SerializeField] private float yerCekimi = 20f;

        [Header("Enerji")]
        [SerializeField] private float enerjiEnFazla = 100f;
        [Tooltip("Karada saniyede azalan enerji (GDD: 1).")]
        [SerializeField] private float karadaEnerjiKaybi = 1f;
        [Tooltip("Shift ile hareket ederken enerji kaybı kaç katına çıkar (GDD: 3).")]
        [SerializeField] private float hizliEnerjiCarpani = 3f;

        [Header("Kazma (A-D sırayla)")]
        [Tooltip("Yuvayı açan doğru vuruş sayısı (GDD: 20).")]
        [SerializeField] private int kazmaVurusu = 20;
        [Tooltip("Yuvayı örten doğru vuruş sayısı (GDD: 10).")]
        [SerializeField] private int ortmeVurusu = 10;
        [Tooltip("Aynı tuşa iki kez basınca geri dökülen vuruş.")]
        [SerializeField] private int yanlisVurusGeri = 1;
        [SerializeField] private float vurusEnerjisi = 0.5f;
        [SerializeField] private float yumurtlamaSuresi = 3f;

        [Header("Yuva")]
        [SerializeField] private int yumurtaSayisi = 12;
        [Tooltip("Kırmızı bantta yaşayan yumurta (GDD: 6).")]
        [SerializeField] private int kirmiziBantYumurta = 6;
        [Tooltip("Yuvanın çıkışına kaç hafta (insan turu) var.")]
        [SerializeField] private int kuluckaHafta = 8;

        [Header("Yuva gecesi")]
        [Tooltip("Her gece deneme hakkı (GDD: 2).")]
        [SerializeField] private int denemeHakki = 2;
        [Tooltip("Denemeler bitince annenin kendiliğinden kazdığı yuvanın deniz kenarından uzaklığı (metre).")]
        [SerializeField] private float kendiligindenYuvaUzakligi = 2f;
        [Tooltip("Son deneme bitince bildirimin okunması için sonuç ekranından önce beklenen süre (sn).")]
        [SerializeField] private float kendiligindenYuvaBekleme = 2.5f;

        [Header("Engeller")]
        [Tooltip("Çöp yığınına takılınca durma süresi (GDD: 2 sn).")]
        [SerializeField] private float copDurmaSuresi = 2f;
        [Tooltip("Çöp yığınına takılınca kaybedilen enerji (GDD: 5).")]
        [SerializeField] private float copEnerjiKaybi = 5f;

        public float YurumeHizi => yurumeHizi;
        public float HizliCarpani => hizliCarpani;
        public float YerCekimi => yerCekimi;
        public float EnerjiEnFazla => enerjiEnFazla;
        public float KaradaEnerjiKaybi => karadaEnerjiKaybi;
        public float HizliEnerjiCarpani => hizliEnerjiCarpani;
        public int KazmaVurusu => kazmaVurusu;
        public int OrtmeVurusu => ortmeVurusu;
        public int YanlisVurusGeri => yanlisVurusGeri;
        public float VurusEnerjisi => vurusEnerjisi;
        public float YumurtlamaSuresi => yumurtlamaSuresi;
        public int YumurtaSayisi => yumurtaSayisi;
        public int KirmiziBantYumurta => kirmiziBantYumurta;
        public int KuluckaHafta => kuluckaHafta;
        public int DenemeHakki => denemeHakki;
        public float KendiligindenYuvaUzakligi => kendiligindenYuvaUzakligi;
        public float KendiligindenYuvaBekleme => kendiligindenYuvaBekleme;
        public float CopDurmaSuresi => copDurmaSuresi;
        public float CopEnerjiKaybi => copEnerjiKaybi;
    }
}
