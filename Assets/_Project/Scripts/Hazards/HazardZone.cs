using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Tehlike bölgesi (Hazard layer, Is Trigger): sıcak kum lekesi. Gün doğdukça büyür, gölgelikli bölgede küçük kalır.</summary>
    [RequireComponent(typeof(Collider))]
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] private KacisAyarlari ayarlar;
        [SerializeField] private GunDogumu gunDogumu;
        [SerializeField] private KumSicakligi kum;
        [SerializeField] private KumsalDurumu kumsal;

        private Vector3 ilkOlcek;
        private int bolge;

        /// <summary>İçindeki yavrunun saniyede kaybettiği enerji.</summary>
        public float SaniyeHasari => ayarlar.SicakLekeHasari;

        private void Awake()
        {
            ilkOlcek = transform.localScale;
            if (kum != null) bolge = kum.Bolge(transform.position);
        }

        private void Update()
        {
            float carpan = Mathf.Lerp(ayarlar.LekeBaslangicOlcegi, ayarlar.LekeSonOlcegi, gunDogumu.Oran);
            if (bolge < kumsal.BolgeSayisi && kumsal.BolgeAl(bolge).Var(BolgeYapisi.Golgelik))
                carpan *= ayarlar.GolgelikLekeCarpani;
            transform.localScale = new Vector3(ilkOlcek.x * carpan, ilkOlcek.y, ilkOlcek.z * carpan);
        }
    }
}
