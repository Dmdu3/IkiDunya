using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IkiDunya.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace IkiDunya.Fragman
{
    /// <summary>Fragman çekimi: oyunu sabit kare hızında oynatır, her kareyi Recordings/Fragman/&lt;çekim&gt;/ altına JPG yazar.
    /// Çekim sırasında oyunun arayüzü gizlenir; istenirse sinema kamerası oyunun kamerasının yerine geçer, üstte yazı ve perde katmanı var.</summary>
    public class FragmanKaydi : MonoBehaviour
    {
        public const int KareHizi = 30;
        public const int En = 1920;
        public const int Boy = 1080;

        /// <summary>Kayıtların kökü (repoya girmez, .gitignore'da Recordings/).</summary>
        public static string Kok => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Recordings", "Fragman");
        public static string Durum { get; private set; } = "";
        public static bool Calisiyor { get; private set; }

        private static FragmanKaydi ornek;

        private UnityEngine.Camera sinema;
        private readonly List<UnityEngine.Camera> kapatilanKameralar = new List<UnityEngine.Camera>();
        private UnityEngine.Camera oyunKamerasi;
        private readonly List<Canvas> gizlenenler = new List<Canvas>();
        private Canvas katman;
        private Image perde;
        private RawImage ufuk;
        private Text yazi;
        private Text altYazi;
        private readonly WaitForEndOfFrame kareSonu = new WaitForEndOfFrame();

        public static FragmanKaydi Ornek
        {
            get
            {
                if (ornek == null)
                {
                    var go = new GameObject("FragmanKaydi");
                    DontDestroyOnLoad(go);
                    ornek = go.AddComponent<FragmanKaydi>();
                    ornek.KatmaniKur();
                }
                return ornek;
            }
        }

        /// <summary>İç içe coroutine'leri çalıştırır; hata olursa durur, her şeyi eski haline getirir.</summary>
        public static void Baslat(IEnumerator is_)
        {
            if (Calisiyor) return;
            Calisiyor = true;
            Durum = "başladı";
            Ornek.StartCoroutine(Ornek.Sar(is_));
        }

        private IEnumerator Sar(IEnumerator is_)
        {
            var yigin = new Stack<IEnumerator>();
            yigin.Push(is_);
            while (yigin.Count > 0)
            {
                bool devam;
                object adim = null;
                try
                {
                    devam = yigin.Peek().MoveNext();
                    if (devam) adim = yigin.Peek().Current;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Durum = "HATA: " + e.Message;
                    break;
                }
                if (!devam) { yigin.Pop(); continue; }
                if (adim is IEnumerator alt) { yigin.Push(alt); continue; }
                yield return adim;
            }
            // Hata ile çıkıldıysa yarım kalanların finally blokları çalışsın.
            while (yigin.Count > 0) (yigin.Pop() as IDisposable)?.Dispose();
            Temizle();
            if (!Durum.StartsWith("HATA")) Durum = "bitti";
            Calisiyor = false;
        }

        /// <summary>sure saniye kaydeder. Her karede önce kare(t) çağrılır (t: çekimin saniyesi), sonra ekran yazılır.</summary>
        public IEnumerator Kaydet(string ad, float sure, Action<float> kare)
        {
            string klasor = Path.Combine(Kok, ad);
            if (Directory.Exists(klasor)) Directory.Delete(klasor, true);
            Directory.CreateDirectory(klasor);
            if (Screen.width != En || Screen.height != Boy)
                Debug.LogWarning($"Fragman: Game görünümü {Screen.width}x{Screen.height}, {En}x{Boy} olmalı.");

            Time.captureFramerate = KareHizi;
            int sayi = Mathf.RoundToInt(sure * KareHizi);
            for (int i = 0; i < sayi; i++)
            {
                ArayuzuGizle();
                kare?.Invoke(i / (float)KareHizi);
                KamerayiEsle();
                yield return kareSonu;
                Texture2D doku = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(klasor, $"{i:00000}.jpg"), doku.EncodeToJPG(93));
                Destroy(doku);
                Durum = $"{ad} {i + 1}/{sayi}";
                yield return null;
            }
            Time.captureFramerate = 0;
        }

        /// <summary>Kayıt olmadan bekler (sahne otursun, sürü yürüsün); arayüz yine gizli.</summary>
        public IEnumerator Bekle(float sure, Action<float> kare = null)
        {
            float gecen = 0f;
            while (gecen < sure)
            {
                ArayuzuGizle();
                kare?.Invoke(gecen);
                KamerayiEsle();
                yield return null;
                gecen += Time.deltaTime;
            }
        }

        // ---- Sinema kamerası ----

        /// <summary>Sahnedeki kameraları kapatır, onların ayarlarıyla kendi kamerasını açar.</summary>
        public UnityEngine.Camera SinemaAc(float gorus = 50f)
        {
            KameralariGeriAc();
            oyunKamerasi = null;
            foreach (UnityEngine.Camera k in FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None))
            {
                if (k == sinema || !k.enabled || k.targetTexture != null) continue;
                if (oyunKamerasi == null || k.depth > oyunKamerasi.depth) oyunKamerasi = k;
                k.enabled = false;
                kapatilanKameralar.Add(k);
            }
            if (sinema == null)
            {
                var go = new GameObject("SinemaKamerasi");
                go.transform.SetParent(transform, false);
                sinema = go.AddComponent<UnityEngine.Camera>();
            }
            if (oyunKamerasi != null)
            {
                sinema.CopyFrom(oyunKamerasi);
                var oyunVerisi = oyunKamerasi.GetUniversalAdditionalCameraData();
                var veri = sinema.GetUniversalAdditionalCameraData();
                veri.renderPostProcessing = oyunVerisi.renderPostProcessing;
                veri.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                veri.antialiasingQuality = AntialiasingQuality.High;
            }
            sinema.cullingMask |= ekKatmanlar;
            sinema.fieldOfView = gorus;
            sinema.nearClipPlane = 0.03f;
            sinema.depth = 100f;
            sinema.enabled = true;
            return sinema;
        }

        /// <summary>Sinema kamerası kapanır, oyunun kameraları ve gizli gövdeler eski haline döner.</summary>
        public void SinemaKapat()
        {
            KameralariGeriAc();
            foreach (Renderer r in gosterilenler) if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            gosterilenler.Clear();
            ekKatmanlar = 0;
        }

        private void KameralariGeriAc()
        {
            foreach (UnityEngine.Camera k in kapatilanKameralar) if (k != null) k.enabled = true;
            kapatilanKameralar.Clear();
            if (sinema != null) sinema.enabled = false;
        }

        public void Koy(Vector3 konum, Vector3 bakilan, float gorus = -1f)
        {
            if (sinema == null) return;
            sinema.transform.SetPositionAndRotation(konum, Quaternion.LookRotation(bakilan - konum));
            if (gorus > 0f) sinema.fieldOfView = gorus;
        }

        public UnityEngine.Camera Sinema => sinema;

        // Su altına inince oyunun kamerasının arka plan rengi değişir; sinema kamerası da onu izler.
        private void KamerayiEsle()
        {
            if (sinema == null || !sinema.enabled || oyunKamerasi == null) return;
            sinema.clearFlags = oyunKamerasi.clearFlags;
            sinema.backgroundColor = oyunKamerasi.backgroundColor;
        }

        // ---- Yazı ve perde ----

        private void KatmaniKur()
        {
            var go = new GameObject("FragmanKatmani", typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            katman = go.GetComponent<Canvas>();
            katman.renderMode = RenderMode.ScreenSpaceOverlay;
            katman.sortingOrder = 32000;
            var olcek = go.GetComponent<CanvasScaler>();
            olcek.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            olcek.referenceResolution = new Vector2(En, Boy);

            ufuk = TamEkran<RawImage>("Ufuk");
            ufuk.gameObject.AddComponent<UI.Menu.UfukZemini>();
            ufuk.enabled = false;
            perde = TamEkran<Image>("Perde");
            perde.color = new Color(0f, 0f, 0f, 0f);

            ArayuzTemasi tema = ArayuzTemasi.Yuklu;
            yazi = TamEkran<Text>("Yazi");
            yazi.alignment = TextAnchor.MiddleCenter;
            yazi.font = tema != null ? tema.BaslikYazisi : Font.CreateDynamicFontFromOSFont("Georgia", 64);
            yazi.supportRichText = true;
            yazi.horizontalOverflow = HorizontalWrapMode.Overflow;
            yazi.verticalOverflow = VerticalWrapMode.Overflow;
            yazi.lineSpacing = 1.15f;

            altYazi = TamEkran<Text>("AltYazi");
            altYazi.alignment = TextAnchor.LowerCenter;
            altYazi.font = tema != null ? tema.GovdeYazisi : yazi.font;
            altYazi.horizontalOverflow = HorizontalWrapMode.Overflow;
            altYazi.rectTransform.offsetMin = new Vector2(0f, 90f);

            Yazi("", 0f);
            AltYazi("", 0f);
        }

        private T TamEkran<T>(string ad) where T : Graphic
        {
            var go = new GameObject(ad, typeof(RectTransform), typeof(T));
            go.transform.SetParent(katman.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            T g = go.GetComponent<T>();
            g.raycastTarget = false;
            return g;
        }

        /// <summary>Ortada büyük serif yazı (başlık kartı ya da görüntü üstü cümle).</summary>
        public void Yazi(string metin, float alfa, int boyut = 64)
        {
            yazi.text = metin;
            yazi.fontSize = boyut;
            Color renk = ArayuzTemasi.Yuklu != null ? ArayuzTemasi.Yuklu.Yazi : new Color(0.95f, 0.92f, 0.86f);
            renk.a = Mathf.Clamp01(alfa);
            yazi.color = renk;
            // Açık görüntü üstünde okunsun diye hafif gölge.
            var golge = yazi.GetComponent<Shadow>();
            if (golge == null) golge = yazi.gameObject.AddComponent<Shadow>();
            golge.effectColor = new Color(0f, 0f, 0f, 0.55f * renk.a);
            golge.effectDistance = new Vector2(2f, -2f);
        }

        /// <summary>Alt ortada küçük yazı (ör. "Yakında").</summary>
        public void AltYazi(string metin, float alfa, int boyut = 30)
        {
            altYazi.text = metin;
            altYazi.fontSize = boyut;
            Color renk = ArayuzTemasi.Yuklu != null ? ArayuzTemasi.Yuklu.Vurgu : new Color(0.89f, 0.67f, 0.43f);
            renk.a = Mathf.Clamp01(alfa);
            altYazi.color = renk;
        }

        public void Perde(float alfa) => perde.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alfa));

        public void Ufuk(bool acik) => ufuk.enabled = acik;

        // ---- Arayüzü gizleme ve temizlik ----

        /// <summary>Kapalıysa oyunun arayüzü çekimde görünür (yönetim ekranı).</summary>
        public bool ArayuzGizli { get; set; } = true;

        private readonly List<Renderer> gosterilenler = new List<Renderer>();
        private int ekKatmanlar;

        /// <summary>Birinci şahıs kamerası kaplumbağanın kendi modelini gizler (sadece gölge); sinema kamerasında görünsün.</summary>
        public void Goster(Transform kok)
        {
            if (kok == null) return;
            foreach (Renderer r in kok.GetComponentsInChildren<Renderer>(true))
            {
                // Gövde göz kamerasının çizmediği bir katmanda da olabilir; sinema kamerası o katmanı da çizer.
                ekKatmanlar |= 1 << r.gameObject.layer;
                if (sinema != null) sinema.cullingMask |= ekKatmanlar;
                if (r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                gosterilenler.Add(r);
            }
        }

        private void ArayuzuGizle()
        {
            if (!ArayuzGizli) return;
            foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c == katman || !c.isRootCanvas || !c.enabled || c.renderMode == RenderMode.WorldSpace) continue;
                c.enabled = false;
                gizlenenler.Add(c);
            }
        }

        public void ArayuzuGeriAc()
        {
            foreach (Canvas c in gizlenenler) if (c != null) c.enabled = true;
            gizlenenler.Clear();
        }

        private void Temizle()
        {
            Time.captureFramerate = 0;
            Time.timeScale = 1f;
            GameInput.Otomatik = null;
            SinemaKapat();
            ArayuzuGeriAc();
            ArayuzGizli = true;
            Yazi("", 0f);
            AltYazi("", 0f);
            Perde(0f);
            Ufuk(false);
        }
    }
}
