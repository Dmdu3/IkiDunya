using System.Collections.Generic;
using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın bir nesneyi başka bir şey sanması: poşet uzaktan denizanası, sokak lambası gece ay gibi görünür.
    /// Yaklaşınca, doğrudan bakınca ya da (lambada) ışık yasaklanınca kalıcı olarak kırılır: kaplumbağa gerçeği bir kez gördüyse artık aldanmaz.
    /// Görünüşleri kurulum komutu hazırlar; burada sadece hangisinin açık olduğu seçilir.</summary>
    public class AlgiNesnesi : MonoBehaviour
    {
        private static readonly List<AlgiNesnesi> kayitlar = new List<AlgiNesnesi>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => kayitlar.Clear();

        [Tooltip("Gerçek görünüş; yanılgı açıkken renderer'ları kapanır. Boşsa (lamba) gerçek hep görünür kalır.")]
        [SerializeField] private Transform gercekGorunus;
        [Tooltip("Yanılgı görünüşü (kapalı başlar).")]
        [SerializeField] private GameObject yanilgiGorunus;
        [Tooltip("Uzaklığın ve bakışın ölçüldüğü nokta (ör. lambanın başı). Boşsa bu obje.")]
        [SerializeField] private Transform odak;
        [Tooltip("Yanılgı görünüşü gök cismi gibi uzağa, sabit çapta konur (sahte ay).")]
        [SerializeField] private bool gokCismi;
        [Tooltip("Yanılgı sadece gece görünür.")]
        [SerializeField] private bool geceGerekli;
        [Tooltip("Doluysa Işık Yasağı yanılgıyı kırar (lamba söner, ay kaybolur).")]
        [SerializeField] private KumsalDurumu isikYasagiKumsali;
        [Tooltip("Genel yarıçapları bu nesne için ölçekler (suda görüş kısa: poşet daha yakında ele verir).")]
        [SerializeField] private float yaricapCarpani = 1f;

        private Renderer[] gercekRendererlari;
        private bool yanilgiAcik;
        private bool kirildi;
        private float bakilanSure;

        /// <summary>Yanılgı şu an görünüyor mu.</summary>
        public bool YanilgiAcik => yanilgiAcik;
        /// <summary>Yanılgı kalıcı olarak kırıldı mı.</summary>
        public bool Kirildi => kirildi;

        private void Awake()
        {
            if (odak == null) odak = transform;
            gercekRendererlari = gercekGorunus != null ? gercekGorunus.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        }

        private void OnEnable() => kayitlar.Add(this);

        private void OnDisable()
        {
            kayitlar.Remove(this);
            Goster(false);
        }

        /// <summary>Kayıtlı bütün yanılgıları gözün o anki yerine ve bakışına göre günceller.</summary>
        public static void HepsiniGuncelle(Transform goz, AlgiAyarlari ayarlar)
        {
            for (int i = kayitlar.Count - 1; i >= 0; i--) kayitlar[i].Guncelle(goz, ayarlar);
        }

        private void Guncelle(Transform goz, AlgiAyarlari ayarlar)
        {
            if (kirildi) return;
            if (isikYasagiKumsali != null && isikYasagiKumsali.IsikYasagi)
            {
                Kir();
                return;
            }

            Vector3 fark = odak.position - goz.position;
            float mesafe = fark.magnitude;
            float carpan = Mathf.Max(0.01f, yaricapCarpani);
            if (mesafe <= ayarlar.KirilmaYaricapi * carpan)
            {
                Kir();
                return;
            }

            // Uzun süre doğrudan bakmak da gerçeği gösterir; göz kaçınca süre yavaşça geri çekilir.
            bool bakiyor = mesafe <= ayarlar.BakmaYaricapi * carpan && Vector3.Angle(goz.forward, fark) <= ayarlar.BakmaAcisi;
            bakilanSure = bakiyor ? bakilanSure + Time.deltaTime : Mathf.Max(0f, bakilanSure - Time.deltaTime * 0.5f);
            if (bakilanSure >= ayarlar.BakmaSuresi)
            {
                Kir();
                return;
            }

            Goster((!geceGerekli || OrtamIsigi.GeceMi) && mesafe <= ayarlar.YanilgiYaricapi * carpan);
            if (yanilgiAcik && gokCismi) GokteYerlestir(goz, ayarlar);
        }

        /// <summary>Yanılgıyı kalıcı olarak kırar.</summary>
        public void Kir()
        {
            kirildi = true;
            Goster(false);
        }

        private void Goster(bool acik)
        {
            if (yanilgiAcik == acik) return;
            yanilgiAcik = acik;
            if (yanilgiGorunus != null) yanilgiGorunus.SetActive(acik);
            foreach (Renderer renderer in gercekRendererlari)
                if (renderer != null) renderer.enabled = !acik;
        }

        /// <summary>Sahte ay: lambanın yönünde, biraz yukarıda, uzakta ve hep aynı görünür çapta; gerçek ay gibi gökte durur.</summary>
        private void GokteYerlestir(Transform goz, AlgiAyarlari ayarlar)
        {
            Vector3 yon = odak.position - goz.position;
            Vector3 yatay = new Vector3(yon.x, 0f, yon.z);
            if (yatay.sqrMagnitude < 0.0001f) return;
            float yukseklik = (Mathf.Atan2(yon.y, yatay.magnitude) * Mathf.Rad2Deg + ayarlar.AyYukselisi) * Mathf.Deg2Rad;
            Vector3 gok = yatay.normalized * Mathf.Cos(yukseklik) + Vector3.up * Mathf.Sin(yukseklik);
            Transform ay = yanilgiGorunus.transform;
            ay.position = goz.position + gok * ayarlar.AyMesafesi;
            ay.localScale = Vector3.one * (2f * ayarlar.AyMesafesi * Mathf.Tan(ayarlar.AyAcisalCap * 0.5f * Mathf.Deg2Rad)) / Mathf.Max(0.0001f, ay.parent != null ? ay.parent.lossyScale.x : 1f);
        }
    }
}
