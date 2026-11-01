using IkiDunya.Camera;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın gözü (kamerada): sahnedeki yanılgıları her karede günceller.
    /// İçgüdü açıksa (kaçış gecesi) oyuncu bakışını bir süre oynatmayınca bakış en çekici aya döner: ışık yanıyorsa sahte aya (karaya), yasaksa gerçek aya (denize).</summary>
    [RequireComponent(typeof(BirinciSahisKamera))]
    public class KaplumbagaAlgisi : MonoBehaviour
    {
        [SerializeField] private AlgiAyarlari ayarlar;
        [Tooltip("Yavru gece en parlak ayı izler (kaçış gecesi).")]
        [SerializeField] private bool icgudu;

        private BirinciSahisKamera kamera;
        private float bakisiz;

        private void Awake() => kamera = GetComponent<BirinciSahisKamera>();

        private void LateUpdate()
        {
            if (ayarlar == null) return;
            // Kamera bu karede yerine oturduktan sonra: sahte ay göze göre konur.
            AlgiNesnesi.HepsiniGuncelle(transform, ayarlar);
            if (icgudu) Icgudu();
        }

        private void Icgudu()
        {
            if (GameInput.Kilitli || Duraklatma.Acik || !OrtamIsigi.GeceMi || ayarlar.IcguduHizi <= 0f)
            {
                bakisiz = 0f;
                return;
            }

            // Oyuncu bakışını oynattıkça içgüdü susar; bıraktığında yavaşça geri gelir.
            bool bakiyor = GameInput.Bakis.sqrMagnitude > 0.01f || GameInput.BakisCubugu.sqrMagnitude > 0.04f;
            bakisiz = bakiyor ? 0f : bakisiz + Time.deltaTime;
            float guc = Mathf.SmoothStep(0f, 1f, (bakisiz - ayarlar.IcguduGecikmesi) / Mathf.Max(0.01f, ayarlar.IcguduYukselmesi));
            if (guc <= 0f || !EnCekiciAy(out Vector3 hedef)) return;

            Vector3 yon = hedef - transform.position;
            yon.y = 0f;
            Vector3 ileri = transform.forward;
            ileri.y = 0f;
            float fark = Vector3.SignedAngle(ileri, yon, Vector3.up);
            // Hedefe yaklaştıkça yumuşar (üstünden geçip salınmasın).
            float adim = Mathf.Sign(fark) * Mathf.Min(Mathf.Abs(fark), ayarlar.IcguduHizi * guc * Time.deltaTime * Mathf.Clamp01(Mathf.Abs(fark) / 15f + 0.2f));
            kamera.Dondur(adim);
        }

        /// <summary>Görünen ayların en çekicisi: sahte ay daha parlak; arkada kalan ay daha az çeker.</summary>
        private bool EnCekiciAy(out Vector3 hedef)
        {
            hedef = Vector3.zero;
            float enIyi = 0f;
            Transform gercek = GeceGokyuzu.Ay;
            if (gercek != null && gercek.gameObject.activeInHierarchy) Aday(gercek.position, 1f, ref hedef, ref enIyi);
            foreach (AlgiNesnesi nesne in AlgiNesnesi.Kayitlar)
                if (nesne.Tur == AlgiTuru.LambaAy && nesne.YanilgiGorunuyor) Aday(nesne.YanilgiKonumu, ayarlar.SahteAyCekiciligi, ref hedef, ref enIyi);
            return enIyi > 0f;
        }

        private void Aday(Vector3 konum, float parlaklik, ref Vector3 hedef, ref float enIyi)
        {
            Vector3 yon = konum - transform.position;
            yon.y = 0f;
            Vector3 ileri = transform.forward;
            ileri.y = 0f;
            float puan = parlaklik / (1f + Vector3.Angle(ileri, yon) / Mathf.Max(1f, ayarlar.CekicilikYariAcisi));
            if (puan <= enIyi) return;
            enIyi = puan;
            hedef = konum;
        }
    }
}
