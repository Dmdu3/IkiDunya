using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Turtle
{
    /// <summary>Oyuncunun enerjisi eşiğin altına inince tehlikeyi bildirir (ekran kenarı kararır, kalp atar). Oyuncunun kaplumbağasında durur.</summary>
    [RequireComponent(typeof(Enerji))]
    public class EnerjiUyarisi : MonoBehaviour
    {
        [SerializeField] private GeribildirimAyarlari ayarlar;

        private Enerji enerji;

        private void Awake() => enerji = GetComponent<Enerji>();

        private void Update()
        {
            float oran = enerji.Oran;
            Geribildirim.Tehlike = oran >= ayarlar.TehlikeEsigi ? 0f : 1f - oran / ayarlar.TehlikeEsigi;
        }

        private void OnDisable() => Geribildirim.Tehlike = 0f;
    }
}
