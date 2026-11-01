using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IkiDunya.Core;
using IkiDunya.Management;
using IkiDunya.Turtle;
using UnityEngine;

namespace IkiDunya.Fragman
{
    /// <summary>Fragmanın çekimleri. Her çekim oyunu ilgili adıma götürür, otomatik sürer, sinema kamerasıyla kaydeder.
    /// Play'de (Menu sahnesinden) <see cref="Cek"/> ya da <see cref="HepsiniCek"/>; kurgu Editor/FragmanKurgu ile.
    /// Fragman tanıtım içindir, oyunda görünmez: yazıları bu yüzden metinler.md'de değil burada.</summary>
    public static class FragmanCekimleri
    {
        private static FragmanKaydi K => FragmanKaydi.Ornek;

        /// <summary>Kartların yazıları (siyah zeminde; kurguda görüntünün üstüne de bindirilir).</summary>
        private static readonly (string ad, string metin)[] Kartlar =
        {
            ("K_Dogdu", "Otuz yıl önce bu kumsalda doğdu."),
            ("K_Donuyor", "Şimdi yumurtlamak için geri dönüyor."),
            ("K_Kumsal", "Ama kumsal artık eskisi gibi değil."),
            ("K_Gunduz", "Gündüz kumsalı sen yönetirsin."),
            ("K_Gece", "Gece onların yerindesin."),
            ("K_Karar", "Her karar, bir hayat."),
            ("K_Otuz", "Otuz yıl sonra..."),
        };

        /// <summary>Çekimlerin adları, kayıt sırasıyla.</summary>
        public static readonly string[] Adlar =
        {
            "DenizAnne", "PosetSurusu", "YuvaAnne", "YuvaKazma", "KumsalTehdit", "Yonetim",
            "KacisKosu", "KacisYakin", "CopSeli", "GirdapIci", "GirdapDisi", "IlkYuzus", "Final",
            "K_Dogdu", "K_Donuyor", "K_Kumsal", "K_Gunduz", "K_Gece", "K_Karar", "K_Otuz", "Baslik",
        };

        public static void Cek(string ad) => FragmanKaydi.Baslat(KayitKorunarak(Tek(ad)));

        public static void HepsiniCek() => FragmanKaydi.Baslat(KayitKorunarak(Hepsi(Adlar)));

        /// <summary>Virgülle ayrılmış birkaç çekim.</summary>
        public static void BirkacCek(string adlar) => FragmanKaydi.Baslat(KayitKorunarak(Hepsi(adlar.Split(','))));

        private static IEnumerator Hepsi(IEnumerable<string> adlar)
        {
            foreach (string ad in adlar) yield return Tek(ad.Trim());
        }

        private static IEnumerator Tek(string ad)
        {
            foreach (var kart in Kartlar) if (kart.ad == ad) return Kart(kart.ad, kart.metin);
            switch (ad)
            {
                case "DenizAnne": return DenizAnne();
                case "PosetSurusu": return PosetSurusuCekimi();
                case "YuvaAnne": return YuvaAnne();
                case "YuvaKazma": return YuvaKazma();
                case "KumsalTehdit": return KumsalTehdit();
                case "Yonetim": return Yonetim();
                case "KacisKosu": return KacisKosu();
                case "KacisYakin": return KacisYakin();
                case "CopSeli": return CopSeliCekimi();
                case "GirdapIci": return GirdapIci();
                case "GirdapDisi": return GirdapDisi();
                case "IlkYuzus": return IlkYuzus();
                case "Final": return Final();
                case "Baslik": return Baslik();
                default: throw new ArgumentException("Böyle çekim yok: " + ad);
            }
        }

        // ---- Deniz ----

        // Bölüm 1: anne ışık huzmeleri arasında kıyıya yüzer; kamera yanından yavaşça arkasına döner.
        private static IEnumerator DenizAnne()
        {
            yield return AdimaGit(0, 1.5f);
            DenizHareketi anne = Bul<DenizHareketi>();
            K.Goster(anne.transform);
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            K.SinemaAc(50f);
            float boy = Boyut(anne.transform);
            Vector3 kamera = Vector3.zero;
            yield return K.Kaydet("DenizAnne", 9f, t =>
            {
                Transform a = anne.transform;
                float aci = Mathf.Lerp(-70f, -20f, Yumusak(t / 9f));
                Vector3 yan = Quaternion.AngleAxis(aci, Vector3.up) * -a.forward;
                Vector3 hedef = a.position + yan * boy * 3.2f + Vector3.up * boy * 0.6f;
                kamera = t == 0f ? hedef : Vector3.Lerp(kamera, hedef, 0.1f);
                K.Koy(kamera, a.position + a.forward * boy * 1.5f);
            });
        }

        // Poşet sürüsü parkuru: akıntı denizanası ve poşet karışık sürüyü annenin önünden yana taşır; kamera annenin yanında.
        private static IEnumerator PosetSurusuCekimi()
        {
            yield return ParkuraGit(ParkurTuru.PosetSurusu, 2f);
            DenizHareketi anne = Bul<DenizHareketi>();
            K.Goster(anne.transform);
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 0.35f) };
            K.SinemaAc(55f);
            float boy = Boyut(anne.transform);
            yield return K.Kaydet("PosetSurusu", 7f, t =>
            {
                Transform a = anne.transform;
                Vector3 kamera = a.position - a.right * boy * 2.2f - a.forward * boy * 1.2f + Vector3.up * boy * 0.3f;
                K.Koy(kamera, a.position + a.forward * boy * 2.5f + a.right * boy * Mathf.Lerp(0.5f, 1.5f, t / 7f));
            });
        }

        // ---- Yuva gecesi ----

        // Anne ay ışığında kumsala çıkar; kamera arkasından, ay ve kumullar kadrajda.
        private static IEnumerator YuvaAnne()
        {
            yield return AdimaGit(2, 1.5f);
            AnneKara anne = Bul<AnneKara>();
            K.Goster(anne.transform);
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            K.SinemaAc(42f);
            float boy = Boyut(anne.transform);
            Vector3 kamera = Vector3.zero;
            yield return K.Kaydet("YuvaAnne", 8f, t =>
            {
                Transform a = anne.transform;
                float uzak = Mathf.Lerp(4.5f, 2.6f, Yumusak(t / 8f));
                Vector3 hedef = a.position - a.forward * boy * uzak + a.right * boy * 0.8f + Vector3.up * boy * 0.9f;
                kamera = t == 0f ? hedef : Vector3.Lerp(kamera, hedef, 0.08f);
                K.Koy(kamera, a.position + a.forward * boy * 3f + Vector3.up * boy * 0.6f);
            });
        }

        // Anne kumda durur, sırayla A ve D ile yuva kazar; kamera alçaktan yanında.
        private static IEnumerator YuvaKazma()
        {
            yield return AdimaGit(2, 1f);
            AnneKara anne = Bul<AnneKara>();
            K.Goster(anne.transform);
            var girdi = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            GameInput.Otomatik = girdi;
            yield return K.Bekle(5f);
            girdi.Hareket = Vector2.zero;
            yield return K.Bekle(0.5f);
            K.SinemaAc(40f);
            float boy = Boyut(anne.transform);
            int kare = 0;
            yield return K.Kaydet("YuvaKazma", 7f, t =>
            {
                kare++;
                girdi.KazSolBasildi = kare % 8 == 0;
                girdi.KazSagBasildi = kare % 8 == 4;
                Transform a = anne.transform;
                float aci = Mathf.Lerp(60f, 85f, t / 7f);
                Vector3 yan = Quaternion.AngleAxis(aci, Vector3.up) * a.forward;
                K.Koy(a.position + yan * boy * 2.4f + Vector3.up * boy * 0.5f, a.position + Vector3.up * boy * 0.15f);
            });
        }

        // Kumsalın tehditleri: annenin hizasından ışıklara ve fenerli insana doğru yavaş ilerleyen kamera.
        private static IEnumerator KumsalTehdit()
        {
            yield return AdimaGit(2, 1.5f);
            AnneKara anne = Bul<AnneKara>();
            Hazards.Insan insan = UnityEngine.Object.FindObjectsByType<Hazards.Insan>(FindObjectsSortMode.None)
                .OrderBy(i => Vector3.Distance(i.transform.position, anne.transform.position)).FirstOrDefault();
            if (insan == null) throw new InvalidOperationException("Kumsalda insan yok.");
            K.SinemaAc(38f);
            Vector3 bas = anne.transform.position + Vector3.up * 0.5f;
            yield return K.Kaydet("KumsalTehdit", 7f, t =>
            {
                Vector3 hedef = insan.transform.position + Vector3.up * 1f;
                Vector3 yol = Vector3.Lerp(bas, hedef, Mathf.Lerp(0.05f, 0.3f, t / 7f));
                yol.y = Zemin(yol) + 0.45f;
                K.Koy(yol, hedef);
            });
        }

        // ---- İnsan kısmı ----

        // Yönetim ekranı arayüzüyle: rapor, gönüllüler, kartlar.
        private static IEnumerator Yonetim()
        {
            yield return AdimaGit(3, 1.5f);
            K.ArayuzGizli = false;
            K.ArayuzuGeriAc();
            InsanTuru tur = Bul<InsanTuru>();
            // Gönüllüler boştayken İleri önce uyarır, ikinci basışta geçer.
            float[] ileriAnlari = { 2.5f, 5f, 5.05f };
            int basilan = 0;
            yield return K.Kaydet("Yonetim", 9f, t =>
            {
                if (basilan < ileriAnlari.Length && t >= ileriAnlari[basilan]) { basilan++; tur.Ilerle(); }
            });
            K.ArayuzGizli = true;
        }

        // ---- Kaçış gecesi ----

        // Yavrular kumdan çıkıp denize koşar; kamera alçaktan arkalarında, ay ve deniz önde.
        private static IEnumerator KacisKosu()
        {
            Suru suru = null;
            yield return KacisaHazirla(s => suru = s);
            K.SinemaAc(40f);
            Vector3 kamera = Vector3.zero;
            yield return K.Kaydet("KacisKosu", 9f, t =>
            {
                GosterHepsi(suru);
                Vector3 merkez = SuruMerkezi(suru, out Vector3 ileri);
                Vector3 hedef = merkez - ileri * Mathf.Lerp(1.4f, 0.9f, t / 9f) + Vector3.Cross(Vector3.up, ileri) * 0.25f;
                hedef.y = Zemin(hedef) + 0.22f;
                kamera = t == 0f ? hedef : Vector3.Lerp(kamera, hedef, 0.08f);
                K.Koy(kamera, merkez + ileri * 2.5f + Vector3.up * 0.15f);
            });
        }

        // Öncü yavruya önden, çok yakından bakış.
        private static IEnumerator KacisYakin()
        {
            Suru suru = null;
            yield return KacisaHazirla(s => suru = s);
            K.SinemaAc(35f);
            yield return K.Kaydet("KacisYakin", 6f, t =>
            {
                GosterHepsi(suru);
                Transform o = suru.Oncu.transform;
                Vector3 ileri = Vector3.ProjectOnPlane(o.forward, Vector3.up).normalized;
                Vector3 kamera = o.position + ileri * Mathf.Lerp(0.45f, 0.35f, t / 6f) + Vector3.Cross(Vector3.up, ileri) * 0.12f;
                kamera.y = Zemin(kamera) + 0.07f;
                K.Koy(kamera, o.position + Vector3.up * 0.03f);
            });
        }

        private static IEnumerator KacisaHazirla(Action<Suru> sonuc)
        {
            yield return AdimaGit(13, 0.5f);
            KacisGecesi gece = Bul<KacisGecesi>();
            // Yüzey soğumuş olsun: bütün yavrular çıksın.
            typeof(KacisGecesi).GetField("gecen", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(gece, 9999f);
            typeof(KacisGecesi).GetMethod("YuzeyeCik", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(gece, null);
            Suru suru = Bul<Suru>();
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            yield return K.Bekle(1f);
            sonuc(suru);
        }

        // Çöp seli parkuru: ufuktan köpük çizgisi gelir, yavrular kumda; kamera yukarıdan arkalarında.
        private static IEnumerator CopSeliCekimi()
        {
            yield return ParkuraGit(ParkurTuru.CopSeli, 0.2f);
            Suru suru = Bul<Suru>();
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            K.SinemaAc(50f);
            Vector3 kamera = Vector3.zero;
            // Alçaktan, sürünün arkasından denize: köpük çizgisi ufuktan gelip su kumsala yürür.
            yield return K.Kaydet("CopSeli", 10f, t =>
            {
                GosterHepsi(suru);
                Vector3 merkez = SuruMerkezi(suru, out Vector3 ileri);
                Vector3 hedef = merkez - ileri * 1.3f + Vector3.up * 0.4f;
                kamera = t == 0f ? hedef : Vector3.Lerp(kamera, hedef, 0.08f);
                K.Koy(kamera, merkez + ileri * 8f + Vector3.up * 0.2f);
            });
        }

        // ---- İlk yüzüş ----

        // Çöp girdabı, yavrunun gözünden: dönen halkaların arasından boşluğa doğru.
        private static IEnumerator GirdapIci()
        {
            yield return ParkuraGit(ParkurTuru.CopGirdabi, 1.5f);
            var girdi = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            GameInput.Otomatik = girdi;
            yield return K.Kaydet("GirdapIci", 7f, t => girdi.Bakis = new Vector2(1.6f + Mathf.Sin(t) * 0.8f, Mathf.Sin(t * 0.7f) * 0.3f));
        }

        // Çöp girdabı dışarıdan: kamera halkaların çevresinde yavaşça döner, ortada yavru.
        private static IEnumerator GirdapDisi()
        {
            yield return ParkuraGit(ParkurTuru.CopGirdabi, 1.5f);
            CopGirdabi girdap = Bul<CopGirdabi>();
            DenizHareketi yavru = Bul<DenizHareketi>();
            K.Goster(yavru.transform);
            Transform merkez = (Transform)typeof(CopGirdabi).GetField("merkez", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(girdap);
            K.SinemaAc(55f);
            yield return K.Kaydet("GirdapDisi", 8f, t =>
            {
                float aci = 20f + t * 9f;
                Vector3 yon = Quaternion.AngleAxis(aci, Vector3.up) * Vector3.forward;
                Vector3 kamera = merkez.position + yon * Mathf.Lerp(9f, 7f, t / 8f) + Vector3.up * 2.5f;
                K.Koy(kamera, Vector3.Lerp(merkez.position, yavru.transform.position, 0.5f));
            });
        }

        // Açık deniz: yavru maviliğin içinde yüzer; kamera arkasından ve biraz altından, yüzey ışığı üstte.
        private static IEnumerator IlkYuzus()
        {
            yield return AdimaGit(21, 1.5f);
            DenizHareketi yavru = Bul<DenizHareketi>();
            K.Goster(yavru.transform);
            GameInput.Otomatik = new OtomatikGirdi { Hareket = new Vector2(0f, 1f) };
            K.SinemaAc(50f);
            Vector3 kamera = Vector3.zero;
            yield return K.Kaydet("IlkYuzus", 8f, t =>
            {
                // Yavru küçük: kamera sabit, yakın mesafede (bütün boyutlu ölçü yavrunun çevresindeki parçaları da sayıyor).
                Transform y = yavru.transform;
                Vector3 hedef = y.position - y.forward * Mathf.Lerp(0.9f, 0.6f, t / 8f) - Vector3.up * 0.2f + y.right * 0.3f;
                kamera = t == 0f ? hedef : Vector3.Lerp(kamera, hedef, 0.12f);
                K.Koy(kamera, y.position + y.forward * 0.6f + Vector3.up * 0.15f);
            });
        }

        // ---- Final ----

        // Otuz yıl sonra: büyümüş dişi denizden kumsala yürür (oyunun kendi final kamerası, serin güneş).
        private static IEnumerator Final()
        {
            yield return AdimaGit(22, 0f, () => Kumsal().TemizEnerjiAyarla(true));
            yield return K.Kaydet("Final", 11f, null);
        }

        // ---- Kartlar ----

        private static IEnumerator Kart(string ad, string metin)
        {
            K.Perde(1f);
            yield return K.Kaydet(ad, 3.5f, t => K.Yazi(metin, Belirme(t, 3.5f, 0.6f), 64));
            K.Perde(0f);
            K.Yazi("", 0f);
        }

        private static IEnumerator Baslik()
        {
            K.Ufuk(true);
            K.Perde(1f);
            yield return K.Kaydet("Baslik", 8f, t =>
            {
                K.Perde(1f - Yumusak(t / 1.5f));
                K.Yazi("İki Dünya, Tek Yaşam", Yumusak((t - 1f) / 1.5f) * (1f - Yumusak((t - 7.2f) / 0.8f)), 110);
                K.AltYazi("YAKINDA", Yumusak((t - 3f) / 1.2f) * (1f - Yumusak((t - 7.2f) / 0.8f)), 34);
            });
            K.Ufuk(false);
            K.Perde(0f);
            K.Yazi("", 0f);
            K.AltYazi("", 0f);
        }

        // ---- Yardımcılar ----

        /// <summary>Akışın verilen adımına geçer (önce kumsal sıfırlanır, sonra hazirla), geçiş bitince otur saniye bekler.</summary>
        private static IEnumerator AdimaGit(int sira, float otur, Action hazirla = null)
        {
            Type tip = typeof(OyunAkisi);
            const BindingFlags ic = BindingFlags.Instance | BindingFlags.NonPublic;
            object akis = tip.GetField("ornek", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            if (akis == null) throw new InvalidOperationException("OyunAkisi yok: Menu sahnesinden Play'e basın.");
            GameInput.Otomatik = null;
            K.SinemaKapat();
            tip.GetMethod("KumsaliSifirla", ic).Invoke(akis, null);
            hazirla?.Invoke();
            tip.GetMethod("AdimaGec", ic).Invoke(akis, new object[] { sira });
            yield return null;
            while (OyunAkisi.GecisSuruyor) yield return null;
            yield return K.Bekle(otur);
        }

        private static IEnumerator ParkuraGit(ParkurTuru tur, float otur)
        {
            GameInput.Otomatik = null;
            K.SinemaKapat();
            OyunAkisi.ParkurOyna(tur);
            yield return null;
            while (OyunAkisi.GecisSuruyor) yield return null;
            yield return K.Bekle(otur);
        }

        private static KumsalDurumu Kumsal() => (KumsalDurumu)Resources.FindObjectsOfTypeAll(typeof(KumsalDurumu))[0];

        private static T Bul<T>() where T : UnityEngine.Object
        {
            T bulunan = UnityEngine.Object.FindAnyObjectByType<T>();
            if (bulunan == null) throw new InvalidOperationException(typeof(T).Name + " sahnede yok.");
            return bulunan;
        }

        private static float Yumusak(float x) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(x));

        /// <summary>sure boyunca: giris saniyede belirir, sonda aynı sürede kaybolur.</summary>
        private static float Belirme(float t, float sure, float gecis) => Yumusak(t / gecis) * (1f - Yumusak((t - (sure - gecis)) / gecis));

        /// <summary>Modelin en büyük boyutu (kamera mesafeleri buna göre).</summary>
        private static float Boyut(Transform kok)
        {
            Renderer[] r = kok.GetComponentsInChildren<Renderer>();
            if (r.Length == 0) return 1f;
            Bounds b = r[0].bounds;
            foreach (Renderer x in r) b.Encapsulate(x.bounds);
            return Mathf.Max(b.size.x, b.size.z, 0.05f);
        }

        private static float Zemin(Vector3 p)
        {
            return Physics.Raycast(p + Vector3.up * 50f, Vector3.down, out RaycastHit vurus, 100f, ~0, QueryTriggerInteraction.Ignore)
                ? vurus.point.y : p.y;
        }

        private static void GosterHepsi(Suru suru)
        {
            foreach (Yavru y in suru.Yavrular) K.Goster(y.transform);
        }

        /// <summary>Kumdaki yavruların ortası; ileri: öncünün baktığı yön (yatay).</summary>
        private static Vector3 SuruMerkezi(Suru suru, out Vector3 ileri)
        {
            Vector3 toplam = Vector3.zero;
            int sayi = 0;
            foreach (Yavru y in suru.Yavrular)
            {
                if (y.SuAnki != Yavru.Durum.Suruyor) continue;
                toplam += y.transform.position;
                sayi++;
            }
            Transform o = suru.Oncu != null ? suru.Oncu.transform : suru.transform;
            ileri = Vector3.ProjectOnPlane(o.forward, Vector3.up).normalized;
            return sayi > 0 ? toplam / sayi : o.position;
        }

        /// <summary>Çekimler oyunun adımlarını değiştirir, her adımda kayıt yazılır: oyuncunun kaydı önce diske saklanır, sonra geri konur.
        /// Çekim yarıda kesildiyse (Play'de derleme, Unity kapandı) saklanan kopya kalır ve bir sonraki çekimin başında geri konur.</summary>
        private static IEnumerator KayitKorunarak(IEnumerator is_)
        {
            string klasor = Application.persistentDataPath;
            string saklama = Path.Combine(FragmanKaydi.Kok, "_kayit");
            string[] dosyalar = { "kayit.json", "kayit.yedek.json" };
            if (Directory.Exists(saklama)) KayitlariGeriKoy(klasor, saklama, dosyalar);

            Directory.CreateDirectory(saklama);
            foreach (string d in dosyalar)
            {
                string yol = Path.Combine(klasor, d);
                if (File.Exists(yol)) File.Copy(yol, Path.Combine(saklama, d), true);
            }
            try
            {
                yield return is_;
            }
            finally
            {
                KayitlariGeriKoy(klasor, saklama, dosyalar);
            }
        }

        private static void KayitlariGeriKoy(string klasor, string saklama, string[] dosyalar)
        {
            foreach (string d in dosyalar)
            {
                string yol = Path.Combine(klasor, d);
                string saklanan = Path.Combine(saklama, d);
                if (File.Exists(saklanan)) File.Copy(saklanan, yol, true);
                else if (File.Exists(yol)) File.Delete(yol);
            }
            Directory.Delete(saklama, true);
        }
    }
}
