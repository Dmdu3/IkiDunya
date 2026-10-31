using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Denizde yüzme ayarları; anne (Bölüm 1) ve yavru (Bölüm 5) için ayrı asset (GDD: Bölüm 1 ve 5).</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Deniz Ayarları", fileName = "DenizAyarlari")]
    public class DenizAyarlari : ScriptableObject
    {
        [Header("Yüzme")]
        [SerializeField] private float yuzmeHizi = 3f;
        [SerializeField] private float hizliCarpani = 1.8f;
        [Tooltip("Space basılıyken yüzeye çıkma hızı.")]
        [SerializeField] private float cikisHizi = 2f;
        [Tooltip("Yüzeydeyken Space ile dalışın ilk hızı ve süresi.")]
        [SerializeField] private float dalisHizi = 4f;
        [SerializeField] private float dalisSuresi = 0.5f;
        [Tooltip("Açıksa Space sadece daldırır (yavru); kapalıysa basılı tutunca yüzeye çıkarır (anne).")]
        [SerializeField] private bool spaceSadeceDalar;
        [Tooltip("Yüzeyde sayılmak için su yüzeyine en fazla uzaklık (metre).")]
        [SerializeField] private float yuzeyPayi = 0.4f;

        [Header("Enerji")]
        [SerializeField] private float enerjiEnFazla = 100f;
        [Tooltip("Yüzerken saniyede azalan enerji.")]
        [SerializeField] private float hareketEnerjisi = 0.3f;
        [Tooltip("Hareket etmese de saniyede azalan enerji (yavrunun çılgın yüzüşü, GDD: 0,5).")]
        [SerializeField] private float surekliEnerji;
        [SerializeField] private float hizliEnerjiCarpani = 3f;
        [Tooltip("Akıntıya karşı yüzerken enerji kaç katı (GDD: 2).")]
        [SerializeField] private float akintiyaKarsiCarpani = 2f;

        [Header("Nefes (0 ise nefes yok)")]
        [Tooltip("Su altında nefes süresi (GDD: 60 sn).")]
        [SerializeField] private float nefesSuresi = 60f;
        [Tooltip("Yüzeyde nefesin boştan dolma süresi.")]
        [SerializeField] private float nefesDolmaSuresi = 3f;
        [Tooltip("Nefes bitince hız çarpanı.")]
        [SerializeField] private float nefessizHizCarpani = 0.4f;

        [Header("Ağ, misina, besin")]
        [Tooltip("Hayalet ağdan kurtulmak için E basışı (GDD: anne 8, yavru 10).")]
        [SerializeField] private int agBasisi = 8;
        [Tooltip("Misinadan kurtulmak için E basışı (GDD: 6).")]
        [SerializeField] private int misinaBasisi = 6;
        [Tooltip("Ağdan kurtulunca yavaş kalma süresi (GDD: 5 sn).")]
        [SerializeField] private float agSonrasiYavas = 5f;
        [Tooltip("Poşet yiyince yavaş kalma süresi (GDD: 10 sn).")]
        [SerializeField] private float posetYavas = 10f;
        [Tooltip("Parlayan plastiğe değince yavaş kalma süresi (GDD: 5 sn).")]
        [SerializeField] private float plastikYavas = 5f;
        [Tooltip("Yavaşken hız çarpanı (GDD: yarı hız).")]
        [SerializeField] private float yavasCarpani = 0.5f;
        [Tooltip("E ile yiyebilmek için besine en fazla uzaklık (metre).")]
        [SerializeField] private float yemeMesafesi = 1.5f;

        [Header("Final (Bölüm 5)")]
        [SerializeField] private float finalSuresi = 4f;
        [SerializeField] private float finalUzakligi = 10f;

        public float YuzmeHizi => yuzmeHizi;
        public float HizliCarpani => hizliCarpani;
        public float CikisHizi => cikisHizi;
        public float DalisHizi => dalisHizi;
        public float DalisSuresi => dalisSuresi;
        public bool SpaceSadeceDalar => spaceSadeceDalar;
        public float YuzeyPayi => yuzeyPayi;
        public float EnerjiEnFazla => enerjiEnFazla;
        public float HareketEnerjisi => hareketEnerjisi;
        public float SurekliEnerji => surekliEnerji;
        public float HizliEnerjiCarpani => hizliEnerjiCarpani;
        public float AkintiyaKarsiCarpani => akintiyaKarsiCarpani;
        public float NefesSuresi => nefesSuresi;
        public float NefesDolmaSuresi => nefesDolmaSuresi;
        public float NefessizHizCarpani => nefessizHizCarpani;
        public int AgBasisi => agBasisi;
        public int MisinaBasisi => misinaBasisi;
        public float AgSonrasiYavas => agSonrasiYavas;
        public float PosetYavas => posetYavas;
        public float PlastikYavas => plastikYavas;
        public float YavasCarpani => yavasCarpani;
        public float YemeMesafesi => yemeMesafesi;
        public float FinalSuresi => finalSuresi;
        public float FinalUzakligi => finalUzakligi;
    }
}
