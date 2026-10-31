using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>İskeletsiz modele kodla animasyon: kök hareket ettikçe görünüş yürür gibi sallanır, yüzer gibi dalgalanır, kuş gibi süzülür ya da balık gibi kıvrılır.
    /// Anne kazarken geriye yatar, her vuruşta sarsılır. Görünüşün kendi yerini ve dönüşünü değiştirir; kökün oynanışına dokunmaz.</summary>
    public class Salinim : MonoBehaviour
    {
        [SerializeField] private AnimasyonAyarlari ayarlar;
        [SerializeField] private SalinimTuru tur;
        [Tooltip("Sallanan görünüş (model ya da gri gövde); kökün çocuğu.")]
        [SerializeField] private Transform gorunus;
        [Tooltip("İsteğe bağlı: annenin kazması (kazarken eğilme ve sarsıntı).")]
        [SerializeField] private AnneKazma kazma;

        private Vector3 ilkYer;
        private Quaternion ilkDonus;
        private Vector3 oncekiKonum;
        private float faz;
        private float sarsinti;
        private int oncekiVurus;
        private float egim;

        private void Awake()
        {
            if (gorunus == null) return;
            ilkYer = gorunus.localPosition;
            ilkDonus = gorunus.localRotation;
            oncekiKonum = transform.position;
            // Aynı türden canlılar aynı anda adım atmasın.
            faz = Random.value * 10f;
        }

        private void LateUpdate()
        {
            if (gorunus == null || Time.deltaTime <= 0f) return;
            AnimasyonAyarlari.Profil p = ayarlar.ProfilAl(tur);

            Vector3 fark = transform.position - oncekiKonum;
            oncekiKonum = transform.position;
            if (tur != SalinimTuru.Deniz && tur != SalinimTuru.Balik) fark.y = 0f;
            float oran = Mathf.Clamp01(fark.magnitude / Time.deltaTime / Mathf.Max(0.01f, p.TamHiz));
            float guc = Mathf.Max(oran, p.Durgun);
            faz += Time.deltaTime * p.Siklik * Mathf.PI * 2f * Mathf.Max(0.25f, oran);

            float s = Mathf.Sin(faz);
            float y;
            switch (tur)
            {
                case SalinimTuru.Kara:
                    // Yüzgeçlerle kumda itilir gibi: her adımda kalkar, iki yana yatar.
                    y = Mathf.Abs(s) * p.Ziplama * guc;
                    break;
                default:
                    y = s * p.Ziplama * guc;
                    break;
            }
            float yana = s * p.YanaYatma * guc;
            float one = Mathf.Sin(faz * 2f) * p.OneYatma * guc;
            float kivrim = s * p.Kivrilma * guc;

            Kazma();
            gorunus.localPosition = ilkYer + Vector3.up * y;
            gorunus.localRotation = ilkDonus * Quaternion.Euler(one - egim + sarsinti, kivrim, yana);
        }

        /// <summary>Kazarken geriye yatar; her vuruşta kısa bir sarsıntı, sonra söner.</summary>
        private void Kazma()
        {
            bool kaziyor = kazma != null && (kazma.SuAnki == AnneKazma.Asama.Kaziyor || kazma.SuAnki == AnneKazma.Asama.Ortuyor);
            egim = Mathf.MoveTowards(egim, kaziyor ? ayarlar.KazmaEgimi : 0f, ayarlar.KazmaEgimi * 4f * Time.deltaTime);
            if (kazma != null && kazma.Vurus != oncekiVurus)
            {
                if (kaziyor) sarsinti = ayarlar.KazmaSarsintisi;
                oncekiVurus = kazma.Vurus;
            }
            sarsinti = Mathf.MoveTowards(sarsinti, 0f, ayarlar.SarsintiSonmesi * ayarlar.KazmaSarsintisi * Time.deltaTime);
        }
    }
}
