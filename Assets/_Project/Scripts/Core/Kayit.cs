using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyunun kaydı: hangi adımda olunduğu, kumsalın bütün durumu ve diğer oyun verileri (IOyunVerisi), tek bir JSON dosyasında. Her adım başında yazılır.</summary>
    public static class Kayit
    {
        [Serializable]
        private class Veri
        {
            [SerializeField] private int sira;
            [SerializeField] private string kumsal;
            [SerializeField] private List<string> ekler = new List<string>();

            public Veri() { }

            public Veri(int sira, string kumsal, List<string> ekler)
            {
                this.sira = sira;
                this.kumsal = kumsal;
                this.ekler = ekler;
            }

            public int Sira => sira;
            public string Kumsal => kumsal;
            public List<string> Ekler => ekler;
        }

        private static string Yol => Path.Combine(Application.persistentDataPath, "kayit.json");
        /// <summary>Bir önceki adımın kaydı; asıl kayıt bozulursa bundan devam edilir.</summary>
        private static string YedekYol => Path.Combine(Application.persistentDataPath, "kayit.yedek.json");
        private static string GeciciYol => Path.Combine(Application.persistentDataPath, "kayit.tmp");
        /// <summary>Okunamayan asıl kaydın kopyası (hata ayıklamak için); oyun okumaz.</summary>
        private static string BozukYol => Path.Combine(Application.persistentDataPath, "kayit.bozuk.json");

        public static bool Var => File.Exists(Yol) || File.Exists(YedekYol);

        /// <summary>Önce geçici dosyaya yazılır, sonra eski kayıt yedeğe alınıp yerine konur: yazarken oyun kapanırsa kayıt yarım kalmaz.</summary>
        public static void Kaydet(int sira, KumsalDurumu kumsal, IReadOnlyList<IOyunVerisi> ekVeriler)
        {
            var ekler = new List<string>();
            foreach (IOyunVerisi ek in ekVeriler) ekler.Add(ek.Yaz());
            var veri = new Veri(sira, JsonUtility.ToJson(kumsal), ekler);
            File.WriteAllText(GeciciYol, JsonUtility.ToJson(veri));
            if (File.Exists(Yol))
            {
                File.Copy(Yol, YedekYol, true);
                File.Delete(Yol);
            }
            File.Move(GeciciYol, Yol);
        }

        /// <summary>Kaydı kumsala yükler ve kayıtlı adımın sırasını verir; asıl kayıt bozuksa yedekten. İkisi de yoksa ya da bozuksa false (dosyalar silinmez).</summary>
        public static bool Yukle(KumsalDurumu kumsal, IReadOnlyList<IOyunVerisi> ekVeriler, out int sira)
        {
            if (File.Exists(Yol) && Oku(Yol, kumsal, ekVeriler, out sira)) return true;
            if (File.Exists(YedekYol) && Oku(YedekYol, kumsal, ekVeriler, out sira))
            {
                Debug.LogWarning("Kayit: asıl kayıt okunamadı, bir önceki adımın yedeğinden devam ediliyor.");
                // Bozuk asıl kayıt kenara alınır; yoksa sonraki Kaydet onu yedeğin üstüne yazardı.
                if (File.Exists(Yol)) File.Copy(Yol, BozukYol, true);
                if (File.Exists(Yol)) File.Delete(Yol);
                return true;
            }
            sira = -1;
            return false;
        }

        private static bool Oku(string yol, KumsalDurumu kumsal, IReadOnlyList<IOyunVerisi> ekVeriler, out int sira)
        {
            sira = -1;
            try
            {
                var veri = JsonUtility.FromJson<Veri>(File.ReadAllText(yol));
                if (veri == null || string.IsNullOrEmpty(veri.Kumsal)) throw new InvalidDataException("boş kayıt");
                JsonUtility.FromJsonOverwrite(veri.Kumsal, kumsal);
                // Eski kayıtta olmayan veri yeni oyundaki haliyle kalır.
                for (int i = 0; i < ekVeriler.Count; i++)
                {
                    if (veri.Ekler != null && i < veri.Ekler.Count) ekVeriler[i].Oku(veri.Ekler[i]);
                    else ekVeriler[i].YeniOyun();
                }
                sira = veri.Sira;
                return true;
            }
            catch (Exception hata)
            {
                Debug.LogWarning($"Kayit: {Path.GetFileName(yol)} okunamadı ({hata.Message}).");
                return false;
            }
        }

        public static void Sil()
        {
            foreach (string yol in new[] { Yol, YedekYol, GeciciYol })
                if (File.Exists(yol)) File.Delete(yol);
        }
    }
}
