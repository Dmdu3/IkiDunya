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

        public static bool Var => File.Exists(Yol);

        public static void Kaydet(int sira, KumsalDurumu kumsal, IReadOnlyList<IOyunVerisi> ekVeriler)
        {
            var ekler = new List<string>();
            foreach (IOyunVerisi ek in ekVeriler) ekler.Add(ek.Yaz());
            var veri = new Veri(sira, JsonUtility.ToJson(kumsal), ekler);
            File.WriteAllText(Yol, JsonUtility.ToJson(veri));
        }

        /// <summary>Kaydı kumsala yükler ve kayıtlı adımın sırasını verir; kayıt yoksa ya da bozuksa false.</summary>
        public static bool Yukle(KumsalDurumu kumsal, IReadOnlyList<IOyunVerisi> ekVeriler, out int sira)
        {
            sira = -1;
            if (!Var) return false;
            try
            {
                var veri = JsonUtility.FromJson<Veri>(File.ReadAllText(Yol));
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
                Debug.LogWarning($"Kayit: kayıt okunamadı ({hata.Message}).");
                return false;
            }
        }

        public static void Sil()
        {
            if (Var) File.Delete(Yol);
        }
    }
}
