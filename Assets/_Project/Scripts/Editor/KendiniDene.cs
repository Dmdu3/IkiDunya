using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IkiDunya.Core;
using UnityEditor;
using UnityEngine;

namespace IkiDunya.Editor
{
    /// <summary>Otomatik kontroller: akış sırası, kumsal durumunun süreli etkileri, kanıt eşlemesi ve metin id'leri.
    /// "İki Dünya → Kendini dene" ile çalışır; sonucu Console'a yazar. Oyun açmaya gerek yok.</summary>
    public static class KendiniDene
    {
        private const string MetinlerDosyasi = "docs/metinler.md";
        private const string KodKlasoru = "Assets/_Project/Scripts";

        private static int gecen;
        private static readonly List<string> kalan = new List<string>();

        [MenuItem("İki Dünya/Kendini dene", priority = 2)]
        public static void Calistir()
        {
            gecen = 0;
            kalan.Clear();

            AkisiDene();
            GeciciEtkileriDene();
            KanitlariDene();
            MetinleriDene();

            if (kalan.Count == 0)
                Debug.Log($"Kendini dene: {gecen} kontrolün hepsi geçti.");
            else
                Debug.LogError($"Kendini dene: {gecen} geçti, {kalan.Count} kaldı:\n- " + string.Join("\n- ", kalan));
        }

        private static void Bak(bool dogru, string ne)
        {
            if (dogru) gecen++;
            else kalan.Add(ne);
        }

        // ---- Oyun akışı ----

        private static void AkisiDene()
        {
            List<Adim> akis = GriKumsalKur.Ayar<OyunAyarlari>("OyunAyarlari").AkisiKur();
            Bak(akis.Count > 0 && akis[0].Tur == AdimTuru.Donus, "Akış Dönüş ile başlamalı.");
            Bak(akis.Count > 1 && akis[1].Tur == AdimTuru.Parkur && akis[1].Sira == (int)ParkurTuru.PosetSurusu, "Dönüşten sonra poşet sürüsü parkuru gelmeli.");
            Bak(akis.Count > 0 && akis[akis.Count - 1].Tur == AdimTuru.Final, "Akış Final ile bitmeli.");
            Bak(akis.Count(a => a.Tur == AdimTuru.Parkur) == 3, "Akışta 3 parkur olmalı.");
            Bak(akis.Any(a => a.Tur == AdimTuru.Parkur && a.Sira == (int)ParkurTuru.CopSeli), "Çöp seli parkuru akışta olmalı.");
            Bak(akis.Any(a => a.Tur == AdimTuru.Parkur && a.Sira == (int)ParkurTuru.CopGirdabi), "Çöp girdabı parkuru akışta olmalı.");

            // İnsan turları 1'den başlayıp aralıksız artmalı (kayıt ve kartlar tur numarasına bakar).
            List<int> turlar = akis.Where(a => a.Tur == AdimTuru.InsanTuru).Select(a => a.Sira).ToList();
            Bak(turlar.Count > 0 && turlar.Select((t, i) => t == i + 1).All(x => x), "İnsan turları 1, 2, 3… diye sıralı olmalı.");

            // Bölüm numarası hiç geri gitmemeli.
            bool sirali = true;
            for (int i = 1; i < akis.Count; i++) sirali &= akis[i].Bolum >= akis[i - 1].Bolum;
            Bak(sirali, "Bölüm numaraları akışta geri gitmemeli.");

            // Girdap parkuru iki ilk yüzüş adımının arasında.
            int girdap = akis.FindIndex(a => a.Tur == AdimTuru.Parkur && a.Sira == (int)ParkurTuru.CopGirdabi);
            Bak(girdap > 0 && girdap < akis.Count - 1
                && akis[girdap - 1].Tur == AdimTuru.IlkYuzus && akis[girdap - 1].Sira == 0
                && akis[girdap + 1].Tur == AdimTuru.IlkYuzus && akis[girdap + 1].Sira == 1,
                "Girdap parkuru İlk Yüzüş 0 ile 1'in arasında olmalı.");
            Debug.Log($"Kendini dene: akış {akis.Count} adım, {turlar.Count} insan turu.");
        }

        // ---- Kumsal durumu: süreli etkiler ----

        private static void GeciciEtkileriDene()
        {
            var kumsal = ScriptableObject.CreateInstance<KumsalDurumu>();
            try
            {
                kumsal.Sifirla(2, 5);

                kumsal.GeciciKatman(0, BolgeKatmani.Cop, 2, 1);
                Bak(kumsal.BolgeAl(0).Seviye(BolgeKatmani.Cop) == 2, "Süreli katman hemen uygulanmalı.");
                kumsal.BirHaftaGecti();
                Bak(kumsal.BolgeAl(0).Seviye(BolgeKatmani.Cop) == 0, "Süreli katman bir hafta sonra geri alınmalı.");

                // Sınırda kırpılan fark kadar geri alınır: 4 + 3 → 5 (sınır), sonra yine 4.
                kumsal.KatmanDegistir(1, BolgeKatmani.Isik, 4);
                kumsal.GeciciKatman(1, BolgeKatmani.Isik, 3, 1);
                Bak(kumsal.BolgeAl(1).Seviye(BolgeKatmani.Isik) == 5, "Katman sınırı aşmamalı.");
                kumsal.BirHaftaGecti();
                Bak(kumsal.BolgeAl(1).Seviye(BolgeKatmani.Isik) == 4, "Kırpılan süreli katman sadece uygulanan kadar geri alınmalı.");

                // İki turluk etki ilk haftada sürer, ikincide kalkar.
                kumsal.GeciciKumIsinmasi(1, 2);
                kumsal.BirHaftaGecti();
                Bak(kumsal.KumIsinmasi == 1, "İki turluk etki ilk haftadan sonra sürmeli.");
                kumsal.BirHaftaGecti();
                Bak(kumsal.KumIsinmasi == 0, "İki turluk etki ikinci haftada kalkmalı.");
            }
            finally
            {
                Object.DestroyImmediate(kumsal);
            }
        }

        // ---- Kayıp izi → kanıt ----

        private static void KanitlariDene()
        {
            var beklenen = new Dictionary<KayipNedeni, KanitTuru>
            {
                { KayipNedeni.SicakKum, KanitTuru.Isi },
                { KayipNedeni.Isik, KanitTuru.Isik },
                { KayipNedeni.Cop, KanitTuru.Cop },
                { KayipNedeni.Ag, KanitTuru.Cop },
                { KayipNedeni.Dalga, KanitTuru.Su },
                { KayipNedeni.Yengec, KanitTuru.Avci },
                { KayipNedeni.Marti, KanitTuru.Avci },
                { KayipNedeni.Tilki, KanitTuru.Avci },
            };
            foreach (KayipNedeni neden in System.Enum.GetValues(typeof(KayipNedeni)))
            {
                bool bulundu = beklenen.TryGetValue(neden, out KanitTuru kanit);
                Bak(bulundu && new KayipIzi(0, Vector3.zero, neden).Kanit == kanit, $"{neden} kaybı doğru kanıtı açmalı (GDD: Isı, Işık, Çöp, Avcı, Su).");
            }
        }

        // ---- Metinler ----

        private static void MetinleriDene()
        {
            string kok = Path.GetDirectoryName(Application.dataPath);
            string yol = Path.Combine(kok, MetinlerDosyasi);
            if (!File.Exists(yol))
            {
                Bak(false, $"{MetinlerDosyasi} bulunamadı.");
                return;
            }

            // metinler.md'deki id'ler: her tablo satırının ilk hücresi.
            var idBicimi = new Regex("^[a-z0-9_]+$");
            var idler = new HashSet<string>();
            var tekrar = new List<string>();
            foreach (string satir in File.ReadAllLines(yol))
            {
                string[] hucreler = satir.Split('|');
                if (hucreler.Length < 4) continue;
                string id = hucreler[1].Trim();
                if (!idBicimi.IsMatch(id)) continue;
                if (!idler.Add(id)) tekrar.Add(id);
            }
            Bak(tekrar.Count == 0, "metinler.md'de iki kez yazılmış id: " + string.Join(", ", tekrar.Distinct()));

            // Ek sütunlar da id olur (Açıklama → _aciklama …; bkz. MenuKur.MetinleriAktar).
            foreach (string id in idler.ToList())
                foreach (string ek in new[] { "_aciklama", "_not", "_a", "_b" }) idler.Add(id + ek);

            // Kodda Al("id") ile istenen her id metinler.md'de olmalı.
            var cagri = new Regex("\\.Al\\(\"([a-z0-9_]+)\"\\)");
            var eksik = new SortedSet<string>();
            foreach (string dosya in Directory.GetFiles(Path.Combine(kok, KodKlasoru), "*.cs", SearchOption.AllDirectories))
                foreach (Match m in cagri.Matches(File.ReadAllText(dosya)))
                    if (!idler.Contains(m.Groups[1].Value)) eksik.Add(m.Groups[1].Value);
            Bak(eksik.Count == 0, "Kodda kullanılıp metinler.md'de olmayan id: " + string.Join(", ", eksik));
        }
    }
}
