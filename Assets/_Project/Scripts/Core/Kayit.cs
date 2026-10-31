using System;
using System.IO;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyunun kaydı: hangi adımda olunduğu ve kumsalın bütün durumu, tek bir JSON dosyasında. Her adım başında yazılır.</summary>
    public static class Kayit
    {
        [Serializable]
        private class Veri
        {
            [SerializeField] private int sira;
            [SerializeField] private string kumsal;

            public Veri() { }

            public Veri(int sira, string kumsal)
            {
                this.sira = sira;
                this.kumsal = kumsal;
            }

            public int Sira => sira;
            public string Kumsal => kumsal;
        }

        private static string Yol => Path.Combine(Application.persistentDataPath, "kayit.json");

        public static bool Var => File.Exists(Yol);

        public static void Kaydet(int sira, KumsalDurumu kumsal)
        {
            var veri = new Veri(sira, JsonUtility.ToJson(kumsal));
            File.WriteAllText(Yol, JsonUtility.ToJson(veri));
        }

        /// <summary>Kaydı kumsala yükler ve kayıtlı adımın sırasını verir; kayıt yoksa ya da bozuksa false.</summary>
        public static bool Yukle(KumsalDurumu kumsal, out int sira)
        {
            sira = -1;
            if (!Var) return false;
            try
            {
                var veri = JsonUtility.FromJson<Veri>(File.ReadAllText(Yol));
                JsonUtility.FromJsonOverwrite(veri.Kumsal, kumsal);
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
