using System;
using System.Collections.Generic;
using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Oyuncunun gördüğü yazılar: docs/metinler.md'den (İki Dünya → Metinleri aktar) gelen id → metin tablosu. Kod yazıyı id ile ister.</summary>
    [CreateAssetMenu(menuName = "İki Dünya/Metin Tablosu", fileName = "MetinTablosu")]
    public class MetinTablosu : ScriptableObject
    {
        /// <summary>Tek bir satır: id ve metin.</summary>
        [Serializable]
        public class Satir
        {
            [SerializeField] private string id;
            [SerializeField] private string metin;

            public Satir(string id, string metin)
            {
                this.id = id;
                this.metin = metin;
            }

            public string Id => id;
            public string Metin => metin;
        }

        [SerializeField] private List<Satir> satirlar = new List<Satir>();

        private Dictionary<string, string> sozluk;

        /// <summary>Yüklenmiş tablo (OyunAyarlari üstünden yüklenir); bağlantısı kopmuş yazılar bunu kullanır.</summary>
        public static MetinTablosu Yuklu { get; private set; }

        private void OnEnable()
        {
            Yuklu = this;
            sozluk = null;
        }

        /// <summary>Id'nin metni; metinler.md'de henüz yoksa id'nin kendisi köşeli parantezle döner, eksik hemen görünür.</summary>
        public string Al(string id)
        {
            if (sozluk == null)
            {
                sozluk = new Dictionary<string, string>();
                foreach (Satir satir in satirlar) sozluk[satir.Id] = satir.Metin;
            }
            return sozluk.TryGetValue(id, out string metin) && !string.IsNullOrEmpty(metin) ? metin : $"[{id}]";
        }

        /// <summary>Editör aracı tabloyu baştan yazar.</summary>
        public void Yaz(List<Satir> yeni)
        {
            satirlar = yeni;
            sozluk = null;
        }
    }
}
