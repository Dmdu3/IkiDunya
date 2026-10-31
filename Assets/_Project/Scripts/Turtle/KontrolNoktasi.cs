using System;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Denizde kontrol noktası (Is Trigger alan). Yüzücü girince bildirilir; Son işaretliyse bölüm biter, Kapı işaretliyse bölümün ilk parçası biter (parkur gelir).</summary>
    [RequireComponent(typeof(Collider))]
    public class KontrolNoktasi : MonoBehaviour
    {
        [Tooltip("Bölümün sonu (kıyı ya da yosun adası).")]
        [SerializeField] private bool son;
        [Tooltip("Parkura (minigame) giriş kapısı: bölümün ilk parçası burada biter, parkurun sahnesi açılır.")]
        [SerializeField] private bool kapi;

        public bool Son => son;
        public bool Kapi => kapi;

        public static event Action<KontrolNoktasi> Ulasildi;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StatikleriSifirla() => Ulasildi = null;

        private void OnTriggerEnter(Collider diger)
        {
            if (diger.TryGetComponent(out DenizHareketi _)) Ulasildi?.Invoke(this);
        }
    }
}
