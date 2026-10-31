using UnityEngine;

namespace IkiDunya.Hazards
{
    /// <summary>Deniz akıntısı (Is Trigger alan). Yönü objenin ileri yönüdür; içindeki yüzücüyü sürükler, karşı yüzmek daha çok enerji harcatır.</summary>
    [RequireComponent(typeof(Collider))]
    public class Akinti : MonoBehaviour
    {
        [SerializeField] private DenizTehlikeAyarlari ayarlar;

        /// <summary>Akıntının sürükleme hızı (dünya yönünde).</summary>
        public Vector3 Hiz => transform.forward * ayarlar.AkintiHizi;
    }
}
