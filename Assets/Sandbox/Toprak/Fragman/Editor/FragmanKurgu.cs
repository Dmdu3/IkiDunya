using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Networking;

namespace IkiDunya.Fragman
{
    /// <summary>Kaydedilen çekimleri plana göre art arda dizer (kesme, çapraz geçiş, kararma), müziği ve efektleri karıştırır,
    /// Recordings/Fragman/IkiDunya_Fragman.mp4 yazar. Editörü kilitlememek için her editör karesinde birkaç kare kodlar.
    /// Döngüyle tek seferde hepsini kodlama: her kare GPU'ya doku yükler, editör karesi geçmeden boşalmaz (d3d12 belleği doldu, editör çöktü).</summary>
    public static class FragmanKurgu
    {
        private const int OrnekHizi = 48000;
        private const int Kanal = 2;
        private const int KareBasinaKodla = 8;

        public static string Durum { get; private set; } = "";
        public static string CikisYolu => Path.Combine(FragmanKaydi.Kok, "IkiDunya_Fragman.mp4");

        // ---- Plan ----

        /// <summary>Görüntü parçası: çekimin bas. saniyesinden sure saniye. capraz: öncekinin üstüne bu kadar saniye çapraz geçiş (0 kesme).
        /// kararmaGiris/Cikis: parçanın başında/sonunda siyahtan açılma/siyaha kararma.</summary>
        private sealed class Parca
        {
            public string Cekim { get; }
            public float Bas { get; }
            public float Sure { get; }
            public float Capraz { get; }
            public float KararmaGiris { get; }
            public float KararmaCikis { get; }
            /// <summary>Üstüne bindirilen yazı kartı (siyah zeminli kart çekimi) ve parçanın kaçıncı saniyesinde başladığı.</summary>
            public string Ust { get; private set; }
            public float UstBas { get; private set; }

            public Parca(string cekim, float bas, float sure, float capraz = 0f, float kararmaGiris = 0f, float kararmaCikis = 0f)
            {
                Cekim = cekim;
                Bas = bas;
                Sure = sure;
                Capraz = capraz;
                KararmaGiris = kararmaGiris;
                KararmaCikis = kararmaCikis;
            }

            public Parca Yazili(string kart, float bas)
            {
                Ust = kart;
                UstBas = bas;
                return this;
            }
        }

        /// <summary>Ses parçası: dosyanın klipBas. saniyesinden, fragmanın zaman. saniyesinde, sure saniye; giriş/çıkışta ses açılıp kısılır.</summary>
        private sealed class SesParcasi
        {
            public string Dosya { get; }
            public float Zaman { get; }
            public float KlipBas { get; }
            public float Sure { get; }
            public float Giris { get; }
            public float Cikis { get; }
            public float Seviye { get; }

            public SesParcasi(string dosya, float zaman, float klipBas, float sure, float giris, float cikis, float seviye)
            {
                Dosya = dosya;
                Zaman = zaman;
                KlipBas = klipBas;
                Sure = sure;
                Giris = giris;
                Cikis = cikis;
                Seviye = seviye;
            }
        }

        // Akış: anne döner (sakin) → kumsal değişmiş (tehdit) → iki dünya: gündüz yönetim, gece yavrular (hızlanır)
        // → parkurlar (en hızlı) → durak: "Her karar, bir hayat." → otuz yıl sonra → başlık.
        private static readonly Parca[] Plan =
        {
            new Parca("K_Dogdu", 0f, 3.5f),
            new Parca("DenizAnne", 0.5f, 8f, 1f).Yazili("K_Donuyor", 3f),
            new Parca("PosetSurusu", 1f, 4f, 0.8f),
            new Parca("YuvaAnne", 0.5f, 6f, 1f),
            new Parca("YuvaKazma", 1f, 4f),
            new Parca("KumsalTehdit", 0f, 6f, 0.8f).Yazili("K_Kumsal", 1.5f),
            new Parca("K_Gunduz", 0f, 3.5f, 0.5f),
            new Parca("Yonetim", 0.5f, 7f),
            new Parca("K_Gece", 0f, 3.5f, 0.5f),
            new Parca("KacisKosu", 1f, 6f),
            new Parca("KacisYakin", 0.5f, 3f),
            new Parca("CopSeli", 1f, 4f),
            new Parca("GirdapIci", 1f, 3.5f),
            new Parca("GirdapDisi", 1f, 4f),
            new Parca("IlkYuzus", 1f, 5f, 0.5f),
            new Parca("K_Karar", 0f, 3.5f, 0.3f),
            new Parca("Final", 0.5f, 10f, 0.8f).Yazili("K_Otuz", 1f),
            new Parca("Baslik", 0f, 8f, 1f),
        };

        private const string Muzik = "Assets/_Project/Audio/Music/";
        private const string Ortam = "Assets/_Project/Audio/Ambience/";
        private const string Efekt = "Assets/_Project/Audio/SFX/";

        /// <summary>Ses parçaları; zamanlar parçaların başlangıcına göre <see cref="SesleriPlanla"/>'da hesaplanır.</summary>
        private static SesParcasi[] Sesler = new SesParcasi[0];

        /// <summary>Müzik ve ortam sesleri parçalara bağlı: plan değişince kendiliğinden kayar.</summary>
        private static void SesleriPlanla(int[] baslar)
        {
            float Bas(string cekim) => baslar[Array.FindIndex(Plan, p => p.Cekim == cekim)] / (float)FragmanKaydi.KareHizi;
            float Son(string cekim)
            {
                int i = Array.FindIndex(Plan, p => p.Cekim == cekim);
                return (baslar[i] + Kare(Plan[i].Sure)) / (float)FragmanKaydi.KareHizi;
            }

            float sakinSon = Bas("KacisKosu");
            float hizliSon = Bas("K_Karar") + 0.6f;
            float finalBas = Bas("Final");
            float bitis = Son("Baslik");
            Sesler = new[]
            {
                // Müzik: ninni → Action Techno (yavrular koşmaya başlayınca) → final.
                new SesParcasi(Muzik + "Menu.mp3", 0f, MenuBas, sakinSon + 0.3f, 2f, 1.2f, 0.85f),
                new SesParcasi(Muzik + "Parkur.mp3", sakinSon - 0.1f, ParkurBas, hizliSon - sakinSon, 0.15f, 1f, 0.8f),
                new SesParcasi(Muzik + "Final.wav", finalBas - 0.5f, FinalBas, bitis - finalBas + 0.5f, 2.5f, 2.5f, 0.9f),
                // Ortam: su altı, kumsal gecesi, gündüz deniz.
                new SesParcasi(Ortam + "SuAlti.wav", Bas("DenizAnne"), 0f, Son("PosetSurusu") - Bas("DenizAnne"), 1.5f, 1f, 0.35f),
                new SesParcasi(Ortam + "KumsalGecesi.wav", Bas("YuvaAnne"), 0f, Son("KumsalTehdit") - Bas("YuvaAnne"), 1f, 1f, 0.4f),
                new SesParcasi(Ortam + "Final.wav", finalBas, 0f, bitis - finalBas, 2f, 3f, 0.35f),
                // Vurgular.
                new SesParcasi(Efekt + "KumKazma.wav", Bas("YuvaKazma") + 0.3f, 0f, 3.5f, 0.1f, 0.5f, 0.6f),
                new SesParcasi(Efekt + "SelGeliyor.wav", Bas("CopSeli"), 0f, 4.5f, 0.3f, 1f, 0.5f),
                new SesParcasi(Efekt + "KalpAtisi.wav", Bas("K_Karar"), 0f, 3.5f, 0.2f, 0.8f, 0.9f),
            };
        }

        // Müzik dosyalarında nereden girileceği (saniye; SesProfili ile bakıldı). Menü müziği başından yavaşça dolar;
        // Action Techno'nun 23. saniyesindeki vuruş girişi yavruların koşusunun ilk karesine denk gelir; final müziğinin dolgun kısmı 8. saniyede.
        private const float MenuBas = 0f;
        private const float ParkurBas = 22.9f;
        private const float FinalBas = 8f;

        // ---- Çalışma ----

        private sealed class Is
        {
            public int[] Baslar;
            public int ToplamKare;
            public float[] Ses;
            public MediaEncoder Kodlayici;
            public int Kare;
            public Texture2D Okunan;
            public Texture2D Cikis;
            public readonly Dictionary<string, UnityWebRequest> Istekler = new Dictionary<string, UnityWebRequest>();
            public readonly Dictionary<string, float[]> Cozulen = new Dictionary<string, float[]>();
        }

        private static Is suren;

        [MenuItem("İki Dünya/Fragman/Çekimleri kaydet (Menu sahnesinde Play'deyken)")]
        public static void CekimleriKaydet()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Fragman: önce Menu sahnesinden Play'e basın.");
                return;
            }
            // Editör arka plana geçse de oyun ilerlesin; kareler 1080p çıksın.
            Application.runInBackground = true;
            Type pencere = Type.GetType("UnityEditor.PlayModeWindow,UnityEditor");
            pencere?.GetMethod("SetCustomRenderingResolution", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(null, new object[] { (uint)FragmanKaydi.En, (uint)FragmanKaydi.Boy, "Fragman 1080p" });
            FragmanCekimleri.HepsiniCek();
        }

        [MenuItem("İki Dünya/Fragman/Kurguyu oluştur (MP4)")]
        public static void Olustur()
        {
            if (suren != null) return;
            var @is = new Is();

            // Zaman çizelgesi: her parça öncekinin sonundan (çapraz geçiş kadar önce) başlar.
            @is.Baslar = new int[Plan.Length];
            int son = 0;
            for (int i = 0; i < Plan.Length; i++)
            {
                Parca p = Plan[i];
                int kayitli = KareSayisi(p.Cekim);
                int istenen = Mathf.RoundToInt((p.Bas + p.Sure) * FragmanKaydi.KareHizi);
                if (kayitli < istenen)
                {
                    Durum = $"HATA: '{p.Cekim}' çekiminde {kayitli} kare var, {istenen} gerekli.";
                    Debug.LogError(Durum);
                    return;
                }
                int bas = i == 0 ? 0 : son - Kare(p.Capraz);
                @is.Baslar[i] = bas;
                son = bas + Kare(p.Sure);
            }
            @is.ToplamKare = son;
            SesleriPlanla(@is.Baslar);

            foreach (SesParcasi s in Sesler)
            {
                if (@is.Istekler.ContainsKey(s.Dosya)) continue;
                string tam = Path.GetFullPath(s.Dosya);
                AudioType tur = s.Dosya.EndsWith(".mp3") ? AudioType.MPEG : s.Dosya.EndsWith(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV;
                UnityWebRequest istek = UnityWebRequestMultimedia.GetAudioClip(new Uri(tam).AbsoluteUri, tur);
                ((DownloadHandlerAudioClip)istek.downloadHandler).streamAudio = false;
                ((DownloadHandlerAudioClip)istek.downloadHandler).compressed = false;
                istek.SendWebRequest();
                @is.Istekler[s.Dosya] = istek;
            }

            suren = @is;
            Durum = "sesler yükleniyor";
            EditorApplication.update += Adim;
        }

        private static void Adim()
        {
            Is @is = suren;
            try
            {
                if (@is.Kodlayici == null)
                {
                    foreach (var istek in @is.Istekler) if (!istek.Value.isDone) return;
                    SesleriKur(@is);
                    KodlayiciyiAc(@is);
                    return;
                }

                for (int n = 0; n < KareBasinaKodla && @is.Kare < @is.ToplamKare; n++, @is.Kare++)
                {
                    KareyiOlustur(@is, @is.Kare);
                    @is.Kodlayici.AddFrame(@is.Cikis);
                    int ornekKare = OrnekHizi / FragmanKaydi.KareHizi * Kanal;
                    using (var parca = new NativeArray<float>(ornekKare, Allocator.Temp))
                    {
                        NativeArray<float>.Copy(@is.Ses, @is.Kare * ornekKare, parca, 0, ornekKare);
                        @is.Kodlayici.AddSamples(parca);
                    }
                }
                Durum = $"kodlanıyor {@is.Kare}/{@is.ToplamKare}";
                if (@is.Kare >= @is.ToplamKare)
                {
                    Bitir();
                    Durum = "bitti: " + CikisYolu;
                    Debug.Log("Fragman: " + CikisYolu);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Bitir();
                Durum = "HATA: " + e.Message;
            }
        }

        private static void Bitir()
        {
            EditorApplication.update -= Adim;
            if (suren == null) return;
            suren.Kodlayici?.Dispose();
            foreach (var istek in suren.Istekler) istek.Value.Dispose();
            if (suren.Okunan != null) UnityEngine.Object.DestroyImmediate(suren.Okunan);
            if (suren.Cikis != null) UnityEngine.Object.DestroyImmediate(suren.Cikis);
            suren = null;
        }

        private static void KodlayiciyiAc(Is @is)
        {
            var goruntu = new VideoTrackAttributes
            {
                frameRate = new MediaRational(FragmanKaydi.KareHizi),
                width = FragmanKaydi.En,
                height = FragmanKaydi.Boy,
                includeAlpha = false,
                bitRateMode = VideoBitrateMode.High
            };
            var ses = new AudioTrackAttributes
            {
                sampleRate = new MediaRational(OrnekHizi),
                channelCount = Kanal,
                language = "tr"
            };
            if (File.Exists(CikisYolu)) File.Delete(CikisYolu);
            @is.Kodlayici = new MediaEncoder(CikisYolu, goruntu, ses);
            @is.Okunan = new Texture2D(2, 2, TextureFormat.RGB24, false);
            @is.Cikis = new Texture2D(FragmanKaydi.En, FragmanKaydi.Boy, TextureFormat.RGBA32, false);
        }

        // ---- Görüntü ----

        private static void KareyiOlustur(Is @is, int kare)
        {
            NativeArray<byte> cikis = @is.Cikis.GetRawTextureData<byte>();
            bool ilk = true;
            for (int i = 0; i < Plan.Length; i++)
            {
                Parca p = Plan[i];
                int yerel = kare - @is.Baslar[i];
                int uzunluk = Kare(p.Sure);
                if (yerel < 0 || yerel >= uzunluk) continue;

                // Parçanın kendi kararması, ve önceki parçanın üstüne çapraz geçişte ağırlığı.
                float isik = 1f;
                if (p.KararmaGiris > 0f) isik = Mathf.Min(isik, yerel / (p.KararmaGiris * FragmanKaydi.KareHizi));
                if (p.KararmaCikis > 0f) isik = Mathf.Min(isik, (uzunluk - 1 - yerel) / (p.KararmaCikis * FragmanKaydi.KareHizi));
                isik = Mathf.Clamp01(isik);
                float agirlik = ilk || p.Capraz <= 0f ? 1f : Mathf.Clamp01((yerel + 1) / (p.Capraz * FragmanKaydi.KareHizi));

                string dosya = Path.Combine(FragmanKaydi.Kok, p.Cekim, $"{Kare(p.Bas) + yerel:00000}.jpg");
                @is.Okunan.LoadImage(File.ReadAllBytes(dosya));
                NativeArray<byte> kaynak = @is.Okunan.GetRawTextureData<byte>();
                Karistir(kaynak, @is.Okunan.format, cikis, isik, ilk ? 1f : agirlik);
                ilk = false;
            }

            for (int i = 0; i < Plan.Length; i++)
            {
                Parca p = Plan[i];
                if (p.Ust == null) continue;
                int yerel = kare - @is.Baslar[i] - Kare(p.UstBas);
                int uzunluk = KareSayisi(p.Ust);
                if (yerel < 0 || yerel >= uzunluk || kare - @is.Baslar[i] >= Kare(p.Sure)) continue;
                @is.Okunan.LoadImage(File.ReadAllBytes(Path.Combine(FragmanKaydi.Kok, p.Ust, $"{yerel:00000}.jpg")));
                // Yazı varken görüntü biraz kararır (okunsun); kartın belirmesiyle aynı hızda.
                float varlik = Mathf.Clamp01(Mathf.Min(yerel, uzunluk - 1 - yerel) / (0.6f * FragmanKaydi.KareHizi));
                Bindir(@is.Okunan.GetRawTextureData<byte>(), @is.Okunan.format, cikis, 1f - 0.35f * Mathf.SmoothStep(0f, 1f, varlik));
            }
            if (ilk) for (int j = 0; j < cikis.Length; j++) cikis[j] = (j & 3) == 3 ? (byte)255 : (byte)0;
            @is.Cikis.Apply(false);
        }

        private static void Karistir(NativeArray<byte> kaynak, TextureFormat bicim, NativeArray<byte> cikis, float isik, float agirlik)
        {
            int adim = bicim == TextureFormat.RGB24 ? 3 : 4;
            int ofset = bicim == TextureFormat.ARGB32 ? 1 : 0;
            int pikselSayisi = FragmanKaydi.En * FragmanKaydi.Boy;
            int carpan = Mathf.RoundToInt(isik * agirlik * 256f);
            int kalan = Mathf.RoundToInt((1f - agirlik) * 256f);
            for (int p = 0, k = ofset, c = 0; p < pikselSayisi; p++, k += adim, c += 4)
            {
                cikis[c] = (byte)((kaynak[k] * carpan + cikis[c] * kalan) >> 8);
                cikis[c + 1] = (byte)((kaynak[k + 1] * carpan + cikis[c + 1] * kalan) >> 8);
                cikis[c + 2] = (byte)((kaynak[k + 2] * carpan + cikis[c + 2] * kalan) >> 8);
                cikis[c + 3] = 255;
            }
        }

        /// <summary>Bindirilen yazı kartın ortasından bu kadar piksel aşağıda durur (alt üçte bir; ortadaki kaplumbağayı örtmesin).</summary>
        private const int UstYaziAsagi = 330;

        /// <summary>Siyah zeminli yazıyı görüntünün üstüne koyar: yazının parlaklığı saydamlığıdır (siyah = görüntü, krem = yazı).</summary>
        private static void Bindir(NativeArray<byte> kart, TextureFormat bicim, NativeArray<byte> cikis, float karartma)
        {
            int adim = bicim == TextureFormat.RGB24 ? 3 : 4;
            int ofset = bicim == TextureFormat.ARGB32 ? 1 : 0;
            int pikselSayisi = FragmanKaydi.En * FragmanKaydi.Boy;
            int koyu = Mathf.RoundToInt(karartma * 256f);
            // Doku alttan üste dizili: ekranda aşağı kaydırmak için satır r, kartın r + UstYaziAsagi satırını alır.
            int kaynakFarki = UstYaziAsagi * FragmanKaydi.En;
            for (int p = 0, c = 0; p < pikselSayisi; p++, c += 4)
            {
                int kp = p + kaynakFarki;
                int r = 0, g = 0, b = 0;
                if (kp < pikselSayisi)
                {
                    int k = kp * adim + ofset;
                    r = kart[k];
                    g = kart[k + 1];
                    b = kart[k + 2];
                }
                int a = Math.Min(255, Math.Max(r, Math.Max(g, b)) * 255 / 236);
                // JPG gürültüsü siyahı tam sıfır bırakmaz; çok koyu pikseller yazı sayılmaz.
                if (a < 10) { r = g = b = 0; a = 0; }
                int kalan = (255 - a) * koyu >> 8;
                cikis[c] = (byte)Math.Min(255, r + (cikis[c] * kalan >> 8));
                cikis[c + 1] = (byte)Math.Min(255, g + (cikis[c + 1] * kalan >> 8));
                cikis[c + 2] = (byte)Math.Min(255, b + (cikis[c + 2] * kalan >> 8));
            }
        }

        // ---- Ses ----

        private static void SesleriKur(Is @is)
        {
            foreach (var istek in @is.Istekler)
            {
                if (istek.Value.result != UnityWebRequest.Result.Success)
                    throw new IOException($"{istek.Key} okunamadı: {istek.Value.error}");
                AudioClip klip = DownloadHandlerAudioClip.GetContent(istek.Value);
                var veri = new float[klip.samples * klip.channels];
                klip.GetData(veri, 0);
                @is.Cozulen[istek.Key] = Donustur(veri, klip.channels, klip.frequency);
                UnityEngine.Object.DestroyImmediate(klip);
            }

            int ornekKare = OrnekHizi / FragmanKaydi.KareHizi;
            @is.Ses = new float[@is.ToplamKare * ornekKare * Kanal];
            foreach (SesParcasi s in Sesler)
            {
                float[] kaynak = @is.Cozulen[s.Dosya];
                int bas = Mathf.RoundToInt(s.Zaman * OrnekHizi);
                int klipBas = Mathf.RoundToInt(s.KlipBas * OrnekHizi);
                int uzunluk = Mathf.RoundToInt(s.Sure * OrnekHizi);
                for (int i = 0; i < uzunluk; i++)
                {
                    int hedef = bas + i;
                    int okunan = klipBas + i;
                    if (hedef < 0 || hedef * Kanal >= @is.Ses.Length || okunan * Kanal >= kaynak.Length) continue;
                    float t = i / (float)OrnekHizi;
                    float zarf = s.Seviye;
                    if (s.Giris > 0f) zarf *= Mathf.Clamp01(t / s.Giris);
                    if (s.Cikis > 0f) zarf *= Mathf.Clamp01((s.Sure - t) / s.Cikis);
                    @is.Ses[hedef * Kanal] += kaynak[okunan * Kanal] * zarf;
                    @is.Ses[hedef * Kanal + 1] += kaynak[okunan * Kanal + 1] * zarf;
                }
            }
            // Üst üste binen seslerde patlama olmasın: yumuşak sınır.
            for (int i = 0; i < @is.Ses.Length; i++) @is.Ses[i] = (float)Math.Tanh(@is.Ses[i]);
        }

        /// <summary>Herhangi bir kanal sayısı ve örnek hızından 48 kHz stereoya (doğrusal ara değer).</summary>
        private static float[] Donustur(float[] veri, int kanal, int hiz)
        {
            int kaynakUzunluk = veri.Length / kanal;
            int uzunluk = (int)((long)kaynakUzunluk * OrnekHizi / hiz);
            var cikis = new float[uzunluk * Kanal];
            for (int i = 0; i < uzunluk; i++)
            {
                double yer = (double)i * hiz / OrnekHizi;
                int a = (int)yer;
                int b = Math.Min(a + 1, kaynakUzunluk - 1);
                float t = (float)(yer - a);
                for (int c = 0; c < Kanal; c++)
                {
                    int kc = Math.Min(c, kanal - 1);
                    cikis[i * Kanal + c] = Mathf.Lerp(veri[a * kanal + kc], veri[b * kanal + kc], t);
                }
            }
            return cikis;
        }

        /// <summary>Müziğin nereden girileceğini seçmek için: dosyanın her yarım saniyesinin ses gücünü Console'a yazar.</summary>
        public static void SesProfili(string dosya)
        {
            AudioType tur = dosya.EndsWith(".mp3") ? AudioType.MPEG : dosya.EndsWith(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV;
            UnityWebRequest istek = UnityWebRequestMultimedia.GetAudioClip(new Uri(Path.GetFullPath(dosya)).AbsoluteUri, tur);
            ((DownloadHandlerAudioClip)istek.downloadHandler).streamAudio = false;
            istek.SendWebRequest();
            void Bak()
            {
                if (!istek.isDone) return;
                EditorApplication.update -= Bak;
                AudioClip klip = DownloadHandlerAudioClip.GetContent(istek);
                var veri = new float[klip.samples * klip.channels];
                klip.GetData(veri, 0);
                int pencere = klip.frequency / 2 * klip.channels;
                var satir = new System.Text.StringBuilder($"{dosya} {klip.length:0.0} sn, yarım saniyelik güç:\n");
                for (int bas = 0, i = 0; bas + pencere <= veri.Length; bas += pencere, i++)
                {
                    double toplam = 0;
                    for (int j = bas; j < bas + pencere; j++) toplam += veri[j] * veri[j];
                    satir.Append($"{i * 0.5f:0.0}:{Math.Sqrt(toplam / pencere):0.000} ");
                }
                Debug.Log(satir.ToString());
                UnityEngine.Object.DestroyImmediate(klip);
                istek.Dispose();
            }
            EditorApplication.update += Bak;
        }

        // ---- Yardımcılar ----

        private static int Kare(float saniye) => Mathf.RoundToInt(saniye * FragmanKaydi.KareHizi);

        private static int KareSayisi(string cekim)
        {
            string klasor = Path.Combine(FragmanKaydi.Kok, cekim);
            return Directory.Exists(klasor) ? Directory.GetFiles(klasor, "*.jpg").Length : 0;
        }
    }
}
