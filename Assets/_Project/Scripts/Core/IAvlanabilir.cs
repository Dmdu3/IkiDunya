using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Avcıların yakalayabildiği şey (yavru). Tehlikeler kaplumbağa koduna doğrudan bağlanmasın diye burada.</summary>
    public interface IAvlanabilir
    {
        Vector3 Konum { get; }
        /// <summary>Hâlâ kumsalda ve yakalanabilir durumda mı.</summary>
        bool Yakalanabilir { get; }
        /// <summary>Zıplıyor mu (yengeç deliğinin üstünden atlarken yakalanmaz).</summary>
        bool Havada { get; }
        /// <summary>Kumul gölgesi gibi bir siperde mi (tilki görmez).</summary>
        bool Gizli { get; }
        void Yakala(KayipNedeni neden);
    }
}
