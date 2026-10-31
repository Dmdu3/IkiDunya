using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Denizdeki kaplumbağa; deniz tehlikeleri (tekne, avcı balık, dalga) kaplumbağa koduna doğrudan bağlanmasın diye burada.</summary>
    public interface IYuzucu
    {
        Vector3 Konum { get; }
        /// <summary>Su yüzeyinde mi (tekne çarpar, dalga savurur).</summary>
        bool Yuzeyde { get; }
        /// <summary>Kaya arası ya da deniz çayırında mı (avcı balık görmez).</summary>
        bool Gizli { get; }
        void EnerjiKaybet(float miktar);
        /// <summary>Avcı yakaladı: son kontrol noktasına dönülür.</summary>
        void Yakala();
        void Savrul(Vector3 yer);
    }
}
