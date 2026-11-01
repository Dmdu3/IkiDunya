using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Ağ kenarlarını ve misinayı yavaşça parıldatır; oyuncu onları uzaktan seçebilir (GDD: kenarları hafif parlar, ince parlak çizgi).</summary>
    public class Parilti : MonoBehaviour
    {
        private static readonly int RenkId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private DenizTehlikeAyarlari ayarlar;
        [SerializeField] private Renderer[] parcalar;
        [SerializeField] private Color renk = Color.white;
        [Tooltip("Çok malzemeli modelde sadece bu sıradaki malzeme parlar (ör. modelin parıltı malzemesi); -1: bütün parça.")]
        [SerializeField] private int malzemeSirasi = -1;

        private MaterialPropertyBlock blok;
        private float kayma;

        private void Awake()
        {
            blok = new MaterialPropertyBlock();
            // Her ağ aynı anda parlamasın.
            kayma = Random.value * 10f;
        }

        private void Update()
        {
            float t = 0.5f + 0.5f * Mathf.Sin((Time.time + kayma) * ayarlar.ParildamaHizi * 2f * Mathf.PI);
            Color simdi = renk * Mathf.Lerp(ayarlar.ParildamaEnAz, 1f, t);
            simdi.a = renk.a;
            blok.SetColor(RenkId, simdi);
            foreach (Renderer parca in parcalar)
            {
                if (parca == null) continue;
                if (malzemeSirasi < 0) parca.SetPropertyBlock(blok);
                else parca.SetPropertyBlock(blok, malzemeSirasi);
            }
        }
    }
}
