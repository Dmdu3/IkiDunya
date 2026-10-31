using UnityEngine;

namespace IkiDunya.Core
{
    /// <summary>Zeminin (Ground layer) yüksekliği: kumsal düz değilken nesneler yere oturtulur. Gri kutu kumda da aynı çalışır (kutunun üstü).</summary>
    public static class Zemin
    {
        // Kumsalın en yüksek kumulundan yukarıdan aşağı bakılır.
        private const float Tepe = 100f;
        private const float Mesafe = 200f;

        private static int maske;

        private static int Maske => maske != 0 ? maske : maske = LayerMask.GetMask("Ground");

        /// <summary>Noktanın altındaki (ya da üstündeki) zeminin yüksekliği; zemin yoksa verilen değer.</summary>
        public static float Yukseklik(Vector3 nokta, float yoksa)
        {
            var baslangic = new Vector3(nokta.x, Tepe, nokta.z);
            return Physics.Raycast(baslangic, Vector3.down, out RaycastHit carpan, Mesafe, Maske, QueryTriggerInteraction.Ignore)
                ? carpan.point.y
                : yoksa;
        }

        /// <summary>Noktayı zeminin üstüne indirir ya da çıkarır; zemin yoksa olduğu gibi.</summary>
        public static Vector3 Oturt(Vector3 nokta) => new Vector3(nokta.x, Yukseklik(nokta, nokta.y), nokta.z);
    }
}
