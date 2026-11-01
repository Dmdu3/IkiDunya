using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Ekran efektlerinin sayıları: sarsıntı genliği ve süresi, flaşın sönmesi, tehlike eşiği, kenar kararması, kalp atışı.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Geri Bildirim Ayarları", fileName = "GeribildirimAyarlari")]
    public class GeribildirimAyarlari : ScriptableObject
    {
        [Tooltip("Tam güçte kameranın kayma (metre) ve dönme (derece) genliği, sarsıntının süresi (sn).")]
        [SerializeField] private float sarsintiKaymasi = 0.06f;
        [SerializeField] private float sarsintiDonmesi = 2.5f;
        [SerializeField] private float sarsintiSuresi = 0.35f;
        [Tooltip("Flaşın sönme süresi (sn).")]
        [SerializeField] private float flasSuresi = 0.4f;
        [Tooltip("Enerji bu oranın altına düşünce tehlike başlar (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float tehlikeEsigi = 0.25f;
        [Tooltip("Tam tehlikede kenar kararmasının en fazla koyuluğu (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float kenarKoyulugu = 0.7f;
        [Tooltip("Kalp atışları arası (sn): tehlike başlarken ve tam tehlikede.")]
        [SerializeField] private float kalpYavas = 1.1f;
        [SerializeField] private float kalpHizli = 0.5f;

        public float SarsintiKaymasi => sarsintiKaymasi;
        public float SarsintiDonmesi => sarsintiDonmesi;
        public float SarsintiSuresi => sarsintiSuresi;
        public float FlasSuresi => flasSuresi;
        public float TehlikeEsigi => tehlikeEsigi;
        public float KenarKoyulugu => kenarKoyulugu;
        public float KalpYavas => kalpYavas;
        public float KalpHizli => kalpHizli;
    }
}
