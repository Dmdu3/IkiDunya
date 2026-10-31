using System;
using System.Collections.Generic;
using IkiDunya.Core;
using IkiDunya.Hazards;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Denizde birinci şahıs yüzme: baktığın yöne yüzersin; Space ile yüzeye çıkar ya da dalarsın. Nefes, akıntı, ağ, besin, ışık ve tehlikeler burada etki eder.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Enerji))]
    public class DenizHareketi : MonoBehaviour, IYuzucu
    {
        [SerializeField] private DenizAyarlari ayarlar;
        [SerializeField] private DenizTehlikeAyarlari tehlikeAyarlari;
        [Tooltip("Kaplumbağanın gözü; yüzme yönü buradan alınır.")]
        [SerializeField] private Transform goz;
        [Tooltip("Su yüzeyinin yüksekliği (dünya y).")]
        [SerializeField] private float suYuzeyi;

        private CharacterController kontrol;
        private Enerji enerji;
        private readonly List<Akinti> akintilar = new List<Akinti>();
        private readonly List<HazardZone> tehlikeler = new List<HazardZone>();
        private readonly List<IsikKaynagi> isiklar = new List<IsikKaynagi>();
        private readonly Collider[] bulunan = new Collider[16];
        private int siperSayisi;
        private float dalisKalan;
        private float yavasKalan;

        public float Nefes { get; private set; } = 1f;
        public bool NefesVar => ayarlar.NefesSuresi > 0f;
        /// <summary>Ağ ya da misinaya takılıysa kurtulmak için kalan E basışı.</summary>
        public int KalanBasis { get; private set; }
        public int ToplamBasis { get; private set; }
        public bool Takili => KalanBasis > 0;
        /// <summary>Bölüm bitince ya da final sırasında true; kaplumbağa kıpırdamaz.</summary>
        public bool Kilitli { get; set; }
        public Transform Goz => goz;

        // Başka objelerin OnEnable'ı bu Awake'ten önce çalışabilir; bu yüzden gerekince bulunur.
        public Enerji Enerji => enerji != null ? enerji : enerji = GetComponent<Enerji>();

        public Vector3 Konum => transform.position;
        public bool Yuzeyde => transform.position.y >= suYuzeyi - ayarlar.YuzeyPayi;
        public bool Gizli => siperSayisi > 0;

        /// <summary>Avcı yakaladığında; bölüm son kontrol noktasına döndürür.</summary>
        public event Action Yakalandi;

        private void Awake()
        {
            kontrol = GetComponent<CharacterController>();
        }

        /// <summary>Kontrol noktasında yeniden başlar: enerji ve nefes dolu, tuzak ve yavaşlık yok.</summary>
        public void Baslat(Vector3 konum, Quaternion yon)
        {
            kontrol.enabled = false;
            transform.SetPositionAndRotation(konum, yon);
            kontrol.enabled = true;
            Enerji.Doldur(ayarlar.EnerjiEnFazla);
            Nefes = 1f;
            KalanBasis = 0;
            yavasKalan = 0f;
            dalisKalan = 0f;
            Kilitli = false;
        }

        public void EnerjiKaybet(float miktar) => Enerji.Harca(miktar);
        public void Yakala() => Yakalandi?.Invoke();
        public void Savrul(Vector3 yer) => kontrol.Move(yer);

        private void Update()
        {
            if (Kilitli || Enerji.BittiMi) return;

            float harcama = ayarlar.SurekliEnerji;
            foreach (HazardZone bolge in tehlikeler) harcama += bolge.SaniyeHasari;

            if (Takili)
            {
                if (GameInput.EtkilesBasildi && --KalanBasis == 0) yavasKalan = ayarlar.AgSonrasiYavas;
                Enerji.Harca(harcama * Time.deltaTime);
                return;
            }

            if (GameInput.EtkilesBasildi) Ye();
            if (yavasKalan > 0f) yavasKalan -= Time.deltaTime;

            // Yön: baktığın yer. Space: yüzeydeyken dal, su altında basılı tutunca yüzeye çık (yavru sadece dalar).
            Vector2 girdi = Vector2.ClampMagnitude(GameInput.Hareket, 1f);
            Vector3 yon = goz.forward * girdi.y + goz.right * girdi.x;
            bool hareketEdiyor = yon.sqrMagnitude > 0.01f;
            bool hizli = hareketEdiyor && GameInput.HizliBasili;

            if (Yuzeyde && GameInput.ZiplaDalBasildi) dalisKalan = ayarlar.DalisSuresi;
            float dikey = 0f;
            if (dalisKalan > 0f)
            {
                dalisKalan -= Time.deltaTime;
                dikey = -ayarlar.DalisHizi;
            }
            else if (!ayarlar.SpaceSadeceDalar && GameInput.ZiplaDalBasili) dikey = ayarlar.CikisHizi;

            float hiz = ayarlar.YuzmeHizi;
            if (hizli) hiz *= ayarlar.HizliCarpani;
            if (yavasKalan > 0f) hiz *= ayarlar.YavasCarpani;
            if (NefesVar && Nefes <= 0f) hiz *= ayarlar.NefessizHizCarpani;

            Vector3 akinti = Vector3.zero;
            foreach (Akinti a in akintilar) akinti += a.Hiz;
            Vector3 cekim = Vector3.zero;
            foreach (IsikKaynagi isik in isiklar) cekim += isik.Cekim(transform.position);

            kontrol.Move((yon * hiz + Vector3.up * dikey + akinti + cekim) * Time.deltaTime);
            if (transform.position.y > suYuzeyi) kontrol.Move(Vector3.up * (suYuzeyi - transform.position.y));

            // Enerji: yüzmek, hızlı yüzmek ve akıntıya karşı yüzmek harcatır.
            if (hareketEdiyor)
            {
                float yuzme = ayarlar.HareketEnerjisi;
                if (hizli) yuzme *= ayarlar.HizliEnerjiCarpani;
                if (akinti.sqrMagnitude > 0f && Vector3.Dot(yon, akinti) < 0f) yuzme *= ayarlar.AkintiyaKarsiCarpani;
                harcama += yuzme;
            }
            Enerji.Harca(harcama * Time.deltaTime);

            if (!NefesVar) return;
            Nefes = Yuzeyde
                ? Mathf.Min(1f, Nefes + Time.deltaTime / ayarlar.NefesDolmaSuresi)
                : Mathf.Max(0f, Nefes - Time.deltaTime / ayarlar.NefesSuresi);
        }

        /// <summary>Önündeki besini yer: denizanası enerji verir, poşet yavaşlatır.</summary>
        private void Ye()
        {
            Vector3 merkez = goz.position + goz.forward * ayarlar.YemeMesafesi * 0.5f;
            int sayi = Physics.OverlapSphereNonAlloc(merkez, ayarlar.YemeMesafesi, bulunan, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < sayi; i++)
            {
                if (!bulunan[i].TryGetComponent(out Yiyecek yiyecek)) continue;
                if (yiyecek.Ye() == Yiyecek.Turu.Denizanasi) Enerji.Ekle(tehlikeAyarlari.DenizanasiEnerjisi);
                else yavasKalan = ayarlar.PosetYavas;
                return;
            }
        }

        private void OnTriggerEnter(Collider diger)
        {
            if (diger.TryGetComponent(out Akinti akinti) && !akintilar.Contains(akinti)) akintilar.Add(akinti);
            if (diger.TryGetComponent(out HazardZone bolge) && !tehlikeler.Contains(bolge)) tehlikeler.Add(bolge);
            if (diger.TryGetComponent(out IsikKaynagi isik) && !isiklar.Contains(isik)) isiklar.Add(isik);
            if (diger.TryGetComponent(out Cover _)) siperSayisi++;
            if (diger.TryGetComponent(out TrashTrap tuzak)) Takil(tuzak.Tur);
        }

        private void OnTriggerExit(Collider diger)
        {
            if (diger.TryGetComponent(out Akinti akinti)) akintilar.Remove(akinti);
            if (diger.TryGetComponent(out HazardZone bolge)) tehlikeler.Remove(bolge);
            if (diger.TryGetComponent(out IsikKaynagi isik)) isiklar.Remove(isik);
            if (diger.TryGetComponent(out Cover _)) siperSayisi = Mathf.Max(0, siperSayisi - 1);
        }

        private void Takil(TrashTrap.Turu tur)
        {
            if (Takili) return;
            if (tur == TrashTrap.Turu.HayaletAg) ToplamBasis = KalanBasis = ayarlar.AgBasisi;
            else if (tur == TrashTrap.Turu.Misina) ToplamBasis = KalanBasis = ayarlar.MisinaBasisi;
            else if (tur == TrashTrap.Turu.Plastik) yavasKalan = ayarlar.PlastikYavas;
        }
    }
}
