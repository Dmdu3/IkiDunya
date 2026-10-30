using System;
using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Kaçış gecesinin gün doğumu sayacı. Oran 0'dan (gece) 1'e (gün doğdu) gider; sıcak lekeler buna göre büyür.</summary>
    public class GunDogumu : MonoBehaviour
    {
        [SerializeField] private KacisAyarlari ayarlar;

        private float gecen;
        private bool calisiyor;

        public event Action Bitti;

        public float Oran => Mathf.Clamp01(gecen / ayarlar.GunDogumuSuresi);
        public float Kalan => Mathf.Max(0f, ayarlar.GunDogumuSuresi - gecen);
        public bool Calisiyor => calisiyor;

        public void Baslat()
        {
            gecen = 0f;
            calisiyor = true;
        }

        public void Durdur() => calisiyor = false;

        private void Update()
        {
            if (!calisiyor) return;
            gecen += Time.deltaTime;
            if (gecen < ayarlar.GunDogumuSuresi) return;
            calisiyor = false;
            Bitti?.Invoke();
        }
    }
}
