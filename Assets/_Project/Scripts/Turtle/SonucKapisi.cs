using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Gece sonucunun kapanması: panel en az OyunAyarlari.SonucEnAzSure açık kalır ve gecenin sonunda basılı tutulan tuş
    /// (son yavruyu zıplatırken Space) bir kez bırakılmadan basış kabul edilmez; sonuç okunmadan geçilmez.</summary>
    public class SonucKapisi
    {
        private float acilis;
        private bool birakildi;

        public void Ac()
        {
            acilis = Time.unscaledTime;
            birakildi = false;
        }

        /// <summary>Her karede çağrılır; oyuncu sonucu kapatmak istediyse true.</summary>
        public bool KapatIstendi()
        {
            if (!GameInput.TiklaBasili && !GameInput.ZiplaDalBasili) birakildi = true;
            float enAz = OyunAkisi.Ayarlar != null ? OyunAkisi.Ayarlar.SonucEnAzSure : 0f;
            return birakildi && Time.unscaledTime - acilis >= enAz && (GameInput.TiklaBasildi || GameInput.ZiplaDalBasildi);
        }
    }
}
