using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Annenin karadaki hareket ve enerji ayarları (GDD: Kaplumbağa kısmı ortak temel, Bölüm 2).</summary>
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

        public float YurumeHizi => yurumeHizi;
        public float HizliCarpani => hizliCarpani;
        public float YerCekimi => yerCekimi;
        public float EnerjiEnFazla => enerjiEnFazla;
        public float KaradaEnerjiKaybi => karadaEnerjiKaybi;
        public float HizliEnerjiCarpani => hizliEnerjiCarpani;
    }
}
