using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Annenin karada birinci şahıs hareketi: W/S ileri-geri, fare ile döner (A/D kazmaya ayrılmış); enerji saniye saniye azalır.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Enerji))]
    public class AnneKara : MonoBehaviour
    {
        [SerializeField] private AnneAyarlari ayarlar;

        private CharacterController kontrol;
        private Enerji enerji;
        private float dikeyHiz;
        private float takiliKalan;

        /// <summary>Kazarken ya da yumurtlarken true; anne yerinden kıpırdamaz.</summary>
        public bool Kilitli { get; set; }
        public bool Yerde => kontrol.isGrounded;
        // Başka objelerin OnEnable'ı bu Awake'ten önce çalışabilir; bu yüzden gerekince bulunur.
        public Enerji Enerji => enerji != null ? enerji : enerji = GetComponent<Enerji>();

        private void Awake()
        {
            kontrol = GetComponent<CharacterController>();
        }

        private void Start()
        {
            Enerji.Doldur(ayarlar.EnerjiEnFazla);
        }

        private void Update()
        {
            if (Enerji.BittiMi) return;

            if (takiliKalan > 0f) takiliKalan -= Time.deltaTime;
            bool durmali = Kilitli || takiliKalan > 0f;

            float ileri = durmali ? 0f : Mathf.Clamp(GameInput.Hareket.y, -1f, 1f);
            bool hareketEdiyor = Mathf.Abs(ileri) > 0.01f;
            bool hizli = hareketEdiyor && GameInput.HizliBasili;
            float hiz = ayarlar.YurumeHizi * (hizli ? ayarlar.HizliCarpani : 1f);

            if (kontrol.isGrounded && dikeyHiz < 0f) dikeyHiz = -1f;
            dikeyHiz -= ayarlar.YerCekimi * Time.deltaTime;

            kontrol.Move((transform.forward * ileri * hiz + Vector3.up * dikeyHiz) * Time.deltaTime);

            float kayip = ayarlar.KaradaEnerjiKaybi * (hizli ? ayarlar.HizliEnerjiCarpani : 1f);
            Enerji.Harca(kayip * Time.deltaTime);
        }

        /// <summary>Anneyi bir noktaya ışınlar (deneme başı); CharacterController açıkken konum yazılamaz.</summary>
        public void Isinla(Vector3 konum, Quaternion yon)
        {
            kontrol.enabled = false;
            transform.SetPositionAndRotation(konum, yon);
            kontrol.enabled = true;
            dikeyHiz = 0f;
            takiliKalan = 0f;
            Kilitli = false;
        }

        private void OnTriggerEnter(Collider diger)
        {
            if (!diger.TryGetComponent(out TrashTrap tuzak) || tuzak.Tur != TrashTrap.Turu.CopYigini) return;
            takiliKalan = ayarlar.CopDurmaSuresi;
            Enerji.Harca(ayarlar.CopEnerjiKaybi);
        }
    }
}
