using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın algı yanılgılarının sayıları: ne kadar uzaktan neyi sandığı, ne zaman gerçeği gördüğü, sahte ayın gökteki yeri.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Algı Ayarları", fileName = "AlgiAyarlari")]
    public class AlgiAyarlari : ScriptableObject
    {
        [Header("Yanılgının ömrü")]
        [Tooltip("Bu uzaklığın içinde nesne yanılgı gibi görünür; daha uzakta gerçek görünüşü de seçilmez (metre).")]
        [SerializeField] private float yanilgiYaricapi = 60f;
        [Tooltip("Bu uzaklığa girince yanılgı kalıcı olarak kırılır: gerçek görünüş ortaya çıkar (metre).")]
        [SerializeField] private float kirilmaYaricapi = 8f;
        [Tooltip("Bu uzaklık içinde doğrudan bakılırsa yanılgı kırılır (metre).")]
        [SerializeField] private float bakmaYaricapi = 20f;
        [Tooltip("Ekranın ortasından bu açı içi 'doğrudan bakış' sayılır (derece).")]
        [SerializeField] private float bakmaAcisi = 8f;
        [Tooltip("Bu süre boyunca doğrudan bakılırsa yanılgı kırılır (saniye).")]
        [SerializeField] private float bakmaSuresi = 1.5f;

        [Header("Sahte ay (sokak lambası)")]
        [Tooltip("Sahte ayın gökteki görünür çapı (derece).")]
        [SerializeField] private float ayAcisalCap = 3f;
        [Tooltip("Sahte ayın gözden uzaklığı (metre); sisin ve kumsalın ardında gök cismi gibi durur.")]
        [SerializeField] private float ayMesafesi = 150f;
        [Tooltip("Sahte ay lambanın başından bu kadar yukarıda görünür (derece); ufka yapışık durmasın.")]
        [SerializeField] private float ayYukselisi = 6f;

        public float YanilgiYaricapi => yanilgiYaricapi;
        public float KirilmaYaricapi => kirilmaYaricapi;
        public float BakmaYaricapi => bakmaYaricapi;
        public float BakmaAcisi => bakmaAcisi;
        public float BakmaSuresi => bakmaSuresi;
        public float AyAcisalCap => ayAcisalCap;
        public float AyMesafesi => ayMesafesi;
        public float AyYukselisi => ayYukselisi;
    }
}
