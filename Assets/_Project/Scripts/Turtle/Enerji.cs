using System;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Kaplumbağanın enerjisi (0–en fazla). Hareket ve tehditler Harca ile azaltır; bitince Bitti gelir.</summary>
    public class Enerji : MonoBehaviour
    {
        private float enFazla;
        private float deger;

        /// <summary>Doluluk oranı (0–1) değiştiğinde.</summary>
        public event Action<float> Degisti;
        public event Action Bitti;

        public float Deger => deger;
        public float Oran => enFazla > 0f ? deger / enFazla : 0f;
        public bool BittiMi => deger <= 0f;

        /// <summary>Enerjiyi doldurur; bölüm başında ya da kontrol noktasında çağrılır.</summary>
        public void Doldur(float yeniEnFazla)
        {
            enFazla = yeniEnFazla;
            deger = enFazla;
            Degisti?.Invoke(Oran);
        }

        public void Ekle(float miktar)
        {
            if (BittiMi || miktar <= 0f) return;
            deger = Mathf.Min(enFazla, deger + miktar);
            Degisti?.Invoke(Oran);
        }

        public void Harca(float miktar)
        {
            if (BittiMi || miktar <= 0f) return;
            deger = Mathf.Max(0f, deger - miktar);
            Degisti?.Invoke(Oran);
            if (BittiMi) Bitti?.Invoke();
        }
    }
}
