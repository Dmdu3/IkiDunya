using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın algı yanılgılarının sayıları: ne kadar uzakta neyi sandığı, ne zaman kırıldığı ve yanılgı görünüşlerinin ölçüsü.
    /// Sayılar burada durur; nesneler yalnızca kendi çarpanlarını ve koşullarını taşır.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Algı Ayarları", fileName = "AlgiAyarlari")]
    public class AlgiAyarlari : ScriptableObject
    {
        [Header("Yanılgının ömrü")]
        [Tooltip("Bu uzaklığa kadar nesne yanılgı gibi görünür (metre).")]
        [SerializeField] private float yanilgiYaricapi = 60f;
        [Tooltip("Bu uzaklığa girince yanılgı kalıcı olarak kırılır: gerçek görünüş ortaya çıkar (metre).")]
        [SerializeField] private float kirilmaYaricapi = 8f;
        [Tooltip("Bu uzaklık içinde doğrudan bakılırsa yanılgı kırılır (metre).")]
        [SerializeField] private float bakmaYaricapi = 20f;
        [Tooltip("Yanılgının görünmesi için gereken en düşük algı gücü (0–1).")]
        [SerializeField] private float esik = 0.3f;
        [Tooltip("Göründükten sonra ne kadar zayıflayınca kaybolacağı; titremeyi önler.")]
        [SerializeField] private float histerezis = 0.1f;
        [Tooltip("Yanılgı görünüşünün belirme/kaybolma hızı (saniyede 0–1).")]
        [SerializeField] private float gecisHizi = 1.5f;

        [Header("Bakış")]
        [Tooltip("Bu açının içine doğrudan bakmak yanılgıyı zayıflatır (derece).")]
        [SerializeField] private float merkezBakisAcisi = 10f;
        [Tooltip("Bu açının dışında (göz ucuyla) yanılgı en güçlüdür (derece).")]
        [SerializeField] private float kenarBakisAcisi = 55f;
        [Tooltip("Tam merkeze bakarken kalan algı gücü; 1'e yaklaştıkça doğrudan bakış yanılgıyı daha az zayıflatır.")]
        [SerializeField] private float merkezGucu = 0.8f;
        [Tooltip("Bu süre boyunca doğrudan bakılırsa yanılgı kırılır (saniye).")]
        [SerializeField] private float bakmaSuresi = 1.2f;

        [Header("Ay (sokak lambası)")]
        [Tooltip("Ayın gökyüzündeki görünür çapı (derece); sahnedeki gerçek ayla aynı görünsün.")]
        [SerializeField] private float ayAcisalCap = 3.5f;
        [Tooltip("Ayın gözden ne kadar ileride durduğu (metre); gökyüzü küresinin önünde kalmalı.")]
        [SerializeField] private float ayMesafesi = 120f;
        [SerializeField] private Color ayRengi = new Color(1f, 0.97f, 0.86f);
        [Tooltip("Ayın parlaklığı; 1'in üstünde parlar (bloom varsa taşar).")]
        [SerializeField] private float ayParlakligi = 1.6f;

        [Header("Denizanası (poşet)")]
        [Tooltip("Yanılgı denizanasının boyu ( metre).")]
        [SerializeField] private float denizanasiOlcegi = 0.55f;
        [Tooltip("Yanılgı denizanasının nabız hızı (saniyedeki vuruş).")]
        [SerializeField] private float denizanasiNabizHizi = 0.8f;
        [SerializeField] private Color denizanasiRengi = new Color(1f, 0.62f, 0.82f, 0.55f);

        public float YanilgiYaricapi => yanilgiYaricapi;
        public float KirilmaYaricapi => kirilmaYaricapi;
        public float BakmaYaricapi => bakmaYaricapi;
        public float Esik => esik;
        public float Histerezis => histerezis;
        public float GecisHizi => gecisHizi;
        public float MerkezBakisAcisi => merkezBakisAcisi;
        public float KenarBakisAcisi => kenarBakisAcisi;
        public float MerkezGucu => merkezGucu;
        public float BakmaSuresi => bakmaSuresi;
        public float AyAcisalCap => ayAcisalCap;
        public float AyMesafesi => ayMesafesi;
        public Color AyRengi => ayRengi;
        public float AyParlakligi => ayParlakligi;
        public float DenizanasiOlcegi => denizanasiOlcegi;
        public float DenizanasiNabizHizi => denizanasiNabizHizi;
        public Color DenizanasiRengi => denizanasiRengi;
    }
}
