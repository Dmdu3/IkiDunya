using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Tehlike bölgesi (Hazard layer, Is Trigger). Sıcak kum lekesi gün doğdukça büyür, gölgelikli bölgede küçük kalır; denizde sıcak su lekesi sabittir.</summary>
    [RequireComponent(typeof(Collider))]
    public class HazardZone : MonoBehaviour
    {
        public enum Turu { SicakLeke, SicakSu }

        [SerializeField] private Turu tur = Turu.SicakLeke;
        [SerializeField] private KacisAyarlari ayarlar;
        [SerializeField] private GunDogumu gunDogumu;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Sadece denizdeki sıcak su lekesi için.")]
        [SerializeField] private DenizTehlikeAyarlari denizAyarlari;

        private Vector3 ilkOlcek;
        private int bolge;

        public Turu Tur => tur;
        /// <summary>İçindeki yavrunun saniyede kaybettiği enerji.</summary>
        public float SaniyeHasari => tur == Turu.SicakSu ? denizAyarlari.SicakSuHasari : ayarlar.SicakLekeHasari;

        private void Awake()
        {
            ilkOlcek = transform.localScale;
            if (kum != null) bolge = kum.Bolge(transform.position);
        }

        private void Update()
        {
            if (tur != Turu.SicakLeke) return;
            float carpan = Mathf.Lerp(ayarlar.LekeBaslangicOlcegi, ayarlar.LekeSonOlcegi, gunDogumu.Oran);
            if (bolge < kumsal.BolgeSayisi && kumsal.BolgeAl(bolge).Var(BolgeYapisi.Golgelik))
                carpan *= ayarlar.GolgelikLekeCarpani;
            transform.localScale = new Vector3(ilkOlcek.x * carpan, ilkOlcek.y, ilkOlcek.z * carpan);
        }
    }
}
