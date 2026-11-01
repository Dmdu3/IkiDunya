using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Arayüzün görsel dili ("Belgesel" teması): yazı tipleri ve renkler. Kurulum kodu ve arayüz bileşenleri renkleri buradan alır.
    /// Asset: ScriptableObjects/Settings/ArayuzTemasi.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Arayüz teması", fileName = "ArayuzTemasi")]
    public class ArayuzTemasi : ScriptableObject
    {
        [Header("Yazı tipleri (Art/UI/Fonts)")]
        [Tooltip("Başlıklar: serif (Lora).")]
        [SerializeField] private Font baslikYazisi;
        [Tooltip("Gövde, düğme, sayı: sans (Source Sans 3 Regular).")]
        [SerializeField] private Font govdeYazisi;
        [Tooltip("Gövdenin kalını: değerler, kart adları, ana düğme (Source Sans 3 SemiBold).")]
        [SerializeField] private Font kalinYazi;

        [Header("Yazı")]
        [SerializeField] private Color yazi = new Color32(242, 234, 220, 255);
        [Tooltip("İkincil yazı: etiket, seçili olmayan menü seçeneği.")]
        [SerializeField] private Color solukYazi = new Color32(242, 234, 220, 150);
        [Tooltip("Vurgu: seçili seçenek, başlık çizgisi, ana değer.")]
        [SerializeField] private Color vurgu = new Color32(226, 170, 110, 255);
        [SerializeField] private Color olumlu = new Color32(140, 205, 150, 255);
        [SerializeField] private Color olumsuz = new Color32(232, 125, 112, 255);

        [Header("Yüzeyler")]
        [SerializeField] private Color panel = new Color32(16, 20, 28, 242);
        [Tooltip("Panel içindeki kutular (kart, kaynak, bölge).")]
        [SerializeField] private Color panelIci = new Color32(34, 40, 52, 245);
        [Tooltip("İnce çizgi ve çerçeve.")]
        [SerializeField] private Color cizgi = new Color32(242, 234, 220, 46);
        [Tooltip("Tam ekran menülerin arkasını, oyunun üstünde karartan perde.")]
        [SerializeField] private Color perde = new Color32(10, 12, 16, 238);

        [Header("Ufuk zemini (menü)")]
        [SerializeField] private Color gece = new Color32(14, 16, 22, 255);
        [SerializeField] private Color alacakaranlik = new Color32(70, 52, 78, 255);
        [SerializeField] private Color ufuk = new Color32(196, 110, 72, 255);
        [SerializeField] private Color deniz = new Color32(20, 34, 44, 255);

        public Font BaslikYazisi => baslikYazisi != null ? baslikYazisi : Varsayilan;
        public Font GovdeYazisi => govdeYazisi != null ? govdeYazisi : Varsayilan;
        public Font KalinYazi => kalinYazi != null ? kalinYazi : GovdeYazisi;
        public Color Yazi => yazi;
        public Color SolukYazi => solukYazi;
        public Color Vurgu => vurgu;
        public Color Olumlu => olumlu;
        public Color Olumsuz => olumsuz;
        public Color Panel => panel;
        public Color PanelIci => panelIci;
        public Color Cizgi => cizgi;
        public Color Perde => perde;
        public Color Gece => gece;
        public Color Alacakaranlik => alacakaranlik;
        public Color Ufuk => ufuk;
        public Color Deniz => deniz;

        /// <summary>Zengin metin için renk kodu (#RRGGBBAA).</summary>
        public static string Kod(Color renk) => "#" + ColorUtility.ToHtmlStringRGBA(renk);

        private static Font Varsayilan => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>Sahnede ya da ayarlarda bağlı olan tema (yüklenince kendini kaydeder).</summary>
        public static ArayuzTemasi Yuklu { get; private set; }

        private void OnEnable() => Yuklu = this;
    }
}
