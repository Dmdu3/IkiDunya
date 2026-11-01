using IkiDunya.Core;
using UnityEngine.UI;

namespace IkiDunya.UI
{
    /// <summary>Düğmelere tıklama sesi bağlar. Kapalı düğme tıklanmadığı için ses de çıkmaz; sahne geçişi sürerken menü düğmeleri iş yapmadığı için ses de çalınmaz.</summary>
    public static class DugmeSesi
    {
        public static void Bagla(params Button[] dugmeler)
        {
            foreach (Button dugme in dugmeler)
                if (dugme != null) dugme.onClick.AddListener(Cal);
        }

        private static void Cal()
        {
            if (!OyunAkisi.GecisSuruyor) Ses.Cal(SesTuru.DugmeTikla);
        }
    }
}
