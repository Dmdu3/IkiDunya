namespace IkiDunya.Core
{
    /// <summary>KumsalDurumu dışında kayda giren oyun verisi (ör. insan kısmının bütçesi, kanıtları). OyunAkisi yeni oyunda sıfırlar, Kayit yazar ve okur.</summary>
    public interface IOyunVerisi
    {
        /// <summary>Yeni oyunun ilk hali.</summary>
        void YeniOyun();
        /// <summary>Kayda girecek JSON.</summary>
        string Yaz();
        /// <summary>Kayıttan geri yükler; asset bağlantıları korunur.</summary>
        void Oku(string json);
    }
}
