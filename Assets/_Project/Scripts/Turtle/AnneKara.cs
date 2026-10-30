using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Annenin karada birinci şahıs hareketi: WASD baktığı yöne göre, Shift hızlı; enerji saniye saniye azalır.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Enerji))]
    public class AnneKara : MonoBehaviour
    {
        [SerializeField] private AnneAyarlari ayarlar;

        private CharacterController kontrol;
        private Enerji enerji;
        private float dikeyHiz;

        private void Awake()
        {
            kontrol = GetComponent<CharacterController>();
            enerji = GetComponent<Enerji>();
        }

        private void OnEnable()
        {
            enerji.Bitti += EnerjiBitti;
        }

        private void OnDisable()
        {
            enerji.Bitti -= EnerjiBitti;
        }

        private void Start()
        {
            enerji.Doldur(ayarlar.EnerjiEnFazla);
        }

        private void Update()
        {
            if (enerji.BittiMi) return;

            Vector2 girdi = Vector2.ClampMagnitude(GameInput.Hareket, 1f);
            bool hareketEdiyor = girdi.sqrMagnitude > 0.01f;
            bool hizli = hareketEdiyor && GameInput.HizliBasili;

            Vector3 yon = transform.forward * girdi.y + transform.right * girdi.x;
            float hiz = ayarlar.YurumeHizi * (hizli ? ayarlar.HizliCarpani : 1f);

            if (kontrol.isGrounded && dikeyHiz < 0f) dikeyHiz = -1f;
            dikeyHiz -= ayarlar.YerCekimi * Time.deltaTime;

            kontrol.Move((yon * hiz + Vector3.up * dikeyHiz) * Time.deltaTime);

            float kayip = ayarlar.KaradaEnerjiKaybi * (hizli ? ayarlar.HizliEnerjiCarpani : 1f);
            enerji.Harca(kayip * Time.deltaTime);
        }

        private void EnerjiBitti()
        {
            // GDD: enerji biterse son kontrol noktasından devam edilir. Kontrol noktaları gelene kadar sadece durur.
            Debug.Log("AnneKara: enerji bitti.");
        }
    }
}
