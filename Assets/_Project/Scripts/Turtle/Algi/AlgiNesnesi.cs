using System.Collections.Generic;
using IkiDunya.Hazards;
using UnityEngine;
using UnityEngine.Rendering;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın gözünde bir nesnenin başka bir şey sanılması: sokak lambası uzakta ay, poşet denizanası gibi görünür.
    /// Göz ucuyla ve uzaktan güçlenir; yaklaşınca, doğrudan bakınca, termal görünümde ya da ışık sönünce kırılır ve gerçek görünüş geri gelir.
    /// Kırılma kalıcıdır: kaplumbağa bir kez gerçeği gördüyse artık aldanmaz.</summary>
    public class AlgiNesnesi : MonoBehaviour
    {
        [Header("Ne, ne sanılıyor")]
        [SerializeField] private AlgiTuru gercekTuru = AlgiTuru.Bilinmiyor;
        [SerializeField] private AlgiTuru yanilgiTuru = AlgiTuru.Bilinmiyor;

        [Header("Görünüşler")]
        [Tooltip("Gerçek görünüş; yanılgı açıkken gizlenir. Boşsa hiçbir şey gizlenmez (ışık gibi).")]
        [SerializeField] private Transform gercekGorunus;
        [Tooltip("Yanılgı görünüşü. Boşsa türüne göre kendisi kurar (ay ya da denizanası).")]
        [SerializeField] private Transform yanilgiGorunus;
        [Tooltip("Uzaklığın ve bakışın ölçüldüğü nokta (ör. lambanın başı). Boşsa bu objenin kendisi.")]
        [SerializeField] private Transform odak;

        [Header("Koşullar")]
        [Tooltip("Yanılgı yalnızca gece görünür (sokak lambası ay gibi).")]
        [SerializeField] private bool geceGerekli;
        [Tooltip("Işık yanmıyorsa (Işık Yasağı) yanılgı kırılır.")]
        [SerializeField] private IsikKaynagi gercekIsik;
        [Tooltip("Genel yarıçapları bu nesne için büyütür/küçültür (poşet lambadan daha yakında ele verir).")]
        [SerializeField] private float yaricapCarpani = 1f;

        [Header("Malzemeler (isteğe bağlı)")]
        [Tooltip("Boşsa ay için ışıktan etkilenmeyen parlak malzeme kendisi kurulur.")]
        [SerializeField] private Material ayMalzemesi;
        [Tooltip("Boşsa denizanası için yarı saydam malzeme kendisi kurulur.")]
        [SerializeField] private Material denizanasiMalzemesi;

        private readonly List<Renderer> gercekRendererlari = new List<Renderer>();
        private readonly List<Material> uretilenMalzemeler = new List<Material>();
        private bool gercekGizli;
        private bool yanilgiAcik;
        private bool kirildi;
        private float bakmaSuresi;
        private float gorunurluk;
        private Transform denizanasiCan;
        private Vector3 denizanasiCanOlcegi;
        private Material yanilgiMalzemesi;
        private Color yanilgiRengi = Color.white;
        private int yanilgiRenkId;

        /// <summary>Gerçekte ne olduğu.</summary>
        public AlgiTuru Gercek => gercekTuru;
        /// <summary>Kaplumbağanın şu an ne sandığı (yanılgı kapalıysa gerçek tür).</summary>
        public AlgiTuru Gorunen => yanilgiAcik ? yanilgiTuru : gercekTuru;
        /// <summary>Yanılgı şu an görünüyor mu.</summary>
        public bool YanilgiAcik => yanilgiAcik;
        /// <summary>Yanılgı kalıcı olarak kırıldı mı.</summary>
        public bool Kirildi => kirildi;

        private void Awake()
        {
            if (odak == null) odak = transform;
        }

        private void OnEnable() => AlgiDunyasi.Kaydet(this);

        private void OnDisable() => AlgiDunyasi.Birak(this);

        private void OnDestroy()
        {
            AlgiDunyasi.Birak(this);
            foreach (Material malzeme in uretilenMalzemeler)
                if (malzeme != null) Destroy(malzeme);
            uretilenMalzemeler.Clear();
        }

        /// <summary>Algıyı tek kare için günceller: uzaklık, bakış açısı, gece, termal görünüm ve ışık durumu.</summary>
        public void Guncelle(Transform goz, AlgiAyarlari ayarlar, bool gece)
        {
            if (kirildi || goz == null || ayarlar == null) return;

            Transform nokta = odak != null ? odak : transform;
            Vector3 fark = nokta.position - goz.position;
            float mesafe = fark.magnitude;
            if (mesafe < 0.001f)
            {
                Kir(true);
                return;
            }

            float carpan = Mathf.Max(0.01f, yaricapCarpani);
            float yanilgiYaricapi = ayarlar.YanilgiYaricapi * carpan;
            float kirilmaYaricapi = ayarlar.KirilmaYaricapi * carpan;
            float bakmaYaricapi = ayarlar.BakmaYaricapi * carpan;

            // Çok yaklaşmak yanılgıyı hemen ve kalıcı olarak bitirir.
            if (mesafe <= kirilmaYaricapi)
            {
                Kir(true);
                return;
            }

            float aci = Vector3.Angle(goz.forward, fark / mesafe);
            if (mesafe <= bakmaYaricapi && aci <= ayarlar.MerkezBakisAcisi)
            {
                bakmaSuresi += Time.deltaTime;
                if (bakmaSuresi >= ayarlar.BakmaSuresi)
                {
                    Kir(true);
                    return;
                }
            }
            else bakmaSuresi = Mathf.Max(0f, bakmaSuresi - Time.deltaTime * 0.5f);

            // Uzaktan güçlü, yakından zayıf; göz ucuyla güçlü, tam bakışta biraz zayıf.
            float mesafeGucu = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(kirilmaYaricapi, yanilgiYaricapi, mesafe));
            float bakisGucu = Mathf.InverseLerp(ayarlar.MerkezBakisAcisi, ayarlar.KenarBakisAcisi, aci);
            float guc = mesafeGucu * Mathf.Lerp(ayarlar.MerkezGucu, 1f, bakisGucu) * KosulGucu(gece);

            float esik = yanilgiAcik ? ayarlar.Esik - ayarlar.Histerezis : ayarlar.Esik;
            yanilgiAcik = guc >= esik;

            gorunurluk = Mathf.MoveTowards(gorunurluk, yanilgiAcik ? 1f : 0f, ayarlar.GecisHizi * Time.deltaTime);
            if (gorunurluk <= 0.001f) yanilgiAcik = false;

            GorunumuUygula(goz, ayarlar);
        }

        /// <summary>Yanılgıyı kırar; kalıcıysa bu oyunda bir daha geri gelmez.</summary>
        public void Kir(bool kalici = true)
        {
            if (kirildi && kalici) return;
            bool yenimiydi = !kirildi;
            if (kalici) kirildi = true;
            yanilgiAcik = false;
            bakmaSuresi = 0f;
            gorunurluk = 0f;
            GorunumuUygula(null, null);
            if (kalici && yenimiydi) AlgiDunyasi.KirildiBildir(this);
        }

        /// <summary>Yanılgıyı başlangıç hâline döndürür (yeni oyun).</summary>
        public void Sifirla()
        {
            kirildi = false;
            yanilgiAcik = false;
            bakmaSuresi = 0f;
            gorunurluk = 0f;
            GorunumuUygula(null, null);
        }

        [ContextMenu("Yanılgıyı kır")]
        private void YanilgiyiKir() => Kir(true);

        [ContextMenu("Yanılgıyı sıfırla")]
        private void YanilgiyiSifirla() => Sifirla();

        /// <summary>Gece, termal görünüm ve ışık durumuna göre algı çarpanı (0 = yanılgı imkânsız).</summary>
        private float KosulGucu(bool gece)
        {
            if (geceGerekli && !gece) return 0f;
            if (gercekIsik != null && !gercekIsik.Acik) return 0f;
            if (TermalGorunum.AcikMi) return 0f;
            return 1f;
        }

        private void GorunumuUygula(Transform goz, AlgiAyarlari ayarlar)
        {
            bool yanilgiVar = gorunurluk > 0.001f;
            // Gerçek görünüş, yanılgı iyice belirginleşene kadar kalır: geçişte ikisi bir an birlikte görünür.
            bool gercekGizliOlsun = gorunurluk > 0.5f;

            if (gercekGizli != gercekGizliOlsun) GercekGorunusuGizle(gercekGizliOlsun);

            if (yanilgiVar)
            {
                if (yanilgiGorunus == null && ayarlar != null) YanilgiGorunusunuKur(ayarlar);
                if (yanilgiGorunus == null) return;
                if (!yanilgiGorunus.gameObject.activeSelf) yanilgiGorunus.gameObject.SetActive(true);
                YanilgiYerlestir(goz, ayarlar);
                SaydamlikUygula(gorunurluk);
            }
            else if (yanilgiGorunus != null && yanilgiGorunus.gameObject.activeSelf)
            {
                yanilgiGorunus.gameObject.SetActive(false);
            }
        }

        private void GercekGorunusuGizle(bool gizle)
        {
            if (gercekRendererlari.Count == 0 && gercekGorunus != null)
                gercekRendererlari.AddRange(gercekGorunus.GetComponentsInChildren<Renderer>(true));
            foreach (Renderer renderer in gercekRendererlari)
                if (renderer != null) renderer.enabled = !gizle;
            gercekGizli = gizle;
        }

        // ---- Yanılgı görünüşleri ----

        private void YanilgiGorunusunuKur(AlgiAyarlari ayarlar)
        {
            switch (yanilgiTuru)
            {
                case AlgiTuru.Ay: AyKur(ayarlar); break;
                case AlgiTuru.Denizanasi: DenizanasiKur(ayarlar); break;
            }
        }

        /// <summary>Ay: gözden uzakta, hep aynı görünür çapta duran parlak bir küre. Uzaklık hissi gök cismi gibi kalır.</summary>
        private void AyKur(AlgiAyarlari ayarlar)
        {
            GameObject nesne = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nesne.name = "Yanilgi_Ay";
            Destroy(nesne.GetComponent<Collider>());
            nesne.transform.SetParent(transform, true);

            var renderer = nesne.GetComponent<Renderer>();
            renderer.sharedMaterial = SaydamKopya(ayMalzemesi != null ? ayMalzemesi : AyMalzemesi(ayarlar));
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            yanilgiGorunus = nesne.transform;
        }

        /// <summary>Denizanası: yarı saydam bir can ve altında dört ince dokunaç; can nabız gibi kasılır.</summary>
        private void DenizanasiKur(AlgiAyarlari ayarlar)
        {
            var kok = new GameObject("Yanilgi_Denizanasi");
            kok.transform.SetParent(transform, false);
            kok.transform.localPosition = Vector3.zero;

            Material malzeme = SaydamKopya(denizanasiMalzemesi != null ? denizanasiMalzemesi : DenizanasiMalzemesi(ayarlar));

            GameObject can = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            can.name = "Can";
            Destroy(can.GetComponent<Collider>());
            can.transform.SetParent(kok.transform, false);
            can.transform.localPosition = Vector3.up * 0.18f * ayarlar.DenizanasiOlcegi;
            can.transform.localScale = new Vector3(0.5f, 0.34f, 0.5f) * ayarlar.DenizanasiOlcegi;
            GorunusAyarla(can, malzeme);
            denizanasiCan = can.transform;
            denizanasiCanOlcegi = can.transform.localScale;

            for (int i = 0; i < 4; i++)
            {
                float aci = i * 90f * Mathf.Deg2Rad;
                GameObject dokunac = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dokunac.name = $"Dokunac_{i}";
                Destroy(dokunac.GetComponent<Collider>());
                dokunac.transform.SetParent(kok.transform, false);
                dokunac.transform.localPosition = new Vector3(Mathf.Cos(aci) * 0.12f, -0.3f, Mathf.Sin(aci) * 0.12f) * ayarlar.DenizanasiOlcegi;
                dokunac.transform.localScale = new Vector3(0.02f, 0.22f, 0.02f) * ayarlar.DenizanasiOlcegi;
                GorunusAyarla(dokunac, malzeme);
            }

            yanilgiGorunus = kok.transform;
        }

        private static void GorunusAyarla(GameObject nesne, Material malzeme)
        {
            var renderer = nesne.GetComponent<Renderer>();
            renderer.sharedMaterial = malzeme;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        /// <summary>Yanılgı görünüşünü yerleştirir: ay gök cismi gibi uzakta ve sabit çapta, denizanası nesnenin üstünde ve nabız atarak.</summary>
        private void YanilgiYerlestir(Transform goz, AlgiAyarlari ayarlar)
        {
            if (yanilgiGorunus == null || ayarlar == null) return;

            if (yanilgiTuru == AlgiTuru.Ay)
            {
                if (goz == null) return;
                Vector3 yon = ((odak != null ? odak.position : transform.position) - goz.position).normalized;
                float mesafe = Mathf.Max(ayarlar.AyMesafesi, Vector3.Distance(goz.position, transform.position) + ayarlar.AyMesafesi * 0.5f);
                yanilgiGorunus.position = goz.position + yon * mesafe;
                float cap = 2f * mesafe * Mathf.Tan(ayarlar.AyAcisalCap * Mathf.Deg2Rad * 0.5f);
                yanilgiGorunus.localScale = Vector3.one * Mathf.Max(0.01f, cap);
                return;
            }

            if (yanilgiTuru == AlgiTuru.Denizanasi && denizanasiCan != null)
            {
                float nabiz = Mathf.Sin(Time.time * ayarlar.DenizanasiNabizHizi * 2f * Mathf.PI);
                denizanasiCan.localScale = Vector3.Scale(denizanasiCanOlcegi,
                    new Vector3(1f + 0.12f * nabiz, 1f - 0.16f * nabiz, 1f + 0.12f * nabiz));
            }
        }

        // ---- Malzemeler ----

        private Material AyMalzemesi(AlgiAyarlari ayarlar)
        {
            Color renk = ayarlar.AyRengi * ayarlar.AyParlakligi;
            renk.a = 1f;
            return Malzeme("YanilgiAy", renk, false);
        }

        private Material DenizanasiMalzemesi(AlgiAyarlari ayarlar) =>
            Malzeme("YanilgiDenizanasi", ayarlar.DenizanasiRengi, true);

        /// <summary>Işıktan etkilenmeyen malzeme; saydamsa alfa karışımı kurulur. Projenin URP Unlit gölgelendiricisi kullanılır.</summary>
        private Material Malzeme(string ad, Color renk, bool saydam)
        {
            Shader golgelendirici = Shader.Find("Universal Render Pipeline/Unlit");
            if (golgelendirici == null) golgelendirici = Shader.Find("Unlit/Color");
            if (golgelendirici == null) golgelendirici = Shader.Find("Sprites/Default");

            var malzeme = new Material(golgelendirici) { name = ad };
            if (malzeme.HasProperty("_BaseColor")) malzeme.SetColor("_BaseColor", renk);
            if (malzeme.HasProperty("_Color")) malzeme.SetColor("_Color", renk);

            if (saydam)
            {
                if (malzeme.HasProperty("_Surface")) malzeme.SetFloat("_Surface", 1f);
                if (malzeme.HasProperty("_Blend")) malzeme.SetFloat("_Blend", 0f);
                if (malzeme.HasProperty("_SrcBlend")) malzeme.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                if (malzeme.HasProperty("_DstBlend")) malzeme.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                if (malzeme.HasProperty("_ZWrite")) malzeme.SetFloat("_ZWrite", 0f);
                malzeme.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                malzeme.renderQueue = (int)RenderQueue.Transparent;
            }

            uretilenMalzemeler.Add(malzeme);
            return malzeme;
        }

        /// <summary>Yanılgı malzemesinin bu nesneye özel kopyası: belirirken ve kaybolurken saydamlığı değişir, kaynak malzemeye dokunulmaz.</summary>
        private Material SaydamKopya(Material kaynak)
        {
            var kopya = new Material(kaynak) { name = kaynak.name + " (yanılgı)" };
            uretilenMalzemeler.Add(kopya);
            yanilgiMalzemesi = kopya;

            // Gökyüzü ve Unlit gölgelendiricileri rengi _BaseColor'da tutar; eski gölgelendiriciler _Color'da.
            yanilgiRenkId = kopya.HasProperty("_BaseColor") ? Shader.PropertyToID("_BaseColor")
                : kopya.HasProperty("_Color") ? Shader.PropertyToID("_Color")
                : 0;
            yanilgiRengi = yanilgiRenkId != 0 ? kopya.GetColor(yanilgiRenkId) : Color.white;
            return kopya;
        }

        /// <summary>Yanılgının gücünü saydamlığa çevirir; güç arttıkça yanılgı belirir, düştükçe erir.</summary>
        private void SaydamlikUygula(float guc)
        {
            if (yanilgiMalzemesi == null || yanilgiRenkId == 0) return;
            Color renk = yanilgiRengi;
            renk.a *= Mathf.Clamp01(guc);
            yanilgiMalzemesi.SetColor(yanilgiRenkId, renk);
        }
    }
}
