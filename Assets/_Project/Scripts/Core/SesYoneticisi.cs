using System.Collections;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Sistemler'de durur: ses olaylarını çalar, adıma göre ortam sesini ve müziği değiştirir (yumuşak geçişle). Ana ses düzeyi OyunTercihleri'nden.</summary>
    public class SesYoneticisi : MonoBehaviour
    {
        [SerializeField] private SesAyarlari ayarlar;

        private AudioSource efekt;
        private AudioSource ortam;
        private AudioSource muzik;
        private Coroutine gecis;

        private void Awake()
        {
            efekt = Kaynak(false);
            ortam = Kaynak(true);
            muzik = Kaynak(true);
            // Duraklatma menüsünde düğme sesi çalsın, müzik sürsün; ortam sesi ve dünyadaki sesler durur.
            efekt.ignoreListenerPause = true;
            muzik.ignoreListenerPause = true;
        }

        private void OnEnable()
        {
            if (ayarlar == null)
            {
                Debug.LogWarning("SesYoneticisi: Ayarlar alanına ScriptableObjects/Settings'teki SesAyarlari'nı bağla (İki Dünya → Menü ve duraklatmayı kur).", this);
                return;
            }
            if (!Ses.YoneticiAyarla(this))
            {
                enabled = false;
                return;
            }
            OyunAkisi.AdimBasladi += AdimBasladi;
            OyunAkisi.MenuyeDonuldu += MenuyeDonuldu;
            if (OyunAkisi.MenudeMi) MenuyeDonuldu();
        }

        private void OnDisable()
        {
            Ses.YoneticiBirak(this);
            OyunAkisi.AdimBasladi -= AdimBasladi;
            OyunAkisi.MenuyeDonuldu -= MenuyeDonuldu;
        }

        private AudioSource Kaynak(bool dongu)
        {
            var kaynak = gameObject.AddComponent<AudioSource>();
            kaynak.playOnAwake = false;
            kaynak.loop = dongu;
            kaynak.spatialBlend = 0f;
            return kaynak;
        }

        internal void Cal(SesTuru tur, Vector3? konum)
        {
            SesAyarlari.Efekt e = ayarlar.EfektAl(tur);
            AudioClip klip = e?.Klip;
            if (klip == null) return;
            if (konum == null)
            {
                efekt.PlayOneShot(klip, e.Duzey);
                return;
            }

            // Konumlu ses: kısa ömürlü bir kaynak (PlayClipAtPoint gibi), duyulma mesafesi ayarlardan.
            var nesne = new GameObject($"Ses_{tur}");
            nesne.transform.position = konum.Value;
            var kaynak = nesne.AddComponent<AudioSource>();
            kaynak.clip = klip;
            kaynak.volume = e.Duzey;
            kaynak.spatialBlend = 1f;
            kaynak.rolloffMode = AudioRolloffMode.Linear;
            kaynak.maxDistance = ayarlar.DuyulmaMesafesi;
            kaynak.Play();
            Destroy(nesne, klip.length + 0.1f);
        }

        internal AudioClip Klip(SesTuru tur, out float duzey)
        {
            SesAyarlari.Efekt e = ayarlar.EfektAl(tur);
            duzey = e != null ? e.Duzey : 1f;
            return e?.Klip;
        }

        private void AdimBasladi(Adim adim)
        {
            Degistir(ayarlar.OrtamAl(adim), ayarlar.AdimSesiAl(adim.Tur)?.Muzik);
        }

        private void MenuyeDonuldu() => Degistir(null, ayarlar.MenuMuzigi);

        private void Degistir(AudioClip yeniOrtam, AudioClip yeniMuzik)
        {
            if (gecis != null) StopCoroutine(gecis);
            gecis = StartCoroutine(Gec(yeniOrtam, yeniMuzik));
        }

        /// <summary>Değişen kaynak kısılır, klip değişir, yeniden açılır; aynı klip çalıyorsa dokunulmaz.</summary>
        private IEnumerator Gec(AudioClip yeniOrtam, AudioClip yeniMuzik)
        {
            bool ortamDegisir = ortam.clip != yeniOrtam;
            bool muzikDegisir = muzik.clip != yeniMuzik;
            float yari = ayarlar.GecisSuresi / 2f;

            for (float t = 0f; t < yari; t += Time.unscaledDeltaTime)
            {
                float oran = 1f - t / yari;
                if (ortamDegisir) ortam.volume = ayarlar.OrtamDuzeyi * oran;
                if (muzikDegisir) muzik.volume = ayarlar.MuzikDuzeyi * oran;
                yield return null;
            }

            if (ortamDegisir) Yerlestir(ortam, yeniOrtam);
            if (muzikDegisir) Yerlestir(muzik, yeniMuzik);

            for (float t = 0f; t < yari; t += Time.unscaledDeltaTime)
            {
                float oran = t / yari;
                if (ortamDegisir) ortam.volume = ayarlar.OrtamDuzeyi * oran;
                if (muzikDegisir) muzik.volume = ayarlar.MuzikDuzeyi * oran;
                yield return null;
            }
            ortam.volume = ayarlar.OrtamDuzeyi;
            muzik.volume = ayarlar.MuzikDuzeyi;
            gecis = null;
        }

        private static void Yerlestir(AudioSource kaynak, AudioClip klip)
        {
            kaynak.Stop();
            kaynak.clip = klip;
            if (klip != null) kaynak.Play();
        }
    }
}
